using System.Diagnostics;
using WpfApp_IC.Services.ModbusT;

namespace WpfApp_IC.Devices
{
    /// <summary>
    /// Отбраковщик, управляемый через Modbus coil.
    /// </summary>
    public class ModbusRejector(IModbusService modbus, int coil)
    {
        public event Action<string>? ErrorOccurred;

        public int Coil { get; set; } = coil;

        public void Activate()
        {
            try
            {
                Debug.WriteLine("ACTIVATE START");
                modbus.WriteCoil(Coil, true);
                Thread.Sleep(500);
                modbus.WriteCoil(Coil, false);
                Debug.WriteLine("ACTIVATE END");
            }
            catch (Exception ex)
            {
                Debug.WriteLine("ACTIVATE ERROR: " + ex.Message);
            }
        }
    }
}
