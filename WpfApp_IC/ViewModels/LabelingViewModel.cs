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
                await LabelPrintingViewModel.PrintInitiate();

                await using var db = await dbContextFactory.CreateDbContextAsync();
                db.printer_tasks.Add(LabelingSession.CurrentTask);
                await db.SaveChangesAsync();
            }
            catch { }
        }
        public void WorkTerminate()
        {
            LabelPrintingViewModel.PrintTerminate();
        }
        public void Exit() => mainViewModel.CurrentViewModel = mainViewModel.GetViewModel<HomeViewModel>();
    }
}
