using System.Windows;
using WpfApp_IC.ViewModels;

namespace WpfApp_IC
{
    public partial class MainWindow : Window
    {
        private readonly MainViewModel _vm;

        public MainWindow(MainViewModel vm)
        {
            InitializeComponent();
            DataContext = _vm = vm;
        }

        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {

        }

        // КНОПКА ДОМОЙ
        private void GoHome_Click(object sender, RoutedEventArgs e) => _vm.CurrentViewModel = _vm.GetViewModel<HomeViewModel>();

        // ПОМОЩЬ
        private void HelpButton_Click(object sender, RoutedEventArgs e)
        {

        }

        // НАЗАД
        private void BackButton_Click(object sender, RoutedEventArgs e)
        {

        }

        // ВПЕРЁД
        private void ForwardButton_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}