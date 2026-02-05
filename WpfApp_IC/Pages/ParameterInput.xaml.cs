using LabelDesigner.Models;
using Microsoft.Win32;
using System.Windows;
using System.Windows.Controls;

namespace WpfApp_IC.Pages
{
    public partial class ParameterInput : UserControl
    {
        public ParameterInput(Field model)
        {
            InitializeComponent();
            DataContext = model;
        }

        private void Browse_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new()
            {
                Title = "Select Image",
                Filter = "Image Files (*.BMP;*.JPG;*.GIF;*.PNG)|*.BMP;*.JPG;*.GIF;*.PNG|All Files (*.*)|*.*"
            };

            if (openFileDialog.ShowDialog() == true)
                if (DataContext is ImageField image)
                    image.Source = openFileDialog.FileName;
        }
    }
}
