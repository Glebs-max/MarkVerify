using Microsoft.EntityFrameworkCore;
using System.Timers;
using System.Windows;
using System.Windows.Media.Imaging;
using WpfApp_IC.Data;
using WpfApp_IC.Models;
using WpfApp_IC.Models.DbContext;
using WpfApp_IC.Models.Devices;
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
        HikrobotCamera camera,
        ModbusSensor sensor,
        ModbusRejector rejector,
        MindeoScanner scanner,
        IModbusService modbus,
        IDbContextFactory<AppDbContext> dbContextFactory,
        ImageSaverService imageSaver)
    {
        private Timer? _sensorPollTimer;
        private bool _filter;
        private BitmapSource? _lastFrame;
        private int _currentSignal = 0;

        public string ModbusIp { get; set; } = "192.168.0.127";
        public int ModbusPort { get; set; } = 502;
        public int RejectDelay { get; set; } = 300;
        public int MotionFilterInterval { get; set; } = 200;
        public int SensorPollInterval { get; set; } = 10;

        public event Action<string, bool>? CodeChecked;
        public event Action? MotionDetected;

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
                camera.Connect();
                log.AddEntry($"Соединение с камерой установлено [{camera.IP}]");
            }
            catch
            {
                log.AddEntry($"Не удалось установить соединение с камерой [{camera.IP}]", LogColorCode.Red);
            }

            MotionDetected += Inspect;

            camera.FrameReceived += CaptureLastFrame;
            scanner.DataMatrixRead += ScannerHandler;

            _sensorPollTimer = new Timer(SensorPollInterval);
            _sensorPollTimer.Elapsed += PollSensor;
            _sensorPollTimer.AutoReset = true;
            _sensorPollTimer.Start();
        }

        public void Stop()
        {
            MotionDetected -= Inspect;

            camera.FrameReceived -= CaptureLastFrame;
            scanner.DataMatrixRead -= ScannerHandler;

            if (_sensorPollTimer != null)
            {
                _sensorPollTimer.Stop();
                _sensorPollTimer.Elapsed -= PollSensor;
                _sensorPollTimer.Dispose();
                _sensorPollTimer = null;
            }

            camera.Disconnect();
            modbus?.Disconnect();
        }

        private void Inspect()
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

                string? code = null;

                try
                {
                    code = camera.TriggerSnapshot();
                }
                catch { }

                ValidationResult result = VerifyCode(code);

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
                            log.AddEntry($"Неверный код: {code}", LogColorCode.Red);
                            workSession.Rejected++;
                            break;
                        case "DUPLICATE":
                            log.AddEntry($"Дубликат: {code}", LogColorCode.Red);
                            workSession.Rejected++;
                            break;
                    }

                    if (_lastFrame != null)
                        imageSaver.SaveReject(_lastFrame, code);
                }

                CodeChecked?.Invoke(code ?? "<NO READ>", result.IsOk);
            });
        }
        private void PollSensor(object? sender, ElapsedEventArgs e)
        {
            try
            {
                int signal = sensor.Read();

                if (signal == 1 && _currentSignal == 0 && !_filter)
                {
                    _filter = true;
                    MotionDetected?.Invoke();
                    Task.Delay(MotionFilterInterval).ContinueWith(_ => _filter = false);
                }

                _currentSignal = signal;
            }
            catch { }
        }
        private ValidationResult VerifyCode(string? code)
        {
            if (string.IsNullOrWhiteSpace(code))
                return ValidationResult.NoRead();

            bool verified;

            switch (workSession.WorkMode)
            {
                case WorkMode.Default:
                    if (!workSession.Codes.TryGetValue(code, out verified))
                        return ValidationResult.NotFound();

                    if (verified)
                        return ValidationResult.Duplicate();

                    workSession.Codes[code] = true;
                    break;
                case WorkMode.NoPrint:
                    if (workSession.Codes.TryGetValue(code, out verified) && verified)
                        return ValidationResult.Duplicate();

                    workSession.Codes.Add(code, true);
                    break;
                case WorkMode.SkipDuplicates:
                    return ValidationResult.Ok();
            }

            Task.Run(async () =>
            {
                try
                {
                    await using var db = await dbContextFactory.CreateDbContextAsync();
                    printer_base? printed = await db.printer_bases.FirstOrDefaultAsync(c => c.Code == code);

                    db.tmp_mains.Add(new()
                    {
                        Code = printed?.Code ?? code,
                        StatusId = 2,
                        DateImport = printed?.DateImport,
                        DatePrint = printed?.DatePrint,
                        DateVerify = DateTime.Now,
                        GtinId = printed?.GtinId ?? workSession.GTIN?.GtinId ?? 0,
                        OperatorName = printed?.OperatorName ?? workSession.MachineName,
                        OrderID = printed?.OrderID
                    });

                    await db.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при занесении кода в базу данных.\n\nException message:\n\n{ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            });

            return ValidationResult.Ok();
        }
        private bool RejectCode(string? code)
        {
            if (string.IsNullOrWhiteSpace(code))
                return false;

            if (workSession.Codes.TryGetValue(code, out bool verified) && verified)
            {
                workSession.Codes.Remove(code);

                Task.Run(async () =>
                {
                    try
                    {
                        await using var db = await dbContextFactory.CreateDbContextAsync();
                        tmp_main? reject = await db.tmp_mains.FirstOrDefaultAsync(c => c.Code == code);

                        if (reject != null)
                        {
                            db.tmp_mains.Remove(reject);
                            await db.SaveChangesAsync();
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка при удалении кода из базы данных.\n\nException message:\n\n{ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                });

                return true;
            }

            return false;
        }
        private void ScannerHandler(string code)
        {
            Task.Run(async () =>
            {
                switch (workSession.ScannerMode)
                {
                    case ScannerMode.Verify:
                        ValidationResult result = VerifyCode(code);

                        if (result.IsOk)
                        {
                            workSession.Verified++;
                            log.AddEntry($"[СКАНЕР] Код успешно верифицирован: {code}");
                        }
                        else
                        {
                            switch (result.ErrorCode)
                            {
                                case "NO_READ":
                                    log.AddEntry($"[СКАНЕР] Код не считан: NULL", LogColorCode.Yellow);
                                    break;
                                case "NOT_FOUND":
                                    log.AddEntry($"[СКАНЕР] Неверный код: {code}", LogColorCode.Red);
                                    break;
                                case "DUPLICATE":
                                    log.AddEntry($"[СКАНЕР] Дубликат: {code}", LogColorCode.Red);
                                    break;
                            }
                        }
                        break;
                    case ScannerMode.Reject:
                        if (RejectCode(code))
                        {
                            workSession.Rejected++;
                            log.AddEntry($"[СКАНЕР] Код успешно отбракован: {code}");
                        }
                        else
                        {
                            log.AddEntry($"[СКАНЕР] Не удалось отбраковать код: {code}");
                        }

                        break;
                }
            });
        }
        private void CaptureLastFrame(BitmapSource frame) => _lastFrame = frame;
    }
}
