using System;
using System.IO;
using System.Text;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;
using DeviceManagementApp.Services;
using DeviceManagementApp.Interfaces;
using DeviceManagementApp.ViewModels;
using DeviceManagementApp.Views.Windows;
using Application = System.Windows.Application;

namespace DeviceManagementApp
{
    public partial class App : Application
    {
        public IHost Host { get; }

        public App() : this(BuildHost()) { }

        internal App(IHost host)
        {
            Host = host;
        }

        private static IHost BuildHost() => Microsoft.Extensions.Hosting.Host.CreateDefaultBuilder()
            .ConfigureLogging((context, logging) =>
            {
                var logsDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory,
                    context.Configuration["Logging:Directory"] ?? "Logs");
                Directory.CreateDirectory(logsDir);
                var logFile = Path.Combine(logsDir, "app-.log");

                Log.Logger = new LoggerConfiguration()
                    .MinimumLevel.Debug()
                    .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
                    .Enrich.FromLogContext()
                    .WriteTo.Debug()
                    .WriteTo.Async(w => w.File(
                        path: logFile,
                        rollingInterval: RollingInterval.Day,
                        retainedFileCountLimit: 14,
                        shared: true,
                        encoding: Encoding.UTF8))
                    .CreateLogger();

                logging.ClearProviders();
                logging.AddSerilog(Log.Logger, dispose: true);
            })
            .ConfigureServices((context, services) =>
            {
                services.AddSingleton<DatabaseService>(sp =>
                {
                    var config = sp.GetRequiredService<IConfiguration>();
                    var logger = sp.GetRequiredService<ILogger<DatabaseService>>();
                    var dbPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory,
                        config["Database:Path"] ?? "inventory.db");
                    return new DatabaseService(dbPath, logger);
                });
                services.AddSingleton<IDatabaseService>(sp => sp.GetRequiredService<DatabaseService>());
                services.AddSingleton<IDatabaseBackupService>(sp => sp.GetRequiredService<DatabaseService>());
                services.AddSingleton<IDeviceService, DeviceService>();
                services.AddSingleton<IDeviceFileService, DeviceFileService>();
                services.AddSingleton<IDeviceGroupService, DeviceGroupService>();
                services.AddSingleton<IStaffService, StaffService>();
                services.AddSingleton<IDeviceDiscoveryService, DeviceDiscoveryService>();
                services.AddTransient<Func<string, InfoDialogWindow>>(sp => message => new InfoDialogWindow(message));
                services.AddTransient<Func<string, ConfirmDialogWindow>>(sp => message => new ConfirmDialogWindow(message));
                services.AddSingleton<IDialogService, DialogService>();
                services.AddSingleton<ISettingsService, SettingsService>();
                services.AddSingleton<INavigationService, NavigationService>();
                services.AddSingleton<DevicesViewModel>();
                services.AddSingleton<DashboardViewModel>();
                services.AddSingleton<SettingsViewModel>();
                services.AddSingleton<StaffManagementViewModel>();
                services.AddSingleton<IMainViewModel, MainViewModel>();
                services.AddSingleton<MainWindow>();
            })
            .Build();

        protected override async void OnStartup(StartupEventArgs e)
        {
            await Host.StartAsync();
            var main = Host.Services.GetRequiredService<MainWindow>();
            main.Show();
            base.OnStartup(e);
        }

        protected override async void OnExit(ExitEventArgs e)
        {
            await Host.StopAsync();
            Host.Dispose();
            Log.CloseAndFlush();
            base.OnExit(e);
        }
    }
}
