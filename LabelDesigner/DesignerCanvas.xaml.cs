using LabelDesigner.Models;
using System.Windows.Controls;
using System.Windows.Input;

namespace LabelDesigner
{
    public partial class DesignerCanvas : UserControl
    {
        public DesignerCanvas()
        {
            InitializeComponent();

            MouseLeftButtonDown += (s, mouse) =>
            {
                Focus();

                if (VM.SelectedTool == ToolboxItemType.None)
                {
                    VM.SelectedField = null;
                    return;
                }

                Field? field = VM.SelectedTool switch
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
                        field.FieldName = $"Field.{VM.Fields.Count(f => f.FieldName.StartsWith("Field.")):D2}";
                        field.KeepAspectRatio = true;
                        field.X = mouse.GetPosition(this).X / VM.Scale;
                        field.Y = mouse.GetPosition(this).Y / VM.Scale;
                        field.H = VM.LabelArea.H * 0.5 / VM.Scale;
                    };

                    field.ZIndex = VM.Fields.Count;
                    Mouse.OverrideCursor = null;
                    
                    VM.Fields.Add(field);
                    VM.SelectedTool = ToolboxItemType.None;
                }
            };
            MouseRightButtonDown += (s, e) =>
            {
                Focus();
                VM.SelectedTool = ToolboxItemType.None;
                Mouse.OverrideCursor = null;
            };
            PreviewMouseWheel += (s, e) =>
            {
                if (Keyboard.Modifiers == ModifierKeys.Control)
                {
                    VM.Scale = Math.Clamp(VM.Scale + (e.Delta > 0 ? 0.05 : -0.05), 0.1, 10);
                    e.Handled = true;
                }
            };
            KeyDown += (s, e) =>
            {
                switch (e.Key)
                {
                    case Key.Delete:
                        if (VM.SelectedField != null)
                        {
                            if (CanvasArea.Children.Contains(VM.SelectedField.Adorner))
                                CanvasArea.Children.Remove(VM.SelectedField.Adorner);

                            if (CanvasArea.Children.Contains(VM.SelectedField.Visual))
                                CanvasArea.Children.Remove(VM.SelectedField.Visual);

                            VM.RemoveField();
                        }
                        break;
                }
            };
            DataContextChanged += (s, e) =>
            {
                CanvasArea.Children.Clear();

                if (DataContext is DesignerViewModel)
                {
                    VM.LabelModel = CanvasArea;
                    VM.SelectedField = null;

                    if (Preview)
                    {
                        VM.LabelArea.Grid = false;
                        CanvasArea.Children.Add(VM.LabelArea.Visual);

                        foreach (Field field in VM.Fields)
                        {
                            CanvasArea.Children.Add(field.Visual);
                        }
                    }
                    else
                    {
                        CanvasArea.Children.Add(VM.LabelArea.Visual);
                        CanvasArea.Children.Add(VM.LabelArea.Adorner);

                        foreach (Field field in VM.Fields)
                        {
                            field.Visual.MouseLeftButtonDown += (s, e) =>
                            {
                                if (VM.SelectedTool != ToolboxItemType.None)
                                    return;

                                VM.SelectedField = field;
                                e.Handled = true;
                                Mouse.OverrideCursor = null;
                            };
                            field.Visual.MouseEnter += (s, e) =>
                            {
                                if (VM.SelectedTool == ToolboxItemType.None)
                                    Mouse.OverrideCursor = Cursors.Hand;
                            };
                            field.Visual.MouseLeave += (s, e) =>
                            {
                                if (VM.SelectedTool == ToolboxItemType.None)
                                    Mouse.OverrideCursor = null;
                            };

                            CanvasArea.Children.Add(field.Visual);
                            CanvasArea.Children.Add(field.Adorner);
                        }

                        SetupDataContext();
                    }
                }
            };
        }

        public bool Preview { get; set; }

        private DesignerViewModel VM => (DesignerViewModel)DataContext;

        private void SetupDataContext()
        {
            VM.PropertyChanged += (s, e) =>
            {
                switch (e.PropertyName)
                {
                    case nameof(VM.LabelArea):
                        CanvasArea.Children.RemoveRange(0, 2);
                        CanvasArea.Children.Insert(0, VM.LabelArea.Visual);
                        CanvasArea.Children.Insert(1, VM.LabelArea.Adorner);
                        break;
                }
            };
            VM.Fields.CollectionChanged += (s, e) =>
            {
                if (e.NewItems != null)
                {
                    foreach (Field field in e.NewItems)
                    {
                        field.Visual.MouseLeftButtonDown += (s, e) =>
                        {
                            if (VM.SelectedTool != ToolboxItemType.None)
                                return;

                            VM.SelectedField = field;
                            e.Handled = true;
                            Mouse.OverrideCursor = null;
                        };
                        field.Visual.MouseEnter += (s, e) =>
                        {
                            if (VM.SelectedTool == ToolboxItemType.None)
                                Mouse.OverrideCursor = Cursors.Hand;
                        };
                        field.Visual.MouseLeave += (s, e) =>
                        {
                            if (VM.SelectedTool == ToolboxItemType.None)
                                Mouse.OverrideCursor = null;
                        };
                        field.Visual.Loaded += (s, e) =>
                        {
                            VM.SelectedField = field;
                        };

                        CanvasArea.Children.Add(field.Visual);
                        CanvasArea.Children.Add(field.Adorner);
                    }
                }

                if (e.OldItems != null)
                {
                    foreach (Field field in e.OldItems)
                    {
                        field.Visual.MouseLeftButtonDown -= (s, e) =>
                        {
                            if (VM.SelectedTool != ToolboxItemType.None)
                                return;

                            VM.SelectedField = field;
                            e.Handled = true;
                            Mouse.OverrideCursor = null;
                        };
                        field.Visual.MouseEnter -= (s, e) =>
                        {
                            if (VM.SelectedTool == ToolboxItemType.None)
                                Mouse.OverrideCursor = Cursors.Hand;
                        };
                        field.Visual.MouseLeave -= (s, e) =>
                        {
                            if (VM.SelectedTool == ToolboxItemType.None)
                                Mouse.OverrideCursor = null;
                        };

                        CanvasArea.Children.Remove(field.Visual);
                        CanvasArea.Children.Remove(field.Adorner);
                    }
                }
            };
        }
    }
}
