using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;
using InventoryManagementApp.Interfaces;
using InventoryManagementApp.Services;
using InventoryManagementApp.Services.Core;
using InventoryManagementApp.Services.Customers;
using InventoryManagementApp.Services.Rentals;
using InventoryManagementApp.Services.Items;
using InventoryManagementApp.Services.Users;
using InventoryManagementApp.Services.Settings;
using InventoryManagementApp.ViewModels;
using InventoryManagementApp.Utilities.Helpers;
using InventoryManagementApp.Views.Pages;
using InventoryManagementApp.Views.Windows;
using InventoryManagementApp.Services.Devices;
using InventoryManagementApp.Data;
using InventoryManagementApp.Utilities;
using Microsoft.Data.Sqlite;

namespace InventoryManagementApp
{
    public partial class App : System.Windows.Application
    {
        public IHost Host { get; }
        private readonly ILogger<App> _logger;
        private readonly IDialogService _dialogService;

        public App() : this(BuildHost()) { }

        internal App(IHost host)
        {
            Host = host;
            _logger = Host.Services.GetRequiredService<ILogger<App>>();
            _dialogService = Host.Services.GetRequiredService<IDialogService>();

            DispatcherUnhandledException += (s, e) => HandleDispatcherException(e.Exception, e);
            AppDomain.CurrentDomain.UnhandledException += (s, e) =>
                HandleDomainException(e.ExceptionObject as Exception ?? new Exception("Unknown"), e);
            TaskScheduler.UnobservedTaskException += (s, e) => HandleTaskException(e.Exception, e);
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
                services.AddSingleton(sp =>
                {
                    var config = sp.GetRequiredService<IConfiguration>();
                    var dbPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory,
                        config["Database:Path"] ?? "inventory.db");
                    var builder = new SqliteConnectionStringBuilder
                    {
                        DataSource = dbPath,
                        Pooling = true,
                        Cache = SqliteCacheMode.Shared,
                        Mode = SqliteOpenMode.ReadWriteCreate
                    };
                    return new SqliteConnectionFactory(builder.ToString());
                });
                services.AddSingleton<IItemRepository, ItemRepository>();
                services.AddSingleton<IUserContext, ApplicationUserContext>();
                services.AddSingleton<IAuthorizationService, AuthorizationService>();
                services.AddSingleton<IItemService, ItemService>();
                services.AddSingleton<ICustomerService, CustomerService>();
                services.AddSingleton<IUserService, UserService>();
                services.AddSingleton<IRentalService, RentalService>();
                services.AddSingleton<ActivityLogService>();
                services.AddSingleton<IFileDialogService, FileDialogService>();
                services.AddSingleton<ISettingsService, SettingsService>();
                services.AddSingleton<IDialogService, DialogService>();
                services.AddSingleton<IScannerService, ScannerService>();
                services.AddSingleton<MemoryBudget>();
                services.AddTransient<ItemsViewModel>();
                services.AddSingleton<IMainViewModel, MainViewModel>();
                services.AddSingleton<ILoginViewModel, LoginViewModel>();
                services.AddTransient<ItemEditWindow>();
                services.AddTransient<PasswordPromptWindow>();
                services.AddTransient<ISetupWizard, SetupWizardWindow>();
                services.AddTransient<SetupWizardWindow>();
                services.AddTransient<PrintLabelWindow>();
                services.AddSingleton<IMainWindow>(sp =>
                    new MainWindow(sp.GetRequiredService<IMainViewModel>()));
                services.AddTransient<ILoginWindow>(sp =>
                    new LoginWindow(sp.GetRequiredService<ILoginViewModel>()));
            })
            .Build();

        protected async override void OnStartup(StartupEventArgs e)
        {
            await StartAsync();
            base.OnStartup(e);
        }

        public async Task StartAsync()
        {
            ShutdownMode = ShutdownMode.OnMainWindowClose;

            await Host.StartAsync();

            // Ensure database initialization and migrations are executed at startup
            Host.Services.GetRequiredService<DatabaseService>();

            var loggerFactory = Host.Services.GetRequiredService<ILoggerFactory>();
            PathHelper.Configure(loggerFactory.CreateLogger("PathHelper"));
            var settingsService = Host.Services.GetRequiredService<ISettingsService>();
            SecurityHelper.SettingsService = settingsService;
            await SecurityHelper.GetIterationsAsync().ConfigureAwait(false);
            await LabelProvider.Instance.InitializeAsync(settingsService).ConfigureAwait(false);

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

        internal void HandleDispatcherException(Exception ex, DispatcherUnhandledExceptionEventArgs? e = null)
        {
            _logger.LogError(ex, "Unhandled dispatcher exception");
            _dialogService.ShowInfo("An unexpected error occurred. Please try again.", "Error");
            if (e != null) e.Handled = true;
        }

        internal void HandleDomainException(Exception ex, UnhandledExceptionEventArgs? e = null)
        {
            _logger.LogError(ex, "Unhandled domain exception");
            _dialogService.ShowInfo("An unexpected error occurred. The application may need to close.", "Error");
        }

        internal void HandleTaskException(AggregateException ex, UnobservedTaskExceptionEventArgs? e = null)
        {
            _logger.LogError(ex, "Unobserved task exception");
            _dialogService.ShowInfo("An unexpected background error occurred.", "Error");
            if (e != null) e.SetObserved();
        }

        protected async override void OnExit(ExitEventArgs e)
        {
            await Host.StopAsync();
            Host.Dispose();
            Log.CloseAndFlush();
            base.OnExit(e);
        }
    }
}
