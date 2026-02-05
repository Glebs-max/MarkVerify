using LabelDesigner.Models;
using LabelDesigner.Services;
using Observable;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Xml.Serialization;

namespace LabelDesigner
{
    public class DesignerViewModel : ObservableObject
    {
        private double _scale;
        private LabelArea _labelArea;
        private Field? _selectedField;
        private DesignerItem _propertiesTarget;
        private ToolboxItemType _selectedTool;

        public DesignerViewModel()
        {
            _scale = 4.0;
            _propertiesTarget = _labelArea = new();
            _selectedTool = ToolboxItemType.None;

            TimerService.DesignerCanvasTimer.Start();
        }

        public double Scale
        {
            get => _scale;
            set => Set(ref _scale, value);
        }
        [XmlIgnore]
        public DesignerItem PropertiesTarget
        {
            get => _propertiesTarget;
            set => Set(ref _propertiesTarget, value);
        }
        public LabelArea LabelArea
        {
            get => _labelArea;
            set
            {
                PropertiesTarget = value;
                Set(ref _labelArea, value);
            }
        }
        [XmlIgnore]
        public Field? SelectedField
        {
            get => _selectedField;
            set
            {
                if (_selectedField != null)
                    _selectedField.Adorner.Visibility = Visibility.Hidden;

                LabelArea.Adorner.Visibility = Visibility.Visible;

                if (value != null)
                {
                    value.Adorner.Visibility = Visibility.Visible;
                    LabelArea.Adorner.Visibility = Visibility.Hidden;
                }

                PropertiesTarget = value != null ? value : LabelArea;
                Set(ref _selectedField, value);
            }
        }
        [XmlIgnore]
        public ToolboxItemType SelectedTool
        {
            get => _selectedTool;
            set
            {
                SelectedField = null;
                Set(ref _selectedTool, value);
            }
        }
        [XmlIgnore]
        public Canvas LabelModel { get; set; } = new();
        public ObservableCollection<Field> Fields { get; } = [];

        public string ConvertToZpl(double dpi = 300) => BitmapService.ConvertToZpl(BitmapService.Monochrome(BitmapService.GetBitmap(LabelModel, dpi, new(LabelArea.W, LabelArea.H))));
    }
}