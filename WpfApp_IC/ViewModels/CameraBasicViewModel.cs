using Observable;
using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using WpfApp_IC.Services.Inspectors;
using WpfApp_IC.Services.SettingsD;

namespace WpfApp_IC.ViewModels
{
    public class CameraBasicViewModel : ObservableObject
    {
        private readonly IInspectorController _controller;
        private readonly AppSettings _settings;

        public CameraBasicViewModel(IInspectorController controller)
        {
            _controller = controller;
            _settings = SettingsService.Load();

            ShowImage = true;
            ShowLog = true;

            // === Подписки на события контроллера ===

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
                {
                    DispatchUI(() =>
                    {
                        Frame = frame;
                    });
                }
            };

            _controller.ErrorOccurred += err =>
            {
                AddLog($"Ошибка: {err}");
            };

            _controller.SignalChanged += s =>
            {
                AddLog($"Сигнал: {s}");
            };

            _controller.CodeChecked += (actual, expected, ok) =>
            {
                AddLog($"Проверка: ожидалось [{expected}], считано [{actual}], результат: {(ok ? "OK" : "BRK")}");

                DispatchUI(() =>
                {
                    DataMatrix = actual;
                    DataMatrixBrush = ok ? Brushes.LimeGreen : Brushes.Red;
                });
            };
        }

        // === Свойства ===

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

        private bool _isRunning;
        public bool IsRunning
        {
            get => _isRunning;
            set => Set(ref _isRunning, value);
        }

        // === Управление инспекцией ===

        public void StartStop()
        {
            if (!IsRunning)
            {
                try
                {
                    _controller.Start();
                    AddLog("Инспекция запущена");
                    IsRunning = true;
                }
                catch (Exception ex)
                {
                    AddLog("Ошибка запуска инспекции: " + ex.Message);
                }
            }
            else
            {
                _controller.Stop();
                AddLog("Инспекция остановлена");
                IsRunning = false;
            }
        }

        // === Потокобезопасные методы ===

        private void AddLog(string msg)
        {
            DispatchUI(() =>
            {
                Log.Add(msg);
            });
        }

        private static void DispatchUI(Action action)
        {
            if (Application.Current.Dispatcher.CheckAccess())
                action();
            else
                Application.Current.Dispatcher.BeginInvoke(action);
        }
    }
}
