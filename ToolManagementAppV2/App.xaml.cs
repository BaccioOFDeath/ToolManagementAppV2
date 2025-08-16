using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;
using ToolManagementAppV2.Interfaces;
using ToolManagementAppV2.Services;
using ToolManagementAppV2.Services.Core;
using ToolManagementAppV2.Services.Customers;
using ToolManagementAppV2.Services.Rentals;
using ToolManagementAppV2.Services.Tools;
using ToolManagementAppV2.Services.Users;
using ToolManagementAppV2.Services.Settings;
using ToolManagementAppV2.ViewModels;
using ToolManagementAppV2.Utilities.Helpers;
using ToolManagementAppV2.Views;
using ToolManagementAppV2.Services.Devices;

namespace ToolManagementAppV2
{
    public partial class App : System.Windows.Application
    {
        public IHost Host { get; }

        public App()
        {
            Host = Microsoft.Extensions.Hosting.Host.CreateDefaultBuilder()
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
                            config["Database:Path"] ?? "tool_inventory.db");
                        return new DatabaseService(dbPath, logger);
                    });
                    services.AddSingleton<IDatabaseService>(sp => sp.GetRequiredService<DatabaseService>());
                    services.AddSingleton<IDatabaseBackupService>(sp => sp.GetRequiredService<DatabaseService>());
                    services.AddSingleton<IUserContext, ApplicationUserContext>();
                    services.AddSingleton<IToolService, ToolService>();
                    services.AddSingleton<ICustomerService, CustomerService>();
                    services.AddSingleton<IUserService, UserService>();
                    services.AddSingleton<IRentalService, RentalService>();
                    services.AddSingleton<ActivityLogService>();
                    services.AddSingleton<IFileDialogService, FileDialogService>();
                    services.AddSingleton<ISettingsService, SettingsService>();
                    services.AddSingleton<IDialogService, DialogService>();
                    services.AddSingleton<IScannerService, ScannerService>();
                    services.AddSingleton<IMainViewModel, MainViewModel>();
                    services.AddSingleton<ILoginViewModel, LoginViewModel>();
                    services.AddTransient<ToolEditWindow>();
                    services.AddTransient<AvatarSelectionWindow>();
                    services.AddTransient<ScannerStatusWindow>();
                    services.AddTransient<PasswordPromptWindow>();
                    services.AddTransient<PrintLabelWindow>();
                    services.AddSingleton<IMainWindow>(sp =>
                        new MainWindow(sp.GetRequiredService<IMainViewModel>()));
                    services.AddSingleton<ILoginWindow>(sp =>
                        new LoginWindow(sp.GetRequiredService<ILoginViewModel>()));
                })
                .Build();
        }

        [STAThread]
        public static async Task Main()
        {
            var app = new App();
            try
            {
                await app.StartAsync();
                app.Run();
            }
            catch (Exception ex)
            {
                var logger = app.Host.Services.GetRequiredService<ILogger<App>>();
                logger.LogCritical(ex, "Application failed to start");
            }
            finally
            {
                await app.Host.StopAsync();
                app.Host.Dispose();
                Log.CloseAndFlush();
            }
        }

        public async Task StartAsync()
        {
            ShutdownMode = ShutdownMode.OnMainWindowClose;

            await Host.StartAsync();

            var loggerFactory = Host.Services.GetRequiredService<ILoggerFactory>();
            PathHelper.Configure(loggerFactory.CreateLogger("PathHelper"));
            SecurityHelper.SettingsService = Host.Services.GetRequiredService<ISettingsService>();

            var main = (Window)Host.Services.GetRequiredService<IMainWindow>();
            Current.MainWindow = main;
            main.Show();

            var login = Host.Services.GetRequiredService<ILoginWindow>();
            login.Owner = main;
            login.WindowStartupLocation = WindowStartupLocation.CenterOwner;

            var lvm = login.ViewModel;
            await lvm.InitializeAsync();

            var ok = login.ShowDialog() == true;
            if (!ok)
            {
                main.Close();
                return;
            }

            if (main.WindowState == WindowState.Minimized) main.WindowState = WindowState.Normal;
            main.Activate();
            main.Focus();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            Log.CloseAndFlush();
            base.OnExit(e);
        }
    }
}
