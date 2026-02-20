using System;
using WpfApp_IC.Services.Inspectors;

namespace WpfApp_IC.ViewModels
{
    public class CameraBasicViewModel : CameraViewModel
    {
        private readonly IInspectorController _controller;

        public CameraBasicViewModel(IInspectorController controller) : base(controller)
        {
            _controller = controller;
        }

        public void StartStop()
        {
            if (!IsRunning)
            {
                try
                {
                    _controller.Start();
                    AddLog("Инспекция запущена. Запущен тестовый режим. Отсутствует проверка с БД");
                    IsRunning = true;
                }
                catch (Exception ex)
                {
                    AddLog("Ошибка запуска инспекции (ТР): " + ex.Message);
                }
            }
            else
            {
                _controller.Stop();
                AddLog("Инспекция остановлена (ТР)");
                IsRunning = false;
            }
        }
    }
}
