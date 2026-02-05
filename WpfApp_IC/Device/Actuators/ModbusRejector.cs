using WpfApp_IC.Services.ModbusT;
using System.Threading.Tasks;

namespace WpfApp_IC.Device.Actuators
{
    /// <summary>
    /// Отбраковщик, управляемый через Modbus coil.
    /// </summary>
    public class ModbusRejector : IRejector
    {
        private readonly IModbusService _modbus;
        private readonly int _coil;

        public ModbusRejector(IModbusService modbus, IoModuleConfig config)
        {
            _modbus = modbus;
            _coil = config.RejectCoil;
        }

        public async void Activate()
        {
            _modbus.WriteCoil(_coil, true);
            await Task.Delay(300);
            _modbus.WriteCoil(_coil, false);
        }
    }

}
