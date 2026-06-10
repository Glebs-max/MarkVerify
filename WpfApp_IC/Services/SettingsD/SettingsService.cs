using System;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using WpfApp_IC.Devices;
using WpfApp_IC.Services.Inspectors;

namespace WpfApp_IC.Services.SettingsD
{
    /// <summary>
    /// Загружаем/сохраняет settings.json и принимает настройки к объектам
    /// Регистрируем как Singleton: services.AddSingleton<ISettingsService, SettingsService>()
    /// </summary>   
    public class SettingsService : ISettingsService
    {
        private static readonly string FilePath = Path.Combine(AppContext.BaseDirectory, "settings.json");
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            WriteIndented = true
        };

        private readonly VideojetPrinter _printer;
        private readonly ModbusSensor _sensor;
        private readonly ModbusRejector _rejector;
        private readonly IInspectorController _inspector;
        private readonly IImageSaverService _imageSaver;

        public AppSettings Current { get; private set; }

        public SettingsService(ModbusSensor sensor, ModbusRejector rejector, VideojetPrinter printer, IInspectorController inspector, IImageSaverService imageSaver)
        {
            _printer = printer;
            _inspector = inspector;
            _imageSaver = imageSaver;
            _sensor = sensor;
            _rejector = rejector;

            Current = Load();
            Apply(Current);
        }

        // Публичные методы

        public void Save(AppSettings settings)
        {
            Current = settings;
            WriteFile(settings);
            Apply(settings);
        }
        public void ApplyCurrent() => Apply(Current);

        // Приватные методы

        private static AppSettings Load()
        {
            if (!File.Exists(FilePath))
            {
                var defaults = new AppSettings();
                WriteFile(defaults); // создать файл с дефолтами
                return defaults;
            }
            try
            {
                string json = File.ReadAllText(FilePath);
                return JsonSerializer.Deserialize<AppSettings>(json) ?? new AppSettings();
            }
            catch
            {
                return new AppSettings();
            }
        }

        private static void WriteFile(AppSettings settings)
        {
            try
            {
                File.WriteAllText(FilePath, JsonSerializer.Serialize(settings, JsonOptions));
            }
            catch { /* при необходимости добавить логирование */ }
        }

        /// <summary>
        /// Применяет настройки к живым объектам без перезапуска.
        /// VideojetPrinter.IP/Port имеют OnConfigChanged — переподключится сам.
        /// </summary>
        private void Apply(AppSettings s)
        {
            // Принтер
            _printer.IP = s.PrinterIp;
            _printer.PortTextComms = s.PrinterPortTextComms;
            _printer.PortZplEmulation = s.PrinterPortZpl;

            // Modbus + катушки
            _inspector.ModbusIp = s.ModbusIp;
            _inspector.ModbusPort = s.ModbusPort;
            _inspector.SignalCoil = s.SignalCoil;
            _inspector.RejectCoil = s.RejectCoil;

            // Инспекция
            _inspector.RejectDelayMs = s.RejectDelayMs;
            _inspector.SensorFilterCount = s.SensorFilterCount;
            _inspector.SensorPollIntervalMs = s.SensorPollIntervalMs;

            _sensor.Coil = s.SignalCoil;
            _rejector.Coil = s.RejectCoil;

            // Камера 
            _inspector.CameraIp = s.CameraIp;
            Debug.WriteLine($"Apply: CameraIp = '{s.CameraIp}'");

            _imageSaver.UpdatePath(s.RejectImagesPath);


            Debug.WriteLine("=== SettingsService.Apply ===");
            Debug.WriteLine($"  ModbusIp         = {s.ModbusIp}");
            Debug.WriteLine($"  ModbusPort       = {s.ModbusPort}");
            Debug.WriteLine($"  SignalCoil       = {s.SignalCoil}");
            Debug.WriteLine($"  RejectCoil       = {s.RejectCoil}");
            Debug.WriteLine($"  CameraIp         = {s.CameraIp}");
            Debug.WriteLine($"  RejectDelayMs    = {s.RejectDelayMs}");
            Debug.WriteLine($"  SensorFilter     = {s.SensorFilterCount}");
            Debug.WriteLine($"  SensorPollMs     = {s.SensorPollIntervalMs}");
            Debug.WriteLine($"  RejectImagesPath = {s.RejectImagesPath}");
            Debug.WriteLine("--- inspector после Apply ---");
            Debug.WriteLine($"  inspector.ModbusIp   = {_inspector.ModbusIp}");
            Debug.WriteLine($"  inspector.CameraIp   = {_inspector.CameraIp}");
            Debug.WriteLine($"  inspector.SignalCoil = {_inspector.SignalCoil}");
            Debug.WriteLine($"  inspector.RejectCoil = {_inspector.RejectCoil}");


        }
    }
}
