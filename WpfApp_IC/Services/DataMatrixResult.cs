using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfApp_IC.Services
{
    /// <summary>
    /// Результат чтения DataMatrix-кода.
    /// Raw — исходная строка с управляющими символами.
    /// Normalized — очищенная строка, готовая для сравнения.
    /// </summary>
    public class DataMatrixResult
    {
        public string Raw { get; set; } = "";
        public string Normalized { get; set; } = "";
    }
}

