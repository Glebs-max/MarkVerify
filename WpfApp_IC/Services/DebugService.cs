using Microsoft.EntityFrameworkCore;
using System.Windows;
using WpfApp_IC.Data;
using WpfApp_IC.Services.SettingsD;

namespace WpfApp_IC.Services
{
    public enum DebugType
    {
        Info = 0,
        Error = 1,
        UserAction = 2
    }

    public class DebugService(SettingsService settingsService, IDbContextFactory<AppDbContext> dbContextFactory)
    {
        public async Task CreateDebugEntryAsync(DebugType type, string source, string? info = null)
        {
            try
            {
                await using var db = await dbContextFactory.CreateDbContextAsync();
                db.debugs.Add(new()
                {
                    timestamp = DateTime.Now,
                    type = GetDebugType(type),
                    source = source,
                    machine = settingsService.Settings.MachineName,
                    info = info
                });
                await db.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Возникла ошибка при обращении к базе данных. Проверьте соединение с сервером.\n\nException message:\n\n{ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private static string GetDebugType(DebugType type)
        {
            return type switch
            {
                DebugType.Info => "INFO",
                DebugType.Error => "ERROR",
                DebugType.UserAction => "USER_ACTION",
                _ => "MISC"
            };
        }
    }
}
