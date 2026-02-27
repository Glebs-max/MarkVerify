using LabelDesigner;
using LabelDesigner.Models;
using Microsoft.EntityFrameworkCore;
using Observable;
using System.Windows;
using System.Windows.Threading;
using WpfApp_IC.Data;
using WpfApp_IC.Models;
using WpfApp_IC.Services.Inspectors;

namespace WpfApp_IC.ViewModels
{
    /// <summary>
    /// Модель печати и проверки маркировок
    /// </summary>
    public class LabelPrintingViewModel(MainViewModel mainViewModel, CameraBasicViewModel cameraViewModel, VideojetErrorsViewModel videojetErrorsViewModel, VideojetPrinter videojetPrinter, IInspectorController inspectorController, IDbContextFactory<AppDbContext> dbContextFactory) : ObservableObject
    {
        private readonly DispatcherTimer _printerStatusCheck = new() { Interval = TimeSpan.FromMilliseconds(1000) };

        private DesignerViewModel _designerViewModel = new();
        private gtin _gtin = new();
        private printer_task _currentTask = new() { created_at = DateTime.Now, last_used_at = DateTime.Now };
        private int _verified = 0, _rejected = 0, _count = 0;

        public gtin GTIN
        {
            get => _gtin;
            set => Set(ref _gtin, value);
        }
        public printer_task CurrentTask
        {
            get => _currentTask;
            set => Set(ref _currentTask, value);
        }
        public DesignerViewModel DesignerViewModel
        {
            get => _designerViewModel;
            set => Set(ref _designerViewModel, value);
        }
        public int Verified
        {
            get => _verified;
            set => Set(ref _verified, value);
        }
        public int Rejected
        {
            get => _rejected;
            set => Set(ref _rejected, value);
        }
        public int Count
        {
            get => _count;
            set => Set(ref _count, value);
        }
        public CameraBasicViewModel CameraViewModel => cameraViewModel;
        public VideojetErrorsViewModel VideojetErrorsViewModel => videojetErrorsViewModel;
        public VideojetPrinter VideojetPrinter => videojetPrinter;
        public IInspectorController InspectorController => inspectorController;

        public async Task PrintInitiate()
        {
            try
            {
                //InspectorController.Start();

                VideojetPrinter.Connect();

                await using var db = await dbContextFactory.CreateDbContextAsync();
                db.printer_tasks.Add(CurrentTask);
                await db.SaveChangesAsync();

                await VideojetPrinter.GetStateAsync();
                await VideojetPrinter.ClearQueueAsync();
                await QueueLabel();

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

                InspectorController.CodeValidated += (result) =>
                {
                    if (result.IsOk)
                        Verified++;
                    else
                        Rejected++;
                };
            }
            catch { }
        }
        public void PrintTerminate() => VideojetPrinter.Disconnect();
        public void Exit() => mainViewModel.CurrentViewModel = mainViewModel.GetViewModel<HomeViewModel>();

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
                List<printer_base> codes = await db.printer_bases.Where(c => c.GtinId == GTIN.GtinId && c.StatusId == 0).Take(count ?? VideojetPrinter.MaxQueueSize).ToListAsync();

                foreach (printer_base code in codes)
                {
                    await VideojetPrinter.SendZplAsync(await Application.Current.Dispatcher.InvokeAsync(() =>
                    {
                        DataMatrix.BarcodeData = code.Code;
                        return DesignerViewModel.ConvertToZpl();
                    }));

                    code.StatusId = 1;
                    code.DatePrint = DateTime.Now;
                    code.OperatorName = mainViewModel.MachineName;
                    code.task = CurrentTask;
                    code.code_number = ++Count;
                }

                //await db.SaveChangesAsync();
            }
        }
    }
}
