using System;
using System.IO;
using System.Text.Json;

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
        public bool UsePrinter { get; set; } = true;

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
        public string IdmvsPath { get; set; } = "";
        public string RejectImagesPath { get; set; } = "RejectImages";
        public string CameraIp { get; set; } = "";

        // Инспекция
        public int RejectDelayMs { get; set; } = 500;
        public int SensorFilterCount { get; set; } = 2;
        public int SensorPollIntervalMs { get; set; } = 10;

        /// <summary>
        /// Читает только RejectImagesPath из файла без создания SettingsService.
        /// Используется для разрыва циклической зависимости в DI.
        /// </summary>
        public static string ReadRejectImagesPath()
        {
            try
            {
                string filePath = Path.Combine(
                    AppDomain.CurrentDomain.BaseDirectory, "settings.json");

                if (!File.Exists(filePath))
                    return "RejectImages";

                string json = File.ReadAllText(filePath);
                var s = JsonSerializer.Deserialize<AppSettings>(json);
                return s?.RejectImagesPath ?? "RejectImages";
            }
            catch
            {
                return "RejectImages";
            }
        }

        public static AppSettings LoadFromFile()
        {
            try
            {
                string filePath = Path.Combine(
                    AppDomain.CurrentDomain.BaseDirectory, "settings.json");

                if (!File.Exists(filePath))
                    return new AppSettings();

                string json = File.ReadAllText(filePath);
                return JsonSerializer.Deserialize<AppSettings>(json) ?? new AppSettings();
            }
            catch
            {
                return new AppSettings();
            }
        }

    }
}
