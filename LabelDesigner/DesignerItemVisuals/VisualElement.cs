using LabelDesigner.Services;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace LabelDesigner.DesignerItemVisuals
{
    public abstract class VisualElement : ContentControl
    {
        public VisualElement()
        {
            base.Content = Content;

            Content.LayoutTransform = new TransformGroup();
            (Content.LayoutTransform as TransformGroup)?.Children.Add(new ScaleTransform(1, 1));
            (Content.LayoutTransform as TransformGroup)?.Children.Add(new RotateTransform(0));

            MeasureArrangeVisual();
        }

        public new abstract FrameworkElement Content { get; }

        public ScaleTransform Scale => (Content.LayoutTransform as TransformGroup ?? new()).Children.OfType<ScaleTransform>().SingleOrDefault() ?? new(1, 1);
        public RotateTransform Rotation => (Content.LayoutTransform as TransformGroup ?? new()).Children.OfType<RotateTransform>().SingleOrDefault() ?? new(0);
        public WriteableBitmap Bitmap => BitmapService.GetBitmap(Content, 300);
        public ImageBrush AlphaMask => BitmapService.GetAlphaMask(Bitmap);

        public void MeasureArrangeVisual()
        {
            Content.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            Content.Arrange(new Rect(Content.DesiredSize));
            Width = Content.DesiredSize.Width;
            Height = Content.DesiredSize.Height;
        }
    }
}
