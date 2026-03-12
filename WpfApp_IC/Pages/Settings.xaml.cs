using System.Windows;
using System.Windows.Controls;
using WpfApp_IC.ViewModels;

namespace WpfApp_IC.Pages
{
    public partial class Settings : UserControl
    {
        private SettingsViewModel VM => (SettingsViewModel)DataContext;

        public Settings() => InitializeComponent();

        private void Save_Click(object sender, RoutedEventArgs e) => VM.Save();
        private void Cancel_Click(object sender, RoutedEventArgs e) => VM.Cancel();
        private void BrowseIdmvs_Click(object sender, RoutedEventArgs e) => VM.BrowseIdmvs();

        private void RefreshCameras_Click(object sender, RoutedEventArgs e) => VM.RefreshCameras();

        private void BrowseRejectImages_Click(object sender, RoutedEventArgs e) => VM.BrowseRejectImagesPath();
    }
}