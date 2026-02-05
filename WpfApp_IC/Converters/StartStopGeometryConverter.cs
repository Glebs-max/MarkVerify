using System;
using System.Globalization;
using System.Windows.Data;

namespace WpfApp_IC.Converters
{
    public class StartStopGeometryConverter : IValueConverter
    {
        // (треугольник) — старт
        private const string StartGeometry = "M 0 0 L 0 20 L 17 10 Z";

        // (две полоски) — стоп
        private const string StopGeometry = "M 0 0 H 6 V 20 H 0 Z M 10 0 H 16 V 20 H 10 Z";

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool isRunning = value is bool b && b;

            return isRunning ? StopGeometry : StartGeometry;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}
