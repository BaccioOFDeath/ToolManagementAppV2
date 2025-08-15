using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using Forms = System.Windows.Forms;
using ToolManagementAppV2.Interfaces;
using ToolManagementAppV2.Models.Domain;
using ToolManagementAppV2.Services;
using ToolManagementAppV2.Services.Rentals;
using ToolManagementAppV2.Services.Tools;
using ToolManagementAppV2.Services.Users;
using ToolManagementAppV2.Views;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using ToolManagementAppV2.Utilities.IO;

namespace ToolManagementAppV2.ViewModels
{
    public class MainViewModel : ObservableObject, IDisposable
    {
        readonly IToolService _toolService;
        readonly IUserService _userService;
        readonly IUserContext _userContext;
        readonly ICustomerService _customerService;
        readonly IRentalService _rentalService;
        readonly ActivityLogService _activityLogService;
        readonly ISettingsService _settingsService;
        readonly IFileDialogService _fileDialogService;
        readonly IDialogService _dialogService;
        readonly ILogger<MainViewModel> _logger;
        readonly Func<Task<bool>> _showLoginWindow;

        EventHandler<User?>? _userContextChangedHandler;
        PropertyChangedEventHandler? _toolManagementPropertyChangedHandler;

        public ToolManagementViewModel ToolManagement { get; }
        public UserManagementViewModel UserManagement { get; }
        public CustomerManagementViewModel CustomerManagement { get; }
        public ManageRentalsViewModel ManageRentals { get; }
        public ImportExportViewModel ImportExport { get; }
        public ActivityLogsViewModel ActivityLogs { get; }
        public ReportsViewModel Reports { get; }
        public SettingsViewModel Settings { get; }

        private Page _currentPage;
        public Page CurrentPage
        {
            get => _currentPage;
            set
            {
                if (SetProperty(ref _currentPage, value))
                    CurrentPageTitle = value?.Title ?? value?.GetType().Name ?? "Dashboard";
            }
        }

        private string _currentPageTitle = "Dashboard";
        public string CurrentPageTitle
        {
            get => _currentPageTitle;
            private set => SetProperty(ref _currentPageTitle, value);
        }

        private string _globalSearchText = string.Empty;
        public string GlobalSearchText
        {
            get => _globalSearchText;
            set => SetProperty(ref _globalSearchText, value);
        }

        public bool IsCurrentUserAdmin => _userContext.IsAdmin;

        public string CurrentUserName => _userContext.UserName;

        public string CurrentUserRole => _userContext.Role;

        public ToolModel? SelectedTool => ToolManagement.SelectedTool;

        public void RefreshCurrentUser()
        {
            OnPropertyChanged(nameof(IsCurrentUserAdmin));
            OnPropertyChanged(nameof(CurrentUserName));
            OnPropertyChanged(nameof(CurrentUserRole));
        }

        public IRelayCommand OpenDashboardCommand { get; }
        public IAsyncRelayCommand OpenSearchToolsCommand { get; }
        public IAsyncRelayCommand OpenManageToolsCommand { get; }
        public IAsyncRelayCommand OpenRentalsCommand { get; }
        public IAsyncRelayCommand OpenCustomersCommand { get; }
        public IAsyncRelayCommand OpenUsersCommand { get; }
        public IRelayCommand OpenSettingsCommand { get; }
        public IRelayCommand OpenImportExportCommand { get; }
        public IRelayCommand OpenActivityLogsCommand { get; }
        public IRelayCommand OpenReportsCommand { get; }
        public IAsyncRelayCommand OpenImportMappingWindowCommand { get; }
        public IRelayCommand OpenImageImportMappingWindowCommand { get; }
        public IRelayCommand ExitCommand { get; }
        public IAsyncRelayCommand GlobalSearchCommand { get; }
        public IAsyncRelayCommand SwitchUserCommand { get; }

        public IAsyncRelayCommand<ToolModel?> OpenRentalHistoryWindowCommand { get; }
        public IRelayCommand OpenPrintPreviewWindowCommand { get; }
        public IRelayCommand OpenPrintLabelWindowCommand { get; }
        public IRelayCommand OpenScannerStatusWindowCommand { get; }

