using WpfApp_IC.Services.ModbusT;
using static EasyModbus.ModbusServer;

namespace WpfApp_IC.Devices
{
    /// <summary>
    /// Датчик, подключённый к Modbus coil.
    /// </summary>
    public class ModbusSensor(IModbusService modbus, int coil)
    {
        public int Coil { get; set; } = coil;

        public int Read() => modbus.ReadSignal(Coil);
    }

}
