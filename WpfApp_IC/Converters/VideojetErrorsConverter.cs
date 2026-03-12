using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace WpfApp_IC.Converters
{
    public class VideojetErrorsConverter : IValueConverter
    {
        private struct Result
        {
            public Visibility AllGoodVisibility { get; set; }
            public Visibility ErrorListVisibility { get; set; }
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int count)
            {
                return count > 0
                    ? new Result { AllGoodVisibility = Visibility.Collapsed, ErrorListVisibility = Visibility.Visible }
                    : new Result { AllGoodVisibility = Visibility.Visible, ErrorListVisibility = Visibility.Collapsed };
            }

            return Binding.DoNothing;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Binding.DoNothing;
        }
    }
}
