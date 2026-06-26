using Observable;
using System.Windows.Media;

namespace WpfApp_IC.Models.VirtualKeyboard
{
    public enum KeyboardCulture
    {
        ENG,
        RUS
    }

    public class KeyboardSymbolKey(char baseCulture, char altCulture, char baseShift, char altShift, string? keyDisplay = null) : ObservableObject
    {
        private char? _keyValue;
        private string? _keyDisplay;
        private Brush _background = Brushes.White;
        private double _width = 1, _height = 1;
        private KeyboardCulture _keyboardCulture;
        private bool _capsLock, _shift;

        public double Width
        {
            get => _width;
            set => Set(ref _width, value * 61);
        }
        public double Height
        {
            get => _height;
            set => Set(ref _height, value * 61);
        }
        public Brush Background
        {
            get => _background;
            set => Set(ref _background, value);
        }
        public char KeyValue
        {
            get => _keyValue ?? baseCulture;
            private set => Set(ref _keyValue, value);
        }
        public string KeyDisplay
        {
            get => keyDisplay ?? KeyValue.ToString();
            private set => Set(ref _keyDisplay, value);
        }

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
        public bool CapsLock
        {
            set
            {
                _capsLock = value;
                UpdateKeyValue();
            }
        }

        private void UpdateKeyValue()
        {
            switch (_keyboardCulture)
            {
                case KeyboardCulture.ENG:
                    KeyValue = _capsLock ^ _shift ? char.ToUpper(_shift ? baseShift : baseCulture) : _shift ? baseShift : baseCulture;
                    break;
                case KeyboardCulture.RUS:
                    KeyValue = _capsLock ^ _shift ? char.ToUpper(_shift ? altShift : altCulture) : _shift ? altShift : altCulture;
                    break;
            }

            if (keyDisplay == null)
                KeyDisplay = KeyValue.ToString();
        }
    }
}
