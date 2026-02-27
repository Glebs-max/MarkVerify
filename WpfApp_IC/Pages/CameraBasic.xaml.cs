using Microsoft.Extensions.DependencyInjection;
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

        private CameraBasicViewModel VM => (CameraBasicViewModel)DataContext;

        private void StartStop_Click(object sender, RoutedEventArgs e) => VM.StartStop();
    }
}
