using LabelDesigner.Models;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;

namespace LabelDesigner.Adorners
{
    public class LabelAreaAdorner : DesignerItemAdorner
    {
        private readonly Thumb _right, _bottom;
        private readonly Border _border;
        private readonly LabelArea _labelArea;

        public LabelAreaAdorner(LabelArea labelArea)
        {
            _labelArea = labelArea;

            AddElement(_border = BuildBorder());
            AddElement(_right = BuildResizeThumb(Cursors.SizeWE));
            AddElement(_bottom = BuildResizeThumb(Cursors.SizeNS));
            
            _right.Style = (Style)Application.Current.Resources["ResizeHandle"];
            _bottom.Style = (Style)Application.Current.Resources["ResizeHandle"];

            BuildAdorner();

            BorderSize = 1.5;
            ResizeHandleSize = 13;

            _labelArea.PropertyChanged += (s, e) => ArrangeAdorner();
            _right.DragDelta += (s, e) => HandleResize(+1, -1, e);
            _bottom.DragDelta += (s, e) => HandleResize(-1, +1, e);
        }

        protected override Border BuildBorder()
        {
            return new()
            {
                BorderBrush = Brushes.Black,
                BorderThickness = new(0, 0, BorderSize, BorderSize)
            };
        }
        protected override Thumb BuildDragThumb()
        {
            throw new NotImplementedException();
        }
        protected override Thumb BuildResizeThumb(Cursor cursor)
        {
            return new()
            {
                Cursor = cursor,
                BorderThickness = new(0),
                BorderBrush = Brushes.Transparent,
                Background = Brushes.Black,
                Width = ResizeHandleSize,
                Height = ResizeHandleSize
            };
        }

        protected override void ArrangeAdorner()
        {
            _border.BorderThickness = new(0, 0, BorderSize / Scale, BorderSize / Scale);
            _right.Width = _right.Height = ResizeHandleSize / Scale;
            _bottom.Width = _bottom.Height = ResizeHandleSize / Scale;
            
            double offset = _right.Width / 2;
            double w = _labelArea.W;
            double h = _labelArea.H;

            _border.Arrange(new Rect(0, 0, w, h));
            _right.Arrange(new Rect(w - offset, h / 2 - offset, _right.Width, _right.Height));
            _bottom.Arrange(new Rect(w / 2 - offset, h - offset, _bottom.Width, _bottom.Height));
        }
        protected override void HandleDrag(object sender, DragDeltaEventArgs e)
        {
            throw new NotImplementedException();
        }
        protected override void HandleResize(int dx, int dy, DragDeltaEventArgs e)
        {
            if (dx > 0 && dy < 0)
            {
                _labelArea.W = Math.Max(_labelArea.MinWidth, _labelArea.W + e.HorizontalChange);
            }
            else if (dx < 0 && dy > 0)
            {
                _labelArea.H = Math.Max(_labelArea.MinHeight, _labelArea.H + e.VerticalChange);
            }
        }
    }
}
