namespace WpfApp_IC.Services.Log
{
    public enum LogLevel
    {
        Info,
        Warning,
        Error
    }

    /// <summary>
    /// Модель одной записи в логе
    /// </summary>
    public class LogEntry
    {
        public DateTime Time { get; init; } = DateTime.Now;
        public LogLevel Level { get; init; }
        public string Message { get; init; } = "";

        /// <summary>Готовая строка для отображения в UI</summary>
        public string Display => $"[{Time:HH:mm:ss}] [{Level}] {Message}";
    }
}
