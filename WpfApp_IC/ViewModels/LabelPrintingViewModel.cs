using LabelDesigner;
using LabelDesigner.Models;
using LabelDesigner.Services;
using WpfApp_IC.Data;
using WpfApp_IC.Models;
using Microsoft.EntityFrameworkCore;
using Observable;

namespace WpfApp_IC.ViewModels
{
    public enum PrintingStatus
    {
        Printing,
        Paused,
        Finished,
        Error,
        Warning
    }

    public class LabelPrintingViewModel(MainViewModel mainViewModel, AppDbContext db, VideojetPrinter videojetPrinter) : ObservableObject
    {
        private PrintingStatus _status;
        private gtin _gtin = new();
        private DesignerViewModel _designerViewModel = new();
        private int _verified, _rejected;
        private bool _active;

        public PrintingStatus Status
        {
            get => _status;
            set => Set(ref _status, value);
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
        public bool Active
        {
            get => _active;
            set => Set(ref _active, value, () =>
            {
                if (value)
                {
                    TimerService.PrintTimer.Start();
                    Status = PrintingStatus.Printing;
                }
                else
                {
                    TimerService.PrintTimer.Stop();
                    Status = PrintingStatus.Paused;
                }
            });
        }
        public int QueueSize => videojetPrinter.QueueSize;

        private BarcodeField? DataMatrix => DesignerViewModel.Fields.OfType<BarcodeField>().FirstOrDefault(f => f.DataType == DataType.Database);

        public async Task PrintInitiate()
        {
            try
            {
                await ClearQueueAsync();
                await QueueLabel();
                await StartPrinter();

                TimerService.PrintTimer.Tick += async (s, e) => await PrintAsync();
                videojetPrinter.QueueStatusChanged += async (status) =>
                {
                    switch (status)
                    {
                        case QueueStatus.QLOW:
                            await QueueLabel(videojetPrinter.MaxQueueSize - videojetPrinter.QueueSize);
                            break;
                    }
                };
                videojetPrinter.StateChanged += (state) => Active = false;
                videojetPrinter.ErrorStateChanged += (state) =>
                {
                    Active = false;

                    switch (state)
                    {
                        case ErrorState.None:
                            Status = PrintingStatus.Paused;
                            break;
                        case ErrorState.Warnings:
                            Status = PrintingStatus.Warning;
                            break;
                        case ErrorState.Faults:
                            Status = PrintingStatus.Error;
                            break;
                    }
                };
                TimerService.PrintTimer.Start();

                Active = true;
            }
            catch { }
        }
        public async Task PrintTerminate()
        {
            try
            {
                await StopPrinter();
                Active = false;
                Status = PrintingStatus.Finished;
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
        private async Task ClearQueueAsync() => await videojetPrinter.ClearQueueAsync();
        private async Task SendZplAsync(string zpl) => await videojetPrinter.SendZplAsync(zpl);
        private async Task PrintAsync() => await videojetPrinter.PrintAsync();
        private async Task StartPrinter() => await videojetPrinter.StartAsync();
        private async Task StopPrinter() => await videojetPrinter.StopAsync();
    }
}
