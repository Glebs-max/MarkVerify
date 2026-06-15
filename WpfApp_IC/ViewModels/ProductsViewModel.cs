using LabelDesigner.Services;
using Microsoft.EntityFrameworkCore;
using Observable;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
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
            try
            {
                await using var db = await dbContextFactory.CreateDbContextAsync();
                List<gtin> gtins = await db.gtins.AsNoTracking().ToListAsync();
                GTINs.Clear();
                foreach (gtin gtin in gtins) GTINs.Add(gtin);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Возникла ошибка при обращении к базе данных. Проверьте соединение с сервером.\n\nException message:\n\n{ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        public void LoadLabel(gtin? product)
        {
            if (product == null || product.CountAviable <= 0)
                return;

            labelingSession.GTIN = product;

            LabelPreviewViewModel model = mainViewModel.GetViewModel<LabelPreviewViewModel>();
            model.DesignerViewModel = DesignerService.LoadLabel(Path.Combine(AppContext.BaseDirectory, $"LabelTemplates/{GetLabelTemplate(product)}")) ?? new();
            mainViewModel.CurrentViewModel = model;
        }
        public void GetBack() => mainViewModel.CurrentViewModel = mainViewModel.GetViewModel<HomeViewModel>();

        private static string GetLabelTemplate(gtin product)
        {
            return product.GtinId switch
            {
                04630007402529 => "Horizontal.xml",
                04630007402536 => "Horizontal.xml",
                04630007403113 => "Horizontal.xml",
                04630007402987 => "Vertical.xml",
                04630007402994 => "Vertical.xml",
                04630007403007 => "Vertical.xml",
                04630007403014 => "Vertical.xml",
                04630007402550 => "Set.xml",
                04630007403120 => "Set.xml",
                _ => "default.xml",
            };
        }
    }
}
