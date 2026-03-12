namespace WpfApp_IC.Device.Actuators
{
    /// <summary>
    /// Абстракция отбраковщика.
    /// Может быть Modbus, пневматика, реле, сервопривод.
    /// </summary>
    public interface IRejector
    {
        void Activate();
        void UpdateCoil(int coil);
    }
}
