namespace WpfApp_IC.Services.Printer
{
    /// <summary>
    /// Интерфейс принтера.
    /// Позволяет позже подключить реальный принтер.
    /// </summary>
    public interface IPrinterService
    {
        /// <summary>
        /// Печатает этикетку.
        /// </summary>
        void Print(string template, string filledData);
    }
}
