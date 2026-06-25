using LabelDesigner;
using LabelDesigner.Models;
using Microsoft.EntityFrameworkCore;
using Observable;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Threading;
using System.Xml;
using WpfApp_IC.Data;
using WpfApp_IC.Models;
using WpfApp_IC.Models.DbContext;
using WpfApp_IC.Models.Devices;
using WpfApp_IC.Views;

namespace WpfApp_IC.ViewModels
{
    public class LabelPrintingViewModel (VideojetErrorsViewModel videojetErrorsViewModel, VideojetPrinter videojetPrinter, LabelingSession labelingSession, IDbContextFactory<AppDbContext> dbContextFactory) : ObservableObject
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
        public LabelingSession LabelingSession => labelingSession;

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
                db.printer_tasks.Add(LabelingSession.CurrentTask = new() { created_at = DateTime.Now, last_used_at = DateTime.Now });
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
                LabelingSession.CurrentTask.last_used_at = DateTime.Now;
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
                    List<printer_base> codes = await db.printer_bases.Where(c => c.GtinId == LabelingSession.GTIN.GtinId && c.StatusId == 0).Take(count ?? VideojetPrinter.MaxQueueSize).ToListAsync();
                    gtin gtin = await db.gtins.FirstAsync(g => g.GtinId == LabelingSession.GTIN.GtinId);

                    foreach (printer_base code in codes)
                    {
                        string zpl = await Application.Current.Dispatcher.InvokeAsync(() => {
                            DesignerViewModel.DataMatrix.BarcodeData = code.Code;
                            return DesignerViewModel.ConvertToZpl();
                        });

                        await VideojetPrinter.SendZplAsync(zpl);
                        //await File.WriteAllTextAsync(Path.Combine(AppContext.BaseDirectory, $"TestZPL/{LabelingSession.Count}.txt"), zpl);

                        code.StatusId = 1;
                        code.DatePrint = DateTime.Now;
                        code.OperatorName = LabelingSession.MachineName;
                        code.task_id = LabelingSession.CurrentTask.id;
                        code.code_number = ++LabelingSession.Count;

                        gtin.CountAviable--;
                        LabelingSession.GTIN.CountAviable--;
                    }

                    await db.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Возникла ошибка при обращении к базе данных. Проверьте соединение с сервером.\n\nException message:\n\n{ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
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
                    _queueLock = false;
                }
            });
        }
        private void OnConnectionEstablished() => Ready = true;
    }
}
