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
        int SensorFilterCount { get; set; }
        int SensorPollIntervalMs { get; set; }
        int CameraDeviceIndex { get; set; }

        event Action<int>? SignalChanged;
        event Action<DataMatrixResult>? DataMatrixRead;
        event Action<string>? ErrorOccurred;
        event Action<string?, BitmapSource>? FrameReceived;
        event Action<string, string?, bool>? CodeChecked;
         event Action<ValidationResult>? CodeValidated;

        void Start();
        void Stop();
        void TriggerManual();
    }
}
