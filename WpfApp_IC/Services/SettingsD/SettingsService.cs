using System;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using WpfApp_IC.Device;
using WpfApp_IC.Services.Inspectors;

namespace WpfApp_IC.Services.SettingsD
{
    /// <summary>
    /// Загружаем/сохраняет settings.json и принимает настройки к объектам
    /// Регистрируем как Singleton: services.AddSingleton<ISettingsService, SettingsService>()
    /// </summary>   
    public class SettingsService : ISettingsService
    {
        private static readonly string FilePath =
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "settings.json");

        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            WriteIndented = true
        };

        // объекты, которые нужно обновлять при смене настроек
        private readonly VideojetPrinter _printer;
        private readonly IInspectorController _inspector;
        private readonly IoModuleConfig _ioConfig;

        public AppSettings Current { get; private set; }

        public SettingsService(
            VideojetPrinter printer,
            IInspectorController inspector,
            IoModuleConfig ioConfig)
        {
            _printer = printer;
            _inspector = inspector;
            _ioConfig = ioConfig;

            Current = Load();
            Apply(Current); // применяем сразу при старте
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

            // Инспекция
            _inspector.RejectDelayMs = s.RejectDelayMs;
            _inspector.SensorFilterCount = s.SensorFilterCount;
            _inspector.SensorPollIntervalMs = s.SensorPollIntervalMs;
            //_inspector.CameraDeviceIndex = s.CameraDeviceIndex;
            _inspector.CameraIp = s.CameraIp;
            Debug.WriteLine($"Apply: CameraIp = '{s.CameraIp}'");

            // Modbus IoConfig — применится при следующем подключении
            _ioConfig.ModbusIp = s.ModbusIp;
            _ioConfig.ModbusPort = s.ModbusPort;


        }
    }
}
