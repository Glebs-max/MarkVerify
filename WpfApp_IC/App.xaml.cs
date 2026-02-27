using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Windows;
using WpfApp_IC.Data;
using WpfApp_IC.Device;
using WpfApp_IC.Device.Actuators;
using WpfApp_IC.Device.Sensors;
using WpfApp_IC.Services.Camera;
using WpfApp_IC.Services.Inspectors;
using WpfApp_IC.Services.ModbusT;
using WpfApp_IC.ViewModels;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

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
                    var ioConfig = IoModuleConfig.Load();

                    services.AddDbContextFactory<AppDbContext>(options => options.UseMySql(conn, ServerVersion.AutoDetect(conn)));

                    services.AddSingleton(ioConfig);

                    services.AddSingleton<MainViewModel>();
                    services.AddSingleton<HomeViewModel>();
                    services.AddSingleton<ProductsViewModel>();
                    services.AddSingleton<VideojetErrorsViewModel>();
                    services.AddTransient<LabelPreviewViewModel>();
                    services.AddTransient<LabelPrintingViewModel>();
                    services.AddTransient<CameraBasicViewModel>();
                    services.AddTransient<CameraViewModel>();
                    services.AddTransient<CameraAdvancedViewModel>();

                    services.AddSingleton<VideojetPrinter>();
                    services.AddSingleton<IModbusService, ModbusService>();
                    services.AddSingleton<ICameraService, CameraService>();
                    services.AddSingleton<IDataMatrixValidator, DataMatrixValidator>();
                    services.AddSingleton<IInspectorController, InspectorController>();
                    services.AddSingleton<ISensor, ModbusSensor>();
                    services.AddSingleton<IRejector, ModbusRejector>();

                    services.AddSingleton<MainWindow>();
                })
                .Build();
        }

        protected override async void OnStartup(StartupEventArgs e)
        {
            if (AppHost != null)
            {
                await AppHost.StartAsync();

                MainViewModel mainVm = AppHost.Services.GetRequiredService<MainViewModel>();
                mainVm.CurrentViewModel = AppHost.Services.GetRequiredService<HomeViewModel>();

                MainWindow mw = AppHost.Services.GetRequiredService<MainWindow>();
                mw.Show();

                base.OnStartup(e);
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