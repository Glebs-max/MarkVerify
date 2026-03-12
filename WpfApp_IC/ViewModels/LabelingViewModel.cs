using Microsoft.EntityFrameworkCore;
using Observable;
using WpfApp_IC.Data;

namespace WpfApp_IC.ViewModels
{
    public enum WorkState
    {
        Ready,
        Active,
        Pause,
        Finished
    }

    /// <summary>
    /// Модель печати и проверки маркировок
    /// </summary>
    public class LabelingViewModel(MainViewModel mainViewModel, LabelingSession labelingSession, LabelPrintingViewModel labelPrintingViewModel, CameraViewModel cameraViewModel, IDbContextFactory<AppDbContext> dbContextFactory) : ObservableObject
    {
        private WorkState _workState = WorkState.Ready;

        public WorkState WorkState
        {
            get => _workState;
            set => Set(ref _workState, value);
        }
        public LabelingSession LabelingSession => labelingSession;
        public CameraViewModel CameraViewModel => cameraViewModel;
        public LabelPrintingViewModel LabelPrintingViewModel => labelPrintingViewModel;

        public async Task WorkInitiate()
        {
            await LabelPrintingViewModel.PrintInitiate();
            CameraViewModel.Start();
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
        public async Task Report()
        {

        }
        public void Exit()
        {
            LabelingSession.Reset();
            mainViewModel.CurrentViewModel = mainViewModel.GetViewModel<HomeViewModel>();
        }
    }
}
