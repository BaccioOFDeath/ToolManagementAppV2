// App.xaml.cs
using System;
using System.IO;
using System.Text;
using System.Windows;
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

        protected override void OnStartup(StartupEventArgs e)
        {
            ShutdownMode = ShutdownMode.OnMainWindowClose;
            base.OnStartup(e);

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

            var services = new ServiceCollection();
            services.AddLogging(builder =>
            {
                builder.ClearProviders();
                builder.AddSerilog(Log.Logger, dispose: true);
            });

            _serviceProvider = services.BuildServiceProvider();

            var loggerFactory = _serviceProvider.GetRequiredService<ILoggerFactory>();
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
            IDialogService dialogService = new DialogService(loggerFactory.CreateLogger<DialogService>());

            var mainVm = new MainViewModel(toolService, userService, userContext, customerService, rentalService, fileDialogService, activityLogService, settingsService, db, dialogService, loggerFactory.CreateLogger<MainViewModel>());
            var main = new MainWindow(mainVm, db);

            Current.MainWindow = main;
            main.Show();

            var login = new LoginWindow(userContext, userService, settingsService, dialogService)
            {
                Owner = main,
                WindowStartupLocation = WindowStartupLocation.CenterOwner
            };

            var ok = login.ShowDialog() == true;
            if (!ok)
            {
                main.Close();
                return;
            }

            if (main.DataContext is MainViewModel vm)
                vm.RefreshCurrentUser();

            if (main.WindowState == WindowState.Minimized) main.WindowState = WindowState.Normal;
            main.Activate();
            main.Focus();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            _serviceProvider?.Dispose();
            Log.CloseAndFlush();
            base.OnExit(e);
        }
    }
}
