using Observable;
using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using WpfApp_IC.Services.Inspectors;
using WpfApp_IC.Services.Log;

namespace WpfApp_IC.ViewModels
{
    public class CameraViewModel : ObservableObject
    {
        protected readonly IInspectorController _controller;
        protected readonly ILogService _log;

        // Публичное свойство для биндинга в XAML
        public ILogService LogService => _log;
        // Коллекция логов, к которой привязывается UI
        public ObservableCollection<LogEntry> Entries => _log.Entries;

        public CameraViewModel(IInspectorController controller, ILogService log)
        {
            _controller = controller;
            _log = log;

            // === Подписки на события инспектора ===
            _controller.ErrorOccurred += err =>
            {
                _log.Error(err);
            };
            _controller.SignalChanged += s =>
            {
                _log.Info($"Сигнал датчика: {s}");
            };
            _controller.CodeChecked += (actual, expected, ok) =>
            {
                string result = ok ? "OK" : "BRK";
                _log.Info($"Проверка: [{actual}] → {result}");

                DispatchUI(() =>
                {
                    DataMatrix = actual;
                    DataMatrixBrush = ok ? Brushes.LimeGreen : Brushes.Red;
                });
            };
            _controller.DataMatrixRead += dm =>
            {
                DispatchUI(() =>
                {
                    DataMatrix = dm.Normalized;
                    DataMatrixBrush = Brushes.LimeGreen;
                });
            };
            _controller.FrameReceived += (dm, frame) =>
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

        public void Start()
        {
            try
            {
                _controller.Start();
                AddLog("Инспекция запущена");
                IsRunning = true;
            }
            catch (Exception ex)
            {
                AddLog("Ошибка запуска: " + ex.Message);
            }
        }
        public void Stop()
        {
            _controller.Stop();
            AddLog("Инспекция остановлена");
            IsRunning = false;
        }
        // === Удобный метод для записи в лог ===
        public void AddLog(string msg) => _log.Info(msg);

        // === UI dispatcher ===
        private static void DispatchUI(Action action)
        {
            if (Application.Current.Dispatcher.CheckAccess())
                action();
            else
                Application.Current.Dispatcher.BeginInvoke(action);
        }

        public void TriggerManual()
        {
            if (!IsRunning)
            {
                AddLog("Инспекция не запущена.");
                return;
            }
            _controller.TriggerManual();
        }
    }
}
