using Observable;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Windows.Threading;

namespace WpfApp_IC.Models.Devices
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
    public enum VideojetErrorState
    {
        None,
        Warnings,
        Faults,
        Unknown
    }
    public enum VideojetErrorType
    {
        Warning,
        Fault
    }

    public struct VideojetPrinterError
    {
        public VideojetErrorType ErrorType { get; set; }
        public string Code { get; set; }
        public bool Clearable { get; set; }
        public string Title { get; set; }
    }

    public class VideojetPrinter : ObservableObject, IDisposable
    {
        private readonly DispatcherTimer _statusCheck = new() { Interval = TimeSpan.FromMilliseconds(1000) };

        private TcpClient? _textComms, _zplEmulation;
        private NetworkStream? _streamTextComms, _streamZplEmulation;
        private Task? _listenTask, _connectionTask;
        private CancellationTokenSource? _listenCts, _connectionCts;
        private PrinterState _printerState = PrinterState.Disconnected;
        private VideojetErrorState _errorState = VideojetErrorState.Unknown;
        private int _portTextComms, _portZpl, _queueSize, _maxQueueSize = 19;
        private string _ip;

        public event Action? ConnectionEstablished, ConnectionLost, ErrorListChanged;
        public event Action<PrinterState>? StateChanged;
        public event Action<VideojetErrorState>? ErrorStateChanged;
        public event Action<int>? QueueLow;

        public VideojetPrinter(string ip = "192.168.0.2", int portTextComms = 3003, int portZpl = 1000)
        {
            _ip = ip;
            _portTextComms = portTextComms;
            _portZpl = portZpl;

            _statusCheck.Tick += async (s, e) => await StatusCheck();
        }

        public PrinterState PrinterState
        {
            get => _printerState;
            set => Set(ref _printerState, value, () => StateChanged?.Invoke(value));
        }
        public VideojetErrorState ErrorState
        {
            get => _errorState;
            set => Set(ref _errorState, value, () => ErrorStateChanged?.Invoke(value));
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
        public int PortZpl
        {
            get => _portZpl;
            set => Set(ref _portZpl, value, OnConfigChanged);
        }
        public List<VideojetPrinterError> Errors { get; set; } = [];
        public bool Connected => PrinterState != PrinterState.Disconnected && PrinterState != PrinterState.Connecting;

        public void Dispose()
        {
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

                        await _textComms.ConnectAsync(_ip, _portTextComms);
                        await _zplEmulation.ConnectAsync(_ip, _portZpl);

                        _streamTextComms = _textComms.GetStream();
                        _streamTextComms.WriteTimeout = _streamTextComms.ReadTimeout = 5000;
                        _streamZplEmulation = _zplEmulation.GetStream();
                        _streamZplEmulation.WriteTimeout = _streamZplEmulation.ReadTimeout = 5000;

                        _listenCts = new();
                        _listenTask = Task.Run(() => ListenAsync(_listenCts.Token));
                        _statusCheck.Start();

                        PrinterState = PrinterState.Connected;
                        ConnectionEstablished?.Invoke();
                    }
                    catch { Dispose(); }
                }
            }, _connectionCts.Token);
        }
        public void Disconnect()
        {
            _statusCheck.Stop();
            PrinterState = PrinterState.Disconnected;
            ConnectionLost?.Invoke();

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
        public async Task ClearQueueAsync() => await SendCommandAsync("CQI\r");
        public async Task GetStateAsync() => await SendCommandAsync("GST\r");
        public async Task GetQueueSizeAsync() => await SendCommandAsync("QSZ\r");
        public async Task GetAllFaultsAsync() => await SendCommandAsync("GFT\r");
        public async Task GetAllWarningsAsync() => await SendCommandAsync("GWN\r");
        public async Task ClearAllFaultsAsync() => await SendCommandAsync("CAF\r");
        public async Task ClearAllWarningsAsync() => await SendCommandAsync("CAW\r");
        public async Task PrintAsync() => await SendCommandAsync("PRN\r");
        public async Task StartAsync() => await SendCommandAsync($"SST|{(int)PrinterState.Running}|\r");
        public async Task StopAsync() => await SendCommandAsync($"SST|{(int)PrinterState.Offline}|\r");

        /// <summary>
        /// Запрос текущего состояния принтера
        /// </summary>
        private async Task StatusCheck()
        {
            if (Connected)
            {
                await GetQueueSizeAsync();
                await GetStateAsync();
                await GetAllFaultsAsync();
                await GetAllWarningsAsync();
            }
        }
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
                    if (!token.IsCancellationRequested)
                    {
                        _statusCheck.Stop();
                        PrinterState = PrinterState.Disconnected;
                        ConnectionLost?.Invoke();

                        Dispose();
                        await Task.Delay(5000, token);
                        Connect();
                    }
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
                    if (Enum.TryParse(split[2], out VideojetErrorState sts2))
                        ErrorState = sts2;
                    break;
                case "ERS":
                    if (Enum.TryParse(split[1], out VideojetErrorState ers))
                        ErrorState = ers;
                    break;
                case "QSZ":
                    if (int.TryParse(split[1], out int qsz))
                    {
                        QueueSize = qsz;
                        if (qsz < MaxQueueSize / 2)
                            QueueLow?.Invoke(qsz);
                    }
                    break;
                case "FLT":
                case "WRN":
                    if (int.TryParse(split[1], out int count))
                    {
                        VideojetErrorType errorType = split[0] switch
                        {
                            "FLT" => VideojetErrorType.Fault,
                            "WRN" => VideojetErrorType.Warning,
                            _ => VideojetErrorType.Fault
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
