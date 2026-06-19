using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using Observable;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using WpfApp_IC.Data;
using WpfApp_IC.Devices;
using WpfApp_IC.Models;
using WpfApp_IC.Pages;
using WpfApp_IC.Services.Camera;
using WpfApp_IC.Services.Log;
using WpfApp_IC.Services.ModbusT;

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
        private long _frameCounter = 0;
        private DateTime _signalStartTime;

        public string ModbusIp { get; set; } = "192.168.0.127";
        public int ModbusPort { get; set; } = 502;
        public int SignalCoil { get; set; } = 12;
        public int RejectCoil { get; set; } = 10;

        /// <summary>
        /// Задержка перед активацией отбраковщика.
        /// </summary>
        public int RejectDelayMs { get; set; } = 200;

        /// <summary> SensorFilterCount - Периодичность стабильного сигнала, фильтр дребезга датчика /// </summary>
        public int SensorFilterCount { get; set; } = 0;

        /// <summary> SensorPollIntervalMs - интервал опроса датчика /// </summary>
        public int SensorPollIntervalMs { get; set; } = 10;

        public string CameraIp { get; set; } = "";

        // Фильтрация дребезга
        private int _stableSignal = -1;
        private int _previousSignal = -1;
        private int _sameCount = 0;
        //private const int FILTER_COUNT = 2;

        private bool _triggerInProgress = false;

        // События для ViewModel
        public event Action<int>? SignalChanged;
        public event Action<DataMatrixResult>? DataMatrixRead;
        public event Action<string>? ErrorOccurred;
        public event Action<string?, BitmapSource>? FrameReceived;
        public event Action<string, bool>? CodeChecked;

        /// <summary>
        /// Событие результата проверки DataMatrix.
        /// ViewModel подписывается на него для подсчёта OK/BRK.
        /// </summary>
        public event Action<ValidationResult>? CodeValidated;

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

            _cts = new CancellationTokenSource();
            Task.Run(() => Loop(_cts.Token));

            log.Info("Инспекция запущена");
        }

        /// <summary>
        /// Основной цикл: читает датчик, фильтрует дребезг, вызывает триггер камеры.
        /// </summary>
        private async Task Loop(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                try
                {
                    int rawSignal = sensor.Read();

                    // Фильтрация дребезга
                    if (rawSignal == _stableSignal)
                        _sameCount++;
                    else
                    {
                        _sameCount = 0;
                        _stableSignal = rawSignal;
                        if (rawSignal == 1)
                            _signalStartTime = DateTime.Now;
                    }

                    if (_stableSignal != _previousSignal && _sameCount >= SensorFilterCount)
                    {
                        _previousSignal = _stableSignal;
                        SignalChanged?.Invoke(_stableSignal);
                        log.Info($"Сигнал датчика: {_stableSignal}");

                        if (_stableSignal == 1 && !_triggerInProgress)
                        {
                            _triggerInProgress = true;
                            var filterDelay = (DateTime.Now - _signalStartTime).TotalMilliseconds;

                            long frameId = Interlocked.Increment(ref _frameCounter);
                            log.Info($"[PERF #{frameId}] Задержка фильтра датчика: {filterDelay:F1} мс");
                            log.Info($"[PERF #{frameId}] Старт триггера");

                            var swCamera = Stopwatch.StartNew();
                            var (dm, frame) = camera.TriggerAndRead();
                            swCamera.Stop();

                            log.Info($"[PERF #{frameId}] Камера: {swCamera.ElapsedMilliseconds} мс");

                            if (frame != null)
                            {
                                _lastFrame = frame;
                                FrameReceived?.Invoke(dm?.Raw, frame);
                            }

                            await HandleDataMatrixAsync(dm, frameId);
                        }

                        if (_stableSignal == 0)
                            _triggerInProgress = false;
                    }

                    await Task.Delay(SensorPollIntervalMs, token);
                }
                catch (TaskCanceledException)
                {
                    return;
                }
                catch (Exception ex)
                {
                    log.Error("Ошибка в цикле инспекции", ex);
                    ErrorOccurred?.Invoke(ex.Message);
                }
            }
        }

        /// <summary>
        /// Обрабатывает считанный DataMatrix: вызывает валидатор и генерирует события.
        /// </summary>
        private async Task HandleDataMatrixAsync(DataMatrixResult? dm, long frameId = 0)
        {

            var swTotal = Stopwatch.StartNew();
            if (dm != null)
                DataMatrixRead?.Invoke(dm);

            var swDb = Stopwatch.StartNew();
            var result = await ValidateAsync(dm?.Raw);
            swDb.Stop();

            CodeValidated?.Invoke(result);
            CodeChecked?.Invoke(dm?.Normalized ?? "<NO READ>", result.IsOk);

            swTotal.Stop();
            log.Info($"[PERF #{frameId}] Валидация в БД: {swDb.ElapsedMilliseconds} мс, всего: {swTotal.ElapsedMilliseconds} мс");

            log.Info($"[#{frameId}] Проверка: [{dm?.Normalized ?? "<NO READ>"}] -> {(result.IsOk ? "OK" : "BRK")}");

            if (result.IsOk)
            {
                labelingSession.Verified++;
            }
            else
            {
                if (_lastFrame != null)
                    imageSaver.SaveReject(_lastFrame, dm?.Normalized);

                RejectWithDelay();
                ErrorOccurred?.Invoke(result.ErrorMessage ?? "Ошибка проверки DataMatrix");
                labelingSession.Rejected++;
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

                tmp_main verified1 = new()
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

                db.tmp_mains.Add(verified1);
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
        /// Активирует отбраковщик с задержкой.
        /// </summary>
        public void RejectWithDelay()
        {
            Task.Run(() =>
            {
                Thread.Sleep(RejectDelayMs);
                rejector.Activate();
            });
        }

        /// <summary>
        /// Останавливает инспекцию.
        /// </summary>
        public void Stop()
        {
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
        public async Task TriggerManual()
        {
            if (_triggerInProgress)
            {
                log.Warning("Триггер уже выполняется.");
                return;
            }

            _triggerInProgress = true;

            await Task.Run(async () =>
            {
                try
                {
                    log.Info("Ручной триггер камеры");
                    var (dm, frame) = camera.TriggerAndRead();

                    if (frame != null)
                    {
                        _lastFrame = frame;
                        FrameReceived?.Invoke(dm?.Raw, frame);
                    }
                    else
                        log.Warning("Кадр не получен");

                    await HandleDataMatrixAsync(dm);
                }
                catch (Exception ex)
                {
                    log.Error("Ошибка ручного триггера", ex);
                    ErrorOccurred?.Invoke(ex.Message);
                }
                finally
                {
                    _triggerInProgress = false;
                }
            });
        }
    }
}
