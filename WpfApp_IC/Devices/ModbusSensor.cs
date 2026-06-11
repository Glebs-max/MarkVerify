using WpfApp_IC.Services.ModbusT;

namespace WpfApp_IC.Devices
{
    /// <summary>
    /// Датчик, подключённый к Modbus coil.
    /// </summary>
    public class ModbusSensor(IModbusService modbus)
    {
        public int Coil { get; set; }

        public int Read() => modbus.ReadSignal(Coil);
    }

}
