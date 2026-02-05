using System.Windows.Controls;
using System.Windows.Input;

namespace LabelDesigner
{
    public partial class DesignerExplorer : UserControl
    {
        public DesignerExplorer()
        {
            InitializeComponent();

            KeyDown += (s, e) =>
            {
                switch (e.Key)
                {
                    case Key.Delete:
                        if (VM.SelectedField != null)
                            VM.Fields.Remove(VM.SelectedField);
                        break;
                }
            };
        }

        private DesignerViewModel VM => (DesignerViewModel)DataContext;
    }
}
