using Microsoft.EntityFrameworkCore;
using Observable;
using System.IO;
using WpfApp_IC.Data;
using WpfApp_IC.Models;
using WpfApp_IC.Services;

namespace WpfApp_IC.ViewModels
{
    public enum WorkState
    {
        Initiating,
        Ready,
        Active,
        Pause,
        Finished
    }
    public enum ErrorState
    {
        None,
        Warnings,
        Errors
    }

    public class LabelingViewModel(MainViewModel mainViewModel, LogService logService, WorkSession workSession, LabelPrintingViewModel labelPrintingViewModel, CameraViewModel cameraViewModel) : ObservableObject
    {
        private CancellationTokenSource? _workInitiateCts;
        private WorkState _workState = WorkState.Initiating;
        private ErrorState _errorState = ErrorState.None;

        public WorkState WorkState
        {
            get => _workState;
            set => Set(ref _workState, value);
        }
        public ErrorState ErrorState
        {
            get => _errorState;
            set => Set(ref _errorState, value);
        }
        public LogService LogService => logService;
        public WorkSession WorkSession => workSession;
        public CameraViewModel CameraViewModel => cameraViewModel;
        public LabelPrintingViewModel LabelPrintingViewModel => labelPrintingViewModel;

        public async Task WorkInitiate()
        {
            _workInitiateCts = new();
            await Task.Run(async () =>
            {
                LabelPrintingViewModel.PrintInitiate();
                CameraViewModel.Start();

                while (!LabelPrintingViewModel.Ready && !_workInitiateCts.IsCancellationRequested)
                    continue;

                WorkState = WorkState.Ready;
            }, _workInitiateCts.Token);
        }
        public async Task WorkStart()
        {
            await labelPrintingViewModel.PrintStart();
            WorkState = WorkState.Active;
        }
        public async Task WorkPause()
        {
            await LabelPrintingViewModel.PrintPause();
            CameraViewModel.Stop();
            WorkState = WorkState.Pause;
        }
        public async Task WorkContinue()
        {
            await LabelPrintingViewModel.PrintContinue();
            CameraViewModel.Start();
            WorkState = WorkState.Active;
        }
        public async Task WorkTerminate()
        {
            await LabelPrintingViewModel.PrintTerminate();
            CameraViewModel.Stop();
            WorkState = WorkState.Finished;
        }
        public async Task Exit()
        {
            if (WorkState != WorkState.Finished)
                await WorkTerminate();

            _workInitiateCts?.Cancel();
            logService.ClearEntries();
            workSession.Reset();

            mainViewModel.SetViewModel<HomeViewModel>();
        }
    }
}
