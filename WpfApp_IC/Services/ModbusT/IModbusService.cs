using System;

namespace WpfApp_IC.Services.ModbusT
{
    ///<summary>
    /// Интерфейс для работы с Modbus TSP.
    /// Абстрагирует библиотеку EasyModbus.
    /// </summary>
    public interface IModbusService : IDisposable
    {
        /// <summary> Подключение к Modbus TCP. </summary>
        void Connect(string ip, int port);

        ///<summary> Отключение </summary>
        void Disconnect();

        /// <summary> Чтение coil (0 или 1). </summary>
        int ReadSignal(int coilAddress);

        ///<summary> Запись coil (true/false). </summary>
        void WriteCoil(int coilAddress, bool value);
    }
}