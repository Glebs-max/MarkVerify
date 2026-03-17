using System.Globalization;
using System.Windows.Data;

namespace WpfApp_IC.Converters
{
    public class InspectionStatusConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool active)
                return active ? "/Assets/active.png" : "/Assets/stopped.png";

            return Binding.DoNothing;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Binding.DoNothing;
        }
    }
}
