using LabelDesigner.Services;
using WpfApp_IC.Data;
using WpfApp_IC.Models;
using Microsoft.EntityFrameworkCore;
using Observable;
using System.Collections.ObjectModel;

namespace WpfApp_IC.ViewModels
{
    /// <summary>
    /// Модель просмотра и выбора продукции для печати
    /// </summary>
    public class ProductsViewModel(MainViewModel mainViewModel, AppDbContext db) : ObservableObject
    {
        public ObservableCollection<gtin> GTINs { get; } = [];

        public async Task LoadProductsAsync()
        {
            List<gtin> gtins = await db.gtins.AsNoTracking().ToListAsync();
            GTINs.Clear();
            foreach (var gtin in gtins) GTINs.Add(gtin);
        }
        public void LoadLabel(gtin? product)
        {
            if (product == null || product.CountAviable <= 0)
                return;

            LabelPreviewViewModel model = mainViewModel.GetViewModel<LabelPreviewViewModel>();
            //model.DesignerViewModel = DesignerService.LoadLabel(product.TemplateLabel ?? string.Empty) ?? new();
            model.DesignerViewModel = DesignerService.LoadLabel("label.xml") ?? new();
            model.GTIN = product;
            mainViewModel.CurrentViewModel = model;
        }
        public void GetBack() => mainViewModel.CurrentViewModel = mainViewModel.GetViewModel<HomeViewModel>();
    }
}
