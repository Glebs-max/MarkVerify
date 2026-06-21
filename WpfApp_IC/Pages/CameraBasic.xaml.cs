using System.Windows;
using System.Windows.Controls;
using WpfApp_IC.ViewModels;

namespace WpfApp_IC.Pages
{
    public partial class CameraBasic : UserControl
    {
        public CameraBasic()
        {
            InitializeComponent();
        }

        private CameraViewModel VM => (CameraViewModel)DataContext;

        private void StartStop_Click(object sender, RoutedEventArgs e)
        {
            if (VM.IsRunning)
                VM.Stop();
            else
                VM.Start();
        }

        private async void TriggerManual_Click(object sender, RoutedEventArgs e) => VM.TriggerManual();
    }
}
