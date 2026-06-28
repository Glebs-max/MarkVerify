using WpfApp_IC.Models;

namespace WpfApp_IC.Services
{
    public class LogService
    {
        public event Action<LogEntry>? NewEntry;
        public event Action? EntriesCleared;

        private List<LogEntry> Entries { get; } = [];

        public void AddEntry(string message, LogColorCode colorCode = LogColorCode.Green)
        {
            LogEntry entry = new(message, colorCode);

            Entries.Add(entry);
            NewEntry?.Invoke(entry);
        }
        public void ClearEntries()
        {
            Entries.Clear();
            EntriesCleared?.Invoke();
        }
    }
}
