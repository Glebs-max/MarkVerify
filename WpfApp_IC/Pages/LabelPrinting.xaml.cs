using WpfApp_IC.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace WpfApp_IC.Pages
{
    public partial class LabelPrinting : UserControl
    {
        public LabelPrinting()
        {
            InitializeComponent();

            Loaded += async (s, e) => await VM.PrintInitiate();
        }

        private LabelPrintingViewModel VM => (LabelPrintingViewModel)DataContext;

        private void Pause_Click(object sender, RoutedEventArgs e)
        {
            VM.PrintingStatus = VM.PrintingStatus == PrintingStatus.Printing ? PrintingStatus.Paused : PrintingStatus.Printing;
            Pause.Content = VM.PrintingStatus == PrintingStatus.Printing ? "Пауза" : "Возобновить";
        }
        private async void Stop_Click(object sender, RoutedEventArgs e)
        {
            await VM.PrintTerminate();
            Pause.IsEnabled = false;
            Stop.IsEnabled = false;
            Report.IsEnabled = true;
            Exit.IsEnabled = true;
        }
        private void Exit_Click(object sender, RoutedEventArgs e) => VM.Exit();
    }
}
