using Observable;
using System.Windows.Input;

namespace WpfApp_IC.ViewModels
{
    public class HomeViewModel(MainViewModel mainViewModel) : ObservableObject
    {
        public void Start() => mainViewModel.CurrentViewModel = mainViewModel.GetViewModel<ProductsViewModel>();
        public void Settings() { }
    }
}
