using Microsoft.EntityFrameworkCore;
using System.Timers;
using System.Windows;
using System.Windows.Media.Imaging;
using WpfApp_IC.Data;
using WpfApp_IC.Models;
using WpfApp_IC.Models.DbContext;
using WpfApp_IC.Models.Devices;
using WpfApp_IC.Services.Camera;
using WpfApp_IC.Services.ModbusT;
using Timer = System.Timers.Timer;

namespace WpfApp_IC.Services.Inspectors
{
    /// <summary>
    /// Главный контроллер инспекции.
    /// Управляет камерой, датчиком, отбраковщиком и выполняет проверку DataMatrix.
    /// Не зависит от UI и не содержит логики отображения.
    /// </summary>
    public class InspectorController(
        WorkSession workSession,
        LogService log,
        CameraService camera,
        ModbusSensor sensor,
        ModbusRejector rejector,
        IModbusService modbus,
        IDbContextFactory<AppDbContext> dbContextFactory,
        ImageSaverService imageSaver)
    {
        private Timer? _sensorPollTimer;
        private bool _filter;
        private BitmapSource? _lastFrame;

        public string ModbusIp { get; set; } = "192.168.0.127";
        public int ModbusPort { get; set; } = 502;

        /// <summary>
        /// Задержка перед активацией отбраковщика.
        /// </summary>
        public int RejectDelay { get; set; } = 300;
        public int MotionFilterInterval { get; set; } = 200;
        public int SensorPollInterval { get; set; } = 10;

        public string CameraIp { get; set; } = "";

        // Фильтрация дребезга
        private int _currentSignal = 0;
        private bool _triggerInProgress = false;

        public event Action<DataMatrixResult>? DataMatrixRead;
        public event Action<string?, BitmapSource>? FrameReceived;
        public event Action<string, bool>? CodeChecked;

        public event EventHandler? MotionDetected;

        public void Start()
        {
            try
            {
                modbus.Connect(ModbusIp, ModbusPort);
                log.AddEntry($"MODBUS модуль подключен: {ModbusIp}:{ModbusPort}");
            }
            catch
            {
                log.AddEntry("Не удалось подключиться к модулю MODBUS", LogColorCode.Red);
            }

            try
            {
                camera.Open(CameraIp);
                log.AddEntry($"Соединение с камерой установлено [{CameraIp}]");
            }
            catch
            {
                log.AddEntry($"Не удалось установить соединение с камерой [{CameraIp}]", LogColorCode.Red);
            }

            _triggerInProgress = false;

            MotionDetected += Inspect;

            _sensorPollTimer = new Timer(SensorPollInterval);
            _sensorPollTimer.Elapsed += PollSensor;
            _sensorPollTimer.AutoReset = true;
            _sensorPollTimer.Start();
        }
        public void Stop()
        {
            MotionDetected -= Inspect;

            if (_sensorPollTimer != null)
            {
                _sensorPollTimer.Stop();
                _sensorPollTimer.Elapsed -= PollSensor;
                _sensorPollTimer.Dispose();
                _sensorPollTimer = null;
            }

            camera.Close();
            modbus?.Disconnect();
        }
        public DataMatrixResult? TriggerCamera()
        {
            if (_triggerInProgress)
                return null;

            try
            {
                _triggerInProgress = true;
                var (dm, frame) = camera.TriggerAndRead();
                _triggerInProgress = false;

                if (frame != null)
                {
                    _lastFrame = frame;
                    FrameReceived?.Invoke(dm?.Raw, frame);
                }

                return dm;
            }
            catch
            {
                return null;
            }
        }

        private void Inspect(object? sender, EventArgs e)
        {
            Task.Run(async () =>
            {
                CancellationTokenSource rejectCts = new();

                _ = Task.Run(async () =>
                {
                    try
                    {
                        await Task.Delay(RejectDelay, rejectCts.Token);
                        await rejector.Activate();
                    }
                    catch { }
                }, rejectCts.Token);

                DataMatrixResult? dm = TriggerCamera();

                if (dm != null)
                    DataMatrixRead?.Invoke(dm);

                ValidationResult result = await ValidateAsync(dm?.Raw);

                if (result.IsOk)
                {
                    rejectCts.Cancel();
                    workSession.Verified++;
                }
                else
                {
                    switch (result.ErrorCode)
                    {
                        case "NO_READ":
                            log.AddEntry($"Код не считан: NULL", LogColorCode.Yellow);
                            break;
                        case "NOT_FOUND":
                            log.AddEntry($"Неверный код: {dm?.Raw}", LogColorCode.Red);
                            workSession.Rejected++;
                            break;
                        case "DUPLICATE":
                            if (workSession.WorkMode == WorkMode.SkipDuplicates)
                                rejectCts.Cancel();
                            log.AddEntry($"Дубликат: {dm?.Raw}", LogColorCode.Red);
                            workSession.Rejected++;
                            break;
                    }

                    if (_lastFrame != null)
                        imageSaver.SaveReject(_lastFrame, dm?.Normalized);
                }

                CodeChecked?.Invoke(dm?.Normalized ?? "<NO READ>", result.IsOk);
            });
        }
        private void PollSensor(object? sender, ElapsedEventArgs e)
        {
            try
            {
                int signal = sensor.Read();

                if (signal == 1 && _currentSignal == 0 && !_filter)
                {
                    MotionDetected?.Invoke(this, EventArgs.Empty);
                    _filter = true;
                }

                _currentSignal = signal;

                Task.Delay(MotionFilterInterval).ContinueWith(_ => _filter = false);
            }
            catch { }
        }
        private async Task<ValidationResult> ValidateAsync(string? dm)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(dm))
                    return ValidationResult.NoRead();

                if (!workSession.Codes.TryGetValue(dm, out bool status))
                    return ValidationResult.NotFound();

                if (status)
                    return ValidationResult.Duplicate();

                workSession.Codes[dm] = true;

                _ = Task.Run(async () =>
                {
                    await using var db = await dbContextFactory.CreateDbContextAsync();
                    printer_base? code = await db.printer_bases.FirstAsync(c => c.Code == dm);

                    db.tmp_mains.Add(new()
                    {
                        Code = code.Code,
                        StatusId = 2,
                        DateImport = code.DateImport,
                        DatePrint = code.DatePrint,
                        DateVerify = DateTime.Now,
                        GtinId = code.GtinId,
                        OperatorName = code.OperatorName,
                        OrderID = code.OrderID
                    });

                    await db.SaveChangesAsync();
                });

                return ValidationResult.Ok();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Возникла ошибка при валидации кода в базе данных.\n\nException message:\n\n{ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return ValidationResult.NotFound();
            }
        }
    }
}
