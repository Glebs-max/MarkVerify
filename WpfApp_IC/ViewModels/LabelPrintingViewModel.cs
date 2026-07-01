using LabelDesigner;
using Microsoft.EntityFrameworkCore;
using Observable;
using System.Windows;
using WpfApp_IC.Data;
using WpfApp_IC.Models;
using WpfApp_IC.Models.DbContext;
using WpfApp_IC.Models.Devices;

namespace WpfApp_IC.ViewModels
{
    public class LabelPrintingViewModel (VideojetErrorsViewModel videojetErrorsViewModel, VideojetPrinter videojetPrinter, WorkSession workSession, IDbContextFactory<AppDbContext> dbContextFactory) : ObservableObject
    {
        private DesignerViewModel _designerViewModel = new();
        private bool _queueLock;

        public DesignerViewModel DesignerViewModel
        {
            get => _designerViewModel;
            set => Set(ref _designerViewModel, value);
        }
        public bool Ready { get; set; }
        public VideojetPrinter VideojetPrinter => videojetPrinter;
        public VideojetErrorsViewModel VideojetErrorsViewModel => videojetErrorsViewModel;

        public void PrintInitiate()
        {
            VideojetPrinter.ConnectionEstablished += OnConnectionEstablished;
            VideojetPrinter.Connect();
        }
        public async Task PrintStart()
        {
            try
            {
                await using var db = await dbContextFactory.CreateDbContextAsync();
                db.printer_tasks.Add(workSession.CurrentTask = new() { created_at = DateTime.Now, last_used_at = DateTime.Now });
                await db.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Возникла ошибка при обращении к базе данных. Проверьте соединение с сервером.\n\nException message:\n\n{ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            VideojetPrinter.QueueLow += OnQueueLow;
            await VideojetPrinter.ClearQueueAsync();
            await VideojetPrinter.StartAsync();
        }
        public async Task PrintPause() => await VideojetPrinter.StopAsync();
        public async Task PrintContinue() => await VideojetPrinter.StartAsync();
        public async Task PrintTerminate()
        {
            VideojetPrinter.ConnectionEstablished -= OnConnectionEstablished;
            VideojetPrinter.QueueLow -= OnQueueLow;
            await VideojetPrinter.StopAsync();
            VideojetPrinter.Disconnect();

            try
            {
                await using var db = await dbContextFactory.CreateDbContextAsync();
                db.Attach(workSession.CurrentTask);
                workSession.CurrentTask.last_used_at = DateTime.Now;
                await db.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Возникла ошибка при обращении к базе данных. Проверьте соединение с сервером.\n\nException message:\n\n{ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Пополнение очереди принтера
        /// </summary>
        private async Task QueueLabel(int? count = null)
        {
            if (!VideojetPrinter.Connected)
                return;

            if (DesignerViewModel.DataMatrix != null)
            {
                try
                {
                    await using var db = await dbContextFactory.CreateDbContextAsync();
                    List<printer_base> codes = await db.printer_bases.Where(c => c.GtinId == workSession.GTIN.GtinId && c.StatusId == 0).Take(count ?? VideojetPrinter.MaxQueueSize).ToListAsync();
                    gtin gtin = await db.gtins.FirstAsync(g => g.GtinId == workSession.GTIN.GtinId);

                    foreach (printer_base code in codes)
                    {
                        try { workSession.Codes.Add(code.Code, false); }
                        catch { continue; }

                        await VideojetPrinter.SendZplAsync(Application.Current.Dispatcher.Invoke(() =>
                        {
                            DesignerViewModel.DataMatrix.BarcodeData = code.Code;
                            return DesignerViewModel.ConvertToZpl();
                        }));

                        code.StatusId = 1;
                        code.DatePrint = DateTime.Now;
                        code.OperatorName = workSession.MachineName;
                        code.task_id = workSession.CurrentTask.id;
                        code.code_number = ++workSession.PrintCount;
                        gtin.CountAviable--;

                        workSession.GTIN.CountAviable--;
                    }

                    await db.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Возникла ошибка при обращении к базе данных. Проверьте соединение с сервером.\n\nException message:\n\n{ex.Message}\n\n{ex.InnerException}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
        private void OnQueueLow(int qsz)
        {
            Task.Run(async () =>
            {
                if (_queueLock)
                    return;

                _queueLock = true;

                try
                {
                    if (VideojetPrinter.Connected)
                        await QueueLabel(VideojetPrinter.MaxQueueSize - qsz);
                }
                finally
                {
                    await Task.Delay(1000);
                    _queueLock = false;
                }
            });
        }
        private void OnConnectionEstablished() => Ready = true;
    }
}
