using Observable;
using System.Windows.Media;

namespace WpfApp_IC.Models.VirtualKeyboard
{
    public class KeyboardSpecialKey : ObservableObject
    {
        private string? _keyDisplay;
        private KeyboardCulture _keyboardCulture = KeyboardCulture.ENG;
        private Brush _background = Brushes.White;
        private double _width = 1, _height = 1;
        private bool _isPressed;

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
        public bool IsPressed
        {
            get => _isPressed;
            set => Set(ref _isPressed, value);
        }
        public Brush Background
        {
            get => _background;
            set => Set(ref _background, value);
        }
        public KeyboardCulture KeyboardCulture
        {
            get => _keyboardCulture;
            set => Set(ref _keyboardCulture, value);
        }
        public string? KeyDisplay
        {
            get => _keyDisplay;
            set => Set(ref _keyDisplay, value);
        }
        public Action? KeyAction { get; set; }
    }
}
