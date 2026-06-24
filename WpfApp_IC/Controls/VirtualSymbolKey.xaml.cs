using System.Windows.Media;
using System.Windows.Controls;
using System.Windows.Input;
using WpfApp_IC.Models.VirtualKeyboard;

namespace WpfApp_IC.Controls
{
    public partial class VirtualSymbolKey : UserControl
    {
        public VirtualSymbolKey()
        {
            InitializeComponent();
        }

        private KeyboardSymbolKey VM => (KeyboardSymbolKey)DataContext;

        private void Key_Click(object sender, MouseButtonEventArgs e)
        {
            if (Keyboard.FocusedElement is TextBox input)
            {
                input.SelectedText = VM.KeyValue.ToString();
                input.SelectionStart += 1;
                input.SelectionLength = 0;
            }
        }
        private void Key_Hover(object sender, MouseEventArgs e) => VM.Background = Brushes.Orange;
        private void Key_Leave(object sender, MouseEventArgs e) => VM.Background = Brushes.White;
    }
}
