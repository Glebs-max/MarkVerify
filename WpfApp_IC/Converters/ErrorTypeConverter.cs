using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace WpfApp_IC.Converters
{
    public class ErrorTypeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is ErrorType errorType)
            {
                return errorType switch
                {
                    ErrorType.Warning => "/Assets/warning.png",
                    ErrorType.Fault => "/Assets/fault.png",
                    _ => Binding.DoNothing
                };
            }

            return Binding.DoNothing;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Binding.DoNothing;
        }
    }
}
