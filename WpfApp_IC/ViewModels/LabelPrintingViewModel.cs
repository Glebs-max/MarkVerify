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
            VideojetPrinter.Connect();

            await VideojetPrinter.GetStateAsync();
            await VideojetPrinter.ClearQueueAsync();

            _printerStatusCheck.Tick += async (s, e) =>
            {
                await VideojetPrinter.GetQueueSizeAsync();
                await VideojetPrinter.GetStateAsync();
                await VideojetPrinter.GetAllFaultsAsync();
                await VideojetPrinter.GetAllWarningsAsync();
            };
            _printerStatusCheck.Start();

            VideojetPrinter.QueueSizeChanged += async (size) =>
            {
                if (size <= VideojetPrinter.MaxQueueSize / 3)
                    await QueueLabel(VideojetPrinter.MaxQueueSize - VideojetPrinter.QueueSize);
            };
        }
        public void PrintTerminate() => VideojetPrinter.Disconnect();

        /// <summary>
        /// Пополнение очереди принтера
        /// </summary>
        private async Task QueueLabel(int? count = null)
        {
            if (VideojetPrinter.PrinterState == PrinterState.Disconnected || VideojetPrinter.PrinterState == PrinterState.Connecting)
                return;

            BarcodeField? DataMatrix = DesignerViewModel.Fields.OfType<BarcodeField>().FirstOrDefault(f => f.DataType == DataType.Database);

            if (DataMatrix != null)
            {
                await using var db = await dbContextFactory.CreateDbContextAsync();
                List<printer_base> codes = await db.printer_bases.Where(c => c.GtinId == LabelingSession.GTIN.GtinId && c.StatusId == 0).Take(count ?? VideojetPrinter.MaxQueueSize).ToListAsync();

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
                }

                //await db.SaveChangesAsync();
            }
        }
    }
}
