using Observable;

namespace WpfApp_IC.Models
{
    /// <summary>
    /// Модель настроек приложения
    /// </summary>
    public class AppSettings : ObservableObject
    {
        private string _machineName = "DEV";
        public string MachineName
        {
            get => _machineName;
            set => Set(ref _machineName, value);
        }

        /* --- Modbus --- */

        private string _modbusIP = "";
        public string ModbusIP
        {
            get => _modbusIP;
            set => Set(ref _modbusIP, value);
        }

        private int _modbusPort = 502;
        public int ModbusPort
        {
            get => _modbusPort;
            set => Set(ref _modbusPort, value);
        }

        private int _motionSensorCoil = 0;
        public int MotionSensorCoil
        {
            get => _motionSensorCoil;
            set => Set(ref _motionSensorCoil, value);
        }

        private int _rejectorCoil = 0;
        public int RejectorCoil
        {
            get => _rejectorCoil;
            set => Set(ref _rejectorCoil, value);
        }

        /* --- Printer --- */

        private string _printerIP = "";
        public string PrinterIP
        {
            get => _printerIP;
            set => Set(ref _printerIP, value);
        }

        private int _textCommsPort = 3003;
        public int TextCommsPort
        {
            get => _textCommsPort;
            set => Set(ref _textCommsPort, value);
        }

        private int _zplImagePort = 1000;
        public int ZPLImagePort
        {
            get => _zplImagePort;
            set => Set(ref _zplImagePort, value);
        }

        private byte _maxQueueSize = 19;
        public byte MaxQueueSize
        {
            get => _maxQueueSize;
            set => Set(ref _maxQueueSize, value);
        }

        /* --- Camera --- */

        private string _cameraIP = "";
        public string CameraIP
        {
            get => _cameraIP;
            set => Set(ref _cameraIP, value);
        }

        private uint _frameTimeout = 200;
        public uint FrameTimeout
        {
            get => _frameTimeout;
            set => Set(ref _frameTimeout, value);
        }

        private string _rejectImagesPath = "Rejects";
        public string RejectImagesPath
        {
            get => _rejectImagesPath;
            set => Set(ref _rejectImagesPath, value);
        }

        /* --- Scanner --- */

        private string _scannerComPort = "";
        public string ScannerCOMPort
        {
            get => _scannerComPort;
            set => Set(ref _scannerComPort, value);
        }

        /* --- Inspection --- */

        private int _rejectorDelay = 350;
        public int RejectorDelay
        {
            get => _rejectorDelay;
            set => Set(ref _rejectorDelay, value);
        }

        private int _rejectorActiveTime = 300;
        public int RejectorActiveTime
        {
            get => _rejectorActiveTime;
            set => Set(ref _rejectorActiveTime, value);
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
