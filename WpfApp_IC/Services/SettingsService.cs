using Microsoft.Extensions.Configuration;
using System.IO;
using System.Text.Json;
using WpfApp_IC.Models;
using WpfApp_IC.Models.Devices;
using WpfApp_IC.Services.Inspectors;

namespace WpfApp_IC.Services
{
    public class SettingsService(IConfiguration config, WorkSession workSession, ModbusSensor sensor, ModbusRejector rejector, VideojetPrinter printer, MindeoScanner scanner, InspectorController inspector, ImageSaverService imageSaver)
    {
        private readonly string _settingsPath = Path.Combine(AppContext.BaseDirectory, config.GetValue<string>("SettingsPath") ?? "settings.json");

        public AppSettings AppSettings { get; private set; } = new();

        public void Save()
        {
            try
            {
                File.WriteAllText(_settingsPath, JsonSerializer.Serialize(AppSettings));
                ApplySettings();
            }
            catch { }
        }
        public void Load()
        {
            try
            {
                AppSettings = JsonSerializer.Deserialize<AppSettings>(File.ReadAllText(_settingsPath)) ?? new();
                ApplySettings();
            }
            catch { }
        }

        private void ApplySettings()
        {
            workSession.MachineName = AppSettings.MachineName;

            scanner.ComPort = AppSettings.ScannerCOMPort;

            // Принтер
            printer.IP = AppSettings.PrinterIP;
            printer.PortTextComms = AppSettings.TextCommsPort;
            printer.PortZpl = AppSettings.ZPLImagePort;
            printer.MaxQueueSize = AppSettings.MaxQueueSize;

            // Modbus + катушки
            inspector.ModbusIp = AppSettings.ModbusIP;
            inspector.ModbusPort = AppSettings.ModbusPort;
            sensor.Coil = AppSettings.MotionSensorCoil;
            rejector.Coil = AppSettings.RejectorCoil;
            rejector.ActiveTime = AppSettings.RejectorActiveTime;

            // Инспекция
            inspector.RejectDelay = AppSettings.RejectorDelay;
            inspector.MotionFilterInterval = AppSettings.MotionFilterInterval;
            inspector.SensorPollInterval = AppSettings.SensorPollInterval;

            // Камера 
            inspector.CameraIp = AppSettings.CameraIP;
            imageSaver.SavePath = AppSettings.RejectImagesPath;
        }
    }
}
