using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Diagnostics;
using System.Windows;
using WpfApp_IC.Data;
using WpfApp_IC.Device;
using WpfApp_IC.Device.Actuators;
using WpfApp_IC.Device.Sensors;
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

                    //services.AddSingleton<IoModuleConfig>(); // DI создаёт сам SettingsService заполнит

                    services.AddSingleton<MainViewModel>();
                    services.AddSingleton<HomeViewModel>();
                    services.AddSingleton<ProductsViewModel>();
                    services.AddSingleton<VideojetErrorsViewModel>();
                    services.AddTransient<LabelPreviewViewModel>();
                    services.AddTransient<LabelingViewModel>();
                    services.AddTransient<LabelPrintingViewModel>();
                    services.AddTransient<CameraViewModel>();
                    services.AddTransient<SettingsViewModel>();

                    services.AddSingleton<LabelingSession>();
                    services.AddSingleton<VideojetPrinter>();
                    services.AddSingleton<IModbusService, ModbusService>();
                    services.AddSingleton<ICameraService, CameraService>();
                    services.AddSingleton<IInspectorController, InspectorController>();
                    //services.AddSingleton<ISensor, ModbusSensor>();
                    //services.AddSingleton<IRejector, ModbusRejector>();
                    services.AddSingleton<ISensor>(sp =>
                    {
                        var modbus = sp.GetRequiredService<IModbusService>();
                        // ← Читаем напрямую из файла, не через IInspectorController
                        var settings = AppSettings.LoadFromFile();
                        return new ModbusSensor(modbus, settings.SignalCoil);
                    });

                    services.AddSingleton<IRejector>(sp =>
                    {
                        var modbus = sp.GetRequiredService<IModbusService>();
                        var settings = AppSettings.LoadFromFile();
                        return new ModbusRejector(modbus, settings.RejectCoil);
                    });
                    services.AddSingleton<ISettingsService, SettingsService>();
                    services.AddSingleton<ILogService, LogService>();
                    //services.AddSingleton<IImageSaverService, ImageSaverService>();
                    services.AddSingleton<IImageSaverService>(sp =>
                    {
                        // Читаем путь напрямую из AppSettings — без зависимости от ISettingsService
                        string path = AppSettings.ReadRejectImagesPath();
                        return new ImageSaverService(path);
                    });

                    services.AddSingleton<MainWindow>();
                })
                .Build();
        }

        protected override async void OnStartup(StartupEventArgs e)
        {
            if (AppHost == null) return;

            // Синхронно — исключение не потеряется
            AppHost.StartAsync().GetAwaiter().GetResult();

            try
            {
                AppHost.Services.GetRequiredService<ISettingsService>();
                AppHost.Services.GetRequiredService<IInspectorController>();

                var mainVm = AppHost.Services.GetRequiredService<MainViewModel>();
                mainVm.CurrentViewModel = AppHost.Services.GetRequiredService<HomeViewModel>();

                var mw = AppHost.Services.GetRequiredService<MainWindow>();
                mw.Show();
            }
            catch (System.Exception ex)
            {
                System.Windows.MessageBox.Show(
                    $"Ошибка:\n{ex.Message}\n\n{ex.InnerException?.Message}\n\n{ex.StackTrace}",
                    "Критическая ошибка");
                Shutdown();
                return;
            }

            base.OnStartup(e);

            //try
            //{
            //    if (AppHost != null)
            //    {
            //        await AppHost.StartAsync();

            //        AppHost.Services.GetRequiredService<ISettingsService>(); // Apply() вызывается в конструкторе
            //        AppHost.Services.GetRequiredService<IInspectorController>(); // CameraIp устанавливается

            //        MainViewModel mainVm = AppHost.Services.GetRequiredService<MainViewModel>();
            //        mainVm.CurrentViewModel = AppHost.Services.GetRequiredService<HomeViewModel>();

            //        MainWindow mw = AppHost.Services.GetRequiredService<MainWindow>();
            //        mw.Show();

            //        base.OnStartup(e);
            //    }
            //}
            //catch (System.Exception ex)
            //{
            //    System.Windows.MessageBox.Show(
            //        $"Ошибка запуска:\n\n{ex.Message}\n\n{ex.InnerException?.Message}\n\n{ex.StackTrace}",
            //        "Критическая ошибка",
            //        System.Windows.MessageBoxButton.OK,
            //        System.Windows.MessageBoxImage.Error);

            //    Shutdown();
            //}
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