using Microsoft.Extensions.Logging.Abstractions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfApp_IC.Services.Log
{
    /// <summary>
    /// Центральный сервис логирования 
    /// Singelton - доступен из любого места в приложении
    /// </summary>
    public interface ILogService
    {
        ///<summary> Коллекция для привязки в XML</summary>
        ObservableCollection<LogEntry> Entries { get; }

        void Info(string message);
        void Warning(string message);
        void Error(string message, Exception? ex = null);
    }
}
