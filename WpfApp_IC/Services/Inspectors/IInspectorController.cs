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
        string? ExpectedCode { get; set; }
        int RejectDelayMs { get; set; }

        event Action<int>? SignalChanged;
        event Action<DataMatrixResult>? DataMatrixRead;
        event Action<string>? ErrorOccurred;
        event Action<BitmapSource>? FrameReceived;
        event Action<string, string?, bool>? CodeChecked;

        void Start();
        void Stop();
    }
}
