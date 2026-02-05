using LabelDesigner.Models;
using System.Windows.Controls;

namespace LabelDesigner.DesignerItemProperties
{
    public partial class DateProperties : UserControl
    {
        public DateProperties()
        {
            InitializeComponent();
            DataContextChanged += (s, e) =>
            {
                LoadSeparators();
                LoadDateFormats();
            };
            SeparatorBox.SelectionChanged += (s, e) => UpdateDateFormats();
        }

        private void LoadDateFormats()
        {
            List<string> formats = [
                "dd$MM$yyyy",
                "dd$MM$yy",
                "MM$dd$yy",
                "yy",
                "dd",
                "MMMM",
                "MM",
                "yyyy"
            ];

            foreach (string format in formats)
            {
                DateFormatBox.Items.Add(new ComboBoxItem()
                {
                    Content = format.Replace('$', (DataContext as DateField)?.Separator ?? '.'),
                    Tag = format
                });
            }
        }
        private void UpdateDateFormats()
        {
            foreach (ComboBoxItem item in DateFormatBox.Items)
            {
                item.Content = item.Tag.ToString()?.Replace('$', (DataContext as DateField)?.Separator ?? '.');
            }
        }
        private void LoadSeparators()
        {
            List<(string, char?)> separators = [
                ("Пробел", ' '),
                ("Точка", '.'),
                ("Запятая", ','),
                ("Тире", '-'),
                ("Обратный разделитель", '\\'),
                ("Разделитель", '/'),
                ("Нет", '\0')
            ];

            foreach ((string, char?) separator in separators)
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
