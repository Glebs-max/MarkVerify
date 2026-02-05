using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace WpfApp_IC.Converters
{
    public class StartStopColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool isRunning = value is bool b && b;

            return isRunning ? Brushes.Red : Brushes.Green;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}
