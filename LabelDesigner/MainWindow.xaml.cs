using LabelDesigner.Services;
using Microsoft.Win32;
using System.IO;
using System.Windows;

namespace LabelDesigner
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            DataContext = new DesignerViewModel();
        }

        private DesignerViewModel VM => DataContext as DesignerViewModel ?? new();

        private void FileOpen_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dialog = new()
            {
                Title = "Открыть этикетку",
                Filter = "Label File (*.xml)|*.xml",
                DefaultExt = ".xml",
                AddExtension = true
            };

            if (dialog.ShowDialog() == true)
            {
                try
                {
                    if (DesignerService.LoadLabel(dialog.FileName) is DesignerViewModel model)
                        DataContext = model;
                }
                catch (IOException ex)
                {
                    MessageBox.Show($"Ошибка при открытии файла:\n{ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
        private void FileSave_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog dialog = new()
            {
                Title = "Сохранить этикетку",
                Filter = "Label File (*.xml)|*.xml",
                DefaultExt = ".xml",
                FileName = "label.xml",
                AddExtension = true,
                OverwritePrompt = true
            };

            if (dialog.ShowDialog() == true)
            {
                try
                {
                    DesignerService.SaveLabel(VM, dialog.FileName);
                    MessageBox.Show("Файл успешно сохранён.", "Сохранение", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (IOException ex)
                {
                    MessageBox.Show($"Ошибка при сохранении файла:\n{ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
        private void FileExit_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}