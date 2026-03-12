using LabelDesigner;
using LabelDesigner.Models;
using Microsoft.EntityFrameworkCore;
using Observable;
using System.Windows;
using System.Windows.Threading;
using WpfApp_IC.Data;
using WpfApp_IC.Models;

namespace WpfApp_IC.ViewModels
{
    public class LabelPrintingViewModel (VideojetErrorsViewModel videojetErrorsViewModel, VideojetPrinter videojetPrinter, LabelingSession labelingSession, IDbContextFactory<AppDbContext> dbContextFactory) : ObservableObject
    {
        private readonly DispatcherTimer _printerStatusCheck = new() { Interval = TimeSpan.FromMilliseconds(1000) };
        private DesignerViewModel _designerViewModel = new();
        private bool _initiated;

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
            await using var db = await dbContextFactory.CreateDbContextAsync();
            db.printer_tasks.Add(LabelingSession.CurrentTask = new() { created_at = DateTime.Now, last_used_at = DateTime.Now });
            await db.SaveChangesAsync();

            VideojetPrinter.ConnectionEstablished += async () =>
            {
                _printerStatusCheck.Start();

                if (!_initiated)
                {
                    await VideojetPrinter.ClearQueueAsync();
                    _initiated = true;
                }
            };
            VideojetPrinter.ConnectionLost += _printerStatusCheck.Stop;
            VideojetPrinter.QueueSizeChanged += async (size) =>
            {
                if (VideojetPrinter.Connected && size <= VideojetPrinter.MaxQueueSize / 3)
                    await QueueLabel(VideojetPrinter.MaxQueueSize - VideojetPrinter.QueueSize);
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
            VideojetPrinter.Disconnect();
            await using var db = await dbContextFactory.CreateDbContextAsync();
            LabelingSession.CurrentTask.last_used_at = DateTime.Now;
            await db.SaveChangesAsync();
        }

        /// <summary>
        /// Пополнение очереди принтера
        /// </summary>
        private async Task QueueLabel(int? count = null)
        {
            if (!VideojetPrinter.Connected)
                return;

            BarcodeField? DataMatrix = DesignerViewModel.Fields.OfType<BarcodeField>().FirstOrDefault(f => f.DataType == DataType.Database);

            if (DataMatrix != null)
            {
                await using var db = await dbContextFactory.CreateDbContextAsync();
                List<printer_base> codes = await db.printer_bases.Where(c => c.GtinId == LabelingSession.GTIN.GtinId && c.StatusId == 0 && (DateTime.Now - (c.DateImport ?? DateTime.MinValue)).Days < 25).Take(count ?? VideojetPrinter.MaxQueueSize).ToListAsync();

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
                    code.task = LabelingSession.CurrentTask;
                    code.code_number = ++LabelingSession.Count;

                    LabelingSession.GTIN.CountAviable--;
                }

                //await db.SaveChangesAsync();
            }
        }
    }
}
