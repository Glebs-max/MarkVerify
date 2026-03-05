namespace WpfApp_IC.Services.SettingsD
{
    /// <summary>
    /// Настройки приложения.
    /// </summary>
    public class AppSettings
    {
        // Флаги включения устройств
        public bool UseCamera { get; set; } = true;
        public bool UseModbus { get; set; } = true;
        public bool UsePrinter { get; set; } = false;

        // Modbus 
        public string ModbusIp { get; set; } = "192.168.0.127";
        public int ModbusPort { get; set; } = 502;

        // Принтер 
        public string PrinterIp { get; set; } = "192.168.0.150";
        public int PrinterPortTextComms { get; set; } = 3003;
        public int PrinterPortZpl { get; set; } = 1000;
        
        // Камера 
        public string IdmvsPath { get; set; } = "";
        //public int CameraDeviceIndex { get; set; } = 0;
        public string CameraIp { get; set; } = "";

        // Инспекция
        public int RejectDelayMs { get; set; } = 500;
        public int SensorFilterCount { get; set; } = 2;
        public int SensorPollIntervalMs { get; set; } = 10;
        
    }
}
