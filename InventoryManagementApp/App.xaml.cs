using System;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Threading;
using System.Linq;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;
using InventoryManagementApp.Models;
using InventoryManagementApp.Interfaces;
using InventoryManagementApp.Services;
using InventoryManagementApp.Services.Core;
using InventoryManagementApp.Services.Customers;
using InventoryManagementApp.Services.Rentals;
using InventoryManagementApp.Services.Items;
using InventoryManagementApp.Services.Users;
using InventoryManagementApp.Services.Settings;
using InventoryManagementApp.Services.Maintenance;
using InventoryManagementApp.Services.Calibration;
using InventoryManagementApp.Services.Reservations;
using InventoryManagementApp.Services.Kits;
using InventoryManagementApp.ViewModels;
using InventoryManagementApp.ViewModels.Rental;
using InventoryManagementApp.Utilities.Helpers;
using InventoryManagementApp.Views.Pages;
using InventoryManagementApp.Views.Windows;
using InventoryManagementApp.Data;
using InventoryManagementApp.Utilities;
using Microsoft.Data.Sqlite;
using InventoryManagementApp.Models.Domain;
using InventoryManagementApp.Services.Categories;
using InventoryManagementApp.Services.Notifications;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using CommunityToolkit.Mvvm.Input;

namespace InventoryManagementApp
{
    public partial class App : System.Windows.Application
    {
        internal const string DefaultLogoResourceUri = "pack://application:,,,/InventoryManagementApp;component/Resources/DefaultLogo.png";
        static readonly DependencyProperty HasAppliedBackgroundOverlayProperty =
            DependencyProperty.RegisterAttached(
                "HasAppliedBackgroundOverlay",
                typeof(bool),
                typeof(App),
                new PropertyMetadata(false));

        public IHost Host { get; }
        private readonly ILogger<App> _logger;
        private readonly IDialogService _dialogService;

        public App() : this(BuildHost(), initializeApplicationResources: false) { }

        internal App(IHost host) : this(host, initializeApplicationResources: true) { }

        App(IHost host, bool initializeApplicationResources)
        {
            if (initializeApplicationResources)
            {
                InitializeComponent();
            }

            Host = host;
            _logger = Host.Services.GetRequiredService<ILogger<App>>();
            _dialogService = Host.Services.GetRequiredService<IDialogService>();

            EventManager.RegisterClassHandler(typeof(Window), FrameworkElement.LoadedEvent, new RoutedEventHandler(OnWindowLoaded));
            EventManager.RegisterClassHandler(typeof(UserControl), FrameworkElement.LoadedEvent, new RoutedEventHandler(OnUserControlLoaded));

            DispatcherUnhandledException += (s, e) => HandleDispatcherException(e.Exception, e);
            AppDomain.CurrentDomain.UnhandledException += (s, e) =>
                HandleDomainException(e.ExceptionObject as Exception ?? new Exception("Unknown"), e);
            TaskScheduler.UnobservedTaskException += (s, e) =>
            {
                try 
                { 
                    Log.Error(e.Exception, "Unobserved task exception"); 
                } 
                catch (Exception logEx)
                { 
                    // Fallback if logging fails - write to debug output
                    System.Diagnostics.Debug.WriteLine($"Failed to log unobserved exception: {logEx.Message}");
                }
                e.SetObserved();
            };
        }

        private static IHost BuildHost() => Microsoft.Extensions.Hosting.Host.CreateDefaultBuilder()
            .ConfigureAppConfiguration((context, config) =>
            {
                // Ensure appsettings are loaded from the executable directory.
                config.SetBasePath(AppDomain.CurrentDomain.BaseDirectory);
                config.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                      .AddJsonFile($"appsettings.{context.HostingEnvironment.EnvironmentName}.json", optional: true, reloadOnChange: true);
            })
            .ConfigureLogging((context, logging) =>
            {
                var logsDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory,
                    context.Configuration["Logging:Directory"] ?? "Logs");
                Directory.CreateDirectory(logsDir);
                var logFile = Path.Combine(logsDir, "app-.log");

                Log.Logger = new LoggerConfiguration()
                    .MinimumLevel.Information()
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
                services.AddSingleton<MigrationRunner>();
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
                services.AddSingleton<IThemeService, ThemeService>();
                services.AddSingleton<IEmailAccountDiscoveryService, OutlookEmailAccountDiscoveryService>();
                services.AddSingleton<IDialogService, DialogService>();
                services.AddSingleton<MaintenanceService>();
                services.AddSingleton<CalibrationService>();
                services.AddSingleton<ReservationService>();
                services.AddSingleton<KitService>();
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
                services.AddSingleton<CategoriesService>();
                services.AddTransient<CategoryManagementViewModel>();
                services.AddSingleton<RentalConfigurationService>();
                
                // Email and Reminder Services for server operation
                // Register EmailService factory that returns null if not configured
#pragma warning disable CS8621 // Nullability of reference types in return type doesn't match target delegate (by design)
                services.AddSingleton(sp =>
                {
                    var config = sp.GetRequiredService<IConfiguration>();
                    var logger = sp.GetRequiredService<ILogger<EmailService>>();
                    
                    var smtpHost = config["Email:SmtpHost"];
                    var smtpPortStr = config["Email:SmtpPort"];
                    var smtpUsername = config["Email:SmtpUsername"];
                    var smtpPassword = config["Email:SmtpPassword"];
                    var fromEmail = config["Email:FromEmail"];
                    var fromName = config["Email:FromName"];
                    var enableSslStr = config["Email:EnableSsl"];
                    
                    // Return null if email is not properly configured
                    if (string.IsNullOrWhiteSpace(smtpHost) || 
                        string.IsNullOrWhiteSpace(smtpUsername) || 
                        string.IsNullOrWhiteSpace(smtpPassword) ||
                        string.IsNullOrWhiteSpace(fromEmail) ||
                        smtpHost.Contains("example.com", StringComparison.OrdinalIgnoreCase))
                    {
                        logger.LogWarning("Email service not configured properly. Email features will be disabled.");
                        return (EmailService?)null;
                    }
                    
                    if (!int.TryParse(smtpPortStr, out var smtpPort))
                    {
                        smtpPort = 587; // Default SMTP port
                    }
                    
                    var enableSsl = true; // Default to secure
                    if (!string.IsNullOrWhiteSpace(enableSslStr))
                    {
                        bool.TryParse(enableSslStr, out enableSsl);
                    }
                    
                    return (EmailService?)new EmailService(
                        smtpHost,
                        smtpPort,
                        smtpUsername,
                        smtpPassword,
                        fromEmail,
                        fromName ?? "Equipment Rentals",
                        enableSsl,
                        logger);
                });
#pragma warning restore CS8621
                
                services.AddSingleton<RentalReminderService>(sp =>
                {
                    var rentalService = sp.GetRequiredService<IRentalService>();
                    var emailService = sp.GetService<EmailService>(); // Can be null if not configured
                    var config = sp.GetRequiredService<IConfiguration>();
                    var logger = sp.GetRequiredService<ILogger<RentalReminderService>>();
                    
                    var contactInfo = config["Email:ContactInfo"] ?? "your rental team";
                    
                    return new RentalReminderService(
                        rentalService,
                        emailService,
                        contactInfo,
                        logger);
                });
            })
            .Build();

        protected async override void OnStartup(StartupEventArgs e)
        {
            await StartAsync();
            base.OnStartup(e);
        }

