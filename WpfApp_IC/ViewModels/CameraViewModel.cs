using Observable;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using WpfApp_IC.Services.Inspectors;

namespace WpfApp_IC.ViewModels
{
    public class CameraViewModel : ObservableObject
    {
        private readonly IInspectorController _controller;

        public CameraViewModel(IInspectorController controller)
        {
            _controller = controller;

            ShowImage = true;
            ShowLog = true;

            _controller.FrameReceived += (dm, frame) =>
            {
                DispatchUI(() =>
                {
                    Frame = frame;
                    DataMatrix = dm ?? "-";
                });
            };

            _controller.ErrorOccurred += err =>
            {
                AddLog($"Ошибка: {err}");
            };

            _controller.SignalChanged += s =>
            {
                AddLog($"Сигнал: {s}");
            };

            _controller.CodeValidated += (result) =>
            {
                DispatchUI(() => DataMatrixBrush = result.IsOk ? Brushes.LimeGreen : Brushes.Red);
            };
        }

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

        public ObservableCollection<string> Log { get; } = new();

        private bool _showImage;
        public bool ShowImage
        {
            get => _showImage;
            set => Set(ref _showImage, value);
        }

        private bool _showLog;
        public bool ShowLog
        {
            get => _showLog;
            set => Set(ref _showLog, value);
        }

        public void AddLog(string msg)
        {
            DispatchUI(() => Log.Add(msg));
        }

        protected static void DispatchUI(Action action)
        {
            if (Application.Current.Dispatcher.CheckAccess())
                action();
            else
                Application.Current.Dispatcher.BeginInvoke(action);
        }
    }
}

