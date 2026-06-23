using WpfApp_IC.ViewModels;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows;
using WpfApp_IC.Models.DbContext;

namespace WpfApp_IC.Views
{
    public partial class Products : UserControl
    {
        public Products()
        {
            InitializeComponent();

            ProductsTable.LoadingRow += (s, e) => e.Row.Header = (e.Row.GetIndex() + 1).ToString();
            ProductsTable.MouseDoubleClick += (s, e) => LoadLabel();
            ProductsTable.PreviewKeyDown += (s, e) =>
            {
                if (e.Key == Key.Enter)
                    LoadLabel();
            };

            Loaded += async (s, e) => await VM.LoadProductsAsync();
        }

        private ProductsViewModel VM => (ProductsViewModel)DataContext;

        private void LoadLabel() => VM.LoadLabel(ProductsTable.SelectedItem as gtin);
        private void GetBack_Click(object sender, RoutedEventArgs e) => VM.GetBack();
    }
}
