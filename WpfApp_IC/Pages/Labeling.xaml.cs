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

        private void Pause_Click(object sender, RoutedEventArgs e) { }
        private void Stop_Click(object sender, RoutedEventArgs e) => VM.WorkTerminate();
        private void Exit_Click(object sender, RoutedEventArgs e) => VM.Exit();
    }
}