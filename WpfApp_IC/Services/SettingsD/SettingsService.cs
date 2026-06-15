using System;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using WpfApp_IC.Devices;
using WpfApp_IC.Services.Inspectors;

namespace WpfApp_IC.Services.SettingsD
{
    public class SettingsService(ModbusSensor sensor, ModbusRejector rejector, VideojetPrinter printer, IInspectorController inspector, ImageSaverService imageSaver, LabelingSession labelingSession)
    {
        private static readonly string _settingsPath = Path.Combine(AppContext.BaseDirectory, "settings.json");

        public AppSettings Settings { get; set; } = new();

        public void Save()
        {
            File.WriteAllText(_settingsPath, JsonSerializer.Serialize(Settings));
            Apply(Settings);
        }
        public void Load()
        {
            try
            {
                if (!File.Exists(_settingsPath))
                    Save();
                else
                {
                    Settings = JsonSerializer.Deserialize<AppSettings>(File.ReadAllText(_settingsPath)) ?? new();
                    Apply(Settings);
                }
            }
            catch { }
        }

        private void Apply(AppSettings s)
        {
            labelingSession.MachineName = s.MachineName;

            // Принтер
            printer.IP = s.PrinterIp;
            printer.PortTextComms = s.PrinterPortTextComms;
            printer.PortZplEmulation = s.PrinterPortZpl;

            // Modbus + катушки
            inspector.ModbusIp = s.ModbusIp;
            inspector.ModbusPort = s.ModbusPort;
            inspector.SignalCoil = s.SignalCoil;
            inspector.RejectCoil = s.RejectCoil;

            // Инспекция
            inspector.RejectDelayMs = s.RejectDelayMs;
            inspector.SensorFilterCount = s.SensorFilterCount;
            inspector.SensorPollIntervalMs = s.SensorPollIntervalMs;

            sensor.Coil = s.SignalCoil;
            rejector.Coil = s.RejectCoil;

            // Камера 
            inspector.CameraIp = s.CameraIp;
            imageSaver.SavePath = s.RejectImagesPath;
        }
    }
}
