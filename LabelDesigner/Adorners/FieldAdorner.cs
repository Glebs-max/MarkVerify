using LabelDesigner.Models;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;

namespace LabelDesigner.Adorners
{
    public class FieldAdorner : DesignerItemAdorner
    {
        private readonly Field _field;
        private readonly Border _border;
        private readonly Thumb _dragThumb, _topLeft, _topRight, _bottomLeft, _bottomRight;

        public FieldAdorner(Field field)
        {
            _field = field;

            AddElement(_border = BuildBorder());
            AddElement(_dragThumb = BuildDragThumb());
            AddElement(_topLeft = BuildResizeThumb(Cursors.SizeNWSE));
            AddElement(_topRight = BuildResizeThumb(Cursors.SizeNESW));
            AddElement(_bottomLeft = BuildResizeThumb(Cursors.SizeNESW));
            AddElement(_bottomRight = BuildResizeThumb(Cursors.SizeNWSE));

            _topLeft.Style = (Style)Application.Current.Resources["ResizeHandle"];
            _topRight.Style = (Style)Application.Current.Resources["ResizeHandle"];
            _bottomLeft.Style = (Style)Application.Current.Resources["ResizeHandle"];
            _bottomRight.Style = (Style)Application.Current.Resources["ResizeHandle"];

            BuildAdorner();

            _field.PropertyChanged += (s, e) => ArrangeAdorner();
            _dragThumb.DragDelta += HandleDrag;
            _topLeft.DragDelta += (s, e) => HandleResize(-1, -1, e);
            _topRight.DragDelta += (s, e) => HandleResize(+1, -1, e);
            _bottomLeft.DragDelta += (s, e) => HandleResize(-1, +1, e);
            _bottomRight.DragDelta += (s, e) => HandleResize(+1, +1, e);
        }
        
        protected override Border BuildBorder()
        {
            return new()
            {
                BorderBrush = Brushes.Orange,
                BorderThickness = new(BorderSize)
            };
        }
        protected override Thumb BuildDragThumb()
        {
            return new()
            {
                Cursor = Cursors.SizeAll,
                Opacity = 0
            };
        }
        protected override Thumb BuildResizeThumb(Cursor cursor)
        {
            return new()
            {
                Cursor = cursor,
                Width = ResizeHandleSize,
                Height = ResizeHandleSize,
            };
        }

        protected override void ArrangeAdorner()
        {
            _border.BorderThickness = new(BorderSize / Scale);
            _topLeft.Width = _topLeft.Height = ResizeHandleSize / Scale;
            _topRight.Width = _topRight.Height = ResizeHandleSize / Scale;
            _bottomLeft.Width = _bottomLeft.Height = ResizeHandleSize / Scale;
            _bottomRight.Width = _bottomRight.Height = ResizeHandleSize / Scale;

            double offset = _topLeft.Width / 2;
            double w = _field.W;
            double h = _field.H;

            Canvas.SetLeft(this, _field.X);
            Canvas.SetTop(this, _field.Y);

            _border.Arrange(new Rect(-BorderSize / Scale, -BorderSize / Scale, w + BorderSize / Scale * 2, h + BorderSize / Scale * 2));
            _dragThumb.Arrange(new Rect(0, 0, w, h));
            _topLeft.Arrange(new Rect(-offset, -offset, _topLeft.Width, _topLeft.Height));
            _topRight.Arrange(new Rect(w - offset, -offset, _topLeft.Width, _topRight.Height));
            _bottomLeft.Arrange(new Rect(-offset, h - offset, _bottomLeft.Width, _bottomLeft.Height));
            _bottomRight.Arrange(new Rect(w - offset, h - offset, _bottomRight.Width, _bottomRight.Height));
        }
        protected override void HandleDrag(object sender, DragDeltaEventArgs e)
        {
            double left = double.IsNaN(_field.X) ? 0 : _field.X;
            double top = double.IsNaN(_field.Y) ? 0 : _field.Y;

            _field.X = left + e.HorizontalChange;
            _field.Y = top + e.VerticalChange;
        }
        protected override void HandleResize(int dx, int dy, DragDeltaEventArgs e)
        {
            double left = double.IsNaN(_field.X) ? 0 : _field.X;
            double top = double.IsNaN(_field.Y) ? 0 : _field.Y;

            double aspectRatio = _field.W / _field.H;
            bool keepAspect = Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift) || _field.KeepAspectRatio;

            double newWidth = _field.W;
            double newHeight = _field.H;

            if (dx < 0)
            {
                newWidth = Math.Max(_field.MinWidth, _field.W - e.HorizontalChange);
                _field.X = left + _field.W - newWidth;
            }
            else if (dx > 0)
            {
                newWidth = Math.Max(_field.MinWidth, _field.W + e.HorizontalChange);
            }

            if (dy < 0)
            {
                newHeight = Math.Max(_field.MinHeight, _field.H - e.VerticalChange);
                _field.Y = top + _field.H - newHeight;
            }
            else if (dy > 0)
            {
                newHeight = Math.Max(_field.MinHeight, _field.H + e.VerticalChange);
            }

            if (keepAspect)
            {
                double scaleX = newWidth / _field.W;
                double scaleY = newHeight / _field.H;

                double scale = Math.Max(scaleX, scaleY);

                newWidth = _field.W * scale;
                newHeight = newWidth / aspectRatio;

                if (dx < 0)
                    _field.X = left + _field.W - newWidth;
                if (dy < 0)
                    _field.Y = top + _field.H - newHeight;
            }

            _field.W = newWidth;
            _field.H = newHeight;
        }
    }
}
