using System.Windows.Threading;

namespace LabelDesigner.Services
{
    public static class TimerService
    {
        private static readonly DispatcherTimer _designerCanvasTimer = new()
        {
            Interval = TimeSpan.FromSeconds(1)
        };
        private static readonly DispatcherTimer _printTimer = new()
        {
            Interval = TimeSpan.FromMilliseconds(300)
        };

        public static DispatcherTimer DesignerCanvasTimer => _designerCanvasTimer;
        public static DispatcherTimer PrintTimer => _printTimer;
    }
}