        public async Task StartAsync()
        {
            ShutdownMode = ShutdownMode.OnExplicitShutdown;
            var qaOptions = QaScreenshotRunOptions.Parse(Environment.GetCommandLineArgs());

            await Host.StartAsync();

            // Validate configuration at startup
            var configuration = Host.Services.GetRequiredService<IConfiguration>();
            var configLogger = Host.Services.GetRequiredService<ILogger<ConfigurationValidator>>();
            var configValidator = new ConfigurationValidator(configuration, configLogger);
            var configErrors = configValidator.Validate();
            if (configErrors.Any())
            {
                var errorMessage = $"Configuration validation failed:\n\n{string.Join("\n", configErrors)}\n\nPlease check appsettings.json and try again.";
                _logger.LogCritical("Application startup failed due to configuration errors.");
                _dialogService.ShowInfo(errorMessage, "Configuration Error");
                Shutdown();
                return;
            }

            // Ensure database initialization and migrations are executed at startup
            Host.Services.GetRequiredService<MigrationRunner>().Migrate();

            var loggerFactory = Host.Services.GetRequiredService<ILoggerFactory>();
            PathHelper.Configure(loggerFactory.CreateLogger("PathHelper"));
            var settingsService = Host.Services.GetRequiredService<ISettingsService>();
            var themeService = Host.Services.GetRequiredService<IThemeService>();
            var theme = await settingsService.GetThemeAsync();
            themeService.ApplyTheme(theme);
            ApplyWindowBranding(await settingsService.GetSettingAsync("CompanyLogoPath"));

            SecurityHelper.SettingsService = settingsService;
            await SecurityHelper.GetIterationsAsync();
            var dbPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory,
                configuration["Database:Path"] ?? "inventory.db");
            var permissionWarning = DatabaseSecurityHelper.GetPermissionWarning(dbPath);
            if (!string.IsNullOrWhiteSpace(permissionWarning))
            {
                _logger.LogWarning("Database permissions warning: {Warning}", permissionWarning);
                _dialogService.ShowInfo(permissionWarning, "Database Security");
            }
            var setupDone = await settingsService.GetSettingAsync("SetupComplete");
            if (string.IsNullOrWhiteSpace(setupDone))
            {
                SetupWizardResult? result;
                if (qaOptions != null)
                {
                    result = qaOptions.ToSetupWizardResult();
                }
                else
                {
                    var wizard = Host.Services.GetRequiredService<ISetupWizard>();
                    result = await wizard.RunAsync();
                }

                if (result == null)
                {
                    Shutdown();
                    return;
                }

                await ApplySetupResultAsync(result, disableAutoLogout: qaOptions != null);

                // Refresh settings service for normal operations
                settingsService = Host.Services.GetRequiredService<ISettingsService>();
                theme = await settingsService.GetThemeAsync();
                themeService.ApplyTheme(theme);
                ApplyWindowBranding(await settingsService.GetSettingAsync("CompanyLogoPath"));
            }
            await LabelProvider.Instance.InitializeAsync(settingsService);

            if (qaOptions != null)
            {
                var qaMain = (Window)Host.Services.GetRequiredService<IMainWindow>();
                Current.MainWindow = qaMain;
                ShutdownMode = ShutdownMode.OnMainWindowClose;
                qaMain.Show();

                // Yield once on the UI dispatcher so startup continues after Show()
                // without depending on an ApplicationIdle pump in tests.
                await qaMain.Dispatcher.InvokeAsync(() => { }, DispatcherPriority.Background);

                await RunQaScreenshotsAsync(qaMain, qaOptions);
                qaMain.Close();
                return;
            }

            var login = Host.Services.GetRequiredService<ILoginWindow>();
            login.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            if (login is Window startupLoginWindow)
                startupLoginWindow.ShowInTaskbar = true;

            var lvm = login.ViewModel;
            await lvm.InitializeAsync();

            var ok = login.ShowDialog() == true;
            if (!ok)
            {
                Shutdown();
                return;
            }

            var main = (Window)Host.Services.GetRequiredService<IMainWindow>();
            Current.MainWindow = main;
            ShutdownMode = ShutdownMode.OnMainWindowClose;
            main.Show();

            // Yield once on the UI dispatcher so startup continues after Show()
            // without depending on an ApplicationIdle pump in tests.
            await main.Dispatcher.InvokeAsync(() => { }, DispatcherPriority.Background);

            if (main.WindowState == WindowState.Minimized) main.WindowState = WindowState.Normal;
            main.Activate();
            main.Focus();
            
            // Start the rental reminder service for server operation
            try
            {
                var reminderService = Host.Services.GetService<RentalReminderService>();
                if (reminderService != null)
                {
                    reminderService.Start();
                    _logger.LogInformation("Rental reminder service started successfully");
                }
                else
                {
                    _logger.LogWarning("Rental reminder service not available. Email reminders will not be sent.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to start rental reminder service. Email reminders will not be sent.");
            }
        }

