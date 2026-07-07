using Microsoft.Win32;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using WpfApp_IC.ViewModels;

namespace WpfApp_IC.Views
{
    public partial class Settings : UserControl
    {
        private SettingsViewModel VM => (SettingsViewModel)DataContext;

        public Settings()
        {
            InitializeComponent();

            Loaded += (s, e) =>
            {
                VM.ScanAvailableCameras();
                VM.GetAvailableCOMPorts();
            };
        }

        private void RejectsBrowse_Click(object sender, RoutedEventArgs e)
        {
            OpenFolderDialog path = new()
            {
                Title = "Выберите папку сохранения изображений отбраковки",
                Multiselect = false
            };

            if (path.ShowDialog() == true)
                VM.AppSettings.RejectImagesPath = path.FolderName;
        }
        private void Save_Click(object sender, RoutedEventArgs e) => VM.Save();
        private void Cancel_Click(object sender, RoutedEventArgs e) => VM.GetBack();
        private void Camera_DropDown(object sender, EventArgs e) => VM.ScanAvailableCameras();
        private void Scanner_DropDown(object sender, EventArgs e) => VM.GetAvailableCOMPorts();
    }
}
