namespace WpfApp_IC.Device.Sensors
{
    /// <summary>
    /// Абстракция датчика.
    /// Может быть Modbus, OPC UA, USB, GPIO и т.д.
    /// </summary>
    public interface ISensor
    {
        int Read();
    }
}
