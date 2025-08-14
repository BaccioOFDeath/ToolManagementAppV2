using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

namespace ToolManagementAppV2.ViewModels
{
    public class MainViewModel : ObservableObject
    {
        readonly IToolService _toolService;
        readonly IUserService _userService;
        readonly IUserContext _userContext;
        readonly ICustomerService _customerService;
        readonly IRentalService _rentalService;
        readonly ActivityLogService _activityLogService;
        readonly ISettingsService _settingsService;
        readonly IDialogService _dialogService;
        readonly ILogger<MainViewModel> _logger;
        readonly Func<bool> _showLoginWindow;

        public ToolManagementViewModel ToolManagement { get; }
        public UserManagementViewModel UserManagement { get; }
        public CustomerManagementViewModel CustomerManagement { get; }
        public ManageRentalsViewModel ManageRentals { get; }
        public ImportExportViewModel ImportExport { get; }
        public ActivityLogsViewModel ActivityLogs { get; }
        public ReportsViewModel Reports { get; }

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

        public void RefreshCurrentUser()
        {
            OnPropertyChanged(nameof(IsCurrentUserAdmin));
            OnPropertyChanged(nameof(CurrentUserName));
            OnPropertyChanged(nameof(CurrentUserRole));
        }

        public IRelayCommand OpenDashboardCommand { get; }
        public IRelayCommand OpenSearchToolsCommand { get; }
        public IRelayCommand OpenManageToolsCommand { get; }
        public IAsyncRelayCommand OpenRentalsCommand { get; }
        public IRelayCommand OpenCustomersCommand { get; }
        public IRelayCommand OpenUsersCommand { get; }
        public IRelayCommand OpenSettingsCommand { get; }
        public IRelayCommand OpenImportExportCommand { get; }
        public IRelayCommand OpenActivityLogsCommand { get; }
        public IRelayCommand OpenReportsCommand { get; }
        public IRelayCommand OpenImportMappingWindowCommand { get; }
        public IRelayCommand OpenImageImportMappingWindowCommand { get; }
        public IRelayCommand ExitCommand { get; }
        public IRelayCommand GlobalSearchCommand { get; }
        public IRelayCommand SwitchUserCommand { get; }

        public IRelayCommand OpenRentalHistoryWindowCommand { get; }
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
                             Func<bool>? showLoginWindow = null)
        {
            _toolService = toolService;
            _userService = userService;
            _userContext = userContext;
            _customerService = customerService;
            _rentalService = rentalService;
            _activityLogService = activityLogService;
            _settingsService = settingsService;
            _dialogService = dialogService;
            _logger = logger ?? NullLogger<MainViewModel>.Instance;
            _showLoginWindow = showLoginWindow ?? new Func<bool>(() =>
            {
                var login = new LoginWindow(_userContext, _userService, _settingsService, _dialogService)
                {
                    Owner = System.Windows.Application.Current.MainWindow
                };
                return login.ShowDialog() == true;
            });

            ToolManagement = new ToolManagementViewModel(toolService, customerService, rentalService, _dialogService);
            UserManagement = new UserManagementViewModel(userService, fileDialogService);
            CustomerManagement = new CustomerManagementViewModel(customerService, _dialogService);
            ManageRentals = new ManageRentalsViewModel(rentalService, _dialogService);
            ImportExport = new ImportExportViewModel(toolService, customerService, fileDialogService, databaseService);
            Reports = new ReportsViewModel(new ReportService(toolService, rentalService, activityLogService, customerService, userService));
            ActivityLogs = new ActivityLogsViewModel(activityLogService);

            OpenDashboardCommand = new RelayCommand(() =>
            {
                var vm = new DashboardViewModel(_toolService, _rentalService, _customerService, _userService, _activityLogService, OpenManageToolsCommand, OpenRentalsCommand, OpenImportExportCommand);
                var page = new DashboardPage { DataContext = vm, Title = "Dashboard" };
                CurrentPage = page;
            });

            OpenSearchToolsCommand = new RelayCommand(() =>
            {
                ToolManagement.LoadToolsAsync();
                var page = new ToolSearchPage { DataContext = ToolManagement, Title = "Search Tools" };
                // If your ToolManagement VM supports a query setter, apply GlobalSearchText there.
                CurrentPage = page;
            });

            OpenManageToolsCommand = new RelayCommand(() =>
            {
                ToolManagement.LoadToolsAsync();
                var page = new ManageToolsPage { DataContext = ToolManagement, Title = "Manage Tools" };
                CurrentPage = page;
            });

            OpenRentalsCommand = new AsyncRelayCommand(async () =>
            {
                await ManageRentals.LoadRentalsAsync();
                var page = new ManageRentalsPage { DataContext = ManageRentals, Title = "Manage Rentals" };
                CurrentPage = page;
            });

            OpenCustomersCommand = new RelayCommand(() =>
            {
                CustomerManagement.LoadCustomersAsync();
                var page = new CustomersPage { DataContext = CustomerManagement, Title = "Customers" };
                CurrentPage = page;
            });

            OpenUsersCommand = new RelayCommand(() =>
            {
                UserManagement.LoadUsers();
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
                        DataContext = new SettingsViewModel(fileDialogService, _settingsService, _dialogService),
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

            OpenImportMappingWindowCommand = new RelayCommand(() =>
            {
                var path = fileDialogService.OpenFile("CSV Files|*.csv");
                if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
                    return;

                var headers = File.ReadLines(path)
                                   .First()
                                   .Split(',')
                                   .Select(h => h.Trim())
                                   .ToList();
                var properties = typeof(ToolModel)
                                    .GetProperties()
                                    .Select(p => p.Name)
                                    .ToList();
                var map = _dialogService.ShowImportMapping(headers, properties);
                if (map != null)
                {
                    var invalid = _toolService.ImportToolsFromCsv(path, map);
                    var msg = invalid.Count == 0
                        ? "Successfully imported tools."
                        : $"Imported with {invalid.Count} invalid rows.";
                    _dialogService.ShowInfo(msg, "Import Tools");
                }
            });

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

            GlobalSearchCommand = new RelayCommand(() =>
            {
                ToolManagement.SearchText = GlobalSearchText;
                OpenSearchToolsCommand.Execute(null);
                ToolManagement.SearchCommand?.Execute(null);
                GlobalSearchText = string.Empty;
            });

            SwitchUserCommand = new RelayCommand(() =>
            {
                if (_showLoginWindow())
                {
                    RefreshCurrentUser();
                    OpenDashboardCommand.Execute(null);
                }
            });

            ExitCommand = new RelayCommand(() =>
            {
                try { System.Windows.Application.Current.Shutdown(); }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to shutdown application");
                    System.Environment.Exit(0);
                }
            });

            OpenRentalHistoryWindowCommand = new RelayCommand(() =>
            {
                _dialogService.ShowRentalHistory(new ToolModel(), Enumerable.Empty<RentalModel>());
            });

            OpenPrintPreviewWindowCommand = new RelayCommand(() =>
            {
                var doc = new FlowDocument(new Paragraph(new Run("Preview document")));
                _dialogService.ShowPrintPreview(doc, "Print Preview", string.Empty);
            });

            OpenPrintLabelWindowCommand = new RelayCommand(() =>
            {
                _dialogService.ShowPrintLabelDialog();
            });

            OpenScannerStatusWindowCommand = new RelayCommand(() =>
            {
                _dialogService.ShowScannerStatus();
            });

            OpenDashboardCommand.Execute(null);
        }
    }
}
