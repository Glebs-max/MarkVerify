using Observable;

namespace WpfApp_IC.Services.SettingsD
{
    /// <summary>
    /// Настройки приложения
    /// </summary>
    public class AppSettings : ObservableObject
    {
        private string _machineName = "";
        public string MachineName
        {
            get => _machineName;
            set => Set(ref _machineName, value);
        }

        /* --- Modbus --- */

        private string _modbusIp = "172.19.43.21";
        public string ModbusIp
        {
            get => _modbusIp;
            set => Set(ref _modbusIp, value);
        }

        private int _modbusPort = 502;
        public int ModbusPort
        {
            get => _modbusPort;
            set => Set(ref _modbusPort, value);
        }

        private int _signalCoil = 0;
        public int SignalCoil
        {
            get => _signalCoil;
            set => Set(ref _signalCoil, value);
        }

        private int _rejectCoil = 0;
        public int RejectCoil
        {
            get => _rejectCoil;
            set => Set(ref _rejectCoil, value);
        }

        /* --- Printer --- */

        private string _printerIp = "172.19.43.5";
        public string PrinterIp
        {
            get => _printerIp;
            set => Set(ref _printerIp, value);
        }

        private int _printerPortTextComms = 3003;
        public int PrinterPortTextComms
        {
            get => _printerPortTextComms;
            set => Set(ref _printerPortTextComms, value);
        }

        private int _printerPortZpl = 1000;
        public int PrinterPortZpl
        {
            get => _printerPortZpl;
            set => Set(ref _printerPortZpl, value);
        }

        /* --- Camera --- */

        private string _cameraIp = "";
        public string CameraIp
        {
            get => _cameraIp;
            set => Set(ref _cameraIp, value);
        }

        private string _rejectImagesPath = "RejectImages";
        public string RejectImagesPath
        {
            get => _rejectImagesPath;
            set => Set(ref _rejectImagesPath, value);
        }

        /* --- Inspection --- */

        private int _rejectDelay = 300;
        public int RejectDelay
        {
            get => _rejectDelay;
            set => Set(ref _rejectDelay, value);
        }

        private int _motionFilterInterval = 200;
        public int MotionFilterInterval
        {
            get => _motionFilterInterval;
            set => Set(ref _motionFilterInterval, value);
        }

        private int _sensorPollInterval = 10;
        public int SensorPollInterval
        {
            get => _sensorPollInterval;
            set => Set(ref _sensorPollInterval, value);
        }
    }
}
