using System.Windows.Media;

namespace WpfApp_IC.Models
{
    public enum LogColorCode
    {
        Green,
        Yellow,
        Red
    }

    /// <summary>
    /// Модель записи в логе
    /// </summary>
    public class LogEntry(string message, LogColorCode colorCode)
    {
        public DateTime Time { get; } = DateTime.Now;
        public LogColorCode ColorCode { get; } = colorCode;
        public string Message { get; } = message;

        public override string ToString() => $"[{Time:HH:mm:ss}] {Message}";
    }
}
