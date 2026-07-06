using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;
using System.IO;
using System.Windows;
using WpfApp_IC.Data;
using WpfApp_IC.Models;
using WpfApp_IC.Models.Devices;
using WpfApp_IC.Services;
using WpfApp_IC.Services.Inspectors;
using WpfApp_IC.Services.ModbusT;
using WpfApp_IC.ViewModels;
using WpfApp_IC.Views;

namespace WpfApp_IC
{
    public partial class App : Application
    {
        private IConfiguration? _configuration;

        public static IHost? AppHost { get; private set; }

        public App()
        {
            AppHost = Host.CreateDefaultBuilder()
                .ConfigureAppConfiguration((context, config) =>
                {
                    config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

                    _configuration = config.Build();
                })
                .ConfigureServices((context, services) =>
                {
                    services.AddDbContextFactory<AppDbContext>(options => options.UseMySql(_configuration?.GetConnectionString("DefaultConnection"), ServerVersion.Create(new(10, 7, 3), ServerType.MariaDb)));

                    services.AddSingleton<MainViewModel>();
                    services.AddSingleton<HomeViewModel>();
                    services.AddSingleton<ProductsViewModel>();
                    services.AddSingleton<VideojetErrorsViewModel>();
                    services.AddTransient<SettingsViewModel>();
                    services.AddTransient<LabelPreviewViewModel>();
                    services.AddTransient<LabelingViewModel>();
                    services.AddTransient<LabelPrintingViewModel>();
                    services.AddTransient<CameraViewModel>();

                    services.AddSingleton<VideojetPrinter>();
                    services.AddSingleton<MindeoScanner>();
                    services.AddSingleton<ModbusSensor>();
                    services.AddSingleton<ModbusRejector>();
                    services.AddSingleton<HikrobotCamera>();
                    services.AddSingleton<WorkSession>();

                    services.AddSingleton<LogService>();
                    services.AddSingleton<SettingsService>();
                    services.AddSingleton<IModbusService, ModbusService>();
                    services.AddSingleton<HikrobotService>();
                    services.AddSingleton<InspectorController>();
                    services.AddSingleton<ImageSaverService>();
                    services.AddSingleton<DebugService>();

                    services.AddSingleton<MainWindow>();
                })
                .Build();
        }
        protected override async void OnStartup(StartupEventArgs e)
        {
            if (AppHost != null)
            {
                try
                {
                    await AppHost.StartAsync();

                    if (!File.Exists(_configuration?.GetValue<string>("SettingsPath")))
                        AppHost.Services.GetRequiredService<SettingsService>().Save();
                    else
                        AppHost.Services.GetRequiredService<SettingsService>().Load();

                    AppHost.Services.GetRequiredService<MainViewModel>().SetViewModel<HomeViewModel>();
                    AppHost.Services.GetRequiredService<MainWindow>().Show();

                    base.OnStartup(e);
                }
                catch (Exception ex)
                {
                    await AppHost.Services.GetRequiredService<DebugService>().CreateDebugEntryAsync(DebugType.Error, "App.xaml", $"Ошибка:\n{ex.Message}\n\n{ex.InnerException?.Message}\n\n{ex.StackTrace}");
                    MessageBox.Show($"Ошибка:\n{ex.Message}\n\n{ex.InnerException?.Message}\n\n{ex.StackTrace}", "Критическая ошибка");
                    Shutdown();
                }
            }
        }
        protected override async void OnExit(ExitEventArgs e)
        {
            if (AppHost != null)
            {
                if (AppHost.Services.GetRequiredService<MainViewModel>().CurrentViewModel is LabelingViewModel lvm)
                    await lvm.Exit();

                await AppHost.StopAsync();
                AppHost.Dispose();
                base.OnExit(e);
            }
        }
    }
}