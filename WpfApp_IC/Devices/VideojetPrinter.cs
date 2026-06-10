using Observable;
using System.IO;
using System.Net.Sockets;
using System.Text;

namespace WpfApp_IC.Devices
{
    public enum PrinterState
    {
        Shutdown,
        StartingUp,
        ShuttingDown,
        Running,
        Offline,
        Disconnected,
        Connecting,
        Connected
    }
    public enum ErrorState
    {
        None,
        Warnings,
        Faults,
        Unknown
    }
    public enum ErrorType
    {
        Warning,
        Fault
    }

    public struct VideojetPrinterError
    {
        public ErrorType ErrorType { get; set; }
        public string Code { get; set; }
        public bool Clearable { get; set; }
        public string Title { get; set; }
    }

    public class VideojetPrinter(string ip = "192.168.0.2", int portTextComms = 3003, int portZplEmulation = 1000) : ObservableObject, IDisposable
    {
        private TcpClient? _textComms, _zplEmulation;
        private NetworkStream? _streamTextComms, _streamZplEmulation;
        private Task? _listenTask, _connectionTask;
        private CancellationTokenSource? _listenCts, _connectionCts;
        private PrinterState _printerState = PrinterState.Disconnected;
        private ErrorState _errorState = ErrorState.Unknown;
        private int _queueSize, _maxQueueSize = 18;

        public event Action? ConnectionEstablished;
        public event Action? ConnectionLost;
        public event Action<PrinterState>? StateChanged;
        public event Action<int>? QueueSizeChanged;
        public event Action<ErrorState>? ErrorStateChanged;
        public event Action? ErrorListChanged;

        public PrinterState PrinterState
        {
            get => _printerState;
            set => Set(ref _printerState, value, () => StateChanged?.Invoke(value));
        }
        public ErrorState ErrorState
        {
            get => _errorState;
            set => Set(ref _errorState, value, () => ErrorStateChanged?.Invoke(value));
        }
        public int QueueSize
        {
            get => _queueSize;
            private set => Set(ref _queueSize, value, () => QueueSizeChanged?.Invoke(value));
        }
        public int MaxQueueSize
        {
            get => _maxQueueSize;
            private set => Set(ref _maxQueueSize, value);
        }
        public string IP
        {
            get => ip;
            set => Set(ref ip, value, OnConfigChanged);
        }
        public int PortTextComms
        {
            get => portTextComms;
            set => Set(ref portTextComms, value, OnConfigChanged);
        }
        public int PortZplEmulation
        {
            get => portZplEmulation;
            set => Set(ref portZplEmulation, value, OnConfigChanged);
        }
        public List<VideojetPrinterError> Errors { get; set; } = [];
        public bool Connected => PrinterState != PrinterState.Disconnected && PrinterState != PrinterState.Connecting;

        public void Dispose()
        {
            PrinterState = PrinterState.Disconnected;
            ErrorState = ErrorState.Unknown;
            QueueSize = 0;
            Errors.Clear();

            ConnectionLost?.Invoke();
            ErrorListChanged?.Invoke();

            _streamTextComms?.Dispose();
            _streamZplEmulation?.Dispose();
            _textComms?.Close();
            _zplEmulation?.Close();

            GC.SuppressFinalize(this);
        }
        public void Connect()
        {
            _connectionCts = new();
            _connectionTask = Task.Run(async () =>
            {
                while (!Connected && !_connectionCts.Token.IsCancellationRequested)
                {
                    try
                    {
                        PrinterState = PrinterState.Connecting;

                        _textComms = new();
                        _zplEmulation = new();

                        await _textComms.ConnectAsync(ip, portTextComms);
                        await _zplEmulation.ConnectAsync(ip, portZplEmulation);

                        _streamTextComms = _textComms.GetStream();
                        _streamTextComms.WriteTimeout = _streamTextComms.ReadTimeout = 5000;
                        _streamZplEmulation = _zplEmulation.GetStream();
                        _streamZplEmulation.WriteTimeout = _streamZplEmulation.ReadTimeout = 5000;

                        PrinterState = PrinterState.Connected;
                        ConnectionEstablished?.Invoke();

                        _listenTask = Task.Run(() => ListenAsync((_listenCts = new()).Token), _connectionCts.Token);
                    }
                    catch { Dispose(); }
                }
            });
        }
        public void Disconnect()
        {
            Task.Run(() =>
            {
                try
                {
                    _connectionCts?.Cancel();
                    _connectionTask?.Wait(500);
                    _connectionCts?.Dispose();
                }
                catch { }

                try
                {
                    _listenCts?.Cancel();
                    _listenTask?.Wait(500);
                    _listenCts?.Dispose();
                }
                catch { }

                Dispose();
            });
        }
        /// <summary>
        /// Отправка в очередь принтера изображения в формате ZPL
        /// </summary>
        public async Task SendZplAsync(string zpl)
        {
            if (!Connected || _streamZplEmulation == null)
                return;

            StreamWriter _writer = new(_streamZplEmulation, Encoding.ASCII) { AutoFlush = true };

            try { await _writer.WriteAsync(zpl); } catch { }
        }
        public async Task GetStateAsync() => await SendCommandAsync("GST\r");
        public async Task GetQueueSizeAsync() => await SendCommandAsync("QSZ\r");
        public async Task GetAllFaultsAsync() => await SendCommandAsync("GFT\r");
        public async Task GetAllWarningsAsync() => await SendCommandAsync("GWN\r");
        public async Task ClearQueueAsync() => await SendCommandAsync("CQI\r");
        public async Task ClearAllFaultsAsync() => await SendCommandAsync("CAF\r");
        public async Task ClearAllWarningsAsync() => await SendCommandAsync("CAW\r");
        public async Task PrintAsync() => await SendCommandAsync("PRN\r");
        public async Task StartAsync() => await SendCommandAsync($"SST|{(int)PrinterState.Running}|\r");
        public async Task StopAsync() => await SendCommandAsync($"SST|{(int)PrinterState.Offline}|\r");

        /// <summary>
        /// Отправка текстовой команды принтеру (протокол TextComms)
        /// </summary>
        private async Task SendCommandAsync(string command)
        {
            if (!Connected || _streamTextComms == null)
                return;

            StreamWriter _writer = new(_streamTextComms, Encoding.ASCII) { AutoFlush = true };

            try { await _writer.WriteAsync(command); } catch { }
        }
        /// <summary>
        /// Слушание и обработка сообщений от принтера
        /// </summary>
        private async Task ListenAsync(CancellationToken token)
        {
            if (_streamTextComms == null)
                return;

            StreamReader reader = new(_streamTextComms, Encoding.ASCII);

            while (Connected && !token.IsCancellationRequested)
            {
                try
                {
                    string message = await ReadMessage(reader, token);

                    if (!string.IsNullOrEmpty(message))
                        ProcessMessage(message);
                }
                catch
                {
                    if (token.IsCancellationRequested)
                        break;

                    Dispose();
                    Connect();
                    break;
                }
            }
        }
        /// <summary>
        /// Чтение сообщения до символа <CR>
        /// </summary>
        private async Task<string> ReadMessage(StreamReader reader, CancellationToken token)
        {
            StringBuilder sb = new();
            char[] buffer = new char[1];

            while (Connected && !token.IsCancellationRequested)
            {
                int read = await reader.ReadAsync(buffer, 0, 1);

                if (buffer[0] == '\r' || read == 0)
                    break;

                sb.Append(buffer[0]);
            }

            return sb.ToString();
        }
        /// <summary>
        /// Обработка полученных сообщений от принтера
        /// </summary>
        private void ProcessMessage(string message)
        {
            string[] split = message.Split('|');

            switch (split[0])
            {
                case "STS":
                    if (Enum.TryParse(split[1], out PrinterState sts1))
                        PrinterState = sts1;
                    if (Enum.TryParse(split[2], out ErrorState sts2))
                        ErrorState = sts2;
                    break;
                case "ERS":
                    if (Enum.TryParse(split[1], out ErrorState ers))
                        ErrorState = ers;
                    break;
                case "QSZ":
                    if (int.TryParse(split[1], out int qsz))
                        QueueSize = qsz;
                    break;
                case "FLT":
                case "WRN":
                    if (int.TryParse(split[1], out int count))
                    {
                        ErrorType errorType = split[0] switch
                        {
                            "FLT" => ErrorType.Fault,
                            "WRN" => ErrorType.Warning,
                            _ => ErrorType.Fault
                        };
                        Errors.RemoveAll(e => e.ErrorType == errorType);
                        
                        split = [.. split.Skip(2)];

                        for (int i = 0; i < count; i++)
                        {
                            string[] error = [.. split.Take(3)];

                            Errors.Add(new()
                            {
                                ErrorType = errorType,
                                Code = $"E{error[0]}",
                                Clearable = bool.TryParse(error[1], out bool c) && c,
                                Title = error[2]
                            });

                            split = [.. split.Skip(3)];
                        }

                        ErrorListChanged?.Invoke();
                    }
                    break;
            }
        }
        private void OnConfigChanged()
        {
            if (Connected)
            {
                Disconnect();
                Connect();
            }
        }
    }
}
