using Microsoft.Extensions.DependencyInjection;
using Observable;
using WpfApp_IC.Services.Inspectors;

namespace WpfApp_IC.ViewModels
{
    /// <summary>
    /// Базовая модель приложения
    /// </summary>
    public class MainViewModel(IServiceProvider provider) : ObservableObject
    {
        private string _machineName = "Машина 1";
        private ObservableObject? _currentViewModel = null;

        public string MachineName
        {
            get => _machineName;
            set => Set(ref _machineName, value);
        }
        /// <summary>
        /// Текущая ViewModel - определяет текущую страницу приложения
        /// </summary>
        public ObservableObject? CurrentViewModel
        {
            get => _currentViewModel;
            set => Set(ref _currentViewModel, value);
        }

        /// <summary>
        /// Получение требуемой ViewModel, зарегистрированной в App()
        /// </summary>
        public T GetViewModel<T>() where T : ObservableObject => provider.GetRequiredService<T>();
    }
}
