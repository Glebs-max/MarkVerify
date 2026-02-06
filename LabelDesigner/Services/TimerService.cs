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
        private static readonly DispatcherTimer _queueSizeTimer = new()
        {
            Interval = TimeSpan.FromMilliseconds(1000)
        };

        public static DispatcherTimer DesignerCanvasTimer => _designerCanvasTimer;
        public static DispatcherTimer PrintTimer => _printTimer;
        public static DispatcherTimer QueueSizeTimer => _queueSizeTimer;
    }
}
