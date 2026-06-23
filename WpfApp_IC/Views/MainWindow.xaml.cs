using System.Windows;
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
    }
}