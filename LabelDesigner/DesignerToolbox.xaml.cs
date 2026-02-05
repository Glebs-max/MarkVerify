using LabelDesigner.CustomControls;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace LabelDesigner
{
    public enum ToolboxItemType
    {
        None,
        Text,
        Date,
        Time,
        Image,
        Barcode
    }

    public partial class DesignerToolbox : UserControl
    {
        public DesignerToolbox()
        {
            InitializeComponent();

            Loaded += (s, e) =>
            {
                foreach (IconTextButton btn in Toolbox.Children.OfType<IconTextButton>() ?? [])
                {
                    btn.Click += ToolButton_Click;
                    btn.MouseEnter += (s, e) =>
                    {
                        if (VM.SelectedTool == ToolboxItemType.None)
                            Mouse.OverrideCursor = Cursors.Hand;

                        btn.BorderBrush = Brushes.Orange;
                    };
                    btn.MouseLeave += (s, e) =>
                    {
                        if (VM.SelectedTool == ToolboxItemType.None)
                            Mouse.OverrideCursor = null;

                        btn.BorderBrush = Brushes.Transparent;
                    };
                }
            };
            MouseRightButtonDown += (s, e) =>
            {
                VM.SelectedTool = ToolboxItemType.None;
                Mouse.OverrideCursor = null;
            };
        }

        private DesignerViewModel VM => (DesignerViewModel)DataContext;

        private void ToolButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button b && Enum.TryParse(b.Tag.ToString(), out ToolboxItemType tool))
                VM.SelectedTool = tool;

            Mouse.OverrideCursor = Cursors.Cross;
        }
    }
}
