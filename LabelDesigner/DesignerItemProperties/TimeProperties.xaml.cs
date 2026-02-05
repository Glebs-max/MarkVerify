using LabelDesigner.Models;
using System.Windows.Controls;

namespace LabelDesigner.DesignerItemProperties
{
    public partial class TimeProperties : UserControl
    {
        public TimeProperties()
        {
            InitializeComponent();
            DataContextChanged += (s, e) =>
            {
                LoadSeparators();
                LoadTimeFormats();
            };
            SeparatorBox.SelectionChanged += (s, e) => UpdateTimeFormats();
            
        }

        private void LoadTimeFormats()
        {
            List<string> formats = [
                "HH$mm$ss",
                "hh$mm$ss",
                "h$m$s",
                "h$mm",
                "hh$mm",
                "HH$mm",
                "hh",
                "HH",
                "mm",
                "ss"
            ];

            foreach (string format in formats)
            {
                TimeFormatBox.Items.Add(new ComboBoxItem()
                {
                    Content = format.Replace('$', (DataContext as TimeField)?.Separator ?? ':'),
                    Tag = format
                });
            }
        }
        private void UpdateTimeFormats()
        {
            foreach (ComboBoxItem item in TimeFormatBox.Items)
            {
                item.Content = item.Tag?.ToString()?.Replace('$', (DataContext as TimeField)?.Separator ?? ':');
            }
        }
        private void LoadSeparators()
        {
            List<(string, char)> separators = [
                ("Двоеточие", ':'),
                ("Пробел", ' '),
                ("Точка", '.'),
                ("Запятая", ','),
                ("Тире", '-'),
                ("Обратный разделитель", '\\')
            ];

            foreach ((string, char) separator in separators)
            {
                SeparatorBox.Items.Add(new ComboBoxItem()
                {
                    Content = $"({separator.Item2}) {separator.Item1}",
                    Tag = separator.Item2
                });
            }
        }
    }
}
