using Microsoft.Extensions.DependencyInjection;
using System.Windows;
using System.Windows.Controls;
using WpfApp_IC.ViewModels;

namespace WpfApp_IC.Pages
{
    public partial class CameraBasic : UserControl
    {
        private CameraBasicViewModel VM => (CameraBasicViewModel)DataContext;

        public CameraBasic()
        {
            InitializeComponent();
        }

        private void StartStop_Click(object sender, RoutedEventArgs e) => VM.StartStop();
    }
}
