
namespace WpfApp_IC.Services.SettingsD
{
    public interface ISettingsService
    {
        AppSettings Load();
        void Save(AppSettings settings);
    }
}
