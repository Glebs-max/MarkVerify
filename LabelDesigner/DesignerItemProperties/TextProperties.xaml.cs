using System.Windows.Controls;
using System.Windows.Media;

namespace LabelDesigner.DesignerItemProperties
{
    public partial class TextProperties : UserControl
    {
        public TextProperties()
        {
            InitializeComponent();
            LoadFonts();
        }

        private void LoadFonts()
        {
            foreach (FontFamily font in (List<FontFamily>)[.. Fonts.SystemFontFamilies.OrderBy(f => f.Source)])
            {
                FontNameBox.Items.Add(new ComboBoxItem()
                {
                    Content = font.Source,
                    Tag = font.Source,
                    FontFamily = font,
                });
            }
        }
    }
}
