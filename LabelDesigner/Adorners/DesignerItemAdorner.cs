using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace LabelDesigner.Adorners
{
    public abstract class DesignerItemAdorner : ContentControl
    {
        private readonly Canvas _adorner = new();
        private double _resizeHandleSize = 10;
        private double _borderSize = 2;
        private double _scale = 1;

        public DesignerItemAdorner()
        {
            Visibility = Visibility.Hidden;
            Panel.SetZIndex(this, 100);

            Loaded += (s, e) => ArrangeAdorner();
            DataContextChanged += (s, e) =>
            {
                if (DataContext is DesignerViewModel model)
                {
                    Scale = model.Scale;
                    
                    ArrangeAdorner();

                    model.PropertyChanged += (s, e) =>
                    {
                        switch (e.PropertyName)
                        {
                            case nameof(model.Scale):
                                Scale = model.Scale;
                                break;
                        }
                    };
                }
            };
        }

        public double Scale
        {
            get => _scale;
            set
            {
                _scale = value;
                ArrangeAdorner();
            }
        }
        protected double ResizeHandleSize
        {
            get => _resizeHandleSize;
            set
            {
                _resizeHandleSize = value;
                ArrangeAdorner();
            }
        }
        protected double BorderSize
        {
            get => _borderSize;
            set
            {
                _borderSize = value;
                ArrangeAdorner();
            }
        }

        protected void AddElement(FrameworkElement element) => _adorner.Children.Add(element);
        protected void BuildAdorner() => Content = _adorner;
        protected abstract Border? BuildBorder();
        protected abstract Thumb BuildDragThumb();
        protected abstract Thumb BuildResizeThumb(Cursor cursor);
        protected abstract void ArrangeAdorner();
        protected abstract void HandleDrag(object sender, DragDeltaEventArgs e);
        protected abstract void HandleResize(int dx, int dy, DragDeltaEventArgs e);
    }
}
