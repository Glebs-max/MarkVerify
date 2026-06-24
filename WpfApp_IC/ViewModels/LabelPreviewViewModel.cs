using LabelDesigner;
using WpfApp_IC.Models;
using Observable;
using WpfApp_IC.Models.Devices;
using System.Windows;

namespace WpfApp_IC.ViewModels
{
    /// <summary>
    /// Модель предпросмотра шаблонов этикеток
    /// </summary>
    public class LabelPreviewViewModel(MainViewModel mainViewModel) : ObservableObject
    {
        private DesignerViewModel _designerViewModel = new();

        public DesignerViewModel DesignerViewModel
        {
            get => _designerViewModel;
            set => Set(ref _designerViewModel, value);
        }

        public void PrintLabel()
        {
            LabelingViewModel model = mainViewModel.GetViewModel<LabelingViewModel>();
            model.LabelPrintingViewModel.DesignerViewModel = DesignerViewModel;
            model.LabelPrintingViewModel.DesignerViewModel.Scale = 3.0;
            mainViewModel.CurrentViewModel = model;
        }
        public void GetBack() => mainViewModel.CurrentViewModel = mainViewModel.GetViewModel<ProductsViewModel>();
    }
}
