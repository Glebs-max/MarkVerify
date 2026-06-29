using Microsoft.Win32;
using Observable;
using System.Collections.ObjectModel;
using System.Windows.Media.Media3D;
using WpfApp_IC.Services.Camera;
using WpfApp_IC.Services.SettingsD;
using System.IO;

namespace WpfApp_IC.ViewModels
{
    /// <summary>
    /// ViewModel страницы настроек.
    /// Только Observable-свойства и две команды: Save / Cancel.
    /// Никакой работы с файлами — это делает ISettingsService.
    /// MessageBox убран — ошибки отображаются через ValidationMessage.
    /// </summary>
    public class SettingsViewModel : ObservableObject
    {
        private readonly MainViewModel _main;
        private readonly SettingsService _settings;
        private readonly ICameraService _camera;
        public bool HasValidationError => !string.IsNullOrEmpty(ValidationMessage);

        private int _signalCoil;
        private int _rejectCoil;

        private string _rejectImagesPath = "";
        public string RejectImagesPath
        {
            get => _rejectImagesPath;
            set => Set(ref _rejectImagesPath, value);
        }   

        public int SignalCoil
        {
            get => _signalCoil;
            set => Set(ref _signalCoil, value);
        }

        public int RejectCoil
        {
            get => _rejectCoil;
            set => Set(ref _rejectCoil, value);
        }

        // Принтер
        private string _printerIp = "";
        private int _printerPortTextComms;
        private int _printerPortZpl;

        public string PrinterIp
        {
            get => _printerIp;
            set => Set(ref _printerIp, value);
        }
        public int PrinterPortTextComms
        {
            get => _printerPortTextComms;
            set => Set(ref _printerPortTextComms, value);
        }
        public int PrinterPortZpl
        {
            get => _printerPortZpl;
            set => Set(ref _printerPortZpl, value);
        }

        // Modbus
        private string _modbusIp = "";
        private int _modbusPort;

        public string ModbusIp
        {
            get => _modbusIp;
            set => Set(ref _modbusIp, value);
        }
        public int ModbusPort
        {
            get => _modbusPort;
            set => Set(ref _modbusPort, value);
        }

        // Камера
        private string _cameraIp = "";
        private string _idmvsPath = "";

        public string CameraIp
        {
            get => _cameraIp;
            set => Set(ref _cameraIp, value);
        }
        public string IdmvsPath
        {
            get => _idmvsPath;
            set => Set(ref _idmvsPath, value);
        }

        // Инспекция
        private int _rejectDelayMs;
        private int _motionFilterInterval;
        private int _sensorPollIntervalMs;

        public int RejectDelayMs
        {
            get => _rejectDelayMs;
            set => Set(ref _rejectDelayMs, value);
        }
        public int MotionFilterInterval
        {
            get => _motionFilterInterval;
            set => Set(ref _motionFilterInterval, value);
        }
        public int SensorPollIntervalMs
        {
            get => _sensorPollIntervalMs;
            set => Set(ref _sensorPollIntervalMs, value);
        }

        public string _machineName = "";
        public string MachineName
        {
            get => _machineName;
            set => Set(ref _machineName, value);
        }

        // Валидация
        private string _validationMessage = "";

        /// <summary>Сообщение об ошибке валидации — привязывается в XAML вместо MessageBox</summary>
        public string ValidationMessage
        {
            get => _validationMessage;
            set
            {
                Set(ref _validationMessage, value);
                OnPropertyChanged(nameof(HasValidationError)); // уведомить View
            }
        }

        // Конструктор
        public SettingsViewModel(MainViewModel main, SettingsService settings, ICameraService camera)
        {
            _main = main;
            _settings = settings;
            _camera = camera;
            LoadFromCurrent();
        }

        private ObservableCollection<CameraDeviceInfo> _availableCameras = [];
        public ObservableCollection<CameraDeviceInfo> AvailableCameras
        {
            get => _availableCameras;
            set => Set(ref _availableCameras, value);
        }

        private CameraDeviceInfo? _selectedCamera;
        public CameraDeviceInfo? SelectedCamera
        {
            get => _selectedCamera;
            set
            {
                Set(ref _selectedCamera, value);
                if (value != null)
                    CameraIp = value.IP;
            }
        }

