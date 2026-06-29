using System;
using System.Windows.Media.Imaging;

namespace WpfApp_IC.Services.Inspectors
{
    /// <summary>
    /// Главный контроллер инспекции.
    /// Работает с одним ожидаемым кодом.
    /// Подписать поля
    /// </summary>
    public interface IInspectorController
    {
        int RejectDelay { get; set; }
        int MotionFilterInterval { get; set; }
        int SensorPollInterval { get; set; }
        string CameraIp { get; set; }
        string ModbusIp { get; set; }
        int ModbusPort { get; set; }
        int SignalCoil { get; set; } 
        int RejectCoil { get; set; }

        event Action<DataMatrixResult>? DataMatrixRead;
        event Action<string?, BitmapSource>? FrameReceived;
        event Action<string, bool>? CodeChecked;

        void Start();
        void Stop();
        DataMatrixResult? TriggerCamera();
    }
}
