using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Windows;
using WpfApp_IC.Data;
using WpfApp_IC.Devices;
using WpfApp_IC.Services;
using WpfApp_IC.Services.Camera;
using WpfApp_IC.Services.Inspectors;
using WpfApp_IC.Services.Log;
using WpfApp_IC.Services.ModbusT;
using WpfApp_IC.Services.SettingsD;
using WpfApp_IC.ViewModels;

namespace WpfApp_IC
{
    public partial class App : Application
    {
        public static IHost? AppHost { get; private set; }

        public App()
        {
            AppHost = Host.CreateDefaultBuilder()
                .ConfigureAppConfiguration((context, config) =>
                {
                    config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
                })
                .ConfigureServices((context, services) =>
                {
                    string? conn = context.Configuration.GetConnectionString("DefaultConnection");

                    services.AddDbContextFactory<AppDbContext>(options => options.UseMySql(conn, ServerVersion.AutoDetect(conn)));

                    services.AddSingleton<MainViewModel>();
                    services.AddSingleton<HomeViewModel>();
                    services.AddSingleton<ProductsViewModel>();
                    services.AddSingleton<VideojetErrorsViewModel>();
                    services.AddSingleton<LogViewModel>();
                    services.AddTransient<LabelPreviewViewModel>();
                    services.AddTransient<LabelingViewModel>();
                    services.AddTransient<LabelPrintingViewModel>();
                    services.AddTransient<CameraViewModel>();
                    services.AddTransient<SettingsViewModel>();


                    services.AddSingleton<LabelingSession>();
                    services.AddSingleton<VideojetPrinter>();
                    services.AddSingleton<LogService>();
                    services.AddSingleton<IModbusService, ModbusService>();
                    services.AddSingleton<ICameraService, CameraService>();
                    services.AddSingleton<IInspectorController, InspectorController>();
                    services.AddSingleton(sp =>
                    {
                        var modbus = sp.GetRequiredService<IModbusService>();
                        var settings = AppSettings.LoadFromFile();
                        return new ModbusSensor(modbus, settings.SignalCoil);
                    });
                    services.AddSingleton(sp =>
                    {
                        var modbus = sp.GetRequiredService<IModbusService>();
                        var settings = AppSettings.LoadFromFile();
                        return new ModbusRejector(modbus, settings.RejectCoil);
                    });
                    services.AddSingleton<ISettingsService, SettingsService>();
                    services.AddSingleton<IImageSaverService>(sp =>
                    {
                        string path = AppSettings.ReadRejectImagesPath();
                        return new ImageSaverService(path);
                    });

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

                    MainViewModel mainVm = AppHost.Services.GetRequiredService<MainViewModel>();
                    mainVm.CurrentViewModel = AppHost.Services.GetRequiredService<HomeViewModel>();

                    MainWindow mw = AppHost.Services.GetRequiredService<MainWindow>();
                    mw.Show();

                    base.OnStartup(e);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка:\n{ex.Message}\n\n{ex.InnerException?.Message}\n\n{ex.StackTrace}", "Критическая ошибка");
                    Shutdown();
                }
            }
        }
        protected override async void OnExit(ExitEventArgs e)
        {
            if (AppHost != null)
            {
                await AppHost.StopAsync();
                AppHost.Dispose();
                base.OnExit(e);
            }
        }
    }
}