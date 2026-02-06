using LabelDesigner;
using LabelDesigner.Models;
using LabelDesigner.Services;
using WpfApp_IC.Data;
using WpfApp_IC.Models;
using Microsoft.EntityFrameworkCore;
using Observable;
using WpfApp_IC.Services.Inspectors;

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

    public class LabelPrintingViewModel(MainViewModel mainViewModel, AppDbContext db, VideojetPrinter videojetPrinter) : ObservableObject
    {
        private PrintingStatus _printingStatus;
        private ErrorStatus _errorStatus;
        private gtin _gtin = new();
        private DesignerViewModel _designerViewModel = new();
        private CameraBasicViewModel _cameraBasicViewModel = new(mainViewModel.InspectorController);
        private int _verified = 0, _rejected = 0;

        public PrintingStatus PrintingStatus
        {
            get => _printingStatus;
            set => Set(ref _printingStatus, value, () =>
            {
                switch (_printingStatus)
                {
                    case PrintingStatus.Printing:
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
        public DesignerViewModel DesignerViewModel
        {
            get => _designerViewModel;
            set => Set(ref _designerViewModel, value);
        }
        public CameraBasicViewModel CameraBasicViewModel
        {
            get => _cameraBasicViewModel;
            set => Set(ref _cameraBasicViewModel, value);
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
        public VideojetPrinter VideojetPrinter => videojetPrinter;
        public IInspectorController InspectorController => mainViewModel.InspectorController;

        private BarcodeField? DataMatrix => DesignerViewModel.Fields.OfType<BarcodeField>().FirstOrDefault(f => f.DataType == DataType.Database);

        public async Task PrintInitiate()
        {
            try
            {
                await ClearQueueAsync();
                await QueueLabel();
                await StartPrinter();

                TimerService.PrintTimer.Tick += async (s, e) => await PrintAsync();
                TimerService.QueueSizeTimer.Tick += async (s, e) => await GetQueueSize();
                TimerService.QueueSizeTimer.Start();

                videojetPrinter.StateChanged += (state) =>
                {
                    if (state != PrinterState.Running)
                        PrintingStatus = PrintingStatus.Paused;
                };
                videojetPrinter.QueueStatusChanged += async (status) =>
                {
                    if (status == QueueStatus.QLOW)
                        await QueueLabel(videojetPrinter.MaxQueueSize - videojetPrinter.QueueSize);
                };
                videojetPrinter.ErrorStateChanged += (state) =>
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
                };
                InspectorController.FrameReceived += (dm, frame) =>
                {
                    if (dm != null)
                    {
                        
                    }
                    if (InspectorController is InspectorController inspector)
                        inspector.RejectWithDelay();
                };

                PrintingStatus = PrintingStatus.Printing;
            }
            catch { }
        }
        public async Task PrintTerminate()
        {
            try
            {
                PrintingStatus = PrintingStatus.Finished;
                await StopPrinter();
            }
            catch { }
        }
        public void Exit() => mainViewModel.CurrentViewModel = mainViewModel.GetViewModel<HomeViewModel>();

        private async Task QueueLabel(int? count = null)
        {
            if (DataMatrix != null)
            {
                List<printer_base> codes = await LoadCodesAsync(count ?? videojetPrinter.MaxQueueSize);

                foreach (printer_base code in codes)
                {
                    DataMatrix.BarcodeData = code.Code;
                    await SendZplAsync(DesignerViewModel.ConvertToZpl());
                    code.StatusId = 1;
                }

                await db.SaveChangesAsync();
            }
        }
        private async Task<List<printer_base>> LoadCodesAsync(int count) => await db.printer_bases.Where(c => c.GtinId == GTIN.GtinId && c.StatusId == 0).Take(count).ToListAsync();
        private async Task SendZplAsync(string zpl) => await videojetPrinter.SendZplAsync(zpl);
        private async Task GetQueueSize() => await videojetPrinter.GetQueueSize();
        private async Task ClearQueueAsync() => await videojetPrinter.ClearQueueAsync();
        private async Task PrintAsync() => await videojetPrinter.PrintAsync();
        private async Task StartPrinter() => await videojetPrinter.StartAsync();
        private async Task StopPrinter() => await videojetPrinter.StopAsync();
    }
}
