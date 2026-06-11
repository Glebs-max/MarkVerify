using Observable;

namespace WpfApp_IC.Services.SettingsD
{
    /// <summary>
    /// Настройки приложения.
    /// </summary>
    public class AppSettings : ObservableObject
    {
        public string MachineName { get; set; } = "";

        // Modbus 
        public string ModbusIp { get; set; } = "192.168.0.127";
        public int ModbusPort { get; set; } = 502;
        public int SignalCoil { get; set; } = 0;
        public int RejectCoil { get; set; } = 0;

        // Принтер 
        public string PrinterIp { get; set; } = "192.168.0.150";
        public int PrinterPortTextComms { get; set; } = 3003;
        public int PrinterPortZpl { get; set; } = 1000;

        // Камера 
        public string CameraIp { get; set; } = "";
        public string RejectImagesPath { get; set; } = "RejectImages";

        // Инспекция
        public int RejectDelayMs { get; set; } = 500;
        public int SensorFilterCount { get; set; } = 2;
        public int SensorPollIntervalMs { get; set; } = 10;
    }
}
