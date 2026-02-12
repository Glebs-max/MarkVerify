using Observable;
using System.Collections.ObjectModel;
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
    public enum ErrorType
    {
        Warning,
        Fault
    }

    public struct VideojetPrinterError
    {
        public ErrorType ErrorType;
        public string Code { get; set; }
        public bool Clearable { get; set; }
        public string Title { get; set; }
    }

    public class VideojetPrinter(string ip = "192.168.10.2", int portTextComms = 9100, int portZplEmulation = 1000) : ObservableObject, IDisposable
    {
        private readonly TcpClient _textComms = new(), _zplEmulation = new();
        private CancellationTokenSource _cts = new();
        private bool _connected;
        private string _ip = ip;
        private int _portTextComms = portTextComms, _portZplEmulation = portZplEmulation, _queueSize, _maxQueueSize = 20;
        private NetworkStream? _streamTextComms, _streamZplEmulation;
        private Task? _listenTask;

        public event Action<PrinterState>? StateChanged;
        public event Action<QueueStatus>? QueueStatusChanged;
        public event Action<ErrorState>? ErrorStateChanged;

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
        public ObservableCollection<VideojetPrinterError> Errors { get; set; } = [];

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
        /// <summary>
        /// Отправка в очередь принтера изображения в формате ZPL
        /// </summary>
        public async Task SendZplAsync(string zpl)
        {
            if (!Connected || _streamZplEmulation == null)
                throw new InvalidOperationException("Нет соединения с принтером");

            byte[] bytes = Encoding.ASCII.GetBytes(zpl);

            await _streamZplEmulation.WriteAsync(bytes);
            await _streamZplEmulation.FlushAsync();
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
                throw new InvalidOperationException("Нет соединения с принтером");

            StreamWriter _writer = new(_streamTextComms, Encoding.ASCII) { AutoFlush = true };

            await _writer.WriteAsync(command);
        }
        /// <summary>
        /// Слушание и обработка сообщений от принтера
        /// </summary>
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
        /// <summary>
        /// Чтение сообщения до символа <CR>
        /// </summary>
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
                        StateChanged?.Invoke(sts1);
                    if (Enum.TryParse(split[2], out ErrorState sts2))
                        ErrorStateChanged?.Invoke(sts2);
                    break;
                case "ERS":
                    if (Enum.TryParse(split[1], out ErrorState ers))
                        ErrorStateChanged?.Invoke(ers);
                    break;
                case "OUT":
                    if (Enum.TryParse(split[1], out QueueStatus qst))
                        QueueStatusChanged?.Invoke(qst);
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

                        List<VideojetPrinterError> errors = [.. Errors.Where(e => e.ErrorType == errorType)];
                        foreach (VideojetPrinterError error in errors) Errors.Remove(error);

                        split = [.. split.Skip(2)];

                        for (int i = 0; i < count; i++)
                        {
                            string[] error = [.. split.Take(3)];
                            VideojetPrinterError e = new() { ErrorType = errorType, Code = $"#{error[0]}", Clearable = bool.TryParse(error[1], out bool c) && c, Title = error[2] };
                            Errors.Add(e);
                            split = [.. split.Skip(3)];
                        }
                    }
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
