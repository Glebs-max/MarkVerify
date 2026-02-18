using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using WpfApp_IC.ViewModels;

namespace WpfApp_IC.Pages
{
    public partial class LabelPrinting : UserControl
    {
        private LabelPrintingViewModel VM => (LabelPrintingViewModel)DataContext;

        public LabelPrinting()
        {
            InitializeComponent();

            Loaded += async (s, e) => await VM.PrintInitiate();
        }

        private void Pause_Click(object sender, RoutedEventArgs e)
        {
            VM.PrintingStatus = VM.PrintingStatus == PrintingStatus.Printing 
                ? PrintingStatus.Paused 
                : PrintingStatus.Printing;
        }
        private async void Stop_Click(object sender, RoutedEventArgs e) => await VM.PrintTerminate();
        private void StatusBar_Click(object sender, MouseButtonEventArgs e) => (LabelPreview.Visibility, VideojetErrors.Visibility) = (VideojetErrors.Visibility, LabelPreview.Visibility);
        private void Exit_Click(object sender, RoutedEventArgs e) => VM.Exit();

        private void CameraBasic_Loaded(object sender, RoutedEventArgs e)
        {

        }
    }
}