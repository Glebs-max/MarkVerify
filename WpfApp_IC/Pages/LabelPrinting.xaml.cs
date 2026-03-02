using System.Windows.Controls;
using System.Windows.Input;

namespace WpfApp_IC.Pages
{
    public partial class LabelPrinting : UserControl
    {
        public LabelPrinting()
        {
            InitializeComponent();
        }

        private void StatusBar_Click(object sender, MouseButtonEventArgs e) => (LabelPreview.Visibility, VideojetErrors.Visibility) = (VideojetErrors.Visibility, LabelPreview.Visibility);
    }
}