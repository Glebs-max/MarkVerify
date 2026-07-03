using WpfApp_IC.Services.ModbusT;

namespace WpfApp_IC.Models.Devices
{
    /// <summary>
    /// Отбраковщик, управляемый через Modbus coil
    /// </summary>
    public class ModbusRejector(IModbusService modbus)
    {
        private CancellationTokenSource? _cts;
        private bool _activated;

        public int Coil { get; set; }
        public int ActiveTime { get; set; } = 300;

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

                await Task.Delay(ActiveTime, _cts.Token);

                modbus.WriteCoil(Coil, false);
                _activated = false;
            }
            catch { }
        }
    }
}
