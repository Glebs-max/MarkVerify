using System.Windows;
using System.Windows.Controls;
using WpfApp_IC.ViewModels;

namespace WpfApp_IC.Pages
{
    public partial class VideojetErrors : UserControl
    {
        public VideojetErrors()
        {
            InitializeComponent();
        }

        private VideojetErrorsViewModel VM => (VideojetErrorsViewModel)DataContext;

        private async void ClearErrors_Click(object sender, RoutedEventArgs e) => await VM.ClearErrorsAsync();
    }
}
