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
using ToolManagementAppV2.Services.Core;
using ToolManagementAppV2.Services.Rentals;
using ToolManagementAppV2.Services.Settings;
using ToolManagementAppV2.Services.Tools;
using ToolManagementAppV2.Services.Users;
using ToolManagementAppV2.ViewModels.Rental;
using ToolManagementAppV2.Views;

namespace ToolManagementAppV2.ViewModels
{
    public class MainViewModel : ObservableObject
    {
        readonly IToolService _toolService;
        readonly IUserService _userService;
        readonly ICustomerService _customerService;
        readonly IRentalService _rentalService;
        readonly ActivityLogService _activityLogService;

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

        public bool IsCurrentUserAdmin =>
            System.Windows.Application.Current.Properties["CurrentUser"] is User u && u.IsAdmin;

        public string CurrentUserName =>
            (System.Windows.Application.Current.Properties["CurrentUser"] as User)?.UserName ?? "Guest";

        public string CurrentUserRole =>
            IsCurrentUserAdmin ? "Admin" : "User";

        public void RefreshCurrentUser()
        {
            OnPropertyChanged(nameof(IsCurrentUserAdmin));
            OnPropertyChanged(nameof(CurrentUserName));
            OnPropertyChanged(nameof(CurrentUserRole));
        }

        public IRelayCommand OpenDashboardCommand { get; }
        public IRelayCommand OpenSearchToolsCommand { get; }
        public IRelayCommand OpenManageToolsCommand { get; }
        public IRelayCommand OpenRentalsCommand { get; }
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

        public IRelayCommand OpenRentalHistoryWindowCommand { get; }
        public IRelayCommand OpenPrintPreviewWindowCommand { get; }
        public IRelayCommand OpenPrintLabelWindowCommand { get; }
        public IRelayCommand OpenScannerStatusWindowCommand { get; }

        public MainViewModel(IToolService toolService,
                             IUserService userService,
                             ICustomerService customerService,
                             IRentalService rentalService,
                             IFileDialogService fileDialogService,
                             ActivityLogService activityLogService)
        {_toolService = toolService;
            _userService = userService;
            _customerService = customerService;
            _rentalService = rentalService;
            _activityLogService = activityLogService;

            ToolManagement = new ToolManagementViewModel(toolService, customerService, rentalService);
            UserManagement = new UserManagementViewModel(userService, fileDialogService);
            CustomerManagement = new CustomerManagementViewModel(customerService);
            ManageRentals = new ManageRentalsViewModel(rentalService);
            ImportExport = new ImportExportViewModel(toolService, customerService, fileDialogService);
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
                ToolManagement.LoadTools();
                var page = new ToolSearchPage { DataContext = ToolManagement, Title = "Search Tools" };
                // If your ToolManagement VM supports a query setter, apply GlobalSearchText there.
                CurrentPage = page;
            });

            OpenManageToolsCommand = new RelayCommand(() =>
            {
                ToolManagement.LoadTools();
                var page = new ManageToolsPage { DataContext = ToolManagement, Title = "Manage Tools" };
                CurrentPage = page;
            });

            OpenRentalsCommand = new RelayCommand(() =>
            {
                ManageRentals.LoadRentals();
                var page = new ManageRentalsPage { DataContext = ManageRentals, Title = "Manage Rentals" };
                CurrentPage = page;
            });

            OpenCustomersCommand = new RelayCommand(() =>
            {
                CustomerManagement.LoadCustomers();
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
                var db = new DatabaseService(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "tool_inventory.db"));
                var settingsService = new SettingsService(db);
                var page = new SettingsPage
                {
                    DataContext = new SettingsViewModel(fileDialogService, settingsService, new DialogService()),
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
                var win = new ImportMappingWindow(headers, properties);
                if (win.ShowDialog() == true)
                {
                    var map = win.VM.Mappings.ToDictionary(m => m.SelectedColumn, m => m.PropertyName);
                    var invalid = _toolService.ImportToolsFromCsv(path, map);
                    var msg = invalid.Count == 0
                        ? "Successfully imported tools."
                        : $"Imported with {invalid.Count} invalid rows.";
                    MessageBox.Show(msg, "Import Tools");
                }
            });

            OpenImageImportMappingWindowCommand = new RelayCommand(() =>
            {
                using var dlg = new Forms.FolderBrowserDialog();
                if (dlg.ShowDialog() != Forms.DialogResult.OK)
                    return;
                var win = new ImageImportMappingWindow();
                if (win.ShowDialog() == true)
                {
                    var result = _toolService.ImportToolImages(dlg.SelectedPath, win.VM.BuildSelector());
                    MessageBox.Show(
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

            ExitCommand = new RelayCommand(() =>
            {
                try { System.Windows.Application.Current.Shutdown(); }
                catch { System.Environment.Exit(0); }
            });

            OpenRentalHistoryWindowCommand = new RelayCommand(() =>
            {
                // Constructor requires (Tool tool, IEnumerable<Rental> history)
                var vm = new RentalHistoryViewModel(null, Enumerable.Empty<Models.Domain.Rental>());
                var win = new RentalHistoryWindow(vm);
                win.ShowDialog();
            });

            OpenPrintPreviewWindowCommand = new RelayCommand(() =>
            {
                var doc = new FlowDocument(new Paragraph(new Run("Preview document")));
                var win = new PrintPreviewWindow();
                win.ShowPreview(doc, "Print Preview", "");
            });

            OpenPrintLabelWindowCommand = new RelayCommand(() =>
            {
                // Avoid missing VM type by opening the window directly
                var win = new PrintLabelWindow();
                win.ShowDialog();
            });

            OpenScannerStatusWindowCommand = new RelayCommand(() =>
            {
                // Avoid missing VM type by opening the window directly
                var win = new ScannerStatusWindow();
                win.ShowDialog();
            });

            OpenDashboardCommand.Execute(null);
        }
    }
}
