using WpfApp_IC.Models;
using WpfApp_IC.ViewModels;
using Microsoft.Win32;
using System.IO;
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
            VM.Active = !VM.Active;
            Pause.Content = VM.Active ? "Пауза" : "Возобновить";
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
