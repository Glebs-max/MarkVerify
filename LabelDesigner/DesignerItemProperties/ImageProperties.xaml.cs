using Microsoft.Win32;
using System.Windows;
using System.Windows.Controls;

namespace LabelDesigner.DesignerItemProperties
{
    public partial class ImageProperties : UserControl
    {
        public ImageProperties()
        {
            InitializeComponent();
        }

        private void Browse_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new()
            {
                Title = "Select Image",
                Filter = "Image Files (*.BMP;*.JPG;*.GIF;*.PNG)|*.BMP;*.JPG;*.GIF;*.PNG|All Files (*.*)|*.*"
            };

            if (openFileDialog.ShowDialog() == true)
                SourceBox.Text = openFileDialog.FileName;
        }
    }
}
