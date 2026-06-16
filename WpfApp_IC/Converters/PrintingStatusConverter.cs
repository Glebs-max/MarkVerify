using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using WpfApp_IC.Devices;
using WpfApp_IC.Models;
using WpfApp_IC.ViewModels;

namespace WpfApp_IC.Converters
{
    public class PrintingStatusConverter : IMultiValueConverter
    {
        private struct Result
        {
            public string StatusText { get; set; }
            public Brush StatusColor { get; set; }
            public string StatusIcon { get; set; }
            public string PauseText { get; set; }
            public bool StopEnabled { get; set; }
            public bool PauseEnabled { get; set; }
            public bool ReportEnabled { get; set; }
            public bool ExitEnabled { get; set; }
        }

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values[0] is PrinterState printerState && values[1] is ErrorState errorState)
            {
                Result result = new();

                switch (printerState)
                {
                    case PrinterState.Disconnected:
                        result.StatusText = "Нет соединения";
                        result.StatusColor = Brushes.WhiteSmoke;
                        break;
                    case PrinterState.Connecting:
                        result.StatusText = "Соединение...";
                        result.StatusColor = Brushes.WhiteSmoke;
                        break;
                    case PrinterState.Connected:
                        result.StatusText = "Соединение установлено";
                        result.StatusColor = Brushes.Lavender;
                        break;
                    case PrinterState.Shutdown:
                        result.StatusText = "Остановлен";
                        result.StatusColor = Brushes.Lavender;
                        break;
                    case PrinterState.StartingUp:
                        result.StatusText = "Запуск...";
                        result.StatusColor = Brushes.Lavender;
                        break;
                    case PrinterState.ShuttingDown:
                        result.StatusText = "Остановка...";
                        result.StatusColor = Brushes.LightBlue;
                        break;
                    case PrinterState.Offline:
                        result.StatusText = "Не в работе";
                        result.StatusColor = Brushes.LightBlue;
                        break;
                    case PrinterState.Running:
                        result.StatusText = "В работе";
                        result.StatusColor = Brushes.LightGreen;
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
                    case ErrorState.Faults:
                        result.StatusIcon = "/Assets/fault.png";
                        result.StatusColor = Brushes.OrangeRed;
                        break;
                    case ErrorState.Unknown:
                        result.StatusIcon = "/Assets/disconnected.png";
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