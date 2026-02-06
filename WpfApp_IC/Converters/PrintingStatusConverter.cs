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
        }

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values[0] is PrintingStatus printingStatus && values[1] is ErrorStatus errorStatus)
            {
                switch (errorStatus)
                {
                    case ErrorStatus.None:
                        switch (printingStatus)
                        {
                            case PrintingStatus.Printing:
                                return new Result { StatusText = "Печать", StatusColor = Brushes.LightGreen, StatusIcon = "/Assets/good.png"};
                            case PrintingStatus.Paused:
                                return new Result { StatusText = "Приостановлено", StatusColor = Brushes.LightBlue, StatusIcon = "/Assets/good.png" };
                            case PrintingStatus.Finished:
                                return new Result { StatusText = "Завершено", StatusColor = Brushes.LightGray, StatusIcon = "/Assets/good.png" };
                        }
                        break;
                    case ErrorStatus.Warnings:
                        switch (printingStatus)
                        {
                            case PrintingStatus.Printing:
                                return new Result { StatusText = "Печать", StatusColor = Brushes.LightYellow, StatusIcon = "/Assets/warning.png" };
                            case PrintingStatus.Paused:
                                return new Result { StatusText = "Приостановлено", StatusColor = Brushes.LightYellow, StatusIcon = "/Assets/warning.png" };
                            case PrintingStatus.Finished:
                                return new Result { StatusText = "Завершено", StatusColor = Brushes.LightGray, StatusIcon = "/Assets/warning.png" };
                        }
                        break;
                    case ErrorStatus.Faults:
                        switch (printingStatus)
                        {
                            case PrintingStatus.Printing:
                                return new Result { StatusText = "Печать", StatusColor = Brushes.OrangeRed, StatusIcon = "/Assets/fault.png" };
                            case PrintingStatus.Paused:
                                return new Result { StatusText = "Приостановлено", StatusColor = Brushes.OrangeRed, StatusIcon = "/Assets/fault.png" };
                            case PrintingStatus.Finished:
                                return new Result { StatusText = "Завершено", StatusColor = Brushes.LightGray, StatusIcon = "/Assets/fault.png" };
                        }
                        break;
                }
            }

            return Binding.DoNothing;
        }
        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}