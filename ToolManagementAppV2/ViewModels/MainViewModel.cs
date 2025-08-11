using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Windows;
using System.Windows.Controls;
using ToolManagementAppV2.Interfaces;
using ToolManagementAppV2.Models.Domain;
using ToolManagementAppV2.Views;
using ToolManagementAppV2.Services.Users;
using ToolManagementAppV2.Services.Tools;

namespace ToolManagementAppV2.ViewModels
{
    public class MainViewModel : ObservableObject
    {
        public ToolManagementViewModel ToolManagement { get; }
        public UserManagementViewModel UserManagement { get; }
        public RentalManagementViewModel RentalManagement { get; }
        public RentalViewModel Rentals { get; }
        public ImportExportViewModel ImportExport { get; }
        public ActivityLogsViewModel ActivityLogs { get; }
        public ReportsViewModel Reports { get; }

        Page _currentPage;
        public Page CurrentPage
        {
            get => _currentPage;
            set => SetProperty(ref _currentPage, value);
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

        public bool IsCurrentUserAdmin =>
            System.Windows.Application.Current.Properties["CurrentUser"] is User u && u.IsAdmin;

        public void RefreshCurrentUser() => OnPropertyChanged(nameof(IsCurrentUserAdmin));

        public MainViewModel(IToolService toolService, IUserService userService, ICustomerService customerService, IRentalService rentalService, IFileDialogService fileDialogService, ActivityLogService activityLogService)
        {
            ToolManagement = new ToolManagementViewModel(toolService);
            UserManagement = new UserManagementViewModel(userService, fileDialogService);
            RentalManagement = new RentalManagementViewModel(customerService);
            Rentals = new RentalViewModel(rentalService);
            ImportExport = new ImportExportViewModel(toolService, fileDialogService);
            ActivityLogs = new ActivityLogsViewModel(activityLogService);
            Reports = new ReportsViewModel(new ReportService(toolService, rentalService, activityLogService, customerService, userService));

            OpenDashboardCommand = new RelayCommand(() => CurrentPage = new DashboardPage());
            OpenSearchToolsCommand = new RelayCommand(() =>
            {
                ToolManagement.LoadTools();
                CurrentPage = new ToolSearchPage { DataContext = ToolManagement };
            });
            OpenManageToolsCommand = new RelayCommand(() =>
            {
                ToolManagement.LoadTools();
                CurrentPage = new ManageToolsPage { DataContext = ToolManagement };
            });
            OpenRentalsCommand = new RelayCommand(() =>
            {
                Rentals.LoadRentals();
                CurrentPage = new RentalsPage { DataContext = Rentals };
            });
            OpenCustomersCommand = new RelayCommand(() =>
            {
                RentalManagement.LoadCustomers();
                CurrentPage = new CustomersPage { DataContext = RentalManagement };
            });
            OpenUsersCommand = new RelayCommand(() =>
            {
                UserManagement.LoadUsers();
                CurrentPage = new UsersPage { DataContext = UserManagement };
            });
            OpenSettingsCommand = new RelayCommand(() =>
                CurrentPage = new SettingsPage { DataContext = new SettingsViewModel() });
            OpenImportExportCommand = new RelayCommand(() =>
                CurrentPage = new ImportExportPage { DataContext = ImportExport });
            OpenActivityLogsCommand = new RelayCommand(() =>
            {
                ActivityLogs.LoadLogs();
                CurrentPage = new ActivityLogsPage { DataContext = ActivityLogs };
            });
            OpenReportsCommand = new RelayCommand(() =>
                CurrentPage = new ReportsPage { DataContext = Reports });
        }
    }
}
