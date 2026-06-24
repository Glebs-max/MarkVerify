using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using WpfApp_IC.Models.VirtualKeyboard;

namespace WpfApp_IC.Controls
{
    public partial class VirtualSpecialKey : UserControl
    {
        public VirtualSpecialKey()
        {
            InitializeComponent();
        }

        private KeyboardSpecialKey VM => (KeyboardSpecialKey)DataContext;

        private void Key_Click(object sender, MouseButtonEventArgs e) => VM.KeyAction?.Invoke();
        private void Key_Hover(object sender, MouseEventArgs e)
        {
            if (!VM.IsPressed)
                VM.Background = Brushes.Orange;
        }
        private void Key_Leave(object sender, MouseEventArgs e)
        {
            if (!VM.IsPressed)
                VM.Background = Brushes.White;
        }
    }
}
