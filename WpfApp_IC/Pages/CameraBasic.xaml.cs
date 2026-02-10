using System.Windows.Controls;
using WpfApp_IC.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace WpfApp_IC.Pages
{
    public partial class CameraBasic : UserControl
    {
        private CameraBasicViewModel VM => (CameraBasicViewModel)DataContext;

        public CameraBasic()
        {
            InitializeComponent();

            // Получаем VM из DI вручную
            DataContext = App.AppHost!.Services.GetRequiredService<CameraBasicViewModel>();
        }

        private void StartStop_Click(object sender, System.Windows.RoutedEventArgs e)
            => VM.StartStop();
    }
}
