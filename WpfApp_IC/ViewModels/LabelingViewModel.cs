using Microsoft.EntityFrameworkCore;
using Observable;
using WpfApp_IC.Data;

namespace WpfApp_IC.ViewModels
{
    /// <summary>
    /// Модель печати и проверки маркировок
    /// </summary>
    public class LabelingViewModel(MainViewModel mainViewModel, LabelingSession labelingSession, LabelPrintingViewModel labelPrintingViewModel, CameraViewModel cameraViewModel, IDbContextFactory<AppDbContext> dbContextFactory) : ObservableObject
    {
        public LabelingSession LabelingSession => labelingSession;
        public CameraViewModel CameraViewModel => cameraViewModel;
        public LabelPrintingViewModel LabelPrintingViewModel => labelPrintingViewModel;

        public async Task WorkInitiate()
        {
            try
            {
                await Task.Run(LabelPrintingViewModel.PrintInitiate);
            }
            catch { }
        }
        public async Task WorkTerminate()
        {
            await LabelPrintingViewModel.PrintTerminateAsync();
        }
        public void Exit()
        {
            LabelingSession.Reset();
            mainViewModel.CurrentViewModel = mainViewModel.GetViewModel<HomeViewModel>();
        }
    }
}
