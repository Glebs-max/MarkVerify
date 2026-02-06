using System.Windows;
using WpfApp_IC.ViewModels;

namespace WpfApp_IC
{
    public partial class MainWindow : Window
    {
        private readonly MainViewModel _vm;
        private bool _isMenuExpanded;

        public MainWindow(MainViewModel vm)
        {
            InitializeComponent();
            DataContext = _vm = vm;

            Loaded += async (s, e) => await _vm.VideojetPrinter.ConnectAsync();
        }

        // ЛЕВОЕ МЕНЮ
        private void MenuButton_Click(object sender, RoutedEventArgs e)
        {
            _isMenuExpanded = !_isMenuExpanded;

            if (_isMenuExpanded)
            {
                LeftColumn.Width = new GridLength(220);
                MenuItemsPanel.Visibility = Visibility.Visible;
                MenuText.Visibility = Visibility.Visible;
            }
            else
            {
                LeftColumn.Width = new GridLength(60);
                MenuItemsPanel.Visibility = Visibility.Collapsed;
                MenuText.Visibility = Visibility.Collapsed;
            }
        }

        // ПУНКТЫ МЕНЮ
        private void CameraBasic_Click(object sender, RoutedEventArgs e)
        {
            var vm = (MainViewModel)DataContext;
            vm.CurrentViewModel = vm.GetViewModel<CameraBasicViewModel>();
        }

        private void CameraAdvanced_Click(object sender, RoutedEventArgs e)
        {

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