using LabelDesigner.Services;
using Microsoft.EntityFrameworkCore;
using Observable;
using System.Collections.ObjectModel;
using System.IO;
using WpfApp_IC.Data;
using WpfApp_IC.Models;

namespace WpfApp_IC.ViewModels
{
    /// <summary>
    /// Модель просмотра и выбора продукции для печати
    /// </summary>
    public class ProductsViewModel(MainViewModel mainViewModel, LabelingSession labelingSession, IDbContextFactory<AppDbContext> dbContextFactory) : ObservableObject
    {
        public ObservableCollection<gtin> GTINs { get; } = [];

        public async Task LoadProductsAsync()
        {
            await using var db = await dbContextFactory.CreateDbContextAsync();
            List<gtin> gtins = await db.gtins.AsNoTracking().ToListAsync();
            GTINs.Clear();
            foreach (gtin gtin in gtins) GTINs.Add(gtin);
        }
        public void LoadLabel(gtin? product)
        {
            if (product == null || product.CountAviable <= 0)
                return;

            labelingSession.GTIN = product;

            LabelPreviewViewModel model = mainViewModel.GetViewModel<LabelPreviewViewModel>();
            model.DesignerViewModel = DesignerService.LoadLabel(Path.Combine(AppContext.BaseDirectory, "label.xml")) ?? new();
            mainViewModel.CurrentViewModel = model;
        }
        public void GetBack() => mainViewModel.CurrentViewModel = mainViewModel.GetViewModel<HomeViewModel>();
    }
}
