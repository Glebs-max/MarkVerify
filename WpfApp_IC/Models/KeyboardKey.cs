using Observable;

namespace WpfApp_IC.Models
{
    public enum KeyboardCulture
    {
        ENG,
        RUS
    }

    public class KeyboardKey(char eng, char rus, char shiftEng, char shiftRus) : ObservableObject
    {
        private KeyboardCulture _keyboardCulture;
        private bool _capsLock, _shift;
        private char? _keyValue;

        public Action? PressAction { get; set; }
        public bool CapsLock { set => _capsLock = value; }
        public KeyboardCulture KeyboardCulture
        {
            set
            {
                _keyboardCulture = value;
                UpdateKeyValue();
            }
        }
        public bool Shift
        {
            set
            {
                _shift = value;
                UpdateKeyValue();
            }
        }
        public char KeyValue
        {
            get => _capsLock ^ _shift ? char.ToUpper(_keyValue ?? eng) : _keyValue ?? eng;
            private set => Set(ref _keyValue, value);
        }

        private void UpdateKeyValue()
        {
            switch (_keyboardCulture)
            {
                case KeyboardCulture.ENG:
                    KeyValue = _shift ? shiftEng : eng;
                    break;
                case KeyboardCulture.RUS:
                    KeyValue = _shift ? shiftRus : rus;
                    break;
            }
        }
    }
}
