using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using WpfApp_IC.ViewModels;

namespace WpfApp_IC.Pages
{
    public partial class VideojetErrors : UserControl
    {
        public VideojetErrors()
        {
            InitializeComponent();
        }

        private VideojetPrinter VM => (VideojetPrinter)DataContext;

        private async void ClearErrors_Click(object sender, RoutedEventArgs e)
        {
            await VM.ClearAllFaultsAsync();
            await VM.ClearAllWarningsAsync();
        }
    }
}
