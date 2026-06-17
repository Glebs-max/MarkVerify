//Конвертер — это класс, который преобразует данные между VM и View в момент привязки.
//Нужен когда тип или формат данных в VM не совпадает с тем что ожидает UI-элемент.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using WpfApp_IC.Devices;

namespace WpfApp_IC.Converters
{
    public class ErrorTypeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is VideojetErrorType errorType)
            {
                return errorType switch
                {
                    VideojetErrorType.Warning => "/Assets/warning.png",
                    VideojetErrorType.Fault => "/Assets/fault.png",
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
