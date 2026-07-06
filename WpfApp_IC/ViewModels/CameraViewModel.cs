using Observable;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using WpfApp_IC.Models.Devices;
using WpfApp_IC.Services.Inspectors;

namespace WpfApp_IC.ViewModels
{
    public class CameraViewModel : ObservableObject
    {
        private readonly InspectorController _controller;
        private readonly HikrobotCamera _camera;

        public CameraViewModel(InspectorController controller, HikrobotCamera camera)
        {
            _controller = controller;
            _camera = camera;

            _controller.CodeChecked += (actual, ok) =>
            {
                DispatchUI(() =>
                {
                    DataMatrix = actual;
                    DataMatrixBrush = ok ? Brushes.LimeGreen : Brushes.Red;
                });
            };
            _camera.FrameReceived += (frame) =>
            {
                if (frame != null)
                    DispatchUI(() => Frame = frame);
            };
        }

        // === Свойства UI ===

        private BitmapSource? _frame;
        public BitmapSource? Frame
        {
            get => _frame;
            set => Set(ref _frame, value);
        }

        private string _dataMatrix = "";
        public string DataMatrix
        {
            get => _dataMatrix;
            set => Set(ref _dataMatrix, value);
        }

        private bool _isRunning;
        public bool IsRunning
        {
            get => _isRunning;
            set => Set(ref _isRunning, value);
        }

        private Brush _dataMatrixBrush = Brushes.Black;
        public Brush DataMatrixBrush
        {
            get => _dataMatrixBrush;
            set => Set(ref _dataMatrixBrush, value);
        }

        public void Start()
        {
            _controller.Start();
            IsRunning = true;
        }
        public void Stop()
        {
            _controller.Stop();
            IsRunning = false;
        }
        public void TriggerManual()
        {
            if (IsRunning)
                _camera.TriggerSnapshot();
        }

        // === UI dispatcher ===
        private static void DispatchUI(Action action)
        {
            if (Application.Current.Dispatcher.CheckAccess())
                action();
            else
                Application.Current.Dispatcher.BeginInvoke(action);
        }
    }
}
