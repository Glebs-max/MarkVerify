using EasyModbus;
using System;

namespace WpfApp_IC.Services.ModbusT
{
    /// <summary>
    /// Реализация Modbus TCP через библиотеку EasyModbus.
    /// Вся низкоуровневая логика изолирована здесь.
    /// </summary>
    public class ModbusService : IModbusService
    {
        private ModbusClient? _client;

        public void Connect(string ip, int port)
        {
            if (_client != null && _client.Connected)
                return;

            _client = new ModbusClient(ip, port);
            _client.Connect();
        }

        public void Disconnect()
        {
            try
            {
                if (_client?.Connected == true)
                    _client.Disconnect();
            }
            catch { }

            _client = null;
        }

        public int ReadSignal(int coilAddress)
        {
            if (_client == null || !_client.Connected)
                return 0; // безопасное поведение


            bool[] coils = _client.ReadCoils(coilAddress, 1);
            return coils[0] ? 1 : 0;
        }

        public void WriteCoil(int coilAddress, bool value)
        {
            if (_client == null || !_client.Connected)
                return;

            _client.WriteSingleCoil(coilAddress, value);
        }

        public void Dispose() => Disconnect();
    }
}
