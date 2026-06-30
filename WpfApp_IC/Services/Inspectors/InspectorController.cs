using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using Observable;
using System;
using System.Diagnostics;
using System.Timers;
using System.Windows;
using System.Windows.Media.Imaging;
using WpfApp_IC.Data;
using WpfApp_IC.Models.DbContext;
using WpfApp_IC.Models.Devices;
using WpfApp_IC.Views;
using WpfApp_IC.Services.Camera;
using WpfApp_IC.Services.ModbusT;
using WpfApp_IC.Models;
using System.Linq.Expressions;
using System.Windows.Xps.Packaging;

namespace WpfApp_IC.Services.Inspectors
{
    /// <summary>
    /// Главный контроллер инспекции.
    /// Управляет камерой, датчиком, отбраковщиком и выполняет проверку DataMatrix.
    /// Не зависит от UI и не содержит логики отображения.
    /// </summary>
    public class InspectorController(
        WorkSession workSession,
        LogService log,
        ICameraService camera,
        ModbusSensor sensor,
        ModbusRejector rejector,
        IModbusService modbus,
        IDbContextFactory<AppDbContext> dbContextFactory,
        ImageSaverService imageSaver) : IInspectorController
    {
        private CancellationTokenSource? _cts;
        private BitmapSource? _lastFrame;

        public string ModbusIp { get; set; } = "192.168.0.127";
        public int ModbusPort { get; set; } = 502;
        public int SignalCoil { get; set; } = 12;
        public int RejectCoil { get; set; } = 10;

        /// <summary>
        /// Задержка перед активацией отбраковщика.
        /// </summary>
        public int RejectDelay { get; set; } = 300;
        public int MotionFilterInterval { get; set; } = 200;
        public int SensorPollInterval { get; set; } = 10;

        public string CameraIp { get; set; } = "";

        // Фильтрация дребезга
        private DateTime _motionDetectedTime;
        private int _currentSignal = 0;
        private bool _triggerInProgress = false;

        public event Action<DataMatrixResult>? DataMatrixRead;
        public event Action<string?, BitmapSource>? FrameReceived;
        public event Action<string, bool>? CodeChecked;

        public event EventHandler? MotionDetected;

        public void Start()
        {
            try
            {
                modbus.Connect(ModbusIp, ModbusPort);
                log.AddEntry($"MODBUS модуль подключен: {ModbusIp}:{ModbusPort}");
            }
            catch
            {
                log.AddEntry("Не удалось подключиться к модулю MODBUS", LogColorCode.Red);
            }

            try
            {
                camera.Open(CameraIp);
                log.AddEntry($"Соединение с камерой установлено [{CameraIp}]");
            }
            catch
            {
                log.AddEntry($"Не удалось установить соединение с камерой [{CameraIp}]", LogColorCode.Red);
            }

            _triggerInProgress = false;

            MotionDetected += (s, e) => Inspect();

            _cts = new();
            Task.Run(() => ListenSensorAsync(_cts.Token));
        }
        public void Stop()
        {
            MotionDetected -= (s, e) => Inspect();
            _cts?.Cancel();
            Thread.Sleep(500);
            camera.Close();
            modbus?.Disconnect();
        }
        public DataMatrixResult? TriggerCamera()
        {
            if (_triggerInProgress)
                return null;

            try
            {
                _triggerInProgress = true;
                var (dm, frame) = camera.TriggerAndRead();
                _triggerInProgress = false;

                if (frame != null)
                {
                    _lastFrame = frame;
                    FrameReceived?.Invoke(dm?.Raw, frame);
                }

                return dm;
            }
            catch
            {
                return null;
            }
        }

        private void Inspect()
        {
            Task.Run(async () =>
            {
                CancellationTokenSource rejectCts = new();

                _ = Task.Run(async () =>
                {
                    try
                    {
                        await Task.Delay(RejectDelay, rejectCts.Token);
                        await rejector.Activate();
                    }
                    catch { }
                }, rejectCts.Token);

                workSession.TotalCount++;

                DataMatrixResult? dm = TriggerCamera();

                if (dm != null)
                    DataMatrixRead?.Invoke(dm);

                ValidationResult result = await ValidateAsync(dm?.Raw);

                if (result.IsOk)
                {
                    rejectCts.Cancel();
                    workSession.Verified++;
                }
                else
                {
                    switch (result.ErrorCode)
                    {
                        case "NO_READ":
                            log.AddEntry($"Код не считан: NULL", LogColorCode.Yellow);
                            break;
                        case "NOT_FOUND":
                            log.AddEntry($"Неверный код: {dm?.Raw}", LogColorCode.Red);
                            break;
                        case "DUPLICATE":
                            if (workSession.WorkMode == WorkMode.SkipDuplicates)
                                rejectCts.Cancel();
                            log.AddEntry($"Дубликат: {dm?.Raw}", LogColorCode.Red);
                            break;
                    }

                    workSession.Rejected++;

                    if (_lastFrame != null)
                        imageSaver.SaveReject(_lastFrame, dm?.Normalized);
                }

                CodeChecked?.Invoke(dm?.Normalized ?? "<NO READ>", result.IsOk);
            });
        }
        private async Task ListenSensorAsync(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                try
                {
                    int signal = sensor.Read();

                    if (signal == 1 && _currentSignal == 0 && (DateTime.Now - _motionDetectedTime).TotalMilliseconds > MotionFilterInterval)
                    {
                        MotionDetected?.Invoke(this, EventArgs.Empty);
                        _motionDetectedTime = DateTime.Now;
                    }

                    _currentSignal = signal;
                    await Task.Delay(SensorPollInterval, token);
                }
                catch { }
            }
        }
        private async Task<ValidationResult> ValidateAsync(string? dm)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(dm))
                    return ValidationResult.NoRead();

                await using var db = await dbContextFactory.CreateDbContextAsync();
                if (db.tmp_mains.Any(c => c.Code == dm))
                    return ValidationResult.Duplicate();

                /*if (!workSession.Codes.TryGetValue(dm, out bool status))
                    return ValidationResult.NotFound();

                if (status)
                    return ValidationResult.Duplicate();

                workSession.Codes[dm] = true;*/

                _ = Task.Run(() =>
                {
                    using var db = dbContextFactory.CreateDbContext();
                    printer_base? code = db.printer_bases.First(c => c.Code == dm);

                    db.tmp_mains.Add(new()
                    {
                        Code = code.Code,
                        StatusId = 2,
                        DateImport = code.DateImport,
                        DatePrint = code.DatePrint,
                        DateVerify = DateTime.Now,
                        GtinId = code.GtinId,
                        OperatorName = code.OperatorName,
                        OrderID = code.OrderID
                    });

                    try { db.SaveChanges(); }
                    catch { log.AddEntry($"Код уже верифицирован: {code.Code}", LogColorCode.Red); }
                });
                //_ = Task.Run(async () =>
                //{
                //    await using var db = await dbContextFactory.CreateDbContextAsync();
                //    printer_base? code = await db.printer_bases.FirstAsync(c => c.Code == dm);

                //    db.tmp_mains.Add(new()
                //    {
                //        Code = code.Code,
                //        StatusId = 2,
                //        DateImport = code.DateImport,
                //        DatePrint = code.DatePrint,
                //        DateVerify = DateTime.Now,
                //        GtinId = code.GtinId,
                //        OperatorName = code.OperatorName,
                //        OrderID = code.OrderID
                //    });

                //    await db.SaveChangesAsync();
                //});

                return ValidationResult.Ok();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Возникла ошибка при валидации кода в базе данных.\n\nException message:\n\n{ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return ValidationResult.NotFound();
            }
        }
    }
}