        async Task ApplySetupResultAsync(SetupWizardResult result, bool disableAutoLogout = false)
        {
            var db = Host.Services.GetRequiredService<DatabaseService>();
            var context = Host.Services.GetRequiredService<IUserContext>();
            var bypassUsers = new UserService(
                db,
                context,
                new NoOpAuthorizationService(),
                Host.Services.GetService<ILogger<UserService>>(),
                Host.Services.GetService<ActivityLogService>());
            var bypassSettings = new SettingsService(
                db,
                new NoOpAuthorizationService(),
                Host.Services.GetService<ILogger<SettingsService>>());

            var users = await bypassUsers.GetAllUsersAsync(System.Threading.CancellationToken.None);
            var admin = users.FirstOrDefault(u => u.IsAdmin);
            if (admin == null)
            {
                admin = new User
                {
                    UserName = "admin",
                    PasswordHash = PasswordDefaults.DefaultAdminPassword,
                    IsAdmin = true,
                    PasswordExpired = true
                };
                await bypassUsers.AddUserAsync(admin);
            }

            await bypassUsers.ChangeUserPasswordAsync(admin.UserID, result.Password);
            await bypassSettings.SaveSettingAsync("ApplicationName", result.ApplicationName);
            await bypassSettings.SaveItemLabelSingularAsync(result.ItemLabelSingular);
            await bypassSettings.SaveItemLabelPluralAsync(result.ItemLabelPlural);
            if (disableAutoLogout)
                await bypassSettings.SaveAutoLogoutMinutesAsync(0);

            if (!string.IsNullOrWhiteSpace(result.ThemeProfilePath))
            {
                try
                {
                    var themeProfilePath = Path.GetFullPath(result.ThemeProfilePath);
                    if (File.Exists(themeProfilePath))
                    {
                        var themeSettings = JsonSerializer.Deserialize<AppThemeSettings>(File.ReadAllText(themeProfilePath))
                            ?? throw new InvalidDataException("Theme profile did not contain app theme settings.");
                        themeSettings.Normalize();
                        await bypassSettings.SaveThemeAsync(themeSettings.BaseTheme);
                        await ((ISettingsService)bypassSettings).SaveAppThemeSettingsAsync(themeSettings);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to apply QA theme profile {ThemeProfilePath}.", result.ThemeProfilePath);
                    throw;
                }
            }

            if (!string.IsNullOrWhiteSpace(result.CompanyLogoPath))
            {
                try
                {
                    var baseDir = Path.GetFullPath(AppDomain.CurrentDomain.BaseDirectory);
                    var fullInputPath = Path.GetFullPath(result.CompanyLogoPath);
                    if (File.Exists(fullInputPath))
                    {
                        string relativePath;
                        if (!fullInputPath.StartsWith(baseDir, StringComparison.OrdinalIgnoreCase))
                        {
                            var assetsDir = Path.Combine(baseDir, "Assets", "CompanyLogo");
                            Directory.CreateDirectory(assetsDir);
                            var destPath = Path.Combine(assetsDir, Path.GetFileName(fullInputPath));
                            File.Copy(fullInputPath, destPath, true);
                            relativePath = Path.GetRelativePath(baseDir, destPath);
                        }
                        else
                        {
                            relativePath = Path.GetRelativePath(baseDir, fullInputPath);
                        }

                        await bypassSettings.SaveSettingAsync("CompanyLogoPath", relativePath);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to save company logo path.");
                }
            }

            await bypassSettings.SaveSettingAsync("SetupComplete", "true");
        }

        async Task RunQaScreenshotsAsync(Window main, QaScreenshotRunOptions options)
        {
            if (main is not MainWindow mainWindow || main.DataContext is not MainViewModel mainViewModel)
                throw new InvalidOperationException("QA screenshot mode requires the concrete main window and view model.");

            Directory.CreateDirectory(options.OutputDirectory);
            var runLogPath = Path.Combine(options.OutputDirectory, "qa-run.log");
            var manifestPath = Path.Combine(options.OutputDirectory, "README.md");
            File.WriteAllText(runLogPath, string.Empty);
            File.WriteAllText(
                manifestPath,
                $"# QA Screenshot Run{Environment.NewLine}{Environment.NewLine}" +
                $"Generated: {DateTime.Now:yyyy-MM-dd HH:mm:ss}{Environment.NewLine}{Environment.NewLine}" +
                $"Folders:{Environment.NewLine}" +
                $"- `00-auth` login and authentication flow{Environment.NewLine}" +
                $"- `01-overview` overview screens and search intelligence{Environment.NewLine}" +
                $"- `02-operations` operational workflows{Environment.NewLine}" +
                $"- `03-insights` reporting and activity surfaces{Environment.NewLine}" +
                $"- `04-data` import and export surfaces{Environment.NewLine}" +
                $"- `05-admin` user and settings administration{Environment.NewLine}" +
                $"- `06-dialogs` standalone windows, dialogs, edit forms, and previews opened by major buttons{Environment.NewLine}");
            void LogStep(string message) =>
                File.AppendAllText(runLogPath, $"{DateTime.Now:O} {message}{Environment.NewLine}");

            var authDir = EnsureCaptureDirectory(options.OutputDirectory, "00-auth");
            var overviewDir = EnsureCaptureDirectory(options.OutputDirectory, "01-overview");
            var operationsDir = EnsureCaptureDirectory(options.OutputDirectory, "02-operations");
            var insightsDir = EnsureCaptureDirectory(options.OutputDirectory, "03-insights");
            var dataDir = EnsureCaptureDirectory(options.OutputDirectory, "04-data");
            var adminDir = EnsureCaptureDirectory(options.OutputDirectory, "05-admin");
            var dialogsDir = EnsureCaptureDirectory(options.OutputDirectory, "06-dialogs");

            var login = Host.Services.GetRequiredService<ILoginWindow>();
            if (login is not Window loginWindow)
                throw new InvalidOperationException("QA screenshot mode requires the concrete login window.");

            login.Owner = main;
            login.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            await login.ViewModel.InitializeAsync();

            LogStep("Showing login window.");
            loginWindow.Show();
            await WaitForUiAsync(main.Dispatcher);
            await Task.Delay(400);
            await CaptureWindowAsync(loginWindow, Path.Combine(authDir, "01-login-window.png"));
            loginWindow.Close();

            var userService = Host.Services.GetRequiredService<IUserService>();
            var userContext = Host.Services.GetRequiredService<IUserContext>();
            var authentication = await userService.AuthenticateUserAsync(options.AdminUserName, options.AdminPassword);
            if (authentication.Result != AuthenticationResult.Success || authentication.User == null)
                throw new InvalidOperationException($"QA screenshot mode failed to authenticate '{options.AdminUserName}'.");

            userContext.CurrentUser = authentication.User;
            mainViewModel.RefreshCurrentUser();
            mainViewModel.Settings.AutoLogoutMinutes = 0;
            LogStep("Authenticated QA user and disabled auto logout.");

            if (main.WindowState == WindowState.Minimized)
                main.WindowState = WindowState.Normal;

            if (options.FullScreen)
            {
                main.WindowState = WindowState.Maximized;
                LogStep("Maximized main window for fullscreen QA capture.");
            }

            main.Activate();
            main.Focus();
            await WaitForUiAsync(main.Dispatcher);
            await Task.Delay(400);

            var itemSlug = options.BuildItemSlug();
            await CaptureSectionPageAsync(
                mainWindow,
                mainViewModel,
                mainViewModel.SelectOverviewSectionCommand,
                mainViewModel.OpenSearchItemsCommand,
                Path.Combine(overviewDir, $"01-search-{itemSlug}-results.png"),
                runLogPath,
                "Overview search");

            await CaptureSelectedTabPageAsync(
                mainWindow,
                OpenSectionPageAsync(mainWindow, mainViewModel, mainViewModel.SelectOverviewSectionCommand, mainViewModel.OpenSearchItemsCommand),
                Path.Combine(overviewDir, $"02-search-{itemSlug}-recent-searches.png"),
                runLogPath,
                "Search intelligence recent searches",
                tabControlIndex: 0,
                tabIndex: 0);
            await CaptureSelectedTabPageAsync(
                mainWindow,
                OpenSectionPageAsync(mainWindow, mainViewModel, mainViewModel.SelectOverviewSectionCommand, mainViewModel.OpenSearchItemsCommand),
                Path.Combine(overviewDir, $"03-search-{itemSlug}-unavailable-demand.png"),
                runLogPath,
                "Search intelligence unavailable demand",
                tabControlIndex: 0,
                tabIndex: 1);

            await CaptureSectionPageAsync(
                mainWindow,
                mainViewModel,
                mainViewModel.SelectOverviewSectionCommand,
                mainViewModel.OpenDashboardCommand,
                Path.Combine(overviewDir, "04-dashboard-summary.png"),
                runLogPath,
                "Dashboard summary");
            await CaptureSelectedTabPageAsync(
                mainWindow,
                OpenSectionPageAsync(mainWindow, mainViewModel, mainViewModel.SelectOverviewSectionCommand, mainViewModel.OpenDashboardCommand),
                Path.Combine(overviewDir, "05-dashboard-recent-activity.png"),
                runLogPath,
                "Dashboard recent activity",
                tabControlIndex: 0,
                tabIndex: 0);
            await CaptureSelectedTabPageAsync(
                mainWindow,
                OpenSectionPageAsync(mainWindow, mainViewModel, mainViewModel.SelectOverviewSectionCommand, mainViewModel.OpenDashboardCommand),
                Path.Combine(overviewDir, "06-dashboard-items-with-issues.png"),
                runLogPath,
                "Dashboard items with issues",
                tabControlIndex: 0,
                tabIndex: 1);
            await CaptureSectionPageAsync(mainWindow, mainViewModel, mainViewModel.SelectOperationsSectionCommand, mainViewModel.OpenManageItemsCommand, Path.Combine(operationsDir, $"01-manage-{itemSlug}.png"), runLogPath, "Manage items");
            await CaptureSectionPageAsync(mainWindow, mainViewModel, mainViewModel.SelectOperationsSectionCommand, mainViewModel.OpenRentalsCommand, Path.Combine(operationsDir, "02-rentals.png"), runLogPath, "Rentals");
            await CaptureSectionPageAsync(mainWindow, mainViewModel, mainViewModel.SelectOperationsSectionCommand, mainViewModel.OpenCustomersCommand, Path.Combine(operationsDir, "03-customers.png"), runLogPath, "Customers");
            await CaptureSectionPageAsync(mainWindow, mainViewModel, mainViewModel.SelectOperationsSectionCommand, mainViewModel.OpenMaintenanceCommand, Path.Combine(operationsDir, "04-maintenance.png"), runLogPath, "Maintenance");
            await CaptureSectionPageAsync(mainWindow, mainViewModel, mainViewModel.SelectOperationsSectionCommand, mainViewModel.OpenCalibrationCommand, Path.Combine(operationsDir, "05-calibration.png"), runLogPath, "Calibration");
            await CaptureSectionPageAsync(mainWindow, mainViewModel, mainViewModel.SelectOperationsSectionCommand, mainViewModel.OpenReservationsCommand, Path.Combine(operationsDir, "06-reservations.png"), runLogPath, "Reservations");
            await CaptureSectionPageAsync(mainWindow, mainViewModel, mainViewModel.SelectOperationsSectionCommand, mainViewModel.OpenKitManagementCommand, Path.Combine(operationsDir, "07-kits.png"), runLogPath, "Kits");
            await CaptureSectionPageAsync(mainWindow, mainViewModel, mainViewModel.SelectOperationsSectionCommand, mainViewModel.OpenCategoriesCommand, Path.Combine(operationsDir, "08-categories.png"), runLogPath, "Categories");

            await CaptureSectionPageAsync(mainWindow, mainViewModel, mainViewModel.SelectInsightsSectionCommand, mainViewModel.OpenReportsCommand, Path.Combine(insightsDir, "01-reports.png"), runLogPath, "Reports");
            await CaptureSectionPageAsync(mainWindow, mainViewModel, mainViewModel.SelectInsightsSectionCommand, mainViewModel.OpenActivityLogsCommand, Path.Combine(insightsDir, "02-activity-logs.png"), runLogPath, "Activity logs");

            await CaptureSectionPageAsync(mainWindow, mainViewModel, mainViewModel.SelectDataSectionCommand, mainViewModel.OpenImportExportCommand, Path.Combine(dataDir, "01-import-export-overview.png"), runLogPath, "Import export overview");
            for (var tabIndex = 1; tabIndex <= 4; tabIndex++)
            {
                await CaptureSelectedTabPageAsync(
                    mainWindow,
                    OpenSectionPageAsync(mainWindow, mainViewModel, mainViewModel.SelectDataSectionCommand, mainViewModel.OpenImportExportCommand),
                    Path.Combine(dataDir, $"{tabIndex + 1:00}-import-export-{GetImportExportTabSlug(tabIndex)}.png"),
                    runLogPath,
                    $"Import export {GetImportExportTabSlug(tabIndex)}",
                    tabControlIndex: 0,
                    tabIndex: tabIndex);
            }

            await CaptureSectionPageAsync(mainWindow, mainViewModel, mainViewModel.SelectAdminSectionCommand, mainViewModel.OpenUsersCommand, Path.Combine(adminDir, "01-users.png"), runLogPath, "Users");
            await CaptureSectionPageAsync(mainWindow, mainViewModel, mainViewModel.SelectAdminSectionCommand, mainViewModel.OpenSettingsCommand, Path.Combine(adminDir, "02-settings-service-status.png"), runLogPath, "Settings service status");
            for (var tabIndex = 1; tabIndex <= 7; tabIndex++)
            {
                await CaptureSelectedTabPageAsync(
                    mainWindow,
                    OpenSectionPageAsync(mainWindow, mainViewModel, mainViewModel.SelectAdminSectionCommand, mainViewModel.OpenSettingsCommand),
                    Path.Combine(adminDir, $"{tabIndex + 2:00}-settings-{GetSettingsTabSlug(tabIndex)}.png"),
                    runLogPath,
                    $"Settings {GetSettingsTabSlug(tabIndex)}",
                    tabControlIndex: 0,
                    tabIndex: tabIndex);
            }

            var printLabelWindow = Host.Services.GetRequiredService<PrintLabelWindow>();
            printLabelWindow.Owner = main;
            LogStep("Showing print labels window.");
            printLabelWindow.Show();
            await WaitForUiAsync(printLabelWindow.Dispatcher);
            await Task.Delay(300);
            await CaptureWindowAsync(printLabelWindow, Path.Combine(dialogsDir, "01-print-labels.png"));
            printLabelWindow.Close();
            await Task.Delay(200);
            LogStep("Captured print labels window.");

            await CaptureStandaloneWindowAsync(mainWindow, CreateInfoDialogWindow(), Path.Combine(dialogsDir, "02-info-dialog.png"), runLogPath, "Info dialog");
            await CaptureStandaloneWindowAsync(mainWindow, CreateConfirmDialogWindow(), Path.Combine(dialogsDir, "03-confirm-dialog.png"), runLogPath, "Confirm dialog");
            await CaptureStandaloneWindowAsync(mainWindow, CreateInputDialogWindow(), Path.Combine(dialogsDir, "04-input-dialog.png"), runLogPath, "Input dialog");
            await CaptureStandaloneWindowAsync(mainWindow, ActivatorUtilities.CreateInstance<ItemDetailsWindow>(Host.Services, CreateSampleItem()), Path.Combine(dialogsDir, "05-item-details.png"), runLogPath, "Item details window");
            await CaptureStandaloneWindowAsync(mainWindow, CreateItemEditWindow(Host.Services), Path.Combine(dialogsDir, "06-item-edit.png"), runLogPath, "Item edit window");
            await CaptureStandaloneWindowAsync(mainWindow, CreateCustomerEditWindow(), Path.Combine(dialogsDir, "07-customer-edit.png"), runLogPath, "Customer edit window");
            await CaptureStandaloneWindowAsync(mainWindow, CreateRentalHistoryWindow(Host.Services), Path.Combine(dialogsDir, "08-rental-history.png"), runLogPath, "Rental history window");
            await CaptureStandaloneWindowAsync(mainWindow, CreateRentalsFilterWindow(mainViewModel.ManageRentals), Path.Combine(dialogsDir, "09-rentals-filter.png"), runLogPath, "Rentals filter window");
            await CaptureStandaloneWindowAsync(mainWindow, CreateImportMappingWindow(), Path.Combine(dialogsDir, "10-import-mapping.png"), runLogPath, "Import mapping window");
            await CaptureStandaloneWindowAsync(mainWindow, new ImageImportMappingWindow(), Path.Combine(dialogsDir, "11-image-import-mapping.png"), runLogPath, "Image import mapping window");
            await CapturePrintPreviewWindowAsync(mainWindow, CreateSamplePrintPreviewDocument(), Path.Combine(dialogsDir, "12-print-preview.png"), runLogPath, "Print preview window");
            await CaptureStandaloneWindowAsync(mainWindow, new MaintenanceEditWindow(CreateSampleMaintenanceRecord(), isNew: false), Path.Combine(dialogsDir, "13-maintenance-edit.png"), runLogPath, "Maintenance edit window");
            await CaptureStandaloneWindowAsync(mainWindow, new CalibrationEditWindow(CreateSampleCalibrationRecord(), isNew: false), Path.Combine(dialogsDir, "14-calibration-edit.png"), runLogPath, "Calibration edit window");
            await CaptureStandaloneWindowAsync(mainWindow, ActivatorUtilities.CreateInstance<ReservationEditWindow>(Host.Services, CreateSampleReservation(), false), Path.Combine(dialogsDir, "15-reservation-edit.png"), runLogPath, "Reservation edit window");
            await CaptureStandaloneWindowAsync(mainWindow, new KitEditWindow(CreateSampleKit(), isNew: false), Path.Combine(dialogsDir, "16-kit-edit.png"), runLogPath, "Kit edit window");
            await CaptureStandaloneWindowAsync(mainWindow, new KitItemEditWindow(CreateSampleKitItem(), isNew: false), Path.Combine(dialogsDir, "17-kit-item-edit.png"), runLogPath, "Kit item edit window");
            await CaptureStandaloneWindowAsync(mainWindow, CreateUsersEditWindow(Host.Services), Path.Combine(dialogsDir, "18-users-edit.png"), runLogPath, "Users edit window");
            await CaptureStandaloneWindowAsync(mainWindow, CreateRentItemPopupWindow(Host.Services), Path.Combine(dialogsDir, "19-rent-item-popup.png"), runLogPath, "Rent item popup");
            await CaptureStandaloneWindowAsync(mainWindow, CreateChangePasswordWindow(), Path.Combine(dialogsDir, "20-change-password.png"), runLogPath, "Change password window");
            await CaptureStandaloneWindowAsync(mainWindow, CreatePasswordPromptWindow(Host.Services), Path.Combine(dialogsDir, "21-password-prompt.png"), runLogPath, "Password prompt window");
            await CaptureStandaloneWindowAsync(mainWindow, CreatePasswordPromptWindow(Host.Services), Path.Combine(dialogsDir, "22-password-reset-prompt.png"), runLogPath, "Password reset prompt window", PreparePasswordResetPromptAsync);
            await CaptureStandaloneWindowAsync(mainWindow, CreateSetupWizardWindow(Host.Services), Path.Combine(dialogsDir, "23-setup-wizard.png"), runLogPath, "Setup wizard window");
            await CaptureStandaloneWindowAsync(mainWindow, CreateActivityDetailDialog(), Path.Combine(dialogsDir, "24-activity-detail-dialog.png"), runLogPath, "Activity detail dialog");
            await CaptureStandaloneWindowAsync(mainWindow, CreateCategoryDetailDialog(), Path.Combine(dialogsDir, "25-category-detail-dialog.png"), runLogPath, "Category detail dialog");
            await CaptureStandaloneWindowAsync(mainWindow, CreateImportExportResultDialog(), Path.Combine(dialogsDir, "26-import-export-result-dialog.png"), runLogPath, "Import export result dialog");
            await CaptureStandaloneWindowAsync(mainWindow, CreateUserDetailDialog(), Path.Combine(dialogsDir, "27-user-detail-dialog.png"), runLogPath, "User detail dialog");
            await CapturePrintPreviewWindowAsync(mainWindow, CreateItemSearchPreviewDocument(), Path.Combine(dialogsDir, "28-item-search-preview.png"), runLogPath, "Item search preview", "Item Search Intelligence");
            await CapturePrintPreviewWindowAsync(mainWindow, CreateDashboardPreviewDocument(), Path.Combine(dialogsDir, "29-dashboard-preview.png"), runLogPath, "Dashboard preview", "Dashboard Snapshot");
            await CapturePrintPreviewWindowAsync(mainWindow, CreateCustomerDirectoryPreviewDocument(), Path.Combine(dialogsDir, "30-customer-directory-preview.png"), runLogPath, "Customer directory preview", "Customer Directory");
            await CapturePrintPreviewWindowAsync(mainWindow, CreateItemDetailsPreviewDocument(), Path.Combine(dialogsDir, "31-item-details-preview.png"), runLogPath, "Item details preview", "Item Details - TL-101");
            await CapturePrintPreviewWindowAsync(mainWindow, CreateRentalRequestPreviewDocument(), Path.Combine(dialogsDir, "32-rental-request-preview.png"), runLogPath, "Rental request preview", "Request 9103");
            await CapturePrintPreviewWindowAsync(mainWindow, CreateRentalPickingSlipPreviewDocument(), Path.Combine(dialogsDir, "33-rental-picking-slip-preview.png"), runLogPath, "Rental picking slip preview", "Picking Slip - Rental 4128");
            await CapturePrintPreviewWindowAsync(mainWindow, CreateRentalInvoicePreviewDocument(), Path.Combine(dialogsDir, "34-rental-invoice-preview.png"), runLogPath, "Rental invoice preview", "Invoice - Rental 4128");
            await CapturePrintPreviewWindowAsync(mainWindow, CreateMaintenanceSchedulePreviewDocument(), Path.Combine(dialogsDir, "35-maintenance-schedule-preview.png"), runLogPath, "Maintenance schedule preview", "Maintenance Schedule");
            await CapturePrintPreviewWindowAsync(mainWindow, CreateCalibrationDuePreviewDocument(), Path.Combine(dialogsDir, "36-calibration-due-preview.png"), runLogPath, "Calibration due preview", "Calibration Due Report");
            await CapturePrintPreviewWindowAsync(mainWindow, CreateReservationHandoffPreviewDocument(), Path.Combine(dialogsDir, "37-reservation-handoff-preview.png"), runLogPath, "Reservation handoff preview", "Reservation 9103");
            await CapturePrintPreviewWindowAsync(mainWindow, CreateReservationDirectoryPreviewDocument(), Path.Combine(dialogsDir, "38-reservation-directory-preview.png"), runLogPath, "Reservation directory preview", "Reservation Directory");
            await CapturePrintPreviewWindowAsync(mainWindow, CreateKitDirectoryPreviewDocument(), Path.Combine(dialogsDir, "39-kit-directory-preview.png"), runLogPath, "Kit directory preview", "Kit Directory");
            await CapturePrintPreviewWindowAsync(mainWindow, CreateCategoryDirectoryPreviewDocument(), Path.Combine(dialogsDir, "40-category-directory-preview.png"), runLogPath, "Category directory preview", "Category Directory");
            await CapturePrintPreviewWindowAsync(mainWindow, CreateCategorySheetPreviewDocument(), Path.Combine(dialogsDir, "41-category-sheet-preview.png"), runLogPath, "Category sheet preview", "Category Sheet - Diagnostics");
            await CapturePrintPreviewWindowAsync(mainWindow, CreateActivityLogsPreviewDocument(), Path.Combine(dialogsDir, "42-activity-logs-preview.png"), runLogPath, "Activity logs preview", "Activity Logs");
            await CapturePrintPreviewWindowAsync(mainWindow, CreateImportExportLogPreviewDocument(), Path.Combine(dialogsDir, "43-import-export-log-preview.png"), runLogPath, "Import export log preview", "Import / Export Log");
            await CapturePrintPreviewWindowAsync(mainWindow, CreateUserDirectoryPreviewDocument(), Path.Combine(dialogsDir, "44-user-directory-preview.png"), runLogPath, "User directory preview", "User Directory");
            await CapturePrintPreviewWindowAsync(mainWindow, CreateReportsPreviewDocument(), Path.Combine(dialogsDir, "45-reports-preview.png"), runLogPath, "Reports preview", "Active Rentals");
        }

        static string EnsureCaptureDirectory(string root, string folderName)
        {
            var path = Path.Combine(root, folderName);
            Directory.CreateDirectory(path);
            return path;
        }

        static string GetSettingsTabSlug(int tabIndex) => tabIndex switch
        {
            1 => "database",
            2 => "general",
            3 => "item-display",
            4 => "email",
            5 => "branding",
            6 => "messaging",
            7 => "backups",
            _ => $"tab-{tabIndex + 1:00}"
        };

        static string GetImportExportTabSlug(int tabIndex) => tabIndex switch
        {
            1 => "item-data",
            2 => "customers",
            3 => "backup-images",
            4 => "run-log",
            _ => $"tab-{tabIndex + 1:00}"
        };

        static async Task CaptureSectionPageAsync(
            MainWindow mainWindow,
            MainViewModel mainViewModel,
            IRelayCommand sectionCommand,
            IAsyncRelayCommand pageCommand,
            string filePath,
            string runLogPath,
            string label)
        {
            await CapturePageAsync(
                mainWindow,
                OpenSectionPageAsync(mainWindow, mainViewModel, sectionCommand, pageCommand),
                filePath,
                runLogPath,
                label);
        }

        static async Task OpenSectionPageAsync(
            MainWindow mainWindow,
            MainViewModel mainViewModel,
            IRelayCommand sectionCommand,
            IAsyncRelayCommand pageCommand)
        {
            await SelectSectionAsync(mainWindow, mainViewModel, sectionCommand);
            await pageCommand.ExecuteAsync(null);
        }

        static async Task SelectSectionAsync(
            MainWindow mainWindow,
            MainViewModel mainViewModel,
            IRelayCommand sectionCommand)
        {
            await mainWindow.Dispatcher.InvokeAsync(() =>
            {
                if (!sectionCommand.CanExecute(null))
                    throw new InvalidOperationException("Unable to execute the requested navigation section command.");

                sectionCommand.Execute(null);
                mainWindow.UpdateLayout();
            }, DispatcherPriority.Background);

            await WaitForUiAsync(mainWindow.Dispatcher);
            await Task.Delay(250);
            await mainWindow.Dispatcher.InvokeAsync(mainWindow.UpdateLayout, DispatcherPriority.Background);
        }

        static async Task CapturePageAsync(MainWindow mainWindow, Task commandTask, string filePath, string runLogPath, string label)
        {
            File.AppendAllText(runLogPath, $"{DateTime.Now:O} Opening {label}.{Environment.NewLine}");
            await commandTask;
            await WaitForUiAsync(mainWindow.Dispatcher);
            await Task.Delay(350);
            await CaptureWindowAsync(mainWindow, filePath);
            File.AppendAllText(runLogPath, $"{DateTime.Now:O} Captured {label}.{Environment.NewLine}");
        }

        static async Task CaptureSelectedTabPageAsync(
            MainWindow mainWindow,
            Task commandTask,
            string filePath,
            string runLogPath,
            string label,
            int tabControlIndex,
            int tabIndex)
        {
            File.AppendAllText(runLogPath, $"{DateTime.Now:O} Opening {label}.{Environment.NewLine}");
            await commandTask;
            await WaitForUiAsync(mainWindow.Dispatcher);
            await SelectTabAsync(mainWindow, tabControlIndex, tabIndex);
            await Task.Delay(350);
            await CaptureWindowAsync(mainWindow, filePath);
            File.AppendAllText(runLogPath, $"{DateTime.Now:O} Captured {label}.{Environment.NewLine}");
        }

        static async Task SelectTabAsync(MainWindow mainWindow, int tabControlIndex, int tabIndex)
        {
            await mainWindow.Dispatcher.InvokeAsync(() =>
            {
                var tabControls = FindDescendants<System.Windows.Controls.TabControl>(mainWindow).ToList();
                if (tabControlIndex < 0 || tabControlIndex >= tabControls.Count)
                    throw new InvalidOperationException($"Unable to find TabControl index {tabControlIndex} on the current page.");

                var tabControl = tabControls[tabControlIndex];
                if (tabIndex < 0 || tabIndex >= tabControl.Items.Count)
                    throw new InvalidOperationException($"Unable to find tab index {tabIndex} on TabControl index {tabControlIndex}.");

                tabControl.SelectedIndex = tabIndex;
                tabControl.UpdateLayout();
            }, DispatcherPriority.Background);

            await WaitForUiAsync(mainWindow.Dispatcher);
        }

        static IEnumerable<T> FindDescendants<T>(DependencyObject root) where T : DependencyObject
        {
            if (root == null)
                yield break;

            var childCount = VisualTreeHelper.GetChildrenCount(root);
            for (var i = 0; i < childCount; i++)
            {
                var child = VisualTreeHelper.GetChild(root, i);
                if (child is T match)
                    yield return match;

                foreach (var descendant in FindDescendants<T>(child))
                    yield return descendant;
            }
        }

        static async Task WaitForUiAsync(Dispatcher dispatcher)
        {
            await dispatcher.InvokeAsync(() => { }, DispatcherPriority.Background);
            await dispatcher.InvokeAsync(() => { }, DispatcherPriority.ApplicationIdle);
        }

        static async Task CaptureWindowAsync(Window window, string filePath)
        {
            await window.Dispatcher.InvokeAsync(() =>
            {
                window.UpdateLayout();

                var width = Math.Max(1d, window.ActualWidth);
                var height = Math.Max(1d, window.ActualHeight);
                var source = PresentationSource.FromVisual(window);
                var dpiX = 96d;
                var dpiY = 96d;
                var pixelWidth = (int)Math.Ceiling(width);
                var pixelHeight = (int)Math.Ceiling(height);

                if (source?.CompositionTarget != null)
                {
                    var transform = source.CompositionTarget.TransformToDevice;
                    dpiX *= transform.M11;
                    dpiY *= transform.M22;
                    pixelWidth = Math.Max(1, (int)Math.Ceiling(width * transform.M11));
                    pixelHeight = Math.Max(1, (int)Math.Ceiling(height * transform.M22));
                }

                var bitmap = new RenderTargetBitmap(pixelWidth, pixelHeight, dpiX, dpiY, PixelFormats.Pbgra32);
                bitmap.Render(window);

                var encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(bitmap));

                using var stream = File.Create(filePath);
                encoder.Save(stream);
            }, DispatcherPriority.Render);
        }

        static async Task CaptureStandaloneWindowAsync(Window owner, Window window, string filePath, string runLogPath, string label, Func<Window, Task>? afterShowAsync = null)
        {
            File.AppendAllText(runLogPath, $"{DateTime.Now:O} Opening {label}.{Environment.NewLine}");
            await owner.Dispatcher.InvokeAsync(() =>
            {
                PrepareWindowForCapture(window);
                window.Owner = owner;
                window.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                window.Show();
                window.Activate();
            }, DispatcherPriority.Background);

            await WaitForUiAsync(window.Dispatcher);
            if (afterShowAsync != null)
                await afterShowAsync(window);
            await Task.Delay(300);
            await CaptureWindowAsync(window, filePath);
            await owner.Dispatcher.InvokeAsync(window.Close, DispatcherPriority.Background);
            File.AppendAllText(runLogPath, $"{DateTime.Now:O} Captured {label}.{Environment.NewLine}");
        }

        static async Task CapturePrintPreviewWindowAsync(Window owner, FlowDocument document, string filePath, string runLogPath, string label, string previewTitle = "QA Preview", string description = "")
        {
            File.AppendAllText(runLogPath, $"{DateTime.Now:O} Opening {label}.{Environment.NewLine}");
            var previewWindow = new PrintPreviewWindow();
            PrepareWindowForCapture(previewWindow);
            previewWindow.Owner = owner;
            previewWindow.WindowStartupLocation = WindowStartupLocation.CenterOwner;

            var showTask = owner.Dispatcher.InvokeAsync(() => previewWindow.ShowPreview(document, previewTitle, description), DispatcherPriority.Background).Task;
            await WaitForWindowVisibleAsync(previewWindow);
            await Task.Delay(300);
            await CaptureWindowAsync(previewWindow, filePath);
            await owner.Dispatcher.InvokeAsync(previewWindow.Close, DispatcherPriority.Background);
            await showTask;
            File.AppendAllText(runLogPath, $"{DateTime.Now:O} Captured {label}.{Environment.NewLine}");
        }

        static async Task WaitForWindowVisibleAsync(Window window, int timeoutMs = 10000)
        {
            var started = Environment.TickCount64;
            while (!window.IsVisible)
            {
                if (Environment.TickCount64 - started > timeoutMs)
                    throw new TimeoutException($"Timed out waiting for window '{window.GetType().Name}' to become visible.");

                await Task.Delay(50);
            }

            await WaitForUiAsync(window.Dispatcher);
        }

        static void PrepareWindowForCapture(Window window)
        {
            const double minimumWidth = 720;
            const double minimumHeight = 420;

            window.SizeToContent = SizeToContent.Manual;
            window.MinWidth = minimumWidth;
            window.MinHeight = minimumHeight;

            if (double.IsNaN(window.Width) || window.Width < minimumWidth)
                window.Width = minimumWidth;

            if (double.IsNaN(window.Height) || window.Height < minimumHeight)
                window.Height = minimumHeight;
        }

        static InfoDialogWindow CreateInfoDialogWindow()
        {
            var window = new InfoDialogWindow("This screenshot captures the standard informational dialog shown by copy, detail, and validation actions throughout the app.")
            {
                Title = "QA Info Dialog"
            };
            return window;
        }

        static ConfirmDialogWindow CreateConfirmDialogWindow()
        {
            var window = new ConfirmDialogWindow("This screenshot captures the confirmation dialog used before destructive or irreversible actions.")
            {
                Title = "QA Confirm Dialog"
            };
            return window;
        }

        static InputDialogWindow CreateInputDialogWindow() => new("QA Input", "Enter the value that completes this workflow step.", true);

        static ItemEditWindow CreateItemEditWindow(IServiceProvider services)
            => ActivatorUtilities.CreateInstance<ItemEditWindow>(
                services,
                CreateSampleItem(),
                (Action)(() => { }),
                (Action)(() => { }));

        static CustomerEditWindow CreateCustomerEditWindow()
            => new(CreateSampleCustomer(), () => { }, () => { });

        static RentalHistoryWindow CreateRentalHistoryWindow(IServiceProvider services)
        {
            var item = CreateSampleItem();
            var history = new[]
            {
                new RentalModel
                {
                    RentalID = 4101,
                    ItemID = item.ItemID,
                    ItemNumber = item.ItemNumber,
                    ItemLocation = item.Location,
                    CustomerName = "North Harbour Motors",
                    RentalDate = DateTime.Today.AddDays(-14),
                    DueDate = DateTime.Today.AddDays(-7),
                    ReturnDate = DateTime.Today.AddDays(-8),
                    Status = "Returned"
                },
                new RentalModel
                {
                    RentalID = 4128,
                    ItemID = item.ItemID,
                    ItemNumber = item.ItemNumber,
                    ItemLocation = item.Location,
                    CustomerName = "Auckland Fleet Service",
                    RentalDate = DateTime.Today.AddDays(-2),
                    DueDate = DateTime.Today.AddDays(3),
                    Status = "Rented"
                }
            };

            var vm = ActivatorUtilities.CreateInstance<RentalHistoryViewModel>(services, item, history);
            vm.SelectedEntry = vm.History.FirstOrDefault();
            return new RentalHistoryWindow(vm);
        }

        static RentalsFilterWindow CreateRentalsFilterWindow(ManageRentalsViewModel viewModel)
        {
            viewModel.FilterFrom = DateTime.Today.AddDays(-7);
            viewModel.FilterTo = DateTime.Today.AddDays(7);
            viewModel.SelectedStatus = "Rented";
            return new RentalsFilterWindow
            {
                DataContext = viewModel
            };
        }

        static ImportMappingWindow CreateImportMappingWindow()
        {
            var headers = new[] { "Item Number", "Item Name", "Location", "Category" };
            var properties = new[] { "ItemNumber", "Name", "Location", "Category" };
            return new ImportMappingWindow(headers, properties, new[] { "ItemNumber", "Name" });
        }

        static RentItemPopupWindow CreateRentItemPopupWindow(IServiceProvider services)
        {
            var customers = new[]
            {
                CreateSampleCustomer(),
                new CustomerModel
                {
                    CustomerID = 18,
                    Company = "Auckland Fleet Service",
                    Contact = "Jordan Patel",
                    Email = "advisor@aucklandfleet.example.com",
                    Phone = "09 555 0118",
                    Mobile = "021 555 0118",
                    Address = "42 Workshop Drive, Auckland"
                }
            };

            var viewModel = ActivatorUtilities.CreateInstance<RentItemPopupViewModel>(services, CreateSampleItem(), customers);
            viewModel.CustomerSearchText = "North";
            viewModel.SelectedCustomer = viewModel.FilteredCustomers.FirstOrDefault();
            viewModel.RentalDays = 5;
            return new RentItemPopupWindow { DataContext = viewModel, Title = "Checkout Item" };
        }

        static ChangePasswordWindow CreateChangePasswordWindow()
        {
            var window = new ChangePasswordWindow();
            if (window.DataContext is ChangePasswordViewModel vm)
            {
                vm.NewPassword = "AdminQ123!";
                vm.ConfirmPassword = "AdminQ123!";
            }

            return window;
        }

        static PasswordPromptWindow CreatePasswordPromptWindow(IServiceProvider services)
        {
            var window = ActivatorUtilities.CreateInstance<PasswordPromptWindow>(services);
            window.SelectedUser = CreateSampleAdminUser();
            window.ValidatePassword = password => password == "AdminQ123!";
            return window;
        }

        static SetupWizardWindow CreateSetupWizardWindow(IServiceProvider services)
        {
            var window = ActivatorUtilities.CreateInstance<SetupWizardWindow>(services);
            if (window.DataContext is SetupWizardViewModel vm)
            {
                vm.ApplicationName = "QA Inventory";
                vm.ItemLabelSingular = "Item";
                vm.ItemLabelPlural = "Tools";
                vm.CompanyLogoPath = @"C:\Branding\qa-logo.png";
                vm.NewPassword = "AdminQ123!";
                vm.ConfirmPassword = "AdminQ123!";
            }

            return window;
        }

        static InfoDialogWindow CreateActivityDetailDialog()
            => CreateDetailDialog(
                "Activity Detail",
                $"Timestamp: {DateTime.Today.AddHours(8):g}{Environment.NewLine}" +
                "User: qa.tech (ID 2)" + Environment.NewLine +
                "Type: Rental" + Environment.NewLine +
                "Action: Checked out Scan Item TL-101 to North Harbour Motors");

        static InfoDialogWindow CreateCategoryDetailDialog()
            => CreateDetailDialog(
                "Category Detail - Diagnostics",
                "Category #: 15" + Environment.NewLine +
                "Name: Diagnostics" + Environment.NewLine +
                "Directory label: Workshop Diagnostics" + Environment.NewLine + Environment.NewLine +
                "Admin handoff: confirm the category name matches staff language, assign matching inventory records, review search and filter coverage, and remove obsolete duplicates.");

        static InfoDialogWindow CreateImportExportResultDialog()
            => CreateDetailDialog(
                "Import / Export Result",
                "Items import completed." + Environment.NewLine +
                "42 row(s) processed." + Environment.NewLine +
                "2 warning(s): missing optional brand, trimmed duplicate whitespace." + Environment.NewLine +
                "0 critical error(s).");

        static InfoDialogWindow CreateUserDetailDialog()
            => CreateDetailDialog(
                "User Detail - qa.tech",
                "User #: 2" + Environment.NewLine +
                "Name: qa.tech" + Environment.NewLine +
                "Role: Workshop Staff" + Environment.NewLine +
                "Active: Yes" + Environment.NewLine +
                "Admin: No" + Environment.NewLine +
                "Access: Rentals / checkout, Customers, Maintenance, Calibration, Activity logs" + Environment.NewLine +
                "Lockout: Ready" + Environment.NewLine +
                "Password expired: No" + Environment.NewLine + Environment.NewLine +
                "Next steps: edit profile details, review permissions, upload a current photo, reset the password if blocked, or inspect recent account activity.");

        static InfoDialogWindow CreateDetailDialog(string title, string message)
        {
            var window = new InfoDialogWindow(message)
            {
                Title = title
            };
            return window;
        }

        static FlowDocument CreateSamplePrintPreviewDocument()
        {
            var document = new FlowDocument
            {
                PagePadding = new Thickness(32),
                FontFamily = new FontFamily("Segoe UI"),
                FontSize = 12
            };
            document.Blocks.Add(new Paragraph(new Bold(new Run("QA Print Preview")))
            {
                FontSize = 20,
                Margin = new Thickness(0, 0, 0, 10)
            });
            document.Blocks.Add(new Paragraph(new Run("This preview represents the printable surfaces shown by report, handoff, and summary buttons across the application.")));
            return document;
        }

        static FlowDocument CreateItemSearchPreviewDocument()
            => CreatePreviewDocument(
                "Item Search Intelligence",
                "Search term: scan item",
                new[]
                {
                    "Top result: TL-101 Scan Item | Available | Bay 2 - Shelf A",
                    "Recent searches show repeat demand from diagnostics and fleet teams.",
                    "Unavailable demand suggests ordering one additional unit before next month."
                });

        static FlowDocument CreateDashboardPreviewDocument()
            => CreatePreviewDocument(
                "Dashboard Snapshot",
                "Printed for qa.tech",
                new[]
                {
                    "Checked out today: 6 items",
                    "Returns due in 48 hours: 3 rentals",
                    "Incomplete items: 2 records need serial or location updates"
                });

        static FlowDocument CreateCustomerDirectoryPreviewDocument()
            => CreatePreviewDocument(
                "Customer Directory",
                "Visible customers: 2",
                new[]
                {
                    "North Harbour Motors | Casey Morgan | 09 555 0190",
                    "Auckland Fleet Service | Jordan Patel | 09 555 0118"
                });

        static FlowDocument CreateItemDetailsPreviewDocument()
            => CreatePreviewDocument(
                "Item Details - TL-101",
                "Scan Item | Launch | Bay 2 - Shelf A",
                new[]
                {
                    "Part number: SCAN-101",
                    "Quantity on hand: 2",
                    "Notes: Bi-directional diagnostic scanner with charger and leads."
                });

        static FlowDocument CreateRentalRequestPreviewDocument()
            => CreatePreviewDocument(
                "Request 9103",
                "Reservation request handoff",
                new[]
                {
                    "Customer: North Harbour Motors",
                    "Requested item: TL-101 Scan Item",
                    "Start: " + DateTime.Today.AddDays(1).ToString("yyyy-MM-dd") + " | End: " + DateTime.Today.AddDays(4).ToString("yyyy-MM-dd")
                });

        static FlowDocument CreateRentalPickingSlipPreviewDocument()
            => CreatePreviewDocument(
                "Picking Slip - Rental 4128",
                "Advisor handoff before checkout",
                new[]
                {
                    "Item: TL-101 Scan Item",
                    "Customer: Auckland Fleet Service",
                    "Verify charger, leads, and carry case before release."
                });

        static FlowDocument CreateRentalInvoicePreviewDocument()
            => CreatePreviewDocument(
                "Invoice - Rental 4128",
                "Customer billing summary",
                new[]
                {
                    "Rental charge: $85.00",
                    "Accessory pack: $15.00",
                    "Tax: $15.00 | Total: $115.00"
                });

        static FlowDocument CreateMaintenanceSchedulePreviewDocument()
            => CreatePreviewDocument(
                "Maintenance Schedule",
                "Upcoming maintenance workload",
                new[]
                {
                    "TL-101 Scan Item | Routine | Scheduled in 4 days",
                    "TL-204 Torque Wrench | Inspection | Scheduled in 9 days"
                });

        static FlowDocument CreateCalibrationDuePreviewDocument()
            => CreatePreviewDocument(
                "Calibration Due Report",
                "Current calibration follow-up",
                new[]
                {
                    "TL-204 Torque Wrench | Due " + DateTime.Today.AddMonths(2).ToString("yyyy-MM-dd"),
                    "TL-318 Pressure Gauge | Overdue by 6 days"
                });

        static FlowDocument CreateReservationHandoffPreviewDocument()
            => CreatePreviewDocument(
                "Reservation 9103",
                "Reservation handoff checklist",
                new[]
                {
                    "Customer: North Harbour Motors",
                    "Status: Confirmed",
                    "Call before pickup after lunch and verify accessories."
                });

        static FlowDocument CreateReservationDirectoryPreviewDocument()
            => CreatePreviewDocument(
                "Reservation Directory",
                "Visible reservations: 2",
                new[]
                {
                    "9103 | TL-101 Scan Item | Confirmed | North Harbour Motors",
                    "9104 | TL-204 Torque Wrench | Pending | Auckland Fleet Service"
                });

        static FlowDocument CreateKitDirectoryPreviewDocument()
            => CreatePreviewDocument(
                "Kit Directory",
                "Visible kits: 1",
                new[]
                {
                    "KIT-640 Diagnostics Starter Kit",
                    "Contents: scan item, charger, adapter pack"
                });

        static FlowDocument CreateCategoryDirectoryPreviewDocument()
            => CreatePreviewDocument(
                "Category Directory",
                "Visible categories: 2",
                new[]
                {
                    "15 | Diagnostics | Verify assignment and search coverage",
                    "21 | Torque Tools | Review calibration workflow alignment"
                });

        static FlowDocument CreateCategorySheetPreviewDocument()
            => CreatePreviewDocument(
                "Category Sheet - Diagnostics",
                "Printed category checklist",
                new[]
                {
                    "[ ] Name matches staff language",
                    "[ ] Matching inventory records are assigned",
                    "[ ] Search and filter coverage has been checked"
                });

        static FlowDocument CreateActivityLogsPreviewDocument()
            => CreatePreviewDocument(
                "Activity Logs",
                "Visible rows: 3",
                new[]
                {
                    "08:00 qa.tech | Checked out Scan Item TL-101",
                    "09:15 admin | Updated backup retention settings",
                    "10:42 qa.tech | Confirmed reservation 9103"
                });

        static FlowDocument CreateImportExportLogPreviewDocument()
            => CreatePreviewDocument(
                "Import / Export Operation Log",
                "Most recent 3 operations",
                new[]
                {
                    "1. Items import completed with 2 warnings",
                    "2. Customers export completed successfully",
                    "3. Database backup saved to nightly archive"
                });

        static FlowDocument CreateUserDirectoryPreviewDocument()
            => CreatePreviewDocument(
                "User Directory",
                "Visible users: 2",
                new[]
                {
                    "2 | qa.tech | Workshop Staff | Rentals / checkout, Maintenance, Activity logs",
                    "1 | admin | Admin | Full admin access"
                });

        static FlowDocument CreateReportsPreviewDocument()
            => CreatePreviewDocument(
                "Active Rentals",
                "Last run just now - 3 actionable rows",
                new[]
                {
                    "Rental: TL-101 checked out to North Harbour Motors | Next action: confirm due-back date",
                    "Overdue: TL-318 Pressure Gauge late by 2 days | Next action: follow up with customer",
                    "Request: reservation 9103 waiting on final pickup confirmation"
                });

        static FlowDocument CreatePreviewDocument(string title, string subtitle, IEnumerable<string> lines)
        {
            var document = new FlowDocument
            {
                PagePadding = new Thickness(32),
                FontFamily = new FontFamily("Segoe UI"),
                FontSize = 12
            };

            document.Blocks.Add(new Paragraph(new Bold(new Run(title)))
            {
                FontSize = 20,
                Margin = new Thickness(0, 0, 0, 8)
            });
            document.Blocks.Add(new Paragraph(new Run(subtitle))
            {
                FontSize = 11,
                Margin = new Thickness(0, 0, 0, 12)
            });

            foreach (var line in lines)
            {
                document.Blocks.Add(new Paragraph(new Run(line))
                {
                    Margin = new Thickness(0, 0, 0, 6)
                });
            }

            return document;
        }

        static MaintenanceRecord CreateSampleMaintenanceRecord() => new()
        {
            MaintenanceID = 7201,
            ItemID = 101,
            ItemNumber = "TL-101",
            ItemName = "Scan Item",
            MaintenanceType = "Routine",
            Description = "Annual inspection and battery replacement",
            ScheduledDate = DateTime.Today.AddDays(4),
            Status = "Scheduled",
            PerformedBy = "Workshop QA",
            Notes = "Prepare charger and calibration check before release.",
            Cost = 149.95m
        };

        static CalibrationRecord CreateSampleCalibrationRecord() => new()
        {
            CalibrationID = 8302,
            ItemID = 102,
            ItemNumber = "TL-204",
            ItemName = "Torque Wrench",
            CalibrationDate = DateTime.Today.AddMonths(-10),
            NextCalibrationDue = DateTime.Today.AddMonths(2),
            CalibratedBy = "Metro Calibrations",
            CertificateNumber = "CAL-2026-0042",
            Standard = "ISO 6789",
            Result = "Pass",
            Notes = "Stored in cabinet B after verification.",
            Cost = 89.50m
        };

        static Reservation CreateSampleReservation() => new()
        {
            ReservationID = 9103,
            ItemID = 101,
            CustomerID = 12,
            ItemNumber = "TL-101",
            ItemName = "Scan Item",
            CustomerName = "North Harbour Motors",
            ReservationDate = DateTime.Today.AddDays(-1),
            StartDate = DateTime.Today.AddDays(1),
            EndDate = DateTime.Today.AddDays(4),
            Quantity = 1,
            Status = "Confirmed",
            Notes = "Call before pickup after lunch."
        };

        static Kit CreateSampleKit() => new()
        {
            KitID = 640,
            KitNumber = "KIT-640",
            Name = "Diagnostics Starter Kit",
            Category = "Electronics",
            Description = "Handheld scan item, charger, and adapter pack.",
            IsActive = true,
            UpdatedAt = DateTime.Now.AddDays(-2)
        };

        static KitItem CreateSampleKitItem() => new()
        {
            KitItemID = 641,
            KitID = 640,
            ItemID = 101,
            ItemNumber = "TL-101",
            ItemName = "Scan Item",
            Quantity = 1,
            IsOptional = false
        };

        static UsersEditWindow CreateUsersEditWindow(IServiceProvider services)
            => ActivatorUtilities.CreateInstance<UsersEditWindow>(
                services,
                new User
                {
                    UserID = 2,
                    UserName = "qa.tech",
                    Role = "Workshop Staff",
                    Email = "qa.tech@example.com",
                    Phone = "09 555 0101",
                    Mobile = "021 555 0101",
                    Address = "17 Item Lane",
                    IsActive = true,
                    IsAdmin = false,
                    Permissions = User.BuildPermissions(User.DefaultUserPermissions)
                },
                (Func<Task>)(() => Task.CompletedTask),
                (Action)(() => { }));

        static ItemModel CreateSampleItem() => new()
        {
            ItemID = 101,
            ItemNumber = "TL-101",
            PartNumber = "SCAN-101",
            Name = "Scan Item",
            Brand = "Launch",
            Location = "Bay 2 - Shelf A",
            Notes = "Bi-directional diagnostic scanner with charger and leads.",
            QuantityOnHand = 2,
            UpdatedAt = DateTime.Now.AddDays(-1)
        };

        static CustomerModel CreateSampleCustomer() => new()
        {
            CustomerID = 12,
            Company = "North Harbour Motors",
            Contact = "Casey Morgan",
            Email = "service@northharbour.example.com",
            Phone = "09 555 0190",
            Mobile = "021 555 0190",
            Address = "17 Foundry Road, Auckland"
        };

        static User CreateSampleAdminUser() => new()
        {
            UserID = 1,
            UserName = "admin",
            Role = "Administrator",
            Email = "admin@example.com",
            IsAdmin = true,
            IsActive = true,
            PasswordExpired = false,
            FailedLoginAttempts = 2,
            Permissions = User.BuildPermissions(User.PermissionLabels.Keys)
        };

        static async Task PreparePasswordResetPromptAsync(Window window)
        {
            await window.Dispatcher.InvokeAsync(() =>
            {
                if (window is not PasswordPromptWindow prompt)
                    return;

                var errorText = prompt.FindName("ErrorTextBlock") as TextBlock;
                if (errorText != null)
                {
                    errorText.Text = "Incorrect password. Please try again.";
                    errorText.Visibility = Visibility.Visible;
                }

                var forgotPasswordButton = prompt.FindName("ForgotPasswordButton") as FrameworkElement;
                if (forgotPasswordButton != null)
                    forgotPasswordButton.Visibility = Visibility.Visible;
            }, DispatcherPriority.Background);

            await WaitForUiAsync(window.Dispatcher);
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

        internal async void HandleTaskException(AggregateException ex, UnobservedTaskExceptionEventArgs? e = null)
        {
            _logger.LogError(ex, "Unobserved task exception");
            await _dialogService.ShowInfoAsync("An unexpected background error occurred.", "Error");
            if (e != null) e.SetObserved();
        }

        protected async override void OnExit(ExitEventArgs e)
        {
            await Host.StopAsync();
            Host.Dispose();
            Log.CloseAndFlush();
            base.OnExit(e);
        }

        void OnWindowLoaded(object sender, RoutedEventArgs e)
        {
            if (sender is Window window)
            {
                window.SetResourceReference(Window.IconProperty, "WindowIcon");
                ApplyBackgroundOverlay(window);
                InformationalTooltipService.Apply(window);
            }
        }

        void OnUserControlLoaded(object sender, RoutedEventArgs e)
        {
            if (sender is UserControl userControl)
                InformationalTooltipService.Apply(userControl);
        }

        internal static void ApplyBackgroundOverlay(Window window)
        {
            if (window is MainWindow ||
                (bool)window.GetValue(HasAppliedBackgroundOverlayProperty) ||
                HasThemedWindowOverlay(window))
            {
                return;
            }

            var existingContent = window.Content;
            if (existingContent is null)
                return;

            window.Content = null;

            var root = new Grid();
            root.SetResourceReference(Panel.BackgroundProperty, "BackgroundBrush");

            var overlay = new Border
            {
                IsHitTestVisible = false
            };
            overlay.SetResourceReference(Border.BackgroundProperty, "ThemeAppBackgroundOverlayBrush");

            var contentPresenter = new ContentPresenter
            {
                Content = existingContent
            };

            root.Children.Add(overlay);
            root.Children.Add(contentPresenter);
            window.Content = root;
            window.SetValue(HasAppliedBackgroundOverlayProperty, true);
        }

        static bool HasThemedWindowOverlay(Window window)
        {
            if (window.Content is not DependencyObject content)
                return false;

            var overlayStyle = Current?.TryFindResource("ThemedWindowOverlay");
            if (overlayStyle is null)
                return false;

            return FindDescendant<Border>(content, border => ReferenceEquals(border.Style, overlayStyle)) is not null;
        }

        static T? FindDescendant<T>(DependencyObject root, Func<T, bool> predicate) where T : DependencyObject
        {
            var childCount = VisualTreeHelper.GetChildrenCount(root);
            for (var i = 0; i < childCount; i++)
            {
                var child = VisualTreeHelper.GetChild(root, i);
                if (child is T match && predicate(match))
                    return match;

                var descendant = FindDescendant(child, predicate);
                if (descendant is not null)
                    return descendant;
            }

            return null;
        }

        public void ApplyWindowBranding(string? logoPath)
        {
            void ApplyOnUiThread()
            {
                var iconSource = LoadWindowIcon(logoPath);
                Resources["WindowIcon"] = iconSource;

                foreach (Window window in Current.Windows)
                {
                    window.SetResourceReference(Window.IconProperty, "WindowIcon");
                    window.Icon = iconSource;
                }
            }

            if (Dispatcher.CheckAccess())
            {
                ApplyOnUiThread();
                return;
            }

            Dispatcher.Invoke(ApplyOnUiThread, DispatcherPriority.Send);
        }

        static ImageSource LoadWindowIcon(string? logoPath)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(logoPath))
                {
                    var fullPath = PathHelper.GetAbsolutePath(logoPath, true);
                    if (!string.IsNullOrWhiteSpace(fullPath) && File.Exists(fullPath))
                        return LoadFrozenBitmap(new Uri(fullPath, UriKind.Absolute));
                }
            }
            catch
            {
            }

            return LoadFrozenBitmap(new Uri(DefaultLogoResourceUri, UriKind.Absolute));
        }

        static BitmapImage LoadFrozenBitmap(Uri uri)
        {
            var bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.UriSource = uri;
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.EndInit();
            bitmap.Freeze();
            return bitmap;
        }
    }
}
