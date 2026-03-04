using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using Observable;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using WpfApp_IC.Data;
using WpfApp_IC.Device;
using WpfApp_IC.Device.Actuators;
using WpfApp_IC.Device.Sensors;
using WpfApp_IC.Models;
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
        IoModuleConfig config,
        ICameraService camera,
        ISensor sensor,
        IRejector rejector,
        IModbusService modbus,
        ILogService log,
        IDbContextFactory<AppDbContext> dbContextFactory) : ObservableObject, IInspectorController, IDisposable
    {
        private CancellationTokenSource? _cts;

        /// <summary>
        /// Ожидаемый код (если используется прямое сравнение).
        /// </summary>
        public string? ExpectedCode { get; set; }

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
        //private const int FILTER_COUNT = 2;

        private bool _triggerInProgress = false;

        // События для ViewModel
        public event Action<int>? SignalChanged;
        public event Action<DataMatrixResult>? DataMatrixRead;
        public event Action<string>? ErrorOccurred;
        public event Action<string?, BitmapSource>? FrameReceived;
        public event Action<string, string?, bool>? CodeChecked;

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
                modbus.Connect(config.ModbusIp, config.ModbusPort);
                log.Info($"Modbus подключён: {config.ModbusIp}:{config.ModbusPort}");
            }
            catch (Exception ex)
            {
                log.Error("Ошибка подключения Modbus", ex); // тут ловим SocketException
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
        }

        /// <summary>
        /// Основной цикл: читает датчик, фильтрует дребезг, вызывает триггер камеры.
        /// </summary>
        private async Task Loop(CancellationToken token)
        {
            try
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
                        }

                        if (_sameCount >= SensorFilterCount)
                        {
                            if (_stableSignal != _previousSignal)
                            {
                                _previousSignal = _stableSignal;
                                SignalChanged?.Invoke(_stableSignal);

                                if (_stableSignal == 1 && !_triggerInProgress)
                                {
                                    _triggerInProgress = true;

                                    var (dm, frame) = camera.TriggerAndRead();

                                    if (frame != null)
                                        FrameReceived?.Invoke(dm?.Raw, frame);

                                    _ = HandleDataMatrixAsync(dm);
                                }

                                if (_stableSignal == 0)
                                    _triggerInProgress = false;
                            }
                        }

                        await Task.Delay(SensorPollIntervalMs, token);
                    }
                    catch (TaskCanceledException)
                    {
                        return;
                    }
                    catch (Exception ex)
                    {
                        ErrorOccurred?.Invoke(ex.Message);
                    }
                }
            }
            catch (TaskCanceledException)
            {
                return;
            }
            catch (Exception ex)
            {
                log.Error("Ошибка в цикле инспекции", ex); // ← вместо ErrorOccurred
                ErrorOccurred?.Invoke(ex.Message);
            }
        }

        /// <summary>
        /// Обрабатывает считанный DataMatrix: вызывает валидатор и генерирует события.
        /// </summary>
        private async Task HandleDataMatrixAsync(DataMatrixResult? dm)
        {
            if (dm != null)
                DataMatrixRead?.Invoke(dm);

            var result = await ValidateAsync(dm?.Raw);

            CodeValidated?.Invoke(result);

            bool ok = result.IsOk && (ExpectedCode == null || dm?.Normalized == ExpectedCode);

            CodeChecked?.Invoke(dm?.Normalized ?? "<NO READ>", ExpectedCode, ok);

            if (result.IsOk)
            {
                labelingSession.Verified++;
            }
            else
            {
                RejectWithDelay();
                ErrorOccurred?.Invoke(result.ErrorMessage ?? "Ошибка проверки DataMatrix");
                labelingSession.Rejected++;
            }
        }
        private async Task<ValidationResult> ValidateAsync(string? dm)
        {
            if (string.IsNullOrWhiteSpace(dm))
                return ValidationResult.NoRead();

            await using var db = await dbContextFactory.CreateDbContextAsync();
            printer_base? code = await db.printer_bases.FirstOrDefaultAsync(c => c.Code == dm && c.GtinId == labelingSession.GTIN.GtinId && c.StatusId == 1);

            if (code == null)
                return ValidationResult.NotFound();

            code.StatusId = 2;
            await db.SaveChangesAsync();

            return ValidationResult.Ok();
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
        public void TriggerManual()
        {
            if (_triggerInProgress)
            {
                log.Warning("Триггер уже выполняется.");
                return;
            }

            _triggerInProgress = true;

            Task.Run(async () =>
            {
                try
                {
                    log.Info("Ручной триггер камеры");
                    var (dm, frame) = camera.TriggerAndRead();

                    if (frame != null)
                        FrameReceived?.Invoke(dm?.Raw, frame);
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
