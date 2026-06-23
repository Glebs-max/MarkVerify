using System.Windows.Controls;
using System.Windows.Input;
using WpfApp_IC.Models;

namespace WpfApp_IC.Controls
{
    public partial class VirtualKey : UserControl
    {
        public VirtualKey()
        {
            InitializeComponent();
        }

        private KeyboardKey VM => (KeyboardKey)DataContext;

        private void Key_Click(object sender, MouseButtonEventArgs e) => VM.PressAction?.Invoke();
    }
}
