using LabelDesigner;
using WpfApp_IC.Models;
using Observable;

namespace WpfApp_IC.ViewModels
{
    /// <summary>
    /// Модель предпросмотра шаблонов этикеток
    /// </summary>
    public class LabelPreviewViewModel(MainViewModel mainViewModel) : ObservableObject
    {
        private gtin _gtin = new();
        private DesignerViewModel _designerViewModel = new();

        public gtin GTIN
        {
            get => _gtin;
            set => Set(ref _gtin, value);
        }
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
            model.LabelingSession.GTIN = GTIN;
            mainViewModel.CurrentViewModel = model;
        }
        public void GetBack() => mainViewModel.CurrentViewModel = mainViewModel.GetViewModel<ProductsViewModel>();
    }
}
