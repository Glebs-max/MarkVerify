using System.Windows;
using System.Windows.Controls;
using WpfApp_IC.ViewModels;

namespace WpfApp_IC.Views
{
    public partial class Settings : UserControl
    {
        private SettingsViewModel VM => (SettingsViewModel)DataContext;

        public Settings()
        {
            InitializeComponent();

            Loaded += (s, e) => VM.ScanAvailableCameras();
        }

        private void RejectsBrowse_Click(object sender, RoutedEventArgs e)
        {

        }
        private void Save_Click(object sender, RoutedEventArgs e) => VM.Save();
        private void GetBack_Click(object sender, RoutedEventArgs e) => VM.GetBack();
    }
}
