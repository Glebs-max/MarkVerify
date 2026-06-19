using LabelDesigner.Services;
using Observable;
using System.IO;
using WpfApp_IC.Pages;

namespace WpfApp_IC.ViewModels
{
    public class HomeViewModel(MainViewModel mainViewModel) : ObservableObject
    {
        public void Start() => mainViewModel.CurrentViewModel = mainViewModel.GetViewModel<ProductsViewModel>();
        public void Camera()
        {
            LabelPreviewViewModel model = mainViewModel.GetViewModel<LabelPreviewViewModel>();
            model.DesignerViewModel = DesignerService.LoadLabel(Path.Combine(AppContext.BaseDirectory, $"LabelTemplates/Horizontal.xml")) ?? new();
            model.DesignerViewModel.Preview = true;
            mainViewModel.CurrentViewModel = model;
        }
        public void Settings() => mainViewModel.CurrentViewModel = mainViewModel.GetViewModel<SettingsViewModel>();
    }
}
