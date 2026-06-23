using System.Windows.Controls;
using System.Windows.Input;
using WpfApp_IC.Models;

namespace WpfApp_IC.Controls
{
    public partial class VirtualKeyboard : UserControl
    {
        private readonly char[][] _eng =
        [
            ['`','1','2','3','4','5','6','7','8','9','0','-','='],
            ['q','w','e','r','t','y','u','i','o','p','[',']','\\'],
            ['a','s','d','f','g','h','j','k','l',';','\''],
            ['z','x','c','v','b','n','m',',','.','/']
        ];
        private readonly char[][] _rus =
        [
            ['ё','1','2','3','4','5','6','7','8','9','0','-','='],
            ['й','ц','у','к','е','н','г','ш','щ','з','х','ъ','\\'],
            ['ф','ы','в','а','п','р','о','л','д','ж','э'],
            ['я','ч','с','м','и','т','ь','б','ю','.']
        ];
        private readonly char[][] _shiftENG =
        [
            ['~','!','@','#','$','%','^','&','*','(',')','_','+'],
            ['q','w','e','r','t','y','u','i','o','p','{','}','|'],
            ['a','s','d','f','g','h','j','k','l',':','"'],
            ['z','x','c','v','b','n','m','<','>','?']
        ];
        private readonly char[][] _shiftRUS =
        [
            ['ё','!','"','№',';','%',':','?','*','(',')','_','+'],
            ['й','ц','у','к','е','н','г','ш','щ','з','х','ъ','/'],
            ['ф','ы','в','а','п','р','о','л','д','ж','э'],
            ['я','ч','с','м','и','т','ь','б','ю',',']
        ];
        private readonly StackPanel[] _rows = new StackPanel[4];

        public VirtualKeyboard()
        {
            InitializeComponent();

            for (int i = 0; i < 4; i++)
            {
                _rows[i] = new() { Margin = new(0.4), Orientation = Orientation.Horizontal };
                for (int j = 0; j < _eng[i].Length; j++)
                {
                    KeyboardKey key = new(_eng[i][j], _rus[i][j], _shiftENG[i][j], _shiftRUS[i][j]);
                    key.PressAction = () =>
                    {
                        if (Keyboard.FocusedElement is TextBox input)
                        {
                            input.SelectedText = key.KeyValue.ToString();
                            input.SelectionStart += 1;
                            input.SelectionLength = 0;
                        }
                    };
                    _rows[i].Children.Add(new VirtualKey()
                    {
                        Margin = new(0.4),
                        DataContext = key
                    });
                }
                Base.Children.Add(_rows[i]);
            }

            KeyboardKey space = new('␣', '␣', '␣', '␣')
            {
                PressAction = () =>
                {
                    if (Keyboard.FocusedElement is TextBox input)
                    {
                        input.SelectedText = " ";
                        input.SelectionStart += 1;
                        input.SelectionLength = 0;
                    }
                }
            };
            Base.Children.Add(new VirtualKey()
            {
                Margin = new(0.4),
                DataContext = space
            });
        }
    }
}
