using System;

namespace WpfApp_IC.Services.Printer
{
    /// <summary>
    /// Заглушка принтера.
    /// Позже заменим на реальную реализацию.
    /// </summary>
    public class PrinterStubService : IPrinterService
    {
        public void Print(string template, string filledData)
        {
            Console.WriteLine("=== Печать ===");
            Console.WriteLine(template);
            Console.WriteLine(filledData);
            Console.WriteLine("================");
        }
    }
}
