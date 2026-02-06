using WpfApp_IC.Services.ModbusT;
using System.Threading.Tasks;
using System.Diagnostics;

namespace WpfApp_IC.Device.Actuators
{
    /// <summary>
    /// Отбраковщик, управляемый через Modbus coil.
    /// </summary>
    public class ModbusRejector : IRejector
    {
        private readonly IModbusService _modbus;
        private readonly int _coil;
        public event Action<string>? ErrorOccurred;

        public ModbusRejector(IModbusService modbus, IoModuleConfig config)
        {
            _modbus = modbus;
            _coil = config.RejectCoil;
        }
        /// <summary>
        /// Раньше был асинхронным, сделала синхронным, чтобы не терялся сигнал 
        /// </summary>
        public void Activate()
        {
            try
            {
                Debug.WriteLine("ACTIVATE START");

                _modbus.WriteCoil(_coil, true);
                Thread.Sleep(500);
                _modbus.WriteCoil(_coil, false);

                Debug.WriteLine("ACTIVATE END");
            }
            catch (Exception ex)
            {
                Debug.WriteLine("ACTIVATE ERROR: " + ex.Message);
                // здесь можно пробросить в UI, если нужно:
                // Log?.Invoke("Rejector ERROR: " + ex.Message);
            }
        }

    }

}
