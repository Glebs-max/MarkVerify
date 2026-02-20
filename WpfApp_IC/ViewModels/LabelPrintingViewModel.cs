using LabelDesigner;
using LabelDesigner.Models;
using LabelDesigner.Services;
using WpfApp_IC.Data;
using WpfApp_IC.Models;
using Microsoft.EntityFrameworkCore;
using Observable;
using WpfApp_IC.Services.Inspectors;
using System.Windows;
using System.Windows.Media;

namespace WpfApp_IC.ViewModels
{
    public enum PrintingStatus
    {
        Printing,
        Paused,
        Finished
    }
    public enum ErrorStatus
    {
        None,
        Warnings,
        Faults
    }

    /// <summary>
    /// Модель печати и проверки маркировок
    /// </summary>
    public class LabelPrintingViewModel(MainViewModel mainViewModel, AppDbContext db, CameraBasicViewModel cameraViewModel) : ObservableObject
    {
        private readonly List<printer_base> _printedCodes = [];
        private PrintingStatus _printingStatus = PrintingStatus.Paused;
        private ErrorStatus _errorStatus = ErrorStatus.None;
        private gtin _gtin = new();
        private DesignerViewModel _designerViewModel = new();
        private printer_task _currentTask = new() { created_at = DateTime.Now, last_used_at = DateTime.Now };
        private int _verified = 0, _rejected = 0, _count = 0;

        public PrintingStatus PrintingStatus
        {
            get => _printingStatus;
            set => Set(ref _printingStatus, value, () =>
            {
                switch (_printingStatus)
                {
                    case PrintingStatus.Printing:
                        if (VideojetPrinter.PrinterState != PrinterState.Running || ErrorStatus == ErrorStatus.Faults)
                            _printingStatus = PrintingStatus.Paused;
                        else
                            TimerService.PrintTimer.Start();
                        break;
                    case PrintingStatus.Paused:
                    case PrintingStatus.Finished:
                        TimerService.PrintTimer.Stop();
                        break;
                }
            });
        }
        public ErrorStatus ErrorStatus
        {
            get => _errorStatus;
            set => Set(ref _errorStatus, value, () =>
            {
                if (_errorStatus == ErrorStatus.Faults)
                    PrintingStatus = PrintingStatus.Paused;
            });
        }
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
        public CameraViewModel CameraViewModel { get => cameraViewModel; }
        public VideojetPrinter VideojetPrinter { get => mainViewModel.VideojetPrinter; }
        public IInspectorController InspectorController { get => mainViewModel.InspectorController; }

        public async Task PrintInitiate()
        {
            try
            {
                //db.printer_tasks.Add(CurrentTask);
                //await db.SaveChangesAsync();

                await VideojetPrinter.GetStateAsync();
                await VideojetPrinter.ClearQueueAsync();
                await VideojetPrinter.StartAsync();
                await QueueLabel();

                //TimerService.PrintTimer.Tick += async (s, e) => await VideojetPrinter.PrintAsync();
                TimerService.QueueSizeTimer.Tick += async (s, e) => await VideojetPrinter.GetQueueSizeAsync();
                TimerService.QueueSizeTimer.Start();

                VideojetPrinter.StateChanged += async (state) =>
                {
                    await VideojetPrinter.GetAllFaultsAsync();
                    await VideojetPrinter.GetAllWarningsAsync();
                };
                VideojetPrinter.QueueStatusChanged += async (status) =>
                {
                    if (status == QueueStatus.QLOW)
                    {
                        await VideojetPrinter.GetQueueSizeAsync();
                        await Application.Current.Dispatcher.InvokeAsync(async () =>
                        {
                            await QueueLabel(VideojetPrinter.MaxQueueSize - VideojetPrinter.QueueSize);
                        });
                    }
                };
                VideojetPrinter.ErrorStateChanged += async (state) =>
                {
                    switch (state)
                    {
                        case ErrorState.None:
                            ErrorStatus = ErrorStatus.None;
                            break;
                        case ErrorState.Warnings:
                            ErrorStatus = ErrorStatus.Warnings;
                            break;
                        case ErrorState.Faults:
                            ErrorStatus = ErrorStatus.Faults;
                            break;
                    }

                    await VideojetPrinter.GetAllFaultsAsync();
                    await VideojetPrinter.GetAllWarningsAsync();
                };
                InspectorController.FrameReceived += async (dm, frame) =>
                {
                    if (db.printer_bases.FirstOrDefault(c => c.Code == dm && c.GtinId == GTIN.GtinId && c.StatusId == 1) is printer_base code)
                    {
                        code.StatusId = 77;
                        Verified++;
                        CameraViewModel.DataMatrixBrush = Brushes.LimeGreen;
                    }
                    else
                    {
                        (InspectorController as InspectorController)?.RejectWithDelay();
                        Rejected++;
                        CameraViewModel.DataMatrixBrush = Brushes.Red;
                        CameraViewModel.AddLog(string.IsNullOrEmpty(dm) ? "DataMatrix не считан" : "Неверный код");
                    }

                    await db.SaveChangesAsync();

                    CameraViewModel.Frame = frame;
                    CameraViewModel.DataMatrix = dm ?? "<NO READ>";
                };
            }
            catch { }
        }
        public async Task PrintTerminate()
        {
            try
            {
                PrintingStatus = PrintingStatus.Finished;
                await VideojetPrinter.StopAsync();
            }
            catch { }
        }
        public void Exit() => mainViewModel.CurrentViewModel = mainViewModel.GetViewModel<HomeViewModel>();

        /// <summary>
        /// Пополнение очереди принтера
        /// </summary>
        private async Task QueueLabel(int? count = null)
        {
            BarcodeField? DataMatrix = DesignerViewModel.Fields.OfType<BarcodeField>().FirstOrDefault(f => f.DataType == DataType.Database);

            if (DataMatrix != null)
            {
                List<printer_base> codes = await LoadCodesAsync(count ?? VideojetPrinter.MaxQueueSize);

                foreach (printer_base code in codes)
                {
                    DataMatrix.BarcodeData = code.Code;

                    await VideojetPrinter.SendZplAsync(DesignerViewModel.ConvertToZpl());

                    code.StatusId = 1;
                    InspectorController.CurrentGtinId = GTIN.GtinId;
                    code.DatePrint = DateTime.Now;
                    code.OperatorName = mainViewModel.MachineName;
                    code.task = CurrentTask;
                    code.code_number = ++Count;

                    _printedCodes.Add(code);
                }

                await db.SaveChangesAsync();
            }
        }
        /// <summary>
        /// Загрузка кодов для DataMatrix из БД
        /// </summary>
        private async Task<List<printer_base>> LoadCodesAsync(int count) => await db.printer_bases.Where(c => c.GtinId == GTIN.GtinId && c.StatusId == 0).Take(count).ToListAsync();
    }
}
