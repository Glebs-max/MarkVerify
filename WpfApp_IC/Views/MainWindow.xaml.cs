using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using WpfApp_IC.ViewModels;

namespace WpfApp_IC.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow(MainViewModel vm)
        {
            InitializeComponent();
            DataContext = vm;
        }

        private void Fullscreen_Click(object sender, MouseButtonEventArgs e) => WindowStyle = WindowStyle == WindowStyle.None ? WindowStyle.SingleBorderWindow : WindowStyle.None;
        private void Minimize_Click(object sender, MouseButtonEventArgs e) => WindowState = WindowState.Minimized;

        private void Tray_MouseEnter(object sender, MouseEventArgs e)
        {
            if (sender is Grid tray && tray.Children.OfType<Border>().FirstOrDefault() is Border b)
                b.Opacity = 0.25;
        }
        private void Tray_MouseLeave(object sender, MouseEventArgs e)
        {
            if (sender is Grid tray && tray.Children.OfType<Border>().FirstOrDefault() is Border b)
                b.Opacity = 0;
        }
    }
}