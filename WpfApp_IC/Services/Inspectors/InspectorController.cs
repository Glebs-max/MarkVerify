using Observable;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using WpfApp_IC.Device;
using WpfApp_IC.Device.Actuators;
using WpfApp_IC.Device.Sensors;
using WpfApp_IC.Services.Camera;
using WpfApp_IC.Services.ModbusT;

namespace WpfApp_IC.Services.Inspectors
{
    /// <summary>
    /// Главный контроллер инспекции.
    /// Управляет камерой, датчиком, отбраковщиком и выполняет проверку DataMatrix.
    /// Не зависит от UI и не содержит логики отображения.
    /// </summary>
    public class InspectorController : ObservableObject, IInspectorController, IDisposable
    {
        private readonly ICameraService _camera;
        private readonly ISensor _sensor;
        private readonly IModbusService _modbus;
        private readonly IoModuleConfig _config;
        private readonly IRejector _rejector;
        private readonly IDataMatrixValidator _validator;

        private CancellationTokenSource? _cts;

        /// <summary>
        /// Текущий GTIN, по которому выполняется проверка.
        /// Устанавливается ViewModel.
        /// </summary>
        public ulong CurrentGtinId { get; set; }

        /// <summary>
        /// Ожидаемый код (если используется прямое сравнение).
        /// </summary>
        public string? ExpectedCode { get; set; }

        /// <summary>
        /// Задержка перед активацией отбраковщика.
        /// </summary>
        public int RejectDelayMs { get; set; } = 200;

        // Фильтрация дребезга
        private int _stableSignal = -1;
        private int _previousSignal = -1;
        private int _sameCount = 0;
        private const int FILTER_COUNT = 2;

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

        public InspectorController(
            ICameraService camera,
            ISensor sensor,
            IRejector rejector,
            IModbusService modbus,
            IoModuleConfig config,
            IDataMatrixValidator validator)
        {
            _camera = camera;
            _sensor = sensor;
            _rejector = rejector;
            _modbus = modbus;
            _config = config;
            _validator = validator;
        }

        /// <summary>
        /// Запускает инспекцию: подключает Modbus, открывает камеру и запускает цикл чтения датчика.
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
            Task.Run(() => Loop(_cts.Token));
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
                    int rawSignal = _sensor.Read();

                    // Фильтрация дребезга
                    if (rawSignal == _stableSignal)
                        _sameCount++;
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

                            if (_stableSignal == 1 && !_triggerInProgress)
                            {
                                _triggerInProgress = true;

                                var (dm, frame) = _camera.TriggerAndRead();

                                if (frame != null)
                                    FrameReceived?.Invoke(dm?.Raw, frame);

                                _ = HandleDataMatrixAsync(dm);
                            }

                            if (_stableSignal == 0)
                                _triggerInProgress = false;
                        }
                    }

                    await Task.Delay(10, token);
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

        /// <summary>
        /// Обрабатывает считанный DataMatrix: вызывает валидатор и генерирует события.
        /// </summary>
        private async Task HandleDataMatrixAsync(DataMatrixResult? dm)
        {
            if (dm != null)
                DataMatrixRead?.Invoke(dm);

            var result = await _validator.ValidateAsync(dm?.Raw, CurrentGtinId);

            CodeValidated?.Invoke(result);

            bool ok = result.IsOk &&
                      (ExpectedCode == null || dm?.Normalized == ExpectedCode);

            CodeChecked?.Invoke(dm?.Normalized ?? "<NO READ>", ExpectedCode, ok);

            if (!result.IsOk)
            {
                RejectWithDelay();
                ErrorOccurred?.Invoke(result.ErrorMessage ?? "Ошибка проверки DataMatrix");
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
                _rejector.Activate();
            });
        }

        /// <summary>
        /// Останавливает инспекцию.
        /// </summary>
        public void Stop()
        {
            _cts?.Cancel();
            Thread.Sleep(100);
            _camera.Close();
        }

        public void Dispose()
        {
            Stop();
            _modbus?.Disconnect();
            _camera.Dispose();
        }
    }
}
