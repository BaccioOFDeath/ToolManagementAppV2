using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Windows.Controls;
using ToolManagementAppV2.Interfaces;
using ToolManagementAppV2.Views;

namespace ToolManagementAppV2.ViewModels
{
    public class MainViewModel : ObservableObject
    {
        public ToolManagementViewModel ToolManagement { get; }
        public UserManagementViewModel UserManagement { get; }
        public RentalManagementViewModel RentalManagement { get; }

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

        public MainViewModel(IToolService toolService, IUserService userService, ICustomerService customerService)
        {
            ToolManagement = new ToolManagementViewModel(toolService);
            UserManagement = new UserManagementViewModel(userService);
            RentalManagement = new RentalManagementViewModel(customerService);

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
                RentalManagement.LoadCustomers();
                CurrentPage = new RentalsPage { DataContext = RentalManagement };
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
                CurrentPage = new ImportExportPage());
            OpenActivityLogsCommand = new RelayCommand(() =>
                CurrentPage = new ActivityLogsPage());
            OpenReportsCommand = new RelayCommand(() =>
                CurrentPage = new ReportsPage());
        }
    }
}
