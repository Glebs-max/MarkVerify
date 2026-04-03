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
                        VM.RemoveField();
                        break;
                }
            };
        }

        private DesignerViewModel VM => (DesignerViewModel)DataContext;

        private void Delete_Click(object sender, MouseButtonEventArgs e) => VM.RemoveField();
        private void Up_Click(object sender, MouseButtonEventArgs e) => VM.MoveFieldUp();
        private void Down_Click(object sender, MouseButtonEventArgs e) => VM.MoveFieldDown();
    }
}
