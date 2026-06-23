using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using WpfApp_IC.ViewModels;

namespace WpfApp_IC.Converters
{
    public class WorkStatusConverter : IMultiValueConverter
    {
        private struct Result
        {
            public string StatusText { get; set; }
            public Brush StatusColor { get; set; }
            public string StatusIcon { get; set; }
            public string StartStopText { get; set; }
            public string PauseText { get; set; }
            public bool StartStopEnabled { get; set; }
            public bool PauseEnabled { get; set; }
            public bool ReportEnabled { get; set; }
            public bool ExitEnabled { get; set; }
        }

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values[0] is WorkState printerState && values[1] is ErrorState errorState)
            {
                Result result = new();

                switch (printerState)
                {
                    case WorkState.Initiating:
                        result.StatusText = "Подключение устройств";
                        result.StatusColor = Brushes.LightBlue;
                        result.PauseEnabled = false;
                        result.ReportEnabled = false;
                        result.ExitEnabled = true;
                        result.StartStopEnabled = false;
                        result.PauseText = "Пауза";
                        result.StartStopText = "Подождите";
                        break;
                    case WorkState.Ready:
                        result.StatusText = "Готов к работе";
                        result.StatusColor = Brushes.WhiteSmoke;
                        result.PauseEnabled = false;
                        result.ReportEnabled = false;
                        result.ExitEnabled = true;
                        result.StartStopEnabled = true;
                        result.PauseText = "Пауза";
                        result.StartStopText = "Старт";
                        break;
                    case WorkState.Active:
                        result.StatusText = "В работе";
                        result.StatusColor = Brushes.LightGreen;
                        result.PauseEnabled = true;
                        result.ReportEnabled = false;
                        result.ExitEnabled = false;
                        result.StartStopEnabled = true;
                        result.PauseText = "Пауза";
                        result.StartStopText = "Завершить";
                        break;
                    case WorkState.Pause:
                        result.StatusText = "Пауза";
                        result.StatusColor = Brushes.LightBlue;
                        result.PauseEnabled = true;
                        result.ReportEnabled = false;
                        result.ExitEnabled = false;
                        result.StartStopEnabled = true;
                        result.PauseText = "Возобновить";
                        result.StartStopText = "Завершить";
                        break;
                    case WorkState.Finished:
                        result.StatusText = "Работа завершена";
                        result.StatusColor = Brushes.WhiteSmoke;
                        result.PauseEnabled = false;
                        result.ReportEnabled = true;
                        result.ExitEnabled = true;
                        result.StartStopEnabled = false;
                        result.PauseText = "Пауза";
                        result.StartStopText = "Завершить";
                        break;
                }

                switch (errorState)
                {
                    case ErrorState.None:
                        result.StatusIcon = "/Assets/good.png";
                        break;
                    case ErrorState.Warnings:
                        result.StatusIcon = "/Assets/warning.png";
                        result.StatusColor = Brushes.LightGoldenrodYellow;
                        break;
                    case ErrorState.Errors:
                        result.StatusIcon = "/Assets/fault.png";
                        result.StatusColor = Brushes.OrangeRed;
                        break;
                }

                return result;
            }

            return Binding.DoNothing;
        }
        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}