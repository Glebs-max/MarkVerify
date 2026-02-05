using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using WpfApp_IC.Device;
using WpfApp_IC.Device.Actuators;
using WpfApp_IC.Device.Sensors;
using WpfApp_IC.Services;
using WpfApp_IC.Services.Camera;
using WpfApp_IC.Services.Inspectors;
using WpfApp_IC.Services.ModbusT;

namespace WpfApp_IC.Services.Inspectors
{
    /// <summary>
    /// Главный контроллер инспекции.
    /// Работает через абстракции датчика и отбраковщика.
    /// Не зависит от Modbus, OPC UA или других протоколов.
    /// </summary>
    public class InspectorController : IInspectorController
    {
        private readonly ICameraService _camera;
        private readonly ISensor _sensor;
        private readonly IModbusService _modbus; 
        private readonly IoModuleConfig _config;
        private readonly IRejector _rejector;

        private CancellationTokenSource? _cts;

        /// <summary>
        /// Ожидаемый DataMatrix-код, который должен быть считан.
        /// Устанавливается UI перед запуском инспекции.
        /// </summary>
        public string? ExpectedCode { get; set; }

        /// <summary>
        /// Задержка перед активацией отбраковщика.
        /// </summary>
        public int RejectDelayMs { get; set; } = 500;

        // Фильтрация дребезга сигнала
        private int _stableSignal = -1;
        private int _previousSignal = -1;
        private int _sameCount = 0;
        private const int FILTER_COUNT = 3;

        private bool _triggerInProgress = false;

        // События для UI
        public event Action<int>? SignalChanged;
        public event Action<DataMatrixResult>? DataMatrixRead;
        public event Action<string>? ErrorOccurred;
        public event Action<BitmapSource>? FrameReceived;
        public event Action<string, string?, bool>? CodeChecked;

        public InspectorController(
            ICameraService camera,
            ISensor sensor,
            IRejector rejector,
            IModbusService modbus,
            IoModuleConfig config)
        {
            _camera = camera;
            _sensor = sensor;
            _rejector = rejector;
            _modbus = modbus;
            _config = config;
        }


        /// <summary>
        /// Запуск инспекции: открытие камеры, запуск цикла чтения сигнала.
        /// </summary>
        public void Start()
        {
            _modbus.Connect(_config.ModbusIp, _config.ModbusPort);
            _camera.Open();

            _stableSignal = -1;
            _previousSignal = -1;
            _sameCount = 0;
            _triggerInProgress = false;

            _cts = new CancellationTokenSource();
            var token = _cts.Token;

            Task.Run(() => Loop(token), token);
        }

        /// <summary>
        /// Основной цикл инспекции.
        /// Читает сигнал датчика, фильтрует дребезг, вызывает триггер камеры.
        /// </summary>
        private async Task Loop(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                try
                {
                    int rawSignal = _sensor.Read();

                    // Фильтрация дребезга
                    if (rawSignal == _stableSignal)
                    {
                        _sameCount++;
                    }
                    else
                    {
                        _sameCount = 0;
                        _stableSignal = rawSignal;
                    }

                    if (_sameCount >= FILTER_COUNT)
                    {
                        if (_stableSignal != _previousSignal)
                        {
                            _previousSignal = _stableSignal;
                            SignalChanged?.Invoke(_stableSignal);

                            // Сигнал = 1 → делаем триггер
                            if (_stableSignal == 1 && !_triggerInProgress)
                            {
                                _triggerInProgress = true;

                                var (dm, frame) = _camera.TriggerAndRead();

                                if (frame != null)
                                    FrameReceived?.Invoke(frame);

                                HandleDataMatrix(dm);
                            }

                            // Сигнал = 0 → сбрасываем флаг
                            if (_stableSignal == 0)
                            {
                                _triggerInProgress = false;
                            }
                        }
                    }

                    await Task.Delay(50, token);
                }
                catch (TaskCanceledException)
                {
                    return; // завершение
                }
                catch (Exception ex)
                {
                    ErrorOccurred?.Invoke(ex.Message);
                }

            }
        }

        /// <summary>
        /// Обработка считанного DataMatrix-кода.
        /// Сравнение с ожидаемым кодом и активация отбраковщика.
        /// </summary>
        private void HandleDataMatrix(DataMatrixResult? dm)
        {
            if (dm == null)
            {
                CodeChecked?.Invoke("<NO READ>", ExpectedCode, false);
                ErrorOccurred?.Invoke("DataMatrix not read, activating rejector");
                RejectWithDelay();
                return;
            }

            bool ok = ExpectedCode != null && dm.Normalized == ExpectedCode;

            CodeChecked?.Invoke(dm.Normalized, ExpectedCode, ok);

            if (!ok)
            {
                ErrorOccurred?.Invoke("DataMatrix mismatch, activating rejector");
                RejectWithDelay();
            }

            DataMatrixRead?.Invoke(dm);
        }

        /// <summary>
        /// Активация отбраковщика с задержкой.
        /// </summary>
        private void RejectWithDelay()
        {
            _ = Task.Run(async () =>
            {
                await Task.Delay(RejectDelayMs);
                _rejector.Activate();
            });
        }

        /// <summary>
        /// Остановка инспекции.
        /// </summary>
        public void Stop()
        {
            _cts?.Cancel();
            Thread.Sleep(100);

            _camera.Close();
            _modbus.Disconnect();
        }



        public void Dispose()
        {
            Stop();
            _camera.Dispose();
        }
    }
}