        public MainViewModel(IToolService toolService,
                             IUserService userService,
                             IUserContext userContext,
                             ICustomerService customerService,
                             IRentalService rentalService,
                             IFileDialogService fileDialogService,
                             ActivityLogService activityLogService,
                             ISettingsService settingsService,
                             IDatabaseBackupService databaseService,
                             IDialogService dialogService,
                             ILogger<MainViewModel>? logger = null,
                             Func<Task<bool>>? showLoginWindow = null)
        {
            _toolService = toolService;
            _userService = userService;
            _userContext = userContext;
            _customerService = customerService;
            _rentalService = rentalService;
            _activityLogService = activityLogService;
            _settingsService = settingsService;
            _dialogService = dialogService;
            _fileDialogService = fileDialogService;
            _logger = logger ?? NullLogger<MainViewModel>.Instance;
            _showLoginWindow = showLoginWindow ?? new Func<Task<bool>>(async () =>
            {
                var login = new LoginWindow(_userContext, _userService, _settingsService, _dialogService)
                {
                    Owner = System.Windows.Application.Current.MainWindow
                };
                if (login.DataContext is LoginViewModel lvm)
                    await lvm.InitializeAsync();
                return login.ShowDialog() == true;
            });

            _userContextChangedHandler = (_, _) => RefreshCurrentUser();
            _userContext.UserChanged += _userContextChangedHandler;

            ToolManagement = new ToolManagementViewModel(toolService, customerService, rentalService, _dialogService);
            _toolManagementPropertyChangedHandler = (s, e) =>
            {
                if (e.PropertyName == nameof(ToolManagementViewModel.SelectedTool))
                {
                    OnPropertyChanged(nameof(SelectedTool));
                    OpenRentalHistoryWindowCommand.NotifyCanExecuteChanged();
                }
            };
            ToolManagement.PropertyChanged += _toolManagementPropertyChangedHandler;
            UserManagement = new UserManagementViewModel(userService, fileDialogService, _dialogService);
            CustomerManagement = new CustomerManagementViewModel(customerService, _dialogService);
            ManageRentals = new ManageRentalsViewModel(rentalService, _dialogService);
            ImportExport = new ImportExportViewModel(toolService, customerService, fileDialogService, databaseService, _dialogService);
            Reports = new ReportsViewModel(new ReportService(toolService, rentalService, activityLogService, customerService, userService));
            ActivityLogs = new ActivityLogsViewModel(activityLogService);
            Settings = new SettingsViewModel(_fileDialogService, _settingsService, _dialogService);

            OpenDashboardCommand = new RelayCommand(() =>
            {
                var vm = new DashboardViewModel(_toolService, _rentalService, _customerService, _userService, _activityLogService, OpenManageToolsCommand, OpenRentalsCommand, OpenImportExportCommand);
                var page = new DashboardPage { DataContext = vm, Title = "Dashboard" };
                CurrentPage = page;
            });

            OpenSearchToolsCommand = new AsyncRelayCommand(async () =>
            {
                await ToolManagement.LoadToolsAsync();
                var page = new ToolSearchPage { DataContext = ToolManagement, Title = "Search Tools" };
                // If your ToolManagement VM supports a query setter, apply GlobalSearchText there.
                CurrentPage = page;
            });

            OpenManageToolsCommand = new AsyncRelayCommand(async () =>
            {
                await ToolManagement.LoadToolsAsync();
                var page = new ManageToolsPage { DataContext = ToolManagement, Title = "Manage Tools" };
                CurrentPage = page;
            });

            OpenRentalsCommand = new AsyncRelayCommand(async () =>
            {
                await ManageRentals.LoadRentalsAsync();
                var page = new ManageRentalsPage { DataContext = ManageRentals, Title = "Manage Rentals" };
                CurrentPage = page;
            });

            OpenCustomersCommand = new AsyncRelayCommand(async () =>
            {
                await CustomerManagement.LoadCustomersAsync();
                var page = new CustomersPage { DataContext = CustomerManagement, Title = "Customers" };
                CurrentPage = page;
            });

            OpenUsersCommand = new AsyncRelayCommand(async () =>
            {
                await UserManagement.LoadUsersAsync();
                var page = new UsersPage
                {
                    // UsersPage expects a UserManagementViewModel as its DataContext
                    DataContext = UserManagement,
                    Title = "Users"
                };
                CurrentPage = page;
            });

            OpenSettingsCommand = new RelayCommand(() =>
            {
                var page = new SettingsPage
                    {
                        DataContext = Settings,
                        Title = "Settings"
                    };
                CurrentPage = page;
            });

            OpenImportExportCommand = new RelayCommand(() =>
            {
                var page = new ImportExportPage { DataContext = ImportExport, Title = "Import / Export" };
                CurrentPage = page;
            });

            OpenActivityLogsCommand = new RelayCommand(() =>
            {
                ActivityLogs.LoadLogs();
                var page = new ActivityLogsPage { DataContext = ActivityLogs, Title = "Activity Logs" };
                CurrentPage = page;
            });

            OpenReportsCommand = new RelayCommand(() =>
            {
                var page = new ReportsPage { DataContext = Reports, Title = "Reports" };
                CurrentPage = page;
            });

            OpenImportMappingWindowCommand = new AsyncRelayCommand(OpenImportMappingWindowAsync);

            OpenImageImportMappingWindowCommand = new RelayCommand(() =>
            {
                using var dlg = new Forms.FolderBrowserDialog();
                if (dlg.ShowDialog() != Forms.DialogResult.OK)
                    return;
                var selector = _dialogService.ShowImageImportMapping();
                if (selector != null)
                {
                    var result = _toolService.ImportToolImages(dlg.SelectedPath, selector);
                    _dialogService.ShowInfo(
                        $"Imported {result.ImportedCount} images. Unmatched: {result.UnmatchedFiles.Count}, Conflicts: {result.ConflictingFiles.Count}",
                        "Import Images");
                }
            });

            GlobalSearchCommand = new AsyncRelayCommand(async () =>
            {
                ToolManagement.SearchText = GlobalSearchText;
                await OpenSearchToolsCommand.ExecuteAsync(null);
                if (ToolManagement.SearchCommand != null)
                    await ToolManagement.SearchCommand.ExecuteAsync(null);
                GlobalSearchText = string.Empty;
            });

            SwitchUserCommand = new AsyncRelayCommand(async () =>
            {
                if (await _showLoginWindow())
                {
                    OpenDashboardCommand.Execute(null);
                }
                else
                {
                    _logger.LogWarning("Switch user cancelled.");
                    _dialogService.ShowInfo("Switch user cancelled.", "Switch User");
                }
            });

            ExitCommand = new RelayCommand(() =>
            {
                try
                {
                    System.Windows.Application.Current.Shutdown();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to shutdown application");
                    _dialogService.ShowInfo("Failed to shutdown application", "Error");
                    try
                    {
                        System.Windows.Application.Current?.Shutdown();
                    }
                    catch (Exception shutdownEx)
                    {
                        _logger.LogError(shutdownEx, "Secondary shutdown attempt failed");
                    }
                }
            });

            OpenRentalHistoryWindowCommand = new AsyncRelayCommand<ToolModel?>(async tool =>
            {
                if (tool == null) return;
                var history = await _rentalService.GetRentalHistoryForToolAsync(tool.ToolID);
                _dialogService.ShowRentalHistory(tool, history);
            }, tool => tool != null);

            OpenPrintPreviewWindowCommand = new RelayCommand(() =>
            {
                var doc = new FlowDocument(new Paragraph(new Run("Preview document")));
                _dialogService.ShowPrintPreview(doc, "Print Preview", string.Empty);
            });

            OpenPrintLabelWindowCommand = new RelayCommand(() =>
            {
                try
                {
                    _dialogService.ShowPrintLabelDialog();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to open print label dialog");
                    _dialogService.ShowInfo($"Failed to open print label dialog: {ex.Message}", "Error");
                }
            });

            OpenScannerStatusWindowCommand = new RelayCommand(() =>
            {
                _dialogService.ShowScannerStatus();
            });

            OpenDashboardCommand.Execute(null);
        }

        async Task OpenImportMappingWindowAsync()
        {
            try
            {
                var path = _fileDialogService.OpenFile("CSV Files|*.csv");
                if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
                    return;

                var headers = (await CsvHelperUtil.ReadHeadersAsync(path)).ToList();
                var properties = typeof(ToolModel)
                                    .GetProperties()
                                    .Select(p => p.Name)
                                    .ToList();
                var map = _dialogService.ShowImportMapping(headers, properties);
                if (map != null)
                {
                    await _dialogService.ShowInfoAsync("Importing tools...", "Import Tools");
                    var invalid = await _toolService.ImportToolsFromCsvAsync(path, map, CancellationToken.None);
                    var msg = invalid.Count == 0
                        ? "Successfully imported tools."
                        : $"Imported with {invalid.Count} invalid rows.";
                    await _dialogService.ShowInfoAsync(msg, "Import Tools");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to import tools from CSV");
                await _dialogService.ShowInfoAsync($"Failed to import tools: {ex.Message}", "Import Tools");
            }
        }

        /// <inheritdoc />
        public void Dispose()
        {
            if (_userContextChangedHandler != null)
            {
                _userContext.UserChanged -= _userContextChangedHandler;
                _userContextChangedHandler = null;
            }

            if (_toolManagementPropertyChangedHandler != null)
            {
                ToolManagement.PropertyChanged -= _toolManagementPropertyChangedHandler;
                _toolManagementPropertyChangedHandler = null;
            }
            ToolManagement.Dispose();
        }
    }
}
