using System;
using System.Collections.ObjectModel;
using WpfApp_IC.Services.Inspectors;
using WpfApp_IC.Services.Log;

namespace WpfApp_IC.ViewModels
{
    public class CameraBasicViewModel : CameraViewModel
    {
        private readonly IInspectorController _controller;
        public ILogService LogService { get; }
        public ObservableCollection<LogEntry> Entries => LogService.Entries;


        public CameraBasicViewModel(IInspectorController controller, ILogService log)
        : base(controller, log)
        {
            _controller = controller;
            LogService = log;

            log.Info("CameraBasicViewModel создан");
            log.Warning("Тест предупреждения");
            log.Error("Тест ошибки");
        }

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
                    AddLog("Ошибка запуска: " + ex.Message);
                }
            }
            else
            {
                _controller.Stop();
                AddLog("Инспекция остановлена");
                IsRunning = false;
            }
        }
    }
}
