namespace WpfApp_IC.Services.SettingsD
{
    /// <summary>
    /// Настройки приложения.
    /// </summary>
    public class AppSettings
    {
        public bool UseCamera { get; set; } = true;
        public bool UseModbus { get; set; } = true;
        public bool UsePrinter { get; set; } = false;

        public string ModbusIp { get; set; } = "192.168.0.127";
        public int ModbusPort { get; set; } = 502;

        public string PrinterType { get; set; } = "Эмулятор (заглушка)";
        public string PrinterIp { get; set; } = "192.168.0.150";
        public int PrinterPort { get; set; } = 9100;

        public string IdmvsPath { get; set; } = ""; // ← ВОЗВРАЩАЕМ ЭТО

        public int RejectDelayMs { get; set; } = 500;
    }
}
