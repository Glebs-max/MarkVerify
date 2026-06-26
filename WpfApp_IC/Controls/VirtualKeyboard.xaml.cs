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
            ['`','q','w','e','r','t','y','u','i','o','p','1','2','3'],
            ['a','s','d','f','g','h','j','k','l',';','\'','4','5','6'],
            ['z','x','c','v','b','n','m',',','.','[',']','/','\\','7','8','9'],
            ['-','=','0']
        ];
        private readonly char[][] _rus =
        [
            ['ё','й','ц','у','к','е','н','г','ш','щ','з','1','2','3'],
            ['ф','ы','в','а','п','р','о','л','д','ж','э','4','5','6'],
            ['я','ч','с','м','и','т','ь','б','ю','х','ъ','.','\\','7','8','9'],
            ['-','=','0']
        ];
        private readonly char[][] _shiftENG =
        [
            ['~','q','w','e','r','t','y','u','i','o','p','!','@','#'],
            ['a','s','d','f','g','h','j','k','l',':','"','$','%','^'],
            ['z','x','c','v','b','n','m','<','>','{','}','?','|','&','*','('],
            ['_','+',')']
        ];
        private readonly char[][] _shiftRUS =
        [
            ['ё','й','ц','у','к','е','н','г','ш','щ','з','!','"','№'],
            ['ф','ы','в','а','п','р','о','л','д','ж','э',';','%',':'],
            ['я','ч','с','м','и','т','ь','б','ю','х','ъ',',','/','?','*','('],
            ['_','+',')']
        ];
        private readonly StackPanel[] _rows = new StackPanel[4];
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
            for (int i = 0; i < _eng.Length; i++)
            {
                _rows[i] = new() { Margin = new(1), Orientation = Orientation.Horizontal };
                KeyboardBase.Children.Add(_rows[i]);
            }
        }
        private void InitializeSymbolKeys()
        {
            for (int i = 0; i < _eng.Length; i++)
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
                Width = 8,
                Height = 1
            };
            _rows[3].Children.Insert(0, new VirtualSymbolKey()
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
            _rows[1].Children.Insert(11, new VirtualSpecialKey()
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
            _rows[3].Children.Insert(0, new VirtualSpecialKey()
            {
                DataContext = shift
            });

            KeyboardSpecialKey culture = new()
            {
                KeyDisplay = "ENG",
                Width = 2,
                Height = 1
            };
            culture.KeyAction = () =>
            {
                if (culture.KeyboardCulture == KeyboardCulture.ENG)
                {
                    culture.KeyboardCulture = KeyboardCulture.RUS;
                    culture.KeyDisplay = "RUS";
                    foreach (KeyboardSymbolKey key in _keys)
                        key.KeyboardCulture = KeyboardCulture.RUS;
                }
                else
                {
                    culture.KeyboardCulture = KeyboardCulture.ENG;
                    culture.KeyDisplay = "ENG";
                    foreach (KeyboardSymbolKey key in _keys)
                        key.KeyboardCulture = KeyboardCulture.ENG;
                }
            };
            _rows[3].Children.Insert(1, new VirtualSpecialKey()
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
            _rows[0].Children.Insert(11, new VirtualSpecialKey()
            {
                DataContext = backspace
            });
        }
    }
}
