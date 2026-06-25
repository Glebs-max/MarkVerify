using LabelDesigner.Models;
using LabelDesigner.Services;
using Observable;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
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
        public bool Preview { get; set; }
        public ObservableCollection<Field> Fields { get; } = [];
        public BarcodeField? DataMatrix => Fields.OfType<BarcodeField>().FirstOrDefault(f => f.DataType == DataType.Database);

        public string ConvertToZpl(double dpi = 300) => BitmapService.WPFToZpl(LabelModel, dpi, LabelArea.W, LabelArea.H);
        public void CreateField(double x = 0, double y = 0)
        {
            if (SelectedTool == ToolboxItemType.None)
            {
                SelectedField = null;
                return;
            }

            Field? field = SelectedTool switch
            {
                ToolboxItemType.Text => new TextField(),
                ToolboxItemType.Image => new ImageField(),
                ToolboxItemType.Date => new DateField(),
                ToolboxItemType.Time => new TimeField(),
                ToolboxItemType.Barcode => new BarcodeField(),
                ToolboxItemType.None => null,
                _ => null
            };

            if (field != null)
            {
                field.Visual.Loaded += (s, e) =>
                {
                    field.FieldName = $"Field.{Fields.Count(f => f.FieldName.StartsWith("Field.")):D2}";
                    field.KeepAspectRatio = true;
                    field.X = x / Scale;
                    field.Y = y / Scale;
                    field.H = LabelArea.H * 0.5 / Scale;
                    field.ZIndex = Fields.Count;
                };
                field.Visual.MouseLeftButtonDown += (s, e) =>
                {
                    if (SelectedTool != ToolboxItemType.None)
                        return;

                    SelectedField = field;
                    e.Handled = true;
                    Mouse.OverrideCursor = null;
                };
                field.Visual.MouseEnter += (s, e) =>
                {
                    if (SelectedTool == ToolboxItemType.None)
                        Mouse.OverrideCursor = Cursors.Hand;
                };
                field.Visual.MouseLeave += (s, e) =>
                {
                    if (SelectedTool == ToolboxItemType.None)
                        Mouse.OverrideCursor = null;
                };

                Fields.Add(field);
                LabelModel.Children.Add(field.Visual);
                LabelModel.Children.Add(field.Adorner);
                SelectedTool = ToolboxItemType.None;
                SelectedField = field;
            }
        }
        public void RemoveField(Field? field)
        {
            if (field != null && Fields.FirstOrDefault(field) != null)
            {
                foreach (Field f in Fields.Where(f => f.ZIndex > field.ZIndex))
                    f.ZIndex--;

                LabelModel.Children.Remove(field.Visual);
                LabelModel.Children.Remove(field.Adorner);
                Fields.Remove(field);
            }
        }
        public void MoveFieldUp()
        {
            if (SelectedField != null)
            {
                if (Fields.FirstOrDefault(f => f.ZIndex == SelectedField.ZIndex + 1) is Field field)
                {
                    SelectedField.ZIndex++;
                    field.ZIndex--;
                }
            }
        }
        public void MoveFieldDown()
        {
            if (SelectedField != null)
            {
                if (Fields.FirstOrDefault(f => f.ZIndex == SelectedField.ZIndex - 1) is Field field)
                {
                    SelectedField.ZIndex--;
                    field.ZIndex++;
                }
            }
        }
    }
}