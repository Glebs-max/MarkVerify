namespace WpfApp_IC.Services.Log
{
    /// <summary>
    /// Реализация сервиса логирования 
    /// Новые записи добавляются в НАЧАЛО коллекции - свежие логи сверху.
    /// Регистрируем как Singleton 
    /// </summary>
    public class LogService
    {
        private const int MaxEntries = 3000;

        public event Action? EntriesChanged;

        public List<LogEntry> Entries { get; } = [];

        public void Info(string message) => Add(new LogEntry { Level = LogLevel.Info, Message = message });
        public void Warning(string message) => Add(new LogEntry { Level = LogLevel.Warning, Message = message });
        public void Error(string message, Exception? ex = null) => Add(new LogEntry { Level = LogLevel.Error, Message = ex != null ? $"{message}: {ex.Message}" : message });

        private void Add(LogEntry entry)
        {
            Entries.Add(entry);

            if (Entries.Count > MaxEntries)
                Entries.RemoveRange(0, 1000);

            EntriesChanged?.Invoke();
        }
    }
}
