using System.Windows;
using System.Windows.Controls;
using WpfApp_IC.ViewModels;

namespace WpfApp_IC.Pages
{
    public partial class Labeling : UserControl
    {
        private LabelingViewModel VM => (LabelingViewModel)DataContext;

        public Labeling()
        {
            InitializeComponent();

            Loaded += async (s, e) => await VM.WorkInitiate();
        }

        private async void StartStop_Click(object sender, RoutedEventArgs e)
        {
            if (VM.WorkState == WorkState.Ready)
                await VM.WorkInitiate();
            else if (VM.WorkState != WorkState.Finished)
                await VM.WorkTerminate();
        }
        private async void Pause_Click(object sender, RoutedEventArgs e)
        {
            if (VM.WorkState == WorkState.Active)
                await VM.WorkPause();
            else if (VM.WorkState == WorkState.Pause)
                await VM.WorkContinue();
        }
        private async void Exit_Click(object sender, RoutedEventArgs e) => await VM.Exit();

        private void SHRATT(object sender, RoutedEventArgs e) => MessageBox.Show("EBALUGA!!!");
    }
}