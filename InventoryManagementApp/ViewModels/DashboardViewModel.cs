using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using InventoryManagementApp.Interfaces;
using InventoryManagementApp.Models;
using InventoryManagementApp.Models.Domain;
using InventoryManagementApp.Services.Users;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using InventoryManagementApp.Utilities.Helpers;

namespace InventoryManagementApp.ViewModels
{
    public class DashboardViewModel : ObservableObject
    {
        readonly IItemService _itemService;
        readonly IRentalService _rentalService;
        readonly ICustomerService _customerService;
        readonly IUserService _userService;
        readonly ActivityLogService _activityLogService;
        readonly IRelayCommand _openManageItemsCommand;
        readonly IRelayCommand _openRentalsCommand;
        readonly IRelayCommand _openImportExportCommand;
        readonly ILogger<DashboardViewModel> _logger;

        public ObservableCollection<StatCard> StatCards { get; } = new();
        public ObservableCollection<ActivityLog> RecentActivity { get; } = new();

        public IRelayCommand NewItemCommand { get; }
        public IRelayCommand OpenRentalsCommand { get; }
        public IRelayCommand OpenImportExportCommand { get; }

        public DashboardViewModel(IItemService itemService,
                                  IRentalService rentalService,
                                  ICustomerService customerService,
                                  IUserService userService,
                                  ActivityLogService activityLogService,
                                  IRelayCommand openManageItemsCommand,
                                  IRelayCommand openRentalsCommand,
                                  IRelayCommand openImportExportCommand,
                                  ILogger<DashboardViewModel>? logger = null)
        {
            _itemService = itemService ?? throw new ArgumentNullException(nameof(itemService));
            _rentalService = rentalService ?? throw new ArgumentNullException(nameof(rentalService));
            _customerService = customerService ?? throw new ArgumentNullException(nameof(customerService));
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
            _activityLogService = activityLogService ?? throw new ArgumentNullException(nameof(activityLogService));
            _openManageItemsCommand = openManageItemsCommand ?? throw new ArgumentNullException(nameof(openManageItemsCommand));
            _openRentalsCommand = openRentalsCommand ?? throw new ArgumentNullException(nameof(openRentalsCommand));
            _openImportExportCommand = openImportExportCommand ?? throw new ArgumentNullException(nameof(openImportExportCommand));
            _logger = logger ?? NullLogger<DashboardViewModel>.Instance;

            NewItemCommand = new RelayCommand(() =>
            {
                try { _openManageItemsCommand.Execute(null); }
                catch (Exception ex) { _logger.LogError(ex, "Failed to open manage {ItemLabelPlural} page", LabelProvider.Instance.ItemLabelPlural.ToLower()); }
            });

            OpenRentalsCommand = new RelayCommand(() =>
            {
                try { _openRentalsCommand.Execute(null); }
                catch (Exception ex) { _logger.LogError(ex, "Failed to open rentals page"); }
            });

            OpenImportExportCommand = new RelayCommand(() =>
            {
                try { _openImportExportCommand.Execute(null); }
                catch (Exception ex) { _logger.LogError(ex, "Failed to open import/export page"); }
            });

            _ = LoadStatsAsync(CancellationToken.None);
            _ = LoadRecentActivityAsync(CancellationToken.None);
        }

        internal async Task LoadStatsAsync(CancellationToken cancellationToken)
        {
            try
            {
                StatCards.Clear();
                var items = await _itemService.GetAllItemsAsync(cancellationToken);
                StatCards.Add(new StatCard { Title = $"Total {LabelProvider.Instance.ItemLabelPlural}", Value = items.Count.ToString() });
                var activeRentals = await _rentalService.GetActiveRentalsAsync();
                var customers = await _customerService.GetAllCustomersAsync(cancellationToken);
                var users = await _userService.GetAllUsersAsync();
                StatCards.Add(new StatCard { Title = "Active Rentals", Value = activeRentals.Count.ToString() });
                StatCards.Add(new StatCard { Title = "Total Customers", Value = customers.Count.ToString() });
                StatCards.Add(new StatCard { Title = "Total Users", Value = users.Count.ToString() });
            }
            catch (OperationCanceledException)
            {
                // Swallow cancellations quietly.
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load dashboard statistics");
            }
        }

        internal async Task LoadRecentActivityAsync(CancellationToken token)
        {
            try
            {
                RecentActivity.Clear();
                var result = await _activityLogService.GetRecentLogsAsync(10, token).ConfigureAwait(false);
                if (!result.Success || result.Value == null)
                {
                    _logger.LogError("Failed to load recent activity: {Error}", result.ErrorMessage);
                    return;
                }
                foreach (var log in result.Value)
                    RecentActivity.Add(log);
            }
            catch (OperationCanceledException)
            {
                // Swallow cancellations quietly.
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load recent activity");
            }
        }
    }
}
