using System.Collections.ObjectModel;
using System.Windows;
using WpfApp_IC.Models;

namespace WpfApp_IC.Services
{
    public class LogService
    {
        public event Action<LogEntry>? NewEntry;
        public event Action? EntriesCleared;

        public ObservableCollection<LogEntry> Entries { get; } = [];

        public void AddEntry(string message, LogColorCode colorCode = LogColorCode.Green)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                LogEntry entry = new(message, colorCode);

                Entries.Insert(0, entry);
                NewEntry?.Invoke(entry);
            });
        }
        public void ClearEntries()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                Entries.Clear();
                EntriesCleared?.Invoke();
            });
        }
    }
}
