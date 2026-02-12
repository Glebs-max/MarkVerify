using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
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
            if (values[0] is PrintingStatus printingStatus && values[1] is ErrorStatus errorStatus)
            {
                Result result = new();

                switch (printingStatus)
                {
                    case PrintingStatus.Printing:
                        result.StatusText = "Печать";
                        result.StopEnabled = result.PauseEnabled = true;
                        result.ReportEnabled = result.ExitEnabled = false;
                        result.PauseText = "Пауза";
                        break;
                    case PrintingStatus.Paused:
                        result.StatusText = "Приостановлено";
                        result.StopEnabled = result.PauseEnabled = true;
                        result.ReportEnabled = result.ExitEnabled = false;
                        result.PauseText = "Возобновить";
                        break;
                    case PrintingStatus.Finished:
                        result.StatusText = "Завершено";
                        result.StopEnabled = result.PauseEnabled = false;
                        result.ReportEnabled = result.ExitEnabled = true;
                        result.PauseText = "Пауза";
                        break;
                }

                switch (errorStatus)
                {
                    case ErrorStatus.None:
                        result.StatusIcon = "/Assets/good.png";
                        switch (printingStatus)
                        {
                            case PrintingStatus.Printing:
                                result.StatusColor = Brushes.LightGreen;
                                break;
                            case PrintingStatus.Paused:
                                result.StatusColor = Brushes.LightBlue;
                                break;
                            case PrintingStatus.Finished:
                                result.StatusColor = Brushes.LightGray;
                                break;
                        }
                        break;
                    case ErrorStatus.Warnings:
                        result.StatusIcon = "/Assets/warning.png";
                        switch (printingStatus)
                        {
                            case PrintingStatus.Printing:
                            case PrintingStatus.Paused:
                                result.StatusColor = Brushes.LightYellow;
                                break;
                            case PrintingStatus.Finished:
                                result.StatusColor = Brushes.LightGray;
                                break;
                        }
                        break;
                    case ErrorStatus.Faults:
                        result.StatusIcon = "/Assets/fault.png";
                        switch (printingStatus)
                        {
                            case PrintingStatus.Printing:
                            case PrintingStatus.Paused:
                                result.StatusColor = Brushes.OrangeRed;
                                break;
                            case PrintingStatus.Finished:
                                result.StatusColor = Brushes.LightGray;
                                break;
                        }
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