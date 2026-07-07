using Observable;
using System.Collections.ObjectModel;
using System.IO.Ports;
using WpfApp_IC.Models;
using WpfApp_IC.Models.Devices;
using WpfApp_IC.Services;

namespace WpfApp_IC.ViewModels
{
    public class SettingsViewModel(MainViewModel mainViewModel, SettingsService settingsService) : ObservableObject
    {
        public AppSettings AppSettings => settingsService.AppSettings;
        public ObservableCollection<HikrobotDeviceInfo> AvailableCameras { get; set; } = [];
        public ObservableCollection<string> AvailableCOMPorts { get; set; } = [];

        public void GetAvailableCOMPorts()
        {
            AvailableCOMPorts.Clear();
            string[] ports = SerialPort.GetPortNames();
            foreach (string port in ports)
                AvailableCOMPorts.Add(port);
        }
        public void ScanAvailableCameras()
        {
            AvailableCameras.Clear();
            List<HikrobotDeviceInfo> devices = HikrobotService.ScanAvailableDevices();
            foreach (HikrobotDeviceInfo device in devices)
                AvailableCameras.Add(device);
        }
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
