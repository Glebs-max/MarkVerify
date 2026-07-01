using LabelDesigner;
using Observable;

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
            LabelingViewModel model = mainViewModel.SetViewModel<LabelingViewModel>();
            model.LabelPrintingViewModel.DesignerViewModel = DesignerViewModel;

        }
        public void GetBack() => mainViewModel.SetViewModel<ProductsViewModel>();
    }
}
