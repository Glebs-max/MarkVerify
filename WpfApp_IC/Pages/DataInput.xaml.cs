using LabelDesigner;
using LabelDesigner.Models;
using System.Windows.Controls;

namespace WpfApp_IC.Pages
{
    public partial class DataInput : UserControl
    {
        public DataInput()
        {
            InitializeComponent();

            DataContextChanged += (s, e) =>
            {
                if (DataContext == null)
                    return;

                foreach (Field field in VM.Fields.Where(f => f.DataType == DataType.Input))
                {
                    LabelParameters.Children.Add(new ParameterInput(field));
                }
            };
        }

        private DesignerViewModel VM => (DesignerViewModel)DataContext;
    }
}
