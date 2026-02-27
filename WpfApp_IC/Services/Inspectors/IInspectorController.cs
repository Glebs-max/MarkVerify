using System;
using System.Windows.Media.Imaging;

namespace WpfApp_IC.Services.Inspectors
{
    /// <summary>
    /// Главный контроллер инспекции.
    /// Работает с одним ожидаемым кодом.
    /// Подписать поля
    /// </summary>
    public interface IInspectorController : IDisposable
    {
        int RejectDelayMs { get; set; }
        ulong CurrentGtinId { get; set; } 

        event Action<int>? SignalChanged;
        event Action<string>? ErrorOccurred;
        event Action<string?, BitmapSource>? FrameReceived;
        event Action<ValidationResult>? CodeValidated;

        void Start();
        void Stop();
    }
}
