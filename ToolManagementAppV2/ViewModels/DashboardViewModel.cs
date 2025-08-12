using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.ObjectModel;
using ToolManagementAppV2.Interfaces;
using ToolManagementAppV2.Models;
using ToolManagementAppV2.Models.Domain;
using ToolManagementAppV2.Services.Users;

namespace ToolManagementAppV2.ViewModels
{
    public class DashboardViewModel : ObservableObject
    {
        readonly IToolService _toolService;
        readonly IRentalService _rentalService;
        readonly ICustomerService _customerService;
        readonly IUserService _userService;
        readonly ActivityLogService _activityLogService;
        readonly IRelayCommand _openManageToolsCommand;
        readonly IRelayCommand _openRentalsCommand;
        readonly IRelayCommand _openImportExportCommand;

        public ObservableCollection<StatCard> StatCards { get; } = new();
        public ObservableCollection<ActivityLog> RecentActivity { get; } = new();

        public IRelayCommand NewToolCommand { get; }
        public IRelayCommand OpenRentalsCommand { get; }
        public IRelayCommand OpenImportExportCommand { get; }

        public DashboardViewModel(IToolService toolService,
                                  IRentalService rentalService,
                                  ICustomerService customerService,
                                  IUserService userService,
                                  ActivityLogService activityLogService,
                                  IRelayCommand openManageToolsCommand,
                                  IRelayCommand openRentalsCommand,
                                  IRelayCommand openImportExportCommand)
        {
            _toolService = toolService ?? throw new ArgumentNullException(nameof(toolService));
            _rentalService = rentalService ?? throw new ArgumentNullException(nameof(rentalService));
            _customerService = customerService ?? throw new ArgumentNullException(nameof(customerService));
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
            _activityLogService = activityLogService ?? throw new ArgumentNullException(nameof(activityLogService));
            _openManageToolsCommand = openManageToolsCommand ?? throw new ArgumentNullException(nameof(openManageToolsCommand));
            _openRentalsCommand = openRentalsCommand ?? throw new ArgumentNullException(nameof(openRentalsCommand));
            _openImportExportCommand = openImportExportCommand ?? throw new ArgumentNullException(nameof(openImportExportCommand));

            NewToolCommand = new RelayCommand(() =>
            {
                try { _openManageToolsCommand.Execute(null); }
                catch (Exception ex) { Console.WriteLine(ex); }
            });

            OpenRentalsCommand = new RelayCommand(() =>
            {
                try { _openRentalsCommand.Execute(null); }
                catch (Exception ex) { Console.WriteLine(ex); }
            });

            OpenImportExportCommand = new RelayCommand(() =>
            {
                try { _openImportExportCommand.Execute(null); }
                catch (Exception ex) { Console.WriteLine(ex); }
            });

            LoadStats();
            LoadRecentActivity();
        }

        void LoadStats()
        {
            try
            {
                StatCards.Clear();
                StatCards.Add(new StatCard { Title = "Total Tools", Value = _toolService.GetAllTools().Count.ToString() });
                StatCards.Add(new StatCard { Title = "Active Rentals", Value = _rentalService.GetActiveRentals().Count.ToString() });
                StatCards.Add(new StatCard { Title = "Total Customers", Value = _customerService.GetAllCustomers().Count.ToString() });
                StatCards.Add(new StatCard { Title = "Total Users", Value = _userService.GetAllUsers().Count.ToString() });
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        void LoadRecentActivity()
        {
            try
            {
                RecentActivity.Clear();
                foreach (var log in _activityLogService.GetRecentLogs(10))
                    RecentActivity.Add(log);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
    }
}
