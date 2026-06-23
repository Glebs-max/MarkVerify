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
using WpfApp_IC.Services.Log;
using WpfApp_IC.Services.ModbusT;
using WpfApp_IC.Models;

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
        ImageSaverService imageSaver) : IInspectorController, IDisposable
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
        public int SensorFilterCount { get; set; } = 2;

        /// <summary> SensorPollIntervalMs - интервал опроса датчика /// </summary>
        public int SensorPollIntervalMs { get; set; } = 10;

        public string CameraIp { get; set; } = "";

        // Фильтрация дребезга
        private int _stableSignal = -1;
        private int _previousSignal = -1;
        private int _sameCount = 0;
        //private int FILTER_COUNT = 2;
        private bool _triggerInProgress = false;

        public event Action<DataMatrixResult>? DataMatrixRead;
        public event Action<string?, BitmapSource>? FrameReceived;
        public event Action<string, bool>? CodeChecked;

        public event EventHandler? MotionDetected;

        /// <summary>
        /// Запускает инспекцию: подключает Modbus, открывает камеру и запускает цикл чтения датчика.
        /// </summary>
        public void Start()
        {
            try
            {
                modbus.Connect(ModbusIp, ModbusPort);
                log.Info($"Modbus подключён: {ModbusIp}:{ModbusPort}");
            }
            catch (Exception ex)
            {
                log.Error("Ошибка подключения Modbus", ex);
            }

            try
            {
                camera.Open(CameraIp);
                log.Info($"Камера [{CameraIp}] открыта");
            }
            catch (Exception ex)
            {
                log.Error($"Ошибка открытия камеры [{CameraIp}]", ex);
            }

            _stableSignal = -1;
            _previousSignal = -1;
            _sameCount = 0;
            _triggerInProgress = false;

            MotionDetected += (s, e) => Inspect();

            _cts = new();
            Task.Run(() => ListenSensorAsync(_cts.Token));
            log.Info("Инспекция запущена");
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
                        log.Info("REJECTOR ACTIVATED");
                    }
                    catch { }
                }, rejectCts.Token);

                var swCamera = Stopwatch.StartNew();
                DataMatrixResult? dm = TriggerCamera();
                swCamera.Stop();
                //log.Info($"[PERF Камера: {swCamera.ElapsedMilliseconds} мс");

                var swTotal = Stopwatch.StartNew();
                if (dm != null)
                    DataMatrixRead?.Invoke(dm);

                var swDb = Stopwatch.StartNew();
                ValidationResult result = await ValidateAsync(dm?.Raw);
                swDb.Stop();

                if (result.IsOk)
                {
                    rejectCts.Cancel();
                    labelingSession.Verified++;
                }
                else
                {
                    if (_lastFrame != null)
                        imageSaver.SaveReject(_lastFrame, dm?.Normalized);

                    if (result.ErrorCode != "NO_READ")
                        labelingSession.Rejected++;
                }

                CodeChecked?.Invoke(dm?.Normalized ?? "<NO READ>", result.IsOk);

                swTotal.Stop();
                //log.Info($"[PERF Валидация в БД: {swDb.ElapsedMilliseconds} мс, всего: {swTotal.ElapsedMilliseconds} мс");
                //log.Info($"Проверка: [{dm?.Normalized ?? "<NO READ>"}] -> {(result.IsOk ? "OK" : "BRK")}");
            });
        }

        /// <summary>
        /// Основной цикл: читает датчик, фильтрует дребезг, вызывает триггер камеры.
        /// </summary>
        private async Task ListenSensorAsync(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                try
                {
                    int rawSignal = sensor.Read();

                    if (rawSignal == _stableSignal)
                        _sameCount++;
                    else
                    {
                        _sameCount = 0;
                        _stableSignal = rawSignal;
                    }

                    if (_stableSignal != _previousSignal && _sameCount >= SensorFilterCount)
                    {
                        _previousSignal = _stableSignal;

                        if (_stableSignal == 1)
                        {
                            MotionDetected?.Invoke(this, EventArgs.Empty);
                            log.Info("MOTION DETECTED");
                        }
                    }

                    await Task.Delay(SensorPollIntervalMs, token);
                }
                catch (Exception ex)
                {
                    log.Error("Ошибка в цикле инспекции", ex);
                }
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

        /// <summary>
        /// Останавливает инспекцию.
        /// </summary>
        public void Stop()
        {
            MotionDetected -= (s, e) => Inspect();
            _cts?.Cancel();
            Thread.Sleep(100);
            camera.Close();
            log.Info("Инспекция остановлена");
        }

        public void Dispose()
        {
            Stop();
            modbus?.Disconnect();
            camera.Dispose();
        }

        /// <summary>
        /// Ручной триггер камеры — для тестирования без датчика.
        /// Запускает снимок напрямую, минуя цикл опроса датчика.
        /// </summary>
        public DataMatrixResult? TriggerCamera()
        {
            while (_triggerInProgress)
                continue;

            try
            {
                log.Info("CAMERA TRIGGERED");
                _triggerInProgress = true;
                var (dm, frame) = camera.TriggerAndRead();
                _triggerInProgress = false;

                if (frame != null)
                {
                    _lastFrame = frame;
                    FrameReceived?.Invoke(dm?.Raw, frame);
                }
                else
                    log.Warning("Кадр не получен");

                return dm;
            }
            catch (Exception ex)
            {
                log.Error("Ошибка триггера камеры", ex);
                return null;
            }
        }
    }
}
