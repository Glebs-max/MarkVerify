using System.Globalization;
using System.Windows.Data;
using WpfApp_IC.ViewModels;

namespace WpfApp_IC.Converters
{
    public class WorkStateConverter : IValueConverter
    {
        private struct Result
        {
            public bool StartStopEnabled { get; set; }
            public bool PauseEnabled { get; set; }
            public bool ReportEnabled { get; set; }
            public bool ExitEnabled { get; set; }
            public string StartStopText { get; set; }
            public string PauseText { get; set; }
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is WorkState state)
            {
                switch (state)
                {
                    case WorkState.Ready:
                        return new Result()
                        {
                            StartStopEnabled = true,
                            PauseEnabled = false,
                            ReportEnabled = false,
                            ExitEnabled = false,
                            StartStopText = "Старт",
                            PauseText = "Пауза"
                        };
                    case WorkState.Active:
                        return new Result()
                        {
                            StartStopEnabled = true,
                            PauseEnabled = true,
                            ReportEnabled = false,
                            ExitEnabled = false,
                            StartStopText = "Завершить",
                            PauseText = "Пауза"
                        };
                    case WorkState.Pause:
                        return new Result()
                        {
                            StartStopEnabled = true,
                            PauseEnabled = true,
                            ReportEnabled = false,
                            ExitEnabled = false,
                            StartStopText = "Завершить",
                            PauseText = "Возобновить"
                        };
                    case WorkState.Finished:
                        return new Result()
                        {
                            StartStopEnabled = false,
                            PauseEnabled = false,
                            ReportEnabled = true,
                            ExitEnabled = true,
                            StartStopText = "Завершить",
                            PauseText = "Пауза"
                        };
                }
            }

            return Binding.DoNothing;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Binding.DoNothing;
        }
    }
}
