using Observable;
using System.Collections.ObjectModel;
using System.IO.Ports;
using WpfApp_IC.Models;
using WpfApp_IC.Services;
using WpfApp_IC.Services.Camera;

namespace WpfApp_IC.ViewModels
{
    public class SettingsViewModel(MainViewModel mainViewModel, SettingsService settingsService) : ObservableObject
    {
        public AppSettings AppSettings => settingsService.AppSettings;
        public ObservableCollection<CameraDeviceInfo> AvailableCameras { get; set; } = [];
        public ObservableCollection<string> AvailableCOMPorts { get; set; } = [];

        public void GetAvailableCOMPorts() => AvailableCOMPorts = new(SerialPort.GetPortNames());
        public void ScanAvailableCameras() => AvailableCameras = new(CameraService.ScanAvailableDevices());
        public void Save()
        {
            settingsService.Save();
            mainViewModel.SetViewModel<HomeViewModel>();
        }
        public void GetBack()
        {
            settingsService.Load();
            mainViewModel.SetViewModel<HomeViewModel>();
        }
    }
}
