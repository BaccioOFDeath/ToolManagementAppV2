// App.xaml.cs
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using Microsoft.Extensions.DependencyInjection;
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

namespace ToolManagementAppV2
{
    public partial class App : System.Windows.Application
    {
        private ServiceProvider? _serviceProvider;
        private ILogger<App>? _logger;
        private IDialogService? _dialogService;

        protected override async void OnStartup(StartupEventArgs e)
        {
            ShutdownMode = ShutdownMode.OnMainWindowClose;
            base.OnStartup(e);

            try
            {
                var logsDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs");
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

                _serviceProvider = ConfigureServices();

                var loggerFactory = _serviceProvider.GetRequiredService<ILoggerFactory>();
                _logger = loggerFactory.CreateLogger<App>();
                PathHelper.Configure(loggerFactory.CreateLogger("PathHelper"));

                var db = new DatabaseService(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "tool_inventory.db"), loggerFactory.CreateLogger<DatabaseService>());
                var toolService = new ToolService(db, loggerFactory.CreateLogger<ToolService>());
                var customerService = new CustomerService(db, loggerFactory.CreateLogger<CustomerService>());
                var userContext = new ApplicationUserContext();
                var userService = new UserService(db, userContext, loggerFactory.CreateLogger<UserService>());
                var rentalService = new RentalService(db, toolService, loggerFactory.CreateLogger<RentalService>());
                var activityLogService = new ActivityLogService(db, loggerFactory.CreateLogger<ActivityLogService>());
                var fileDialogService = new FileDialogService();
                var settingsService = new SettingsService(db, loggerFactory.CreateLogger<SettingsService>());
                SecurityHelper.SettingsService = settingsService;
                _dialogService = new DialogService(loggerFactory.CreateLogger<DialogService>());

                HookGlobalExceptionHandlers();

                var mainVm = new MainViewModel(toolService, userService, userContext, customerService, rentalService, fileDialogService, activityLogService, settingsService, db, _dialogService, loggerFactory.CreateLogger<MainViewModel>());
                var main = new MainWindow(mainVm, db);

                Current.MainWindow = main;
                main.Show();

                var login = new LoginWindow(userContext, userService, settingsService, _dialogService)
                {
                    Owner = main,
                    WindowStartupLocation = WindowStartupLocation.CenterOwner
                };

                if (login.DataContext is LoginViewModel lvm)
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
            catch (Exception ex)
            {
                _logger?.LogCritical(ex, "Fatal error during startup");
                ShowFatalError($"A fatal error occurred: {ex.Message}");
                Shutdown(-1);
            }
        }

        protected virtual ServiceProvider ConfigureServices()
        {
            var services = new ServiceCollection();
            services.AddLogging(builder =>
            {
                builder.ClearProviders();
                builder.AddSerilog(Log.Logger, dispose: true);
            });
            return services.BuildServiceProvider();
        }

        void HookGlobalExceptionHandlers()
        {
            DispatcherUnhandledException += HandleDispatcherUnhandledException;
            AppDomain.CurrentDomain.UnhandledException += HandleDomainUnhandledException;
            TaskScheduler.UnobservedTaskException += HandleUnobservedTaskException;
        }

        internal void HandleDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            _logger?.LogError(e.Exception, "Unhandled dispatcher exception");
            if (_dialogService != null)
                _dialogService.ShowInfo($"An unexpected error occurred: {e.Exception.Message}", "Error");
            else
                ShowFatalError($"An unexpected error occurred: {e.Exception.Message}");
            e.Handled = true;
        }

        internal void HandleDomainUnhandledException(object? sender, UnhandledExceptionEventArgs e)
        {
            if (e.ExceptionObject is Exception ex)
            {
                _logger?.LogError(ex, "Unhandled domain exception");
                if (_dialogService != null)
                    _dialogService.ShowInfo($"An unexpected error occurred: {ex.Message}", "Error");
                else
                    ShowFatalError($"An unexpected error occurred: {ex.Message}");
            }
        }

        internal void HandleUnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs e)
        {
            _logger?.LogError(e.Exception, "Unobserved task exception");
            if (_dialogService != null)
                _dialogService.ShowInfo($"An unexpected error occurred: {e.Exception.Message}", "Error");
            else
                ShowFatalError($"An unexpected error occurred: {e.Exception.Message}");
            e.SetObserved();
        }

        protected virtual void ShowFatalError(string message)
            => MessageBox.Show(message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);

        internal void SetLogger(ILogger<App> logger) => _logger = logger;
        internal void SetDialogService(IDialogService dialogService) => _dialogService = dialogService;

        protected override void OnExit(ExitEventArgs e)
        {
            _serviceProvider?.Dispose();
            Log.CloseAndFlush();
            base.OnExit(e);
        }
    }
}
