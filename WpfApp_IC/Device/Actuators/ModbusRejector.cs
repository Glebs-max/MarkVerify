using System.Diagnostics;
using System.Threading.Tasks;
using WpfApp_IC.Services.Inspectors;
using WpfApp_IC.Services.ModbusT;

namespace WpfApp_IC.Device.Actuators
{
    /// <summary>
    /// Отбраковщик, управляемый через Modbus coil.
    /// </summary>
    public class ModbusRejector : IRejector
    {
        private readonly IModbusService _modbus;
        private int _coil;
        public event Action<string>? ErrorOccurred;

        public ModbusRejector(IModbusService modbus, int coil)
        {
            _modbus = modbus;
            _coil = coil;
        }

        public void UpdateCoil(int coil) => _coil = coil;

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
            catch (System.Exception ex)
            {
                Debug.WriteLine("ACTIVATE ERROR: " + ex.Message);
            }
        }

    }

}
