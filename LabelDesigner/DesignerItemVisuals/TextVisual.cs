using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace LabelDesigner.DesignerItemVisuals
{
    public class TextVisual(string text) : VisualElement
    {
        public override TextBlock Content { get; } = new()
        {
            Text = text,
            FontFamily = new("Arial"),
            FontSize = 32,
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Center,
            Background = Brushes.White,
            Foreground = Brushes.Black
        };
    }
}
