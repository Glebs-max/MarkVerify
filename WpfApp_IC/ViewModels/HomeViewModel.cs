using Observable;

namespace WpfApp_IC.ViewModels
{
    public class HomeViewModel(MainViewModel mainViewModel) : ObservableObject
    {
        public void Start() => mainViewModel.CurrentViewModel = mainViewModel.GetViewModel<ProductsViewModel>();
        public void Camera() => mainViewModel.CurrentViewModel = mainViewModel.GetViewModel<CameraViewModel>();
        public void Settings() => mainViewModel.CurrentViewModel = mainViewModel.GetViewModel<SettingsViewModel>();
    }
}
