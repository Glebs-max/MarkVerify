using LabelDesigner;
using LabelDesigner.Models;
using Microsoft.EntityFrameworkCore;
using Observable;
using System.Diagnostics;
using System.Windows;
using System.Windows.Threading;
using System.Xml;
using WpfApp_IC.Data;
using WpfApp_IC.Devices;
using WpfApp_IC.Models;

namespace WpfApp_IC.ViewModels
{
    public class LabelPrintingViewModel (VideojetErrorsViewModel videojetErrorsViewModel, VideojetPrinter videojetPrinter, LabelingSession labelingSession, IDbContextFactory<AppDbContext> dbContextFactory) : ObservableObject
    {
        private readonly DispatcherTimer _printerStatusCheck = new() { Interval = TimeSpan.FromMilliseconds(750) };
        private DesignerViewModel _designerViewModel = new();
        private bool _initiated, _queueLock;

        public DesignerViewModel DesignerViewModel
        {
            get => _designerViewModel;
            set => Set(ref _designerViewModel, value);
        }
        public VideojetPrinter VideojetPrinter => videojetPrinter;
        public VideojetErrorsViewModel VideojetErrorsViewModel => videojetErrorsViewModel;
        public LabelingSession LabelingSession => labelingSession;

        public async Task PrintInitiate()
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


            VideojetPrinter.ConnectionEstablished += async () =>
            {
                _printerStatusCheck.Start();
                
                if (!_initiated)
                {
                    await VideojetPrinter.ClearQueueAsync();
                    await QueueLabel(VideojetPrinter.MaxQueueSize);
                    _initiated = true;
                }

                await VideojetPrinter.StartAsync();
            };
            VideojetPrinter.ConnectionLost += _printerStatusCheck.Stop;
            VideojetPrinter.QueueSizeChanged += async (size) =>
            {
                if (!_queueLock && VideojetPrinter.Connected && size < VideojetPrinter.MaxQueueSize / 2)
                {
                    _queueLock = true;
                    await QueueLabel(VideojetPrinter.MaxQueueSize - size);
                    _queueLock = false;
                }
            };
            _printerStatusCheck.Tick += async (s, e) =>
            {
                if (VideojetPrinter.Connected)
                {
                    await VideojetPrinter.GetQueueSizeAsync();
                    await VideojetPrinter.GetStateAsync();
                    await VideojetPrinter.GetAllFaultsAsync();
                    await VideojetPrinter.GetAllWarningsAsync();
                }
            };

            VideojetPrinter.Connect();
        }
        public async Task PrintPause() => await VideojetPrinter.StopAsync();
        public async Task PrintContinue() => await VideojetPrinter.StartAsync();
        public async Task PrintTerminate()
        {
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
            if (!VideojetPrinter.Connected) return;

            BarcodeField? DataMatrix = DesignerViewModel.Fields.OfType<BarcodeField>().FirstOrDefault(f => f.DataType == DataType.Database);

            if (DataMatrix != null)
            {
                try
                {
                    await using var db = await dbContextFactory.CreateDbContextAsync();
                    List<printer_base> codes = await db.printer_bases.Where(c => c.GtinId == LabelingSession.GTIN.GtinId && c.StatusId == 0).Take(count ?? VideojetPrinter.MaxQueueSize).ToListAsync();
                    gtin gtin = await db.gtins.FirstAsync(g => g.GtinId == LabelingSession.GTIN.GtinId);

                    foreach (printer_base code in codes)
                    {
                        await VideojetPrinter.SendZplAsync(await Application.Current.Dispatcher.InvokeAsync(() =>
                        {
                            DataMatrix.BarcodeData = code.Code;
                            return DesignerViewModel.ConvertToZpl();
                        }));

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
    }
}
