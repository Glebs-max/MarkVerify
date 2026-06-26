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

        public string Display => $"[{Time:HH:mm:ss}] [{Level}] {Message}";
    }
}
