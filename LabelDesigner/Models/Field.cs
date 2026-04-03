using LabelDesigner.Adorners;
using LabelDesigner.Services;
using System.Windows;
using System.Windows.Controls;

namespace LabelDesigner.Models
{
    public enum DataType
    {
        Fixed,
        Input,
        Current,
        Calculated,
        Database
    }

    public abstract class Field : DesignerItem
    {
        private DataType _dataType;
        private string _name = string.Empty;
        private bool _keepAspectRatio = true, _inverted;
        private Size _initialSize;

        public Field()
        {
            MinWidth = MinHeight = UnitsService.FromMM(1);
            InitialSize = new(W, H);

            PropertyChanged += (s, e) =>
            {
                if (!"X,Y,W,H,Transparent".Split(',').Contains(e.PropertyName ?? ""))
                    Visual.OpacityMask = Transparent ? Visual.AlphaMask : null;
            };
            Visual.SizeChanged += (s, e) => Visual.OpacityMask = Transparent ? Visual.AlphaMask : null;
        }

        public new FieldAdorner Adorner => (FieldAdorner)base.Adorner;

        public abstract string FieldType { get; }
        public DataType DataType
        {
            get => _dataType;
            set => Set(ref _dataType, value);
        }
        public string FieldName
        {
            get => _name;
            set => Set(ref _name, value);
        }
        public Size InitialSize
        {
            get => _initialSize;
            set => Set(ref _initialSize, value);
        }
        public double X
        {
            get => Canvas.GetLeft(Visual);
            set => Set(() => Canvas.SetLeft(Visual, value));
        }
        public double Y
        {
            get => Canvas.GetTop(Visual);
            set => Set(() => Canvas.SetTop(Visual, value));
        }
        public int ZIndex
        {
            get => Panel.GetZIndex(Visual);
            set => Set(() => Panel.SetZIndex(Visual, value));
        }
        public int Orientation
        {
            get => (int)Visual.Rotation.Angle;
            set => Set(() =>
            {
                Visual.Rotation.Angle = value;
                MeasureArrangeVisual();
            });
        }
        public bool MirrorX
        {
            get => Visual.Scale.ScaleX < 0;
            set => Set(() =>
            {
                if (MirrorX != value)
                    Visual.Scale.ScaleX *= -1;
            });
        }
        public bool MirrorY
        {
            get => Visual.Scale.ScaleY < 0;
            set => Set(() =>
            {
                if (MirrorY != value)
                    Visual.Scale.ScaleY *= -1;
            });
        }
        public bool Inverted
        {
            get => _inverted;
            set => Set(ref _inverted, value, InvertField);
        }
        public bool Transparent
        {
            get => Visual.OpacityMask != null;
            set => Set(() => Visual.OpacityMask = value ? Visual.AlphaMask : null);
        }
        public bool KeepAspectRatio
        {
            get => _keepAspectRatio;
            set => Set(ref _keepAspectRatio, value, () =>
            {
                if (value)
                {
                    if (W / H > AspectRatio)
                        H = W / AspectRatio;
                    else
                        W = H * AspectRatio;
                }
            });
        }
        public double W
        {
            get => Visual.Width;
            set => Set(() =>
            {
                if (value < MinWidth || value == Visual.Width)
                    return;

                Visual.Width = value;
                Visual.Scale.ScaleX = (Horizontal ? W : H) / InitialSize.Width * (MirrorX ? -1 : 1);

                if (KeepAspectRatio)
                    H = Visual.Width / AspectRatio;
            });
        }
        public double H
        {
            get => Visual.Height;
            set => Set(() =>
            {
                if (value < MinHeight || value == Visual.Height)
                    return;

                Visual.Height = value;
                Visual.Scale.ScaleY = (Horizontal ? H : W) / InitialSize.Height * (MirrorY ? -1 : 1);

                if (KeepAspectRatio)
                    W = Visual.Height * AspectRatio;
            });
        }

        private bool Horizontal => Orientation == 0 || Orientation == 180;
        private double AspectRatio => Horizontal ? InitialSize.Width / InitialSize.Height : InitialSize.Height / InitialSize.Width;

        protected abstract void InvertField();
        protected override FieldAdorner InitializeAdorner() => new(this);
        protected void MeasureArrangeVisual()
        {
            Visual.MeasureArrangeVisual();
            W = Visual.Width;
            H = Visual.Height;
        }
    }
}