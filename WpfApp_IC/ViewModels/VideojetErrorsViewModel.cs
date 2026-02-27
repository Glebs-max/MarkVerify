using Observable;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
                foreach (VideojetPrinterError error in videojetPrinter.Errors)
                    Errors.Add(error);
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
