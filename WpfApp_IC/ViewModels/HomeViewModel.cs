using LabelDesigner.Services;
using Observable;
using System.IO;
using System.Windows;

namespace WpfApp_IC.ViewModels
{
    public class HomeViewModel(MainViewModel mainViewModel) : ObservableObject
    {
        public void Start() => mainViewModel.SetViewModel<ProductsViewModel>();
        public void Camera()
        {
            LabelPreviewViewModel model = mainViewModel.SetViewModel<LabelPreviewViewModel>();
            model.DesignerViewModel = DesignerService.LoadLabel(Path.Combine(AppContext.BaseDirectory, $"LabelTemplates/Horizontal.xml")) ?? new();
            model.DesignerViewModel.Preview = true;
        }
        public void Settings() => mainViewModel.SetViewModel<SettingsViewModel>();
        public static void Exit() => Application.Current.Shutdown();
    }
}
