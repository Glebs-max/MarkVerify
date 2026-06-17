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
                VM.CreateField(mouse.GetPosition(this).X, mouse.GetPosition(this).Y);
                Mouse.OverrideCursor = null;
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
                            VM.RemoveField(VM.SelectedField);
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

                    if (VM.Preview)
                    {
                        VM.LabelArea.ShowGrid = false;
                        CanvasArea.Children.Add(VM.LabelArea.Visual);

                        foreach (Field field in VM.Fields)
                            CanvasArea.Children.Add(field.Visual);
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
                    }
                }
            };
        }

        private DesignerViewModel VM => (DesignerViewModel)DataContext;
    }
}
