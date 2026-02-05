using System.Globalization;
using System.Windows.Data;

namespace LabelDesigner.Converters
{
    public class LabelAreaConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values != null && values.Length == 2 && values[0] is double size && values[1] is double scale)
            {
                return size * scale + 100;
            }
            
            return Binding.DoNothing;
        }
        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
