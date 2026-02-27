
namespace WpfApp_IC.Services.SettingsD
{
    /// <summary>
    /// Сервис настроек. Регистрируем как Singleton в DI
    /// </summary>
    public interface ISettingsService
    {
        /// <summary> Текущие активные настройки /// </summary>
        AppSettings Current { get; }

        /// <summary>Сохраняет настройки в файл и применяет к объектам/// </summary>
        void Save(AppSettings settings);

        /// <summary>Применяет текущие настройки к живым объектам (принтер, инспекция, modbus)</summary>
        void ApplyCurrent();
    }
}
