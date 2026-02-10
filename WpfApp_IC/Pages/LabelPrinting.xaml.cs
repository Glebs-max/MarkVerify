using System.Windows;
using System.Windows.Controls;
using WpfApp_IC.ViewModels;

namespace WpfApp_IC.Pages
{
    public partial class LabelPrinting : UserControl
    {
        private LabelPrintingViewModel VM => (LabelPrintingViewModel)DataContext;

        public LabelPrinting()
        {
            InitializeComponent();

            Loaded += async (s, e) =>
            {
                await VM.PrintInitiate();
            };
        }

        private void Pause_Click(object sender, RoutedEventArgs e)
        {
            VM.PrintingStatus =
                VM.PrintingStatus == PrintingStatus.Printing
                ? PrintingStatus.Paused
                : PrintingStatus.Printing;

            Pause.Content =
                VM.PrintingStatus == PrintingStatus.Printing
                ? "Пауза"
                : "Возобновить";
        }

        private async void Stop_Click(object sender, RoutedEventArgs e)
        {
            await VM.PrintTerminate();

            Pause.IsEnabled = false;
            Stop.IsEnabled = false;
            Report.IsEnabled = true;
            Exit.IsEnabled = true;
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            VM.Exit();
        }

        private void CameraToggle_Click(object sender, RoutedEventArgs e)
        {
            var vm = (LabelPrintingViewModel)DataContext;

            if (vm.CameraViewModel.IsRunning)
            {
                vm.InspectorController.Stop();
                vm.CameraViewModel.IsRunning = false;
                vm.CameraViewModel.AddLog("Инспекция поставлена на паузу");
            }
            else
            {
                vm.InspectorController.Start();
                vm.CameraViewModel.IsRunning = true;
                vm.CameraViewModel.AddLog("Инспекция запущена");
            }
        }


    }
}
