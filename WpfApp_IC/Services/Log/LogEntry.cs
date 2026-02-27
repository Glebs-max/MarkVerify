using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfApp_IC.Services.Log
{
    /// <summary>
    /// Модель одной записи в логе
    /// </summary>
    public class LogEntry
    {
        public DateTime Time { get; init; } = DateTime.Now;
        public LogLevel Level { get; init; }
        public string Message { get; init; } = "";

        /// <summary>Готовая строка для отображения в UI</summary>
        public string Display =>
            $"[{Time:HH:mm:ss}] [{Level}] {Message}";
    }

    public enum LogLevel
    {
        Info,
        Warning,
        Error
    }
}
