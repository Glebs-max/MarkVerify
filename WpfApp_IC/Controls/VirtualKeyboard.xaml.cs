using System.Globalization;
using System.Windows.Controls;
using System.Windows.Input;
using WpfApp_IC.Models.VirtualKeyboard;

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
        private readonly StackPanel[] _rows = new StackPanel[5];
        private readonly List<KeyboardSymbolKey> _keys = [];

        public VirtualKeyboard()
        {
            InitializeComponent();
            InitializeRows();
            InitializeSymbolKeys();
            InitializeSpecialKeys();
        }

        private void InitializeRows()
        {
            for (int i = 0; i < 5; i++)
            {
                _rows[i] = new() { Margin = new(1), Orientation = Orientation.Horizontal };
                KeyboardBase.Children.Add(_rows[i]);
            }
        }
        private void InitializeSymbolKeys()
        {
            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < _eng[i].Length; j++)
                {
                    KeyboardSymbolKey key = new(_eng[i][j], _rus[i][j], _shiftENG[i][j], _shiftRUS[i][j])
                    {
                        Width = 1,
                        Height = 1
                    };
                    _rows[i].Children.Add(new VirtualSymbolKey()
                    {
                        DataContext = key
                    });
                    _keys.Add(key);
                }
            }

            KeyboardSymbolKey space = new(' ', ' ', ' ', ' ', "␣")
            {
                Width = 9,
                Height = 1
            };
            _rows[4].Children.Add(new VirtualSymbolKey()
            {
                DataContext = space
            });
            _keys.Add(space);
        }
        private void InitializeSpecialKeys()
        {
            KeyboardSpecialKey capsLock = new()
            {
                KeyDisplay = "CapsLock",
                Width = 2,
                Height = 1
            };
            capsLock.KeyAction = () =>
            {
                if (!capsLock.IsPressed)
                {
                    capsLock.IsPressed = true;
                    foreach (KeyboardSymbolKey key in _keys)
                        key.CapsLock = true;
                }
                else
                {
                    capsLock.IsPressed = false;
                    foreach (KeyboardSymbolKey key in _keys)
                        key.CapsLock = false;
                }
            };
            _rows[2].Children.Add(new VirtualSpecialKey()
            {
                DataContext = capsLock
            });

            KeyboardSpecialKey shift = new()
            {
                KeyDisplay = "Shift",
                Width = 3,
                Height = 1
            };
            shift.KeyAction = () =>
            {
                if (!shift.IsPressed)
                {
                    shift.IsPressed = true;
                    foreach (KeyboardSymbolKey key in _keys)
                        key.Shift = true;
                }
                else
                {
                    shift.IsPressed = false;
                    foreach (KeyboardSymbolKey key in _keys)
                        key.Shift = false;
                }
            };
            _rows[3].Children.Add(new VirtualSpecialKey()
            {
                DataContext = shift
            });

            KeyboardSpecialKey culture = new()
            {
                KeyDisplay = "ENG/RUS",
                Width = 2,
                Height = 1
            };
            culture.KeyAction = () =>
            {
                if (culture.KeyboardCulture == KeyboardCulture.ENG)
                {
                    culture.KeyboardCulture = KeyboardCulture.RUS;
                    foreach (KeyboardSymbolKey key in _keys)
                        key.KeyboardCulture = KeyboardCulture.RUS;
                }
                else
                {
                    culture.KeyboardCulture = KeyboardCulture.ENG;
                    foreach (KeyboardSymbolKey key in _keys)
                        key.KeyboardCulture = KeyboardCulture.ENG;
                }
            };
            _rows[4].Children.Add(new VirtualSpecialKey()
            {
                DataContext = culture
            });

            KeyboardSpecialKey backspace = new()
            {
                KeyDisplay = "⬅",
                KeyAction = () =>
                {
                    if (Keyboard.FocusedElement is TextBox input)
                    {
                        int selectionStart = input.SelectionStart;
                        int selectionLength = input.SelectionLength;

                        if (selectionLength > 0)
                        {
                            input.Text = input.Text.Remove(selectionStart, selectionLength);
                            input.SelectionStart = selectionStart;
                        }
                        else if (selectionStart > 0)
                        {
                            input.Text = input.Text.Remove(selectionStart - 1, 1);
                            input.SelectionStart = selectionStart - 1;
                        }
                    }
                },
                Width = 2,
                Height = 1,
            };
            _rows[4].Children.Add(new VirtualSpecialKey()
            {
                DataContext = backspace
            });
        }
    }
}
