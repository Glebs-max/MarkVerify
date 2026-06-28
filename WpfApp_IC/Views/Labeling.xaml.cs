using System.Windows;
using System.Windows.Controls;
using WpfApp_IC.Models;
using WpfApp_IC.ViewModels;

namespace WpfApp_IC.Views
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
                await VM.WorkStart();
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
        private async void Exit_Click(object sender, RoutedEventArgs e)
        {
            await VM.Exit();
        }
        private void WorkMode_Changed(object sender, SelectionChangedEventArgs e)
        {
            if (!IsLoaded)
                return;

            switch (WorkModeBox.SelectedValue)
            {
                case "Default":
                    VM.LabelingSession.WorkMode = WorkMode.Default;
                    break;
                case "SkipDuplicates":
                    VM.LabelingSession.WorkMode = WorkMode.SkipDuplicates;
                    break;
            }
        }
        private void ScannerMode_Changed(object sender, SelectionChangedEventArgs e)
        {
            if (!IsLoaded)
                return;

            switch (ScannerModeBox.SelectedValue)
            {
                case "Verify":
                    VM.LabelingSession.ScannerMode = ScannerMode.Verify;
                    break;
                case "Reject":
                    VM.LabelingSession.ScannerMode = ScannerMode.Reject;
                    break;
            }
        }
    }
}