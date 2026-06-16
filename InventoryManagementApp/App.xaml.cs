using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
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

namespace InventoryManagementApp
{
    public partial class App : System.Windows.Application
    {
        internal const string DefaultLogoResourceUri = "pack://application:,,,/InventoryManagementApp;component/Resources/DefaultLogo.png";
        public IHost Host { get; }
        private readonly ILogger<App> _logger;
        private readonly IDialogService _dialogService;

        public App() : this(BuildHost()) { }

        internal App(IHost host)
        {
            Host = host;
            _logger = Host.Services.GetRequiredService<ILogger<App>>();
            _dialogService = Host.Services.GetRequiredService<IDialogService>();

            EventManager.RegisterClassHandler(typeof(Window), FrameworkElement.LoadedEvent, new RoutedEventHandler(OnWindowLoaded));

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
                ApplyWindowBranding(await settingsService.GetSettingAsync("CompanyLogoPath"));
            }
            await LabelProvider.Instance.InitializeAsync(settingsService);

            var main = (Window)Host.Services.GetRequiredService<IMainWindow>();
            Current.MainWindow = main;
            ShutdownMode = ShutdownMode.OnMainWindowClose;
            main.Show();

            // Yield once on the UI dispatcher so startup continues after Show()
            // without depending on an ApplicationIdle pump in tests.
            await main.Dispatcher.InvokeAsync(() => { }, DispatcherPriority.Background);

            if (qaOptions != null)
            {
                await RunQaScreenshotsAsync(main, qaOptions);
                main.Close();
                return;
            }

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
                $"- `06-dialogs` standalone windows and dialogs{Environment.NewLine}");
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

            main.Activate();
            main.Focus();
            await WaitForUiAsync(main.Dispatcher);
            await Task.Delay(400);

            var itemSlug = options.BuildItemSlug();
            await CaptureWindowAsync(mainWindow, Path.Combine(overviewDir, $"01-search-{itemSlug}-results.png"));
            LogStep("Captured overview search page.");

            await CaptureSelectedTabPageAsync(
                mainWindow,
                mainViewModel.OpenSearchItemsCommand.ExecuteAsync(null),
                Path.Combine(overviewDir, $"02-search-{itemSlug}-recent-searches.png"),
                runLogPath,
                "Search intelligence recent searches",
                tabControlIndex: 0,
                tabIndex: 0);
            await CaptureSelectedTabPageAsync(
                mainWindow,
                mainViewModel.OpenSearchItemsCommand.ExecuteAsync(null),
                Path.Combine(overviewDir, $"03-search-{itemSlug}-unavailable-demand.png"),
                runLogPath,
                "Search intelligence unavailable demand",
                tabControlIndex: 0,
                tabIndex: 1);

            await CapturePageAsync(mainWindow, mainViewModel.OpenDashboardCommand.ExecuteAsync(null), Path.Combine(overviewDir, "04-dashboard-summary.png"), runLogPath, "Dashboard summary");
            await CaptureSelectedTabPageAsync(
                mainWindow,
                mainViewModel.OpenDashboardCommand.ExecuteAsync(null),
                Path.Combine(overviewDir, "05-dashboard-recent-activity.png"),
                runLogPath,
                "Dashboard recent activity",
                tabControlIndex: 0,
                tabIndex: 0);
            await CaptureSelectedTabPageAsync(
                mainWindow,
                mainViewModel.OpenDashboardCommand.ExecuteAsync(null),
                Path.Combine(overviewDir, "06-dashboard-items-with-issues.png"),
                runLogPath,
                "Dashboard items with issues",
                tabControlIndex: 0,
                tabIndex: 1);

            await CapturePageAsync(mainWindow, mainViewModel.OpenManageItemsCommand.ExecuteAsync(null), Path.Combine(operationsDir, $"01-manage-{itemSlug}.png"), runLogPath, "Manage items");
            await CapturePageAsync(mainWindow, mainViewModel.OpenRentalsCommand.ExecuteAsync(null), Path.Combine(operationsDir, "02-rentals.png"), runLogPath, "Rentals");
            await CapturePageAsync(mainWindow, mainViewModel.OpenCustomersCommand.ExecuteAsync(null), Path.Combine(operationsDir, "03-customers.png"), runLogPath, "Customers");
            await CapturePageAsync(mainWindow, mainViewModel.OpenMaintenanceCommand.ExecuteAsync(null), Path.Combine(operationsDir, "04-maintenance.png"), runLogPath, "Maintenance");
            await CapturePageAsync(mainWindow, mainViewModel.OpenCalibrationCommand.ExecuteAsync(null), Path.Combine(operationsDir, "05-calibration.png"), runLogPath, "Calibration");
            await CapturePageAsync(mainWindow, mainViewModel.OpenReservationsCommand.ExecuteAsync(null), Path.Combine(operationsDir, "06-reservations.png"), runLogPath, "Reservations");
            await CapturePageAsync(mainWindow, mainViewModel.OpenKitManagementCommand.ExecuteAsync(null), Path.Combine(operationsDir, "07-kits.png"), runLogPath, "Kits");
            await CapturePageAsync(mainWindow, mainViewModel.OpenCategoriesCommand.ExecuteAsync(null), Path.Combine(operationsDir, "08-categories.png"), runLogPath, "Categories");

            await CapturePageAsync(mainWindow, mainViewModel.OpenReportsCommand.ExecuteAsync(null), Path.Combine(insightsDir, "01-reports.png"), runLogPath, "Reports");
            await CapturePageAsync(mainWindow, mainViewModel.OpenActivityLogsCommand.ExecuteAsync(null), Path.Combine(insightsDir, "02-activity-logs.png"), runLogPath, "Activity logs");

            await CapturePageAsync(mainWindow, mainViewModel.OpenImportExportCommand.ExecuteAsync(null), Path.Combine(dataDir, "01-import-export.png"), runLogPath, "Import export");

            await CapturePageAsync(mainWindow, mainViewModel.OpenUsersCommand.ExecuteAsync(null), Path.Combine(adminDir, "01-users.png"), runLogPath, "Users");
            await CapturePageAsync(mainWindow, mainViewModel.OpenSettingsCommand.ExecuteAsync(null), Path.Combine(adminDir, "02-settings-database.png"), runLogPath, "Settings database");
            for (var tabIndex = 1; tabIndex <= 6; tabIndex++)
            {
                await CaptureSelectedTabPageAsync(
                    mainWindow,
                    mainViewModel.OpenSettingsCommand.ExecuteAsync(null),
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
        }

        static string EnsureCaptureDirectory(string root, string folderName)
        {
            var path = Path.Combine(root, folderName);
            Directory.CreateDirectory(path);
            return path;
        }

        static string GetSettingsTabSlug(int tabIndex) => tabIndex switch
        {
            1 => "general",
            2 => "item-display",
            3 => "email",
            4 => "branding",
            5 => "messaging",
            6 => "backups",
            _ => $"tab-{tabIndex + 1:00}"
        };

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
            }
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
