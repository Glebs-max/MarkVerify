namespace WpfApp_IC.Services.Log
{
    /// <summary>
    /// Реализация сервиса логирования 
    /// Новые записи добавляются в НАЧАЛО коллекции - свежие логи сверху.
    /// Регистрируем как Singleton 
    /// </summary>
    public class LogService
    {
        // Ограничение на максимальное количество записей, чобы не накапливались бесконечно (может стоит убрать или сделать настраиваемым полем)
        private const int MaxEntries = 500;

        public event Action? EntriesChanged;

        public List<LogEntry> Entries { get; } = [];

        public void Info(string message) => Add(new LogEntry { Level = LogLevel.Info, Message = message });
        public void Warning(string message) => Add(new LogEntry { Level = LogLevel.Warning, Message = message });
        public void Error(string message, Exception? ex = null) => Add(new LogEntry { Level = LogLevel.Error, Message = ex != null ? $"{message}: {ex.Message}" : message });

        private void Add(LogEntry entry)
        {
            Entries.Add(entry);

            while (Entries.Count > MaxEntries)
                Entries.RemoveAt(0);

            EntriesChanged?.Invoke();
        }
    }
}
