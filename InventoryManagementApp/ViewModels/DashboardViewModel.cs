using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using InventoryManagementApp.Data;
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
        public ObservableCollection<ItemModel> CheckedOutItems { get; } = new();
        public ObservableCollection<RentalModel> RentedItems { get; } = new();

        public IRelayCommand NewItemCommand { get; }
        public IRelayCommand OpenRentalsCommand { get; }
        public IRelayCommand OpenImportExportCommand { get; }
        public IAsyncRelayCommand<ItemModel> CheckInItemCommand { get; }
        public IAsyncRelayCommand<RentalModel> ReturnRentalCommand { get; }

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

            CheckInItemCommand = new AsyncRelayCommand<ItemModel>(CheckInItemAsync);
            ReturnRentalCommand = new AsyncRelayCommand<RentalModel>(ReturnRentalAsync);
        }

        public Task LoadAsync(CancellationToken cancellationToken)
            => Task.WhenAll(
                LoadStatsAsync(cancellationToken),
                LoadRecentActivityAsync(cancellationToken),
                LoadCheckedOutItemsAsync(cancellationToken),
                LoadRentedItemsAsync(cancellationToken));

        internal async Task LoadStatsAsync(CancellationToken cancellationToken)
        {
            try
            {
                StatCards.Clear();
                var itemCountTask = _itemService.CountItemsAsync(new ItemFilter(null), cancellationToken);
                var rentalCountTask = _rentalService.CountActiveRentalsAsync();
                var customerCountTask = _customerService.CountCustomersAsync(cancellationToken);
                var userCountTask = _userService.CountUsersAsync(cancellationToken);

                await Task.WhenAll(itemCountTask, rentalCountTask, customerCountTask, userCountTask).ConfigureAwait(false);

                StatCards.Add(new StatCard
                {
                    Title = $"Total {LabelProvider.Instance.ItemLabelPlural}",
                    Value = itemCountTask.Result.ToString()
                });
                StatCards.Add(new StatCard { Title = "Active Rentals", Value = rentalCountTask.Result.ToString() });
                StatCards.Add(new StatCard { Title = "Total Customers", Value = customerCountTask.Result.ToString() });
                StatCards.Add(new StatCard { Title = "Total Users", Value = userCountTask.Result.ToString() });
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

        internal async Task LoadCheckedOutItemsAsync(CancellationToken token)
        {
            try
            {
                CheckedOutItems.Clear();
                var items = await _itemService.GetCheckedOutItemsAsync(token).ConfigureAwait(false);
                foreach (var item in items)
                    CheckedOutItems.Add(item);
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load checked-out items");
            }
        }

        internal async Task LoadRentedItemsAsync(CancellationToken token)
        {
            try
            {
                token.ThrowIfCancellationRequested();
                RentedItems.Clear();
                var rentals = await _rentalService.GetActiveRentalsAsync().ConfigureAwait(false);
                foreach (var rental in rentals)
                    RentedItems.Add(rental);
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load rented items");
            }
        }

        private async Task CheckInItemAsync(ItemModel? item, CancellationToken token)
        {
            if (item == null) return;
            try
            {
                var result = await _itemService.ToggleItemCheckOutStatusAsync(item.ItemID, token).ConfigureAwait(false);
                if (result)
                    CheckedOutItems.Remove(item);
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to check in item {ItemID}", item.ItemID);
            }
        }

        private async Task ReturnRentalAsync(RentalModel? rental, CancellationToken token)
        {
            if (rental == null) return;
            try
            {
                await _rentalService.ReturnItemAsync(rental.RentalID, DateTime.Today).ConfigureAwait(false);
                RentedItems.Remove(rental);
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to return rental {RentalID}", rental.RentalID);
            }
        }
    }
}
