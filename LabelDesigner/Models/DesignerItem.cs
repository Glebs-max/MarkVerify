using LabelDesigner.Adorners;
using LabelDesigner.DesignerItemVisuals;
using Observable;
using System.Xml.Serialization;

namespace LabelDesigner.Models
{
    [XmlInclude(typeof(Field))]
    [XmlInclude(typeof(LabelArea))]
    [XmlInclude(typeof(TextField))]
    [XmlInclude(typeof(ImageField))]
    [XmlInclude(typeof(DateField))]
    [XmlInclude(typeof(TimeField))]
    [XmlInclude(typeof(BarcodeField))]
    public abstract class DesignerItem : ObservableObject
    {
        public DesignerItem()
        {
            Visual = InitializeVisual();
            Adorner = InitializeAdorner();
        }

        public VisualElement Visual { get; }
        public DesignerItemAdorner Adorner { get; }

        public double MinWidth
        {
            get => Visual.MinWidth;
            set => Set(() => Visual.MinWidth = value);
        }
        public double MinHeight
        {
            get => Visual.MinHeight;
            set => Set(() => Visual.MinHeight = value);
        }

        protected abstract VisualElement InitializeVisual();
        protected abstract DesignerItemAdorner InitializeAdorner();
    }
}
