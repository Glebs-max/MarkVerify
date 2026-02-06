using EasyModbus;
using System;
using System.Diagnostics;

namespace WpfApp_IC.Services.ModbusT
{
    /// <summary>
    /// Потокобезопасный, устойчивый к ошибкам Modbus TCP сервис.
    /// </summary>
    public class ModbusService : IModbusService
    {
        private ModbusClient? _client;
        private readonly object _sync = new();

        public void Connect(string ip, int port)
        {
            lock (_sync)
            {
                try
                {
                    if (_client != null && _client.Connected)
                        return;

                    _client = new ModbusClient(ip, port);
                    _client.Connect();

                    Debug.WriteLine($"Modbus CONNECTED: {ip}:{port}");
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Modbus CONNECT ERROR: {ex.Message}");
                }
            }
        }

        public void Disconnect()
        {
            lock (_sync)
            {
                try
                {
                    if (_client?.Connected == true)
                    {
                        _client.Disconnect();
                        Debug.WriteLine("Modbus DISCONNECTED");
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Modbus DISCONNECT ERROR: {ex.Message}");
                }

                _client = null;
            }
        }

        public int ReadSignal(int coilAddress)
        {
            lock (_sync)
            {
                try
                {
                    if (_client == null || !_client.Connected)
                        return 0;

                    bool[] coils = _client.ReadCoils(coilAddress, 1);
                    return coils[0] ? 1 : 0;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Modbus READ ERROR: {ex.Message}");
                    return 0;
                }
            }
        }

        public void WriteCoil(int coilAddress, bool value)
        {
            lock (_sync)
            {
                try
                {
                    if (_client == null || !_client.Connected)
                    {
                        Debug.WriteLine("Modbus WRITE SKIPPED — not connected");
                        return;
                    }

                    Debug.WriteLine($"WriteCoil: {coilAddress} = {value}");
                    _client.WriteSingleCoil(coilAddress, value);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Modbus WRITE ERROR: {ex.Message}");
                }
            }
        }

        public void Dispose() => Disconnect();
    }
}
