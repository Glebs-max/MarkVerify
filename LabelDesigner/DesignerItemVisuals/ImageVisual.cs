using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace LabelDesigner.DesignerItemVisuals
{
    public class ImageVisual(ImageSource source) : VisualElement
    {
        public override Image Content { get; } = new()
        {
            Stretch = Stretch.Fill,
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Top,
            Source = source
        };
    }
}
