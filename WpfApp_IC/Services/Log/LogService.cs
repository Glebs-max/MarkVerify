using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace WpfApp_IC.Services.Log
{
    /// <summary>
    /// Реализация сервиса логирования 
    /// Новые записи добавляются в НАЧАЛО коллекции - свежие логи сверху.
    /// Регистрируем как Singleton 
    /// </summary>
    public class LogService : ILogService
    {
        // Ограничение на максимальное количество записей, чобы не накапливались бесконечно (может стоит убрать или сделать настраиваемым полем)
        private const int MaxEntries = 500;

        public ObservableCollection<LogEntry> Entries { get; } = new();

        public void Info(string message) => Add(new LogEntry { Level = LogLevel.Info, Message = message });

        public void Warning(string message) => Add(new LogEntry { Level = LogLevel.Warning, Message = message });

        public void Error (string message, Exception? ex = null)
        {
            // Если есть исключение добавим его текст 
            string full = ex != null ? $"{message}: {ex.Message}" : message;
            Add(new LogEntry { Level = LogLevel.Error, Message = full });
        }

        private void Add(LogEntry entry)
        {
            DispatchUI(() =>
            {
                Entries.Insert(0, entry);

                // Авто‑скролл вверх
                if (Application.Current.MainWindow is MainWindow mw)
                {
                    var list = mw.FindName("LogListBox") as ListBox;
                    list?.ScrollIntoView(entry);
                }

                while (Entries.Count > MaxEntries)
                    Entries.RemoveAt(Entries.Count - 1);
            });
        }


        private static void DispatchUI (Action action)
        {
            if (Application.Current?.Dispatcher.CheckAccess() == true)
                action();
            else
                Application.Current?.Dispatcher.BeginInvoke(action);
        }
    }
}
