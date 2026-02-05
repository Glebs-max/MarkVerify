using WpfApp_IC.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace WpfApp_IC.Pages
{
    public partial class LabelPreview : UserControl
    {
        public LabelPreview()
        {
            InitializeComponent();
        }

        private LabelPreviewViewModel VM => (LabelPreviewViewModel)DataContext;

        private void PrintLabel_Click(object sender, RoutedEventArgs e) => VM.PrintLabel();
        private void GetBack_Click(object sender, RoutedEventArgs e) => VM.GetBack();
    }
}
