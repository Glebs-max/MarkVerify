using LabelDesigner;
using WpfApp_IC.Models;
using Observable;
using WpfApp_IC.Services.Inspectors;

namespace WpfApp_IC.ViewModels
{
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
            LabelPrintingViewModel model = mainViewModel.GetViewModel<LabelPrintingViewModel>();
            //уmodel.CameraBasicViewModel = new(mainViewModel.InspectorController);
            model.DesignerViewModel = DesignerViewModel;
            model.DesignerViewModel.Scale = 3.0;
            model.GTIN = GTIN;
            mainViewModel.CurrentViewModel = model;
        }
        public void GetBack() => mainViewModel.CurrentViewModel = mainViewModel.GetViewModel<ProductsViewModel>();
    }
}
