using Observable;
using System.Diagnostics;
using System.Windows;
using Microsoft.Win32;
using WpfApp_IC.Services.SettingsD;

namespace WpfApp_IC.ViewModels
{
    public class CameraAdvancedViewModel : ObservableObject
    {
        private readonly AppSettings _settings;

        public CameraAdvancedViewModel()
        {
            _settings = SettingsService.Load();

            RejectDelayMs = _settings.RejectDelayMs;
            IdmvsPath = _settings.IdmvsPath;
        }

        // === Свойства ===

        private int _rejectDelayMs;
        public int RejectDelayMs
        {
            get => _rejectDelayMs;
            set => Set(ref _rejectDelayMs, value);
        }

        private string _idmvsPath = "";
        public string IdmvsPath
        {
            get => _idmvsPath;
            set => Set(ref _idmvsPath, value);
        }

        // === Методы ===

        public void SaveRejectDelay()
        {
            if (RejectDelayMs < 0)
            {
                MessageBox.Show("Введите корректное значение задержки.", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            _settings.RejectDelayMs = RejectDelayMs;
            SettingsService.Save(_settings);

            MessageBox.Show($"Задержка сохранена: {RejectDelayMs} мс",
                "Сохранено", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        public void BrowseIDMVS()
        {
            var dialog = new OpenFileDialog
            {
                Title = "Выберите IDMVS.exe",
                Filter = "Исполняемые файлы (*.exe)|*.exe|Все файлы (*.*)|*.*",
                CheckFileExists = true,
                CheckPathExists = true
            };

            if (dialog.ShowDialog() == true)
            {
                IdmvsPath = dialog.FileName;

                _settings.IdmvsPath = dialog.FileName;
                SettingsService.Save(_settings);
            }
        }

        public void OpenIDMVS()
        {
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = IdmvsPath,
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Не удалось открыть IDMVS.\nПуть: {IdmvsPath}\nОшибка: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
