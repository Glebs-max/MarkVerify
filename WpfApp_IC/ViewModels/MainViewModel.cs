using Microsoft.Extensions.DependencyInjection;
using Observable;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WpfApp_IC.Services.Inspectors;

namespace WpfApp_IC.ViewModels
{
    public class MainViewModel : ObservableObject
    {
        private readonly IServiceProvider _provider;
        private string _machineName = "Машина 1";
        private ObservableObject? _currentViewModel = null;
        private VideojetPrinter _videojetPrinter;
        private IInspectorController _inspectorController;

        public MainViewModel(IServiceProvider provider, VideojetPrinter videojetPrinter, IInspectorController inspector)
        {
            _provider = provider;
            _videojetPrinter = videojetPrinter;
            _inspectorController = inspector;
        }

        public string MachineName
        {
            get => _machineName;
            set => Set(ref _machineName, value);
        }
        public ObservableObject? CurrentViewModel
        {
            get => _currentViewModel;
            set => Set(ref _currentViewModel, value);
        }
        public VideojetPrinter VideojetPrinter
        {
            get => _videojetPrinter;
            set => Set(ref _videojetPrinter, value);
        }
        public IInspectorController InspectorController
        {
            get => _inspectorController;
            set => Set(ref _inspectorController, value);
        }

        public T GetViewModel<T>() where T : ObservableObject => _provider.GetRequiredService<T>();
    }
}
