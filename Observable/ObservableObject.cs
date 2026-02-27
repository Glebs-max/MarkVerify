using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Observable
{
    public abstract class ObservableObject : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        protected void Set<T>(ref T field, T value, Action? action = null, [CallerMemberName] string? prop = null)
        {
            if (Equals(field, value))
                return;

            field = value;
            action?.Invoke();
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }
        protected void Set(Action action, [CallerMemberName] string? prop = null)
        {
            action.Invoke();
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }

        protected void OnPropertyChanged([CallerMemberName] string? prop = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
    }
}