        // Добавить команду:
        public void RefreshCameras()
        {
            try
            {
                var devices = _camera.GetAvailableDevices();
                AvailableCameras = new ObservableCollection<CameraDeviceInfo>(devices);

                // Выделить текущую камеру в списке
                SelectedCamera = AvailableCameras
                    .FirstOrDefault(c => c.IP == CameraIp);

                if (AvailableCameras.Count == 0)
                    ValidationMessage = "Камеры не найдены. Проверьте подключение.";
            }
            catch (Exception ex)
            {
                ValidationMessage = $"Ошибка при поиске камер: {ex.Message}";
            }
        }

        // Команды
        public void Save()
        {
            if (!Validate()) return;

            _settings.Settings.RejectDelay = RejectDelayMs;
            _settings.Settings.SignalCoil = SignalCoil;
            _settings.Settings.SensorPollInterval = SensorPollIntervalMs;
            _settings.Settings.MotionFilterInterval = MotionFilterInterval;
            _settings.Settings.ModbusIp = ModbusIp;
            _settings.Settings.ModbusPort = ModbusPort;
            _settings.Settings.CameraIp = CameraIp;
            _settings.Settings.RejectImagesPath = RejectImagesPath;
            _settings.Settings.PrinterIp = PrinterIp;
            _settings.Settings.PrinterPortTextComms = PrinterPortTextComms;
            _settings.Settings.PrinterPortZpl = PrinterPortZpl;
            _settings.Settings.MachineName = MachineName;

            _settings.Save();

            NavigateBack();
        }

        public void Cancel()
        {
            LoadFromCurrent(); // откатываем несохранённые изменения
            NavigateBack();
        }

        /// <summary>Открывает диалог выбора IDMVS.exe</summary>
        public void BrowseIdmvs()
        {
            var dialog = new OpenFileDialog
            {
                Title = "Выберите IDMVS.exe",
                Filter = "Исполняемые файлы (*.exe)|*.exe|Все файлы (*.*)|*.*",
                CheckFileExists = true
            };

            if (dialog.ShowDialog() == true)
                IdmvsPath = dialog.FileName;
        }

        // Приватные методы
        private void LoadFromCurrent()
        {
            var s = _settings.Settings;

            PrinterIp = s.PrinterIp;
            PrinterPortTextComms = s.PrinterPortTextComms;
            PrinterPortZpl = s.PrinterPortZpl;

            ModbusIp = s.ModbusIp;
            ModbusPort = s.ModbusPort;

            SignalCoil = s.SignalCoil;
            RejectCoil = s.RejectCoil;

            CameraIp = s.CameraIp;

            RejectDelayMs = s.RejectDelay;
            MotionFilterInterval = s.MotionFilterInterval;
            SensorPollIntervalMs = s.SensorPollInterval;

            ValidationMessage = "";

            RejectImagesPath = s.RejectImagesPath;

            MachineName = s.MachineName;
        }

        private bool Validate()
        {
            var errors = new List<string>();

            if (RejectDelayMs < 0)
                errors.Add("Задержка отбраковки не может быть отрицательной.");

            if (SensorPollIntervalMs < 1)
                errors.Add("Интервал опроса датчика должен быть не менее 1 мс.");

            if (MotionFilterInterval < 1)
                errors.Add("Фильтр датчика должен быть не менее 1.");

            // Можно добавить проверки IP-адресов
            if (string.IsNullOrWhiteSpace(PrinterIp))
                errors.Add("IP принтера не может быть пустым.");

            if (string.IsNullOrWhiteSpace(ModbusIp))
                errors.Add("IP Modbus не может быть пустым.");

            ValidationMessage = string.Join("\n", errors);
            return errors.Count == 0;
        }

        // Команда выбора папки
        public void BrowseRejectImagesPath()
        {
            // OpenFileDialog с выбором папки через hack
            var dialog = new Microsoft.Win32.SaveFileDialog
            {
                Title = "Выберите папку для сохранения изображений",
                FileName = "Выберите папку",
                Filter = "Папка|*.nope",
                CheckFileExists = false,
                CheckPathExists = true,
                ValidateNames = false
            };

            if (dialog.ShowDialog() == true)
            {
                // Берём только путь к папке, без имени файла
                RejectImagesPath = Path.GetDirectoryName(dialog.FileName) ?? RejectImagesPath;
            }
        }

        private void NavigateBack() =>
            _main.CurrentViewModel = _main.GetViewModel<HomeViewModel>();
    }
}