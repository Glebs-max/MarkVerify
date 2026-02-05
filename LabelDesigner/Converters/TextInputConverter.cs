using LabelDesigner.Models;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace LabelDesigner.Converters
{
    public class TextInputConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is TextField model)
            {
                switch (model)
                {
                    case DateField:
                    case TimeField:
                        return Visibility.Collapsed;
                    case TextField:
                        return Visibility.Visible;
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
