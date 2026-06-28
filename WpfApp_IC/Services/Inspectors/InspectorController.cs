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

namespace WpfApp_IC.Services.Inspectors
{
    /// <summary>
    /// Главный контроллер инспекции.
    /// Управляет камерой, датчиком, отбраковщиком и выполняет проверку DataMatrix.
    /// Не зависит от UI и не содержит логики отображения.
    /// </summary>
    public class InspectorController(
        LabelingSession labelingSession,
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
        public int RejectDelayMs { get; set; } = 200;

        /// <summary> SensorFilterCount - Периодичность стабильного сигнала, фильтр дребезга датчика /// </summary>
        public int SensorOnFilter { get; set; } = 1;
        public int SensorOffFilter { get; set; } = 3;

        /// <summary> SensorPollIntervalMs - интервал опроса датчика /// </summary>
        public int SensorPollIntervalMs { get; set; } = 10;

        public string CameraIp { get; set; } = "";

        // Фильтрация дребезга
        private int _stableSignal = -1;
        private int _previousSignal = -1;
        private int _sameCount = 0;
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

            _stableSignal = 0;
            _previousSignal = -1;
            _sameCount = 0;
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
                        await Task.Delay(RejectDelayMs, rejectCts.Token);
                        await rejector.Activate();
                        labelingSession.Rejected++;
                    }
                    catch { }
                }, rejectCts.Token);

                DataMatrixResult? dm = TriggerCamera();

                if (dm != null)
                    DataMatrixRead?.Invoke(dm);

                ValidationResult result = await ValidateAsync(dm?.Raw);

                if (result.IsOk)
                {
                    rejectCts.Cancel();
                    labelingSession.Verified++;
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
                            if (labelingSession.WorkMode == WorkMode.SkipDuplicates)
                                rejectCts.Cancel();
                            log.AddEntry($"Дубликат: {dm?.Raw}", LogColorCode.Red);
                            break;
                    }

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

                    if (_stableSignal != signal)
                    {
                        _sameCount++;

                        if (_sameCount >= (_stableSignal == 0 ? SensorOnFilter : SensorOffFilter))
                        {
                            if (signal == 1)
                                MotionDetected?.Invoke(this, EventArgs.Empty);

                            _stableSignal = signal;
                            _sameCount = 0;
                        }
                    }
                    else
                        _sameCount = 0;

                    await Task.Delay(SensorPollIntervalMs, token);
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
                printer_base? code = await db.printer_bases.FirstOrDefaultAsync(c => c.Code == dm && c.GtinId == labelingSession.GTIN.GtinId && c.StatusId == 1);

                if (code == null)
                    return ValidationResult.NotFound();

                if (await db.mains.FirstOrDefaultAsync(c => c.Code == dm && c.GtinId == labelingSession.GTIN.GtinId && c.StatusId == 2) != null ||
                    await db.tmp_mains.FirstOrDefaultAsync(c => c.Code == dm && c.GtinId == labelingSession.GTIN.GtinId && c.StatusId == 2) != null)
                    return ValidationResult.Duplicate();

                tmp_main verified = new()
                {
                    Code = code.Code,
                    StatusId = 2,
                    DateImport = code.DateImport,
                    DatePrint = code.DatePrint,
                    DateVerify = DateTime.Now,
                    GtinId = code.GtinId,
                    OperatorName = code.OperatorName,
                    OrderID = code.OrderID
                };

                db.tmp_mains.Add(verified);
                await db.SaveChangesAsync();

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
