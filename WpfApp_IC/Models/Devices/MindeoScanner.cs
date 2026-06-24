using Observable;
using System.IO.Ports;

namespace WpfApp_IC.Models.Devices
{
    public class MindeoScanner(string comPort = "COM3") : ObservableObject
    {
        private SerialPort? _scanner;

        public event Action<string>? DataMatrixRead;

        public string ComPort
        {
            get => comPort;
            set => Set(ref comPort, value);
        }

        public void Connect()
        {
            if (_scanner == null)
            {
                _scanner = new(comPort);
                _scanner.DataReceived += (s, e) => DataMatrixRead?.Invoke(_scanner?.ReadLine() ?? "");
                _scanner.Open();
            }
        }
        public void Disconnect()
        {
            if (_scanner != null)
            {
                _scanner.DataReceived -= (s, e) => DataMatrixRead?.Invoke(_scanner?.ReadLine() ?? "");
                _scanner.Close();
                _scanner = null;
            }
        }
    }
}
