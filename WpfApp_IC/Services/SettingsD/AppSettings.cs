using Observable;

namespace WpfApp_IC.Services.SettingsD
{
    /// <summary>
    /// Настройки приложения
    /// </summary>
    public class AppSettings : ObservableObject
    {
        private string _machineName = "", _modbusIp = "", _printerIp = "", _cameraIp = "", _rejectPath = "";
        private int _modbusPort, _signalCoil, _rejectCoil, _printerPortTextComms, _printerPortZpl, _rejectDelay, _sensorFilterCount, _sensorPollInterval;

        public string MachineName
        { 
            get => _machineName; 
            set => Set(ref _machineName, value);
        }

        // Modbus 
        public string ModbusIp { get; set; } = "172.19.43.21";
        public int ModbusPort { get; set; } = 502;
        public int SignalCoil { get; set; } = 0;
        public int RejectCoil { get; set; } = 0;

        // Принтер 
        public string PrinterIp { get; set; } = "172.19.43.5";
        public int PrinterPortTextComms { get; set; } = 3003;
        public int PrinterPortZpl { get; set; } = 1000;

        // Камера 
        public string CameraIp { get; set; } = "";
        public string RejectImagesPath { get; set; } = "RejectImages";

        // Инспекция
        public int RejectDelayMs { get; set; } = 300;
        public int SensorFilterCount { get; set; } = 2;
        public int SensorPollIntervalMs { get; set; } = 10;
    }
}
