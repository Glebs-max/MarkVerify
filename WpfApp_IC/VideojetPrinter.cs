using Observable;
using System.IO;
using System.Net.Sockets;
using System.Text;

namespace WpfApp_IC
{
    public enum PrinterState
    {
        Shutdown,
        StartingUp,
        ShuttingDown,
        Running,
        Offline
    }
    public enum QueueStatus
    {
        QEMPTY,
        QFULL,
        QHIGH,
        QLOW
    }
    public enum ErrorState
    {
        None,
        Warnings,
        Faults
    }

    public class VideojetPrinter(string ip = "192.168.10.2", int portTextComms = 9100, int portZplEmulation = 1000) : ObservableObject, IDisposable
    {
        private readonly TcpClient _textComms = new(), _zplEmulation = new();
        private CancellationTokenSource _cts = new();
        private PrinterState _state;
        private bool _connected;
        private string _ip = ip;
        private int _portTextComms = portTextComms, _portZplEmulation = portZplEmulation, _queueSize, _maxQueueSize = 20;
        private NetworkStream? _streamTextComms, _streamZplEmulation;
        private Task? _listenTask;

        public event Action<PrinterState>? StateChanged;
        public event Action<QueueStatus>? QueueStatusChanged;
        public event Action<ErrorState>? ErrorStateChanged;

        public PrinterState State
        {
            get => _state;
            private set => Set(ref _state, value);
        }
        public bool Connected
        {
            get => _connected;
            private set => Set(ref _connected, value);
        }
        public string IP
        {
            get => _ip;
            set => Set(ref _ip, value, OnConfigChanged);
        }
        public int PortTextComms
        {
            get => _portTextComms;
            set => Set(ref _portTextComms, value, OnConfigChanged);
        }
        public int PortZplEmulation
        {
            get => _portZplEmulation;
            set => Set(ref _portZplEmulation, value, OnConfigChanged);
        }
        public int QueueSize
        {
            get => _queueSize;
            private set => Set(ref _queueSize, value);
        }
        public int MaxQueueSize
        {
            get => _maxQueueSize;
            private set => Set(ref _maxQueueSize, value);
        }

        public void Dispose()
        {
            Disconnect();
            GC.SuppressFinalize(this);
        }
        public async Task ConnectAsync()
        {
            if (Connected)
                return;
            
            try
            {
                await _textComms.ConnectAsync(_ip, _portTextComms);
                await _zplEmulation.ConnectAsync(_ip, _portZplEmulation);

                _streamTextComms = _textComms.GetStream();
                _streamTextComms.WriteTimeout = _streamTextComms.ReadTimeout = 5000;
                _streamZplEmulation = _zplEmulation.GetStream();
                _streamZplEmulation.WriteTimeout = _streamZplEmulation.ReadTimeout = 5000;

                _cts = new();
                _listenTask = Task.Run(() => ListenAsync(_streamTextComms, _cts.Token));

                Connected = true;
                await GetQueueSize();
            }
            catch { }
        }
        public void Disconnect()
        {
            _cts.Cancel();
            try { _listenTask?.Wait(1000); } catch { }
            _cts.Dispose();

            _streamTextComms?.Dispose();
            _streamZplEmulation?.Dispose();

            _textComms.Close();
            _zplEmulation.Close();

            Connected = false;
        }
        public async Task GetQueueSize()
        {
            if (!Connected || _streamTextComms == null)
                throw new InvalidOperationException("Нет соединения с принтером");

            StreamWriter _writer = new(_streamTextComms, Encoding.ASCII) { AutoFlush = true };

            await _writer.WriteAsync("QSZ\r");
        }
        public async Task SendZplAsync(string zpl)
        {
            if (!Connected || _streamZplEmulation == null)
                throw new InvalidOperationException("Нет соединения с принтером");

            byte[] bytes = Encoding.ASCII.GetBytes(zpl);

            await _streamZplEmulation.WriteAsync(bytes);
            await _streamZplEmulation.FlushAsync();
            await GetQueueSize();
        }
        public async Task ClearQueueAsync()
        {
            if (!Connected || _streamTextComms == null)
                throw new InvalidOperationException("Нет соединения с принтером");

            StreamWriter _writer = new(_streamTextComms, Encoding.ASCII) { AutoFlush = true };

            await _writer.WriteAsync("CQI\r");
            await GetQueueSize();
        }
        public async Task PrintAsync()
        {
            if (!Connected || _streamTextComms == null)
                throw new InvalidOperationException("Нет соединения с принтером");

            StreamWriter _writer = new(_streamTextComms, Encoding.ASCII) { AutoFlush = true };
            
            await _writer.WriteAsync("PRN\r");
            await GetQueueSize();
        }
        public async Task StartAsync()
        {
            if (!Connected || _streamTextComms == null)
                throw new InvalidOperationException("Нет соединения с принтером");

            StreamWriter _writer = new(_streamTextComms, Encoding.ASCII) { AutoFlush = true };

            if (State == PrinterState.Offline)
                await _writer.WriteAsync($"SST|{(int)PrinterState.Running}|\r");
        }
        public async Task StopAsync()
        {
            if (!Connected || _streamTextComms == null)
                throw new InvalidOperationException("Нет соединения с принтером");

            StreamWriter _writer = new(_streamTextComms, Encoding.ASCII) { AutoFlush = true };

            if (State == PrinterState.Running)
                await _writer.WriteAsync($"SST|{(int)PrinterState.Offline}|\r");
        }

        private async Task ListenAsync(NetworkStream streamTextComms, CancellationToken token)
        {
            try
            {
                StreamReader reader = new(streamTextComms, Encoding.ASCII);

                while (!token.IsCancellationRequested)
                {
                    string message = await ReadMessage(reader, token);

                    if (!string.IsNullOrEmpty(message))
                        ProcessMessage(message);
                }
            }
            catch { }
        }
        private static async Task<string> ReadMessage(StreamReader reader, CancellationToken token)
        {
            StringBuilder sb = new();
            char[] buffer = new char[1];

            while (!token.IsCancellationRequested)
            {
                int read = await reader.ReadAsync(buffer, 0, 1);

                if (buffer[0] == '\r' || read == 0)
                    break;

                sb.Append(buffer[0]);
            }

            return sb.ToString();
        }
        private void ProcessMessage(string message)
        {
            string[] split = message.Split('|');

            switch (split[0])
            {
                case "STS":
                    StateChanged?.Invoke(State = Enum.Parse<PrinterState>(split[1]));
                    break;
                case "ERS":
                    ErrorStateChanged?.Invoke(Enum.Parse<ErrorState>(split[1]));
                    break;
                case "OUT":
                    QueueStatusChanged?.Invoke(Enum.Parse<QueueStatus>(split[1]));
                    break;
                case "QSZ":
                    QueueSize = int.Parse(split[1]);
                    break;
            }
        }
        private async void OnConfigChanged()
        {
            if (Connected)
            {
                Disconnect();
                await ConnectAsync();
            }
        }
    }
}
