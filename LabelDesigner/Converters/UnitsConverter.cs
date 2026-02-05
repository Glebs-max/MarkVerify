using LabelDesigner.Services;
using System.Globalization;
using System.Windows.Data;

namespace LabelDesigner.Converters
{
    public class UnitsConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double px)
                return UnitsService.ToMM(px);

            return Binding.DoNothing;
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double mm)
                return UnitsService.FromMM(mm);

            return Binding.DoNothing;
        }
    }
}
