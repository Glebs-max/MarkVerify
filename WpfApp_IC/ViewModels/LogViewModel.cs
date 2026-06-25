using Observable;
using System.Collections.ObjectModel;
using System.Windows;
using WpfApp_IC.Services.Log;

namespace WpfApp_IC.ViewModels
{
    public class LogViewModel : ObservableObject
    {
        private readonly LogService _log;

        public LogViewModel(LogService log)
        {
            _log = log;

            _log.EntriesChanged += () =>
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    try
                    {
                        foreach (LogEntry entry in Entries.ToList())
                        {
                            if (!_log.Entries.Contains(entry))
                                Entries.Remove(entry);
                        }
                        foreach (LogEntry entry in _log.Entries.ToList())
                        {
                            if (!Entries.Contains(entry))
                                Entries.Insert(0, entry);
                        }
                    }
                    catch { }
                });
            };
        }

        public ObservableCollection<LogEntry> Entries { get; set; } = [];

        public void ClearLog() => _log.Entries.Clear();
    }
}
