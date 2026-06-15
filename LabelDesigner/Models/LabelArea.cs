using LabelDesigner.Adorners;
using LabelDesigner.DesignerItemVisuals;
using LabelDesigner.Services;

namespace LabelDesigner.Models
{
    public class LabelArea : DesignerItem
    {
        public LabelArea()
        {
            MinWidth = MinHeight = UnitsService.FromMM(5);
            GridStepX = GridStepY = UnitsService.FromMM(5);
            ShowGrid = true;
        }

        public new LabelAreaVisual Visual => (LabelAreaVisual)base.Visual;
        public new LabelAreaAdorner Adorner => (LabelAreaAdorner)base.Adorner;

        public double W
        {
            get => Visual.Width;
            set
            {
                Set(() =>
                {
                    if (value < MinWidth)
                        return;
                    Visual.Width = value;
                });
            }
        }
        public double H
        {
            get => Visual.Height;
            set
            {
                Set(() =>
                {
                    if (value < MinHeight)
                        return;
                    Visual.Height = value;
                });
            }
        }
        public double GridStepX
        {
            get => Visual.GridStepX;
            set => Set(() => Visual.GridStepX = value);
        }
        public double GridStepY
        {
            get => Visual.GridStepY;
            set => Set(() => Visual.GridStepY = value);
        }
        public bool ShowGrid
        {
            get => Visual.Grid;
            set => Set(() => Visual.Grid = value);
        }

        protected override LabelAreaVisual InitializeVisual() => new(new(UnitsService.FromMM(25), UnitsService.FromMM(25)));
        protected override LabelAreaAdorner InitializeAdorner() => new(this);
    }
}
