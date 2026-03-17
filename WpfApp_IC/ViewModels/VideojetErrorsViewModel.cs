using Observable;
using System.Collections.ObjectModel;
using System.Windows;

namespace WpfApp_IC.ViewModels
{
    public class VideojetErrorsViewModel : ObservableObject
    {
        private readonly VideojetPrinter _videojetPrinter;

        public VideojetErrorsViewModel(VideojetPrinter videojetPrinter)
        {
            _videojetPrinter = videojetPrinter;

            _videojetPrinter.ErrorListChanged += () =>
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    foreach (VideojetPrinterError error in Errors.ToList())
                    {
                        if (!_videojetPrinter.Errors.Contains(error))
                            Errors.Remove(error);
                    }
                    foreach (VideojetPrinterError error in _videojetPrinter.Errors.ToList())
                    {
                        if (!Errors.Contains(error))
                            Errors.Add(error);
                    }
                });
            };
        }

        public ObservableCollection<VideojetPrinterError> Errors { get; set; } = [];

        public async Task ClearErrorsAsync()
        {
            await _videojetPrinter.ClearAllFaultsAsync();
            await _videojetPrinter.ClearAllWarningsAsync();
        }
    }
}
