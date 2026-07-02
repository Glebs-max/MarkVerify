using Observable;
using System.Collections.ObjectModel;
using WpfApp_IC.Models;
using WpfApp_IC.Services;
using WpfApp_IC.Services.Camera;

namespace WpfApp_IC.ViewModels
{
    public class SettingsViewModel(MainViewModel mainViewModel, AppSettings appSettings, SettingsService settingsService) : ObservableObject
    {
        public AppSettings AppSettings => appSettings;
        public ObservableCollection<CameraDeviceInfo> Cameras = [];

        public void ScanAvailableCameras() => Cameras = new(CameraService.ScanAvailableDevices());
        public void Save() => settingsService.Save();
        public void GetBack() => mainViewModel.SetViewModel<HomeViewModel>();
    }
}
