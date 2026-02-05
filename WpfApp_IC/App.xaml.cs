using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Windows;
using WpfApp_IC.Data;
using WpfApp_IC.Device;
using WpfApp_IC.Device.Actuators;
using WpfApp_IC.Device.Sensors;
using WpfApp_IC.Pages;
using WpfApp_IC.Services.Camera;     // CameraService
using WpfApp_IC.Services.Inspectors;
using WpfApp_IC.Services.ModbusT;     // ModbusService
using WpfApp_IC.Services.Products;   // ExpectedCodesService
using WpfApp_IC.ViewModels;



namespace WpfApp_IC
{
    public partial class App : Application
    {
        public static IHost? AppHost { get; private set; }

        // Глобальный доступ к очереди DataMatrix
       // public static ExpectedCodesService ExpectedCodes => AppHost.Services.GetRequiredService<ExpectedCodesService>();

        public App()
        {
            AppHost = Host.CreateDefaultBuilder()
                .ConfigureAppConfiguration((context, config) =>
                {
                    config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
                })
                .ConfigureServices((context, services) =>
                {
                    var conn = context.Configuration.GetConnectionString("DefaultConnection");
                    var ioConfig = IoModuleConfig.Load();

                    services.AddDbContext<AppDbContext>(options => options.UseMySql(conn, ServerVersion.AutoDetect(conn)));

                    services.AddSingleton(ioConfig);

                    services.AddSingleton<MainViewModel>();
                    services.AddSingleton<HomeViewModel>();
                    services.AddSingleton<ProductsViewModel>();
                    services.AddTransient<LabelPreviewViewModel>();
                    services.AddTransient<LabelPrintingViewModel>();

                    services.AddSingleton<MainWindow>();

                    services.AddSingleton<VideojetPrinter>();
                    //services.AddSingleton<ExpectedCodesService>();

                    services.AddSingleton(ioConfig);
                    services.AddSingleton<IModbusService, ModbusService>();
                    services.AddSingleton<ICameraService, CameraService>();
                    services.AddSingleton<IModbusService, ModbusService>();
                    services.AddSingleton<IInspectorController, InspectorController>();
                    services.AddSingleton<ISensor, ModbusSensor>();
                    services.AddSingleton<IRejector, ModbusRejector>();


                    services.AddTransient<CameraBasicViewModel>();
                    services.AddTransient<CameraAdvancedViewModel>();
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