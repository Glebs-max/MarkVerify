using System.Diagnostics;
using WpfApp_IC.Services.ModbusT;

namespace WpfApp_IC.Models.Devices
{
    /// <summary>
    /// Отбраковщик, управляемый через Modbus coil.
    /// </summary>
    public class ModbusRejector(IModbusService modbus)
    {
        private CancellationTokenSource? _cts;
        private bool _activated;

        public int Coil { get; set; }

        public async Task Activate()
        {
            try
            {
                _cts?.Cancel();
                _cts = new();

                if (!_activated)
                {
                    modbus.WriteCoil(Coil, true);
                    _activated = true;
                }

                await Task.Delay(250, _cts.Token);

                modbus.WriteCoil(Coil, false);
                _activated = false;
            }
            catch { }
        }
    }
}
