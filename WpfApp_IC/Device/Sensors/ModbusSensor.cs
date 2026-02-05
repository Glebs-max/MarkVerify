using Microsoft.Extensions.Configuration;
using WpfApp_IC.Services.ModbusT;

namespace WpfApp_IC.Device.Sensors
{
    /// <summary>
    /// Датчик, подключённый к Modbus coil.
    /// </summary>
    public class ModbusSensor : ISensor
    {
        private readonly IModbusService _modbus;
        private readonly int _coil;

        public ModbusSensor(IModbusService modbus, IoModuleConfig config)
        {
            _modbus = modbus;
            _coil = config.SignalCoil;
        }

        public int Read() => _modbus.ReadSignal(_coil);
    }

}
