using LabelDesigner.Models;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace LabelDesigner.Converters
{
    public class DateInputConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is DataType dataType)
            {
                switch (dataType)
                {
                    case DataType.Fixed:
                    case DataType.Input:
                        return Visibility.Visible;
                    case DataType.Calculated:
                    case DataType.Current:
                        return Visibility.Collapsed;
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
