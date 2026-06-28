using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using WpfApp_IC.Models;
using WpfApp_IC.Services;

namespace WpfApp_IC.Controls
{
    public partial class Log : UserControl
    {
        private LogService LogService => (LogService)DataContext;

        public Log()
        {
            InitializeComponent();

            Loaded += (s, e) =>
            {
                LogService.NewEntry += (entry) =>
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        LogBox.Inlines.Add(new Run(entry.ToString()) { Foreground = GetForegroundBrush(entry) });
                        LogBox.Inlines.Add(new LineBreak());
                    });
                };
                LogService.EntriesCleared += () =>
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        LogBox.Inlines.Clear();
                    });
                };
            };
        }

        private static SolidColorBrush GetForegroundBrush(LogEntry entry)
        {
            return entry.ColorCode switch
            {
                LogColorCode.Green => Brushes.DarkGreen,
                LogColorCode.Yellow => Brushes.Gold,
                LogColorCode.Red => Brushes.DarkRed,
                _ => Brushes.Black,
            };
        }
    }
}
