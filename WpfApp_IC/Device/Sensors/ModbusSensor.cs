using Microsoft.Extensions.Configuration;
using WpfApp_IC.Services.Inspectors;
using WpfApp_IC.Services.ModbusT;

namespace WpfApp_IC.Device.Sensors
{
    /// <summary>
    /// Датчик, подключённый к Modbus coil.
    /// </summary>
    public class ModbusSensor : ISensor
    {
        private readonly IModbusService _modbus;
        private int _coil;

        public ModbusSensor(IModbusService modbus, int сoil)
        {
            _modbus = modbus;
            _coil = сoil;
        }
        public void UpdateCoil(int coil) => _coil = coil;

        public int Read() => _modbus.ReadSignal(_coil);
    }

}
