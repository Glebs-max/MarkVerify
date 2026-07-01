using Microsoft.Extensions.DependencyInjection;
using Observable;

namespace WpfApp_IC.ViewModels
{
    /// <summary>
    /// Базовая модель приложения
    /// </summary>
    public class MainViewModel(IServiceProvider provider) : ObservableObject
    {
        private ObservableObject? _currentViewModel = null;

        /// <summary>
        /// Текущая ViewModel - определяет текущую страницу приложения
        /// </summary>
        public ObservableObject? CurrentViewModel
        {
            get => _currentViewModel;
            private set => Set(ref _currentViewModel, value);
        }

        /// <summary>
        /// Устанавливает текущую ViewModel, зарегистрированную в App()
        /// </summary>
        public T SetViewModel<T>() where T : ObservableObject => (T)(CurrentViewModel = provider.GetRequiredService<T>());
    }
}
