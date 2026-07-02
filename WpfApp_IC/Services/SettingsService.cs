using Microsoft.Extensions.Configuration;
using System.IO;
using System.Text.Json;
using WpfApp_IC.Models;
using WpfApp_IC.Models.Devices;
using WpfApp_IC.Services.Inspectors;

namespace WpfApp_IC.Services
{
    public class SettingsService(IConfiguration config, AppSettings appSettings, WorkSession workSession, ModbusSensor sensor, ModbusRejector rejector, VideojetPrinter printer, MindeoScanner scanner, InspectorController inspector, ImageSaverService imageSaver)
    {
        private readonly string _settingsPath = Path.Combine(AppContext.BaseDirectory, config.GetValue<string>("SettingsPath") ?? "settings.json");

        public void Save()
        {
            try
            {
                File.WriteAllText(_settingsPath, JsonSerializer.Serialize(appSettings));
                ApplySettings();
            }
            catch { }
        }
        public void Load()
        {
            try
            {
                appSettings = JsonSerializer.Deserialize<AppSettings>(File.ReadAllText(_settingsPath)) ?? new();
                ApplySettings();
            }
            catch { }
        }

        private void ApplySettings()
        {
            workSession.MachineName = appSettings.MachineName;

            scanner.ComPort = appSettings.ScannerCOMPort;

            // Принтер
            printer.IP = appSettings.PrinterIP;
            printer.PortTextComms = appSettings.TextCommsPort;
            printer.PortZpl = appSettings.ZPLImagePort;
            printer.MaxQueueSize = appSettings.MaxQueueSize;

            // Modbus + катушки
            inspector.ModbusIp = appSettings.ModbusIP;
            inspector.ModbusPort = appSettings.ModbusPort;
            sensor.Coil = appSettings.MotionSensorCoil;
            rejector.Coil = appSettings.RejectorCoil;

            // Инспекция
            inspector.RejectDelay = appSettings.RejectorDelay;
            inspector.MotionFilterInterval = appSettings.MotionFilterInterval;
            inspector.SensorPollInterval = appSettings.SensorPollInterval;

            // Камера 
            inspector.CameraIp = appSettings.CameraIP;
            imageSaver.SavePath = appSettings.RejectImagesPath;
        }
    }
}
