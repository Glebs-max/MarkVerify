using System.IO;
using System.Text.Json;

namespace WpfApp_IC.Device
{
    /// <summary>
    /// Конфигурация модуля ввода-вывода.
    /// Загружается из IoModuleConfig.json.
    /// </summary>
    public class IoModuleConfig
    {
        public string ModbusIp { get; set; } = "192.168.0.127";
        public int ModbusPort { get; set; } = 502;

        public int SignalCoil { get; set; } = 0;
        public int RejectCoil { get; set; } = 1;

        public static IoModuleConfig Load(string path = null)
        {
            // Если путь не передан — строим абсолютный путь к Device/IoModuleConfig.json
            if (path == null)
                path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Device", "IoModuleConfig.json");

            if (!File.Exists(path))
                return new IoModuleConfig();

            string json = File.ReadAllText(path);
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                ReadCommentHandling = JsonCommentHandling.Skip,
                AllowTrailingCommas = true
            };

            return JsonSerializer.Deserialize<IoModuleConfig>(json, options) ?? new IoModuleConfig();
        }

    }
}
