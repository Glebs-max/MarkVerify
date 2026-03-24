using System.Windows.Threading;

namespace LabelDesigner.Services
{
    public static class TimerService
    {
        private static readonly DispatcherTimer _designerCanvasTimer = new()
        {
            Interval = TimeSpan.FromSeconds(1)
        };

        public static DispatcherTimer DesignerCanvasTimer => _designerCanvasTimer;
    }
}
