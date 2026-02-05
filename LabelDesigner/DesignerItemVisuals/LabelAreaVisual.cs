using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;

namespace LabelDesigner.DesignerItemVisuals
{
    public class LabelAreaVisual(Size size) : VisualElement
    {
        private bool _grid;
        private double _gridStepX, _gridStepY;

        public override Rectangle Content { get; } = new()
        {
            Fill = null,
            Stroke = null,
            Width = size.Width,
            Height = size.Height,
            Stretch = Stretch.Fill
        };

        public new double Width
        {
            get => base.Width;
            set
            {
                base.Width = Content.Width = value;
                InvalidateVisual();
            }
        }
        public new double Height
        {
            get => base.Height;
            set
            {
                base.Height = Content.Height = value;
                InvalidateVisual();
            }
        }
        public double GridStepX
        {
            get => _gridStepX;
            set
            {
                _gridStepX = value;
                InvalidateVisual();
            }
        }
        public double GridStepY
        {
            get => _gridStepY;
            set
            {
                _gridStepY = value;
                InvalidateVisual();
            }
        }
        public bool Grid
        {
            get => _grid;
            set
            {
                _grid = value;
                InvalidateVisual();
            }
        }

        protected override void OnRender(DrawingContext dc)
        {
            base.OnRender(dc);
            dc.DrawRectangle(Brushes.White, null, new(new(Width, Height)));

            if (Grid)
            {
                Pen pen = new(Brushes.Gray, 0.5)
                {
                    DashStyle = DashStyles.Dash
                };

                for (double x = 0; x < Width; x += _gridStepX)
                    dc.DrawLine(pen, new Point(x, 0), new Point(x, Height));

                for (double y = 0; y < Height; y += _gridStepY)
                    dc.DrawLine(pen, new Point(0, y), new Point(Width, y));
            }
        }
    }
}
