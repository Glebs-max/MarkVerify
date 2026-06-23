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

namespace WpfApp_IC.Views
{
    public partial class Home : UserControl
    {
        public Home()
        {
            InitializeComponent();
        }

        private HomeViewModel VM => (HomeViewModel)DataContext;

        private void Start_Click(object sender, RoutedEventArgs e) => VM.Start();
        private void Settings_Click(object sender, RoutedEventArgs e) => VM.Settings();
        private void Camera_Click(object sender, RoutedEventArgs e) => VM.Camera();
    }
}
