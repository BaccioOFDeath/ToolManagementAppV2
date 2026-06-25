using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using InventoryManagementApp.Data;
using InventoryManagementApp.Interfaces;
using InventoryManagementApp.Models;
using InventoryManagementApp.Models.Domain;
using InventoryManagementApp.Services.Users;
using InventoryManagementApp.Services.Maintenance;
using InventoryManagementApp.Services.Calibration;
using InventoryManagementApp.Services.Reservations;
using InventoryManagementApp.Services.Kits;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using InventoryManagementApp.Utilities.Helpers;

namespace InventoryManagementApp.ViewModels
{
    public class DashboardViewModel : ObservableObject
    {
        enum DashboardRecordKind
        {
            None,
            CommonItem,
            CheckedOutItem,
            IncompleteItem,
            Rental,
            Activity
        }

        readonly IItemService _itemService;
        readonly IRentalService _rentalService;
        readonly ICustomerService _customerService;
        readonly IUserService _userService;
        readonly ActivityLogService _activityLogService;
        readonly MaintenanceService? _maintenanceService;
        readonly CalibrationService? _calibrationService;
        readonly ReservationService? _reservationService;
        readonly KitService? _kitService;
        readonly IRelayCommand _openManageItemsCommand;
        readonly IRelayCommand _openRentalsCommand;
        readonly IRelayCommand _openImportExportCommand;
        readonly ILogger<DashboardViewModel> _logger;
        readonly IDialogService? _dialogService;
        ItemModel? _selectedCommonlyUsedItem;
        ItemModel? _selectedCheckedOutItem;
        ItemModel? _selectedIncompleteItem;
        RentalModel? _selectedRental;
        ActivityLog? _selectedActivity;
        DashboardRecordKind _selectedRecordKind = DashboardRecordKind.None;

        public ObservableCollection<StatCard> StatCards { get; } = new();
        public ObservableCollection<ActivityLog> RecentActivity { get; } = new();
        public ObservableCollection<ItemModel> CheckedOutItems { get; } = new();
        public ObservableCollection<RentalModel> RentedItems { get; } = new();
        public ObservableCollection<ItemModel> CommonlyUsedItems { get; } = new();
        public ObservableCollection<ItemModel> IncompleteItems { get; } = new();

        public ItemModel? SelectedCommonlyUsedItem
        {
            get => _selectedCommonlyUsedItem;
            set
            {
                if (SetProperty(ref _selectedCommonlyUsedItem, value))
                {
                    if (value != null)
                        _selectedRecordKind = DashboardRecordKind.CommonItem;
                    else if (_selectedRecordKind == DashboardRecordKind.CommonItem)
                        _selectedRecordKind = DashboardRecordKind.None;
                    NotifySelectionStateChanged();
                    UpdateSelectedRecordSummary();
                }
            }
        }

        public ItemModel? SelectedCheckedOutItem
        {
            get => _selectedCheckedOutItem;
            set
            {
                if (SetProperty(ref _selectedCheckedOutItem, value))
                {
                    if (value != null)
                        _selectedRecordKind = DashboardRecordKind.CheckedOutItem;
                    else if (_selectedRecordKind == DashboardRecordKind.CheckedOutItem)
                        _selectedRecordKind = DashboardRecordKind.None;
                    NotifySelectionStateChanged();
                    UpdateSelectedRecordSummary();
                }
            }
        }

        public ItemModel? SelectedIncompleteItem
        {
            get => _selectedIncompleteItem;
            set
            {
                if (SetProperty(ref _selectedIncompleteItem, value))
                {
                    if (value != null)
                        _selectedRecordKind = DashboardRecordKind.IncompleteItem;
                    else if (_selectedRecordKind == DashboardRecordKind.IncompleteItem)
                        _selectedRecordKind = DashboardRecordKind.None;
                    NotifySelectionStateChanged();
                    UpdateSelectedRecordSummary();
                }
            }
        }

        public RentalModel? SelectedRental
        {
            get => _selectedRental;
            set
            {
                if (SetProperty(ref _selectedRental, value))
                {
                    if (value != null)
                        _selectedRecordKind = DashboardRecordKind.Rental;
                    else if (_selectedRecordKind == DashboardRecordKind.Rental)
                        _selectedRecordKind = DashboardRecordKind.None;
                    NotifySelectionStateChanged();
                    UpdateSelectedRecordSummary();
                }
            }
        }

        public ActivityLog? SelectedActivity
        {
            get => _selectedActivity;
            set
            {
                if (SetProperty(ref _selectedActivity, value))
                {
                    if (value != null)
                        _selectedRecordKind = DashboardRecordKind.Activity;
                    else if (_selectedRecordKind == DashboardRecordKind.Activity)
                        _selectedRecordKind = DashboardRecordKind.None;
                    NotifySelectionStateChanged();
                    UpdateSelectedRecordSummary();
                }
            }
        }

        public bool HasSelectedCommonItem => SelectedCommonlyUsedItem != null;
        public bool HasSelectedCheckedOutItem => SelectedCheckedOutItem != null;
        public bool HasSelectedIncompleteItem => SelectedIncompleteItem != null;
        public bool HasSelectedRental => SelectedRental != null;
        public bool HasSelectedActivity => SelectedActivity != null;

        public string OperationsSummary =>
            $"{CheckedOutItems.Count} checked out | {RentedItems.Count} active rentals | {IncompleteItems.Count} with issues | {RecentActivity.Count} recent activity rows";

        public string SelectedRecordSummary { get; private set; } = "Select or double-click a row to open the related workflow.";

        public IRelayCommand NewItemCommand { get; }
        public IRelayCommand OpenItemsCommand { get; }
        public IRelayCommand OpenRentalsCommand { get; }
        public IRelayCommand OpenImportExportCommand { get; }
        public IRelayCommand OpenSelectedCommonItemCommand { get; }
        public IRelayCommand OpenSelectedCheckedOutItemCommand { get; }
        public IRelayCommand OpenSelectedIncompleteItemCommand { get; }
        public IRelayCommand OpenSelectedRentalCommand { get; }
        public IRelayCommand OpenActivityDestinationCommand { get; }
        public IAsyncRelayCommand PrintCheckedOutItemsCommand { get; }
        public IAsyncRelayCommand PrintDashboardSnapshotCommand { get; }
        public IAsyncRelayCommand<ItemModel> CheckInItemCommand { get; }
        public IAsyncRelayCommand<RentalModel> ReturnRentalCommand { get; }
        public IAsyncRelayCommand ToggleSelectedCommonItemCommand { get; }
        public IAsyncRelayCommand CheckInSelectedItemCommand { get; }
        public IAsyncRelayCommand ReturnSelectedRentalCommand { get; }

        public DashboardViewModel(IItemService itemService,
                                  IRentalService rentalService,
                                  ICustomerService customerService,
                                  IUserService userService,
                                  ActivityLogService activityLogService,
                                  IRelayCommand openManageItemsCommand,
                                  IRelayCommand openRentalsCommand,
                                  IRelayCommand openImportExportCommand,
                                  MaintenanceService? maintenanceService = null,
                                  CalibrationService? calibrationService = null,
                                  ReservationService? reservationService = null,
                                  KitService? kitService = null,
                                  ILogger<DashboardViewModel>? logger = null,
                                  IDialogService? dialogService = null)
        {
            _itemService = itemService ?? throw new ArgumentNullException(nameof(itemService));
            _rentalService = rentalService ?? throw new ArgumentNullException(nameof(rentalService));
            _customerService = customerService ?? throw new ArgumentNullException(nameof(customerService));
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
            _activityLogService = activityLogService ?? throw new ArgumentNullException(nameof(activityLogService));
            _maintenanceService = maintenanceService;
            _calibrationService = calibrationService;
            _reservationService = reservationService;
            _kitService = kitService;
            _openManageItemsCommand = openManageItemsCommand ?? throw new ArgumentNullException(nameof(openManageItemsCommand));
            _openRentalsCommand = openRentalsCommand ?? throw new ArgumentNullException(nameof(openRentalsCommand));
            _openImportExportCommand = openImportExportCommand ?? throw new ArgumentNullException(nameof(openImportExportCommand));
            _logger = logger ?? NullLogger<DashboardViewModel>.Instance;
            _dialogService = dialogService;

            NewItemCommand = new RelayCommand(OpenItemsWorkflow);
            OpenItemsCommand = new RelayCommand(OpenItemsWorkflow);
            OpenRentalsCommand = new RelayCommand(OpenRentalsWorkflow);
            OpenImportExportCommand = new RelayCommand(OpenImportExportWorkflow);
            OpenSelectedCommonItemCommand = new RelayCommand(() => OpenItemDetails(SelectedCommonlyUsedItem), () => HasSelectedCommonItem);
            OpenSelectedCheckedOutItemCommand = new RelayCommand(() => OpenItemDetails(SelectedCheckedOutItem), () => HasSelectedCheckedOutItem);
            OpenSelectedIncompleteItemCommand = new RelayCommand(() => OpenItemDetails(SelectedIncompleteItem), () => HasSelectedIncompleteItem);
            OpenSelectedRentalCommand = new RelayCommand(OpenRentalsWorkflow, () => HasSelectedRental);
            OpenActivityDestinationCommand = new RelayCommand(OpenActivityDestination, () => HasSelectedActivity);
            PrintCheckedOutItemsCommand = new AsyncRelayCommand(PrintCheckedOutItemsAsync);
            PrintDashboardSnapshotCommand = new AsyncRelayCommand(PrintDashboardSnapshotAsync);
            CheckInItemCommand = new AsyncRelayCommand<ItemModel>(CheckInItemAsync, item => item != null);
            ReturnRentalCommand = new AsyncRelayCommand<RentalModel>(ReturnRentalAsync, rental => rental != null);
            ToggleSelectedCommonItemCommand = new AsyncRelayCommand(ToggleSelectedCommonItemAsync, () => HasSelectedCommonItem);
            CheckInSelectedItemCommand = new AsyncRelayCommand(CheckInSelectedItemAsync, () => HasSelectedCheckedOutItem);
            ReturnSelectedRentalCommand = new AsyncRelayCommand(ReturnSelectedRentalAsync, () => HasSelectedRental);
        }

        public Task LoadAsync(CancellationToken cancellationToken)
            => Task.WhenAll(
                LoadStatsAsync(cancellationToken),
                LoadRecentActivityAsync(cancellationToken),
                LoadCheckedOutItemsAsync(cancellationToken),
                LoadRentedItemsAsync(cancellationToken),
                LoadCommonlyUsedItemsAsync(cancellationToken),
                LoadIncompleteItemsAsync(cancellationToken));

        internal async Task LoadStatsAsync(CancellationToken cancellationToken)
        {
            try
            {
                StatCards.Clear();
                
                var itemCount = await _itemService.CountItemsAsync(new ItemFilter(null), cancellationToken).ConfigureAwait(false);
                var rentalCount = await _rentalService.CountActiveRentalsAsync().ConfigureAwait(false);
                var customerCount = await _customerService.CountCustomersAsync(cancellationToken).ConfigureAwait(false);
                var userCount = await _userService.CountUsersAsync(cancellationToken).ConfigureAwait(false);

                StatCards.Add(new StatCard
                {
                    Title = $"Total {LabelProvider.Instance.ItemLabelPlural}",
                    Value = itemCount.ToString()
                });
                StatCards.Add(new StatCard { Title = "Active Rentals", Value = rentalCount.ToString() });
                StatCards.Add(new StatCard { Title = "Total Customers", Value = customerCount.ToString() });
                StatCards.Add(new StatCard { Title = "Total Users", Value = userCount.ToString() });

                if (_maintenanceService != null)
                {
                    var overdueMaintenance = await _maintenanceService.GetOverdueMaintenanceAsync().ConfigureAwait(false);
                    StatCards.Add(new StatCard { Title = "Overdue Maintenance", Value = overdueMaintenance.Count.ToString() });
                }

                if (_calibrationService != null)
                {
                    var overdueCalibration = await _calibrationService.GetOverdueCalibrationAsync().ConfigureAwait(false);
                    StatCards.Add(new StatCard { Title = "Overdue Calibrations", Value = overdueCalibration.Count.ToString() });
                }

                if (_reservationService != null)
                {
                    var activeReservations = await _reservationService.GetActiveReservationsAsync().ConfigureAwait(false);
                    StatCards.Add(new StatCard { Title = "Active Reservations", Value = activeReservations.Count.ToString() });
                }

                if (_kitService != null)
                {
                    var activeKits = await _kitService.GetActiveKitsAsync().ConfigureAwait(false);
                    StatCards.Add(new StatCard { Title = "Active Kits", Value = activeKits.Count.ToString() });
                }
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load dashboard statistics");
                ClearDashboardStatsAfterLoadFailure();
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
                    ClearRecentActivityAfterLoadFailure();
                    return;
                }
                foreach (var log in result.Value)
                    RecentActivity.Add(log);
                ClearActivitySelectionIfMissing();
                OnPropertyChanged(nameof(OperationsSummary));
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load recent activity");
                ClearRecentActivityAfterLoadFailure();
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
                ClearCheckedOutSelectionIfMissing();
                OnPropertyChanged(nameof(OperationsSummary));
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load checked-out items");
                ClearCheckedOutItemsAfterLoadFailure();
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
                ClearRentalSelectionIfMissing();
                OnPropertyChanged(nameof(OperationsSummary));
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load rented items");
                ClearRentedItemsAfterLoadFailure();
            }
        }

        private async Task CheckInItemAsync(ItemModel? item, CancellationToken token)
        {
            if (item == null) return;
            try
            {
                var wasCheckedOut = item.IsCheckedOut || CheckedOutItems.Any(existing => existing.ItemID == item.ItemID);
                var result = await _itemService.ToggleItemCheckOutStatusAsync(item.ItemID, token).ConfigureAwait(false);
                if (result)
                {
                    var refreshed = await TryGetItemByIdAsync(item.ItemID, token).ConfigureAwait(false);
                    if (refreshed != null)
                    {
                        ApplyItemState(item, refreshed);
                    }
                    else
                    {
                        item.IsCheckedOut = !wasCheckedOut;
                    }

                    if (item.IsCheckedOut)
                    {
                        UpsertCheckedOutItem(item);
                        UpdateSelectedRecordSummary();
                    }
                    else
                    {
                        RemoveCheckedOutItem(item.ItemID);
                    }

                    if (SelectedCheckedOutItem?.ItemID == item.ItemID && !item.IsCheckedOut)
                        SelectedCheckedOutItem = null;
                    else
                        UpdateSelectedRecordSummary();

                    OnPropertyChanged(nameof(OperationsSummary));
                }
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to check in item {ItemID}", item.ItemID);
            }
        }

        private async Task<ItemModel?> TryGetItemByIdAsync(int itemId, CancellationToken token)
        {
            try
            {
                return await _itemService.GetItemByIDAsync(itemId, token).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex, "Failed to refresh dashboard item {ItemID} after check-out toggle", itemId);
                return null;
            }
        }

        private void UpsertCheckedOutItem(ItemModel item)
        {
            var existing = CheckedOutItems.FirstOrDefault(existing => existing.ItemID == item.ItemID);
            if (existing == null)
            {
                CheckedOutItems.Insert(0, item);
                return;
            }

            ApplyItemState(existing, item);
        }

        private void RemoveCheckedOutItem(int itemId)
        {
            for (var i = CheckedOutItems.Count - 1; i >= 0; i--)
            {
                if (CheckedOutItems[i].ItemID == itemId)
                    CheckedOutItems.RemoveAt(i);
            }
        }

        private static void ApplyItemState(ItemModel target, ItemModel source)
        {
            target.ItemID = source.ItemID;
            target.ItemNumber = source.ItemNumber;
            target.PartNumber = source.PartNumber;
            target.Name = source.Name;
            target.Brand = source.Brand;
            target.Location = source.Location;
            target.Price = source.Price;
            target.QuantityOnHand = source.QuantityOnHand;
            target.RentedQuantity = source.RentedQuantity;
            target.Supplier = source.Supplier;
            target.PurchasedDate = source.PurchasedDate;
            target.Notes = source.Notes;
            target.Keywords = source.Keywords;
            target.IsPowered = source.IsPowered;
            target.IsRentalItem = source.IsRentalItem;
            target.IsCheckedOut = source.IsCheckedOut;
            target.CheckedOutBy = source.CheckedOutBy;
            target.CheckedOutTime = source.CheckedOutTime;
            target.CheckedInBy = source.CheckedInBy;
            target.CheckedInTime = source.CheckedInTime;
            target.ImagePath = source.ImagePath;
            target.UpdatedAt = source.UpdatedAt;
            target.IsIncomplete = source.IsIncomplete;
            target.MissingComponentsNotes = source.MissingComponentsNotes;
            target.IssuesNotes = source.IssuesNotes;
            target.CheckoutCount = source.CheckoutCount;
        }

        private async Task ReturnRentalAsync(RentalModel? rental, CancellationToken token)
        {
            if (rental == null) return;
            try
            {
                if (!await ConfirmRentalReturnAsync(rental).ConfigureAwait(false))
                    return;

                await _rentalService.ReturnItemAsync(rental.RentalID, DateTime.Today).ConfigureAwait(false);
                RunOnUiThread(() => RemoveReturnedRentalFromDashboard(rental.RentalID));
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to return rental {RentalID}", rental.RentalID);
            }
        }

        private void RemoveReturnedRentalFromDashboard(int rentalId)
        {
            for (var i = RentedItems.Count - 1; i >= 0; i--)
            {
                if (RentedItems[i].RentalID == rentalId)
                    RentedItems.RemoveAt(i);
            }

            if (SelectedRental?.RentalID == rentalId)
                SelectedRental = null;
            else
                UpdateSelectedRecordSummary();

            OnPropertyChanged(nameof(OperationsSummary));
        }

        private static void RunOnUiThread(Action action)
        {
            var dispatcher = System.Windows.Application.Current?.Dispatcher;
            if (dispatcher == null || dispatcher.CheckAccess())
            {
                action();
                return;
            }

            dispatcher.Invoke(action);
        }

        private Task<bool> ConfirmRentalReturnAsync(RentalModel rental)
        {
            if (_dialogService == null)
                return Task.FromResult(true);

            return _dialogService.ShowConfirmAsync("Confirm Rental Return", BuildReturnConfirmationMessage(rental));
        }

        private static string BuildReturnConfirmationMessage(RentalModel rental)
        {
            return string.Join(Environment.NewLine,
                $"Return rental #{rental.RentalID}?",
                string.Empty,
                $"Item: {ValueOrNotRecorded(rental.ItemNumber)}",
                $"Customer: {ValueOrNotRecorded(rental.CustomerName)}",
                $"Due back: {rental.DueDate:yyyy-MM-dd HH:mm}",
                $"Return date: {DateTime.Today:yyyy-MM-dd}",
                string.Empty,
                "Confirm only after the item and any documents have been received.");
        }

        internal async Task LoadCommonlyUsedItemsAsync(CancellationToken token)
        {
            try
            {
                CommonlyUsedItems.Clear();
                var items = await _itemService.GetMostCommonlyUsedItemsAsync(10, token).ConfigureAwait(false);
                foreach (var item in items)
                    CommonlyUsedItems.Add(item);
                ClearCommonSelectionIfMissing();
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load commonly used items");
                ClearCommonlyUsedItemsAfterLoadFailure();
            }
        }

        internal async Task LoadIncompleteItemsAsync(CancellationToken token)
        {
            try
            {
                IncompleteItems.Clear();
                var items = await _itemService.GetIncompleteItemsAsync(token).ConfigureAwait(false);
                foreach (var item in items)
                    IncompleteItems.Add(item);
                ClearIncompleteSelectionIfMissing();
                OnPropertyChanged(nameof(OperationsSummary));
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load incomplete items");
                ClearIncompleteItemsAfterLoadFailure();
            }
        }

        private async Task ToggleSelectedCommonItemAsync()
        {
            if (SelectedCommonlyUsedItem != null)
                await CheckInItemAsync(SelectedCommonlyUsedItem, CancellationToken.None).ConfigureAwait(false);
        }

        private async Task CheckInSelectedItemAsync()
        {
            if (SelectedCheckedOutItem != null)
                await CheckInItemAsync(SelectedCheckedOutItem, CancellationToken.None).ConfigureAwait(false);
        }

        private async Task ReturnSelectedRentalAsync()
        {
            if (SelectedRental != null)
                await ReturnRentalAsync(SelectedRental, CancellationToken.None).ConfigureAwait(false);
        }

        private void OpenItemsWorkflow()
        {
            try { _openManageItemsCommand.Execute(null); }
            catch (Exception ex) { _logger.LogError(ex, "Failed to open manage {ItemLabelPlural} page", LabelProvider.Instance.ItemLabelPlural.ToLower()); }
        }

        private void OpenItemDetails(ItemModel? item)
        {
            if (item == null)
                return;

            var dialogService = _dialogService
                ?? (System.Windows.Application.Current as InventoryManagementApp.App)?.Host.Services.GetService<IDialogService>();

            if (dialogService == null)
            {
                _logger.LogWarning("Item details service is not available for dashboard item {ItemID}", item.ItemID);
                return;
            }

            try { dialogService.ShowItemDetails(item); }
            catch (Exception ex) { _logger.LogError(ex, "Failed to open dashboard item details for {ItemID}", item.ItemID); }
        }

        private void OpenRentalsWorkflow()
        {
            try { _openRentalsCommand.Execute(null); }
            catch (Exception ex) { _logger.LogError(ex, "Failed to open rentals page"); }
        }

        private void OpenImportExportWorkflow()
        {
            try { _openImportExportCommand.Execute(null); }
            catch (Exception ex) { _logger.LogError(ex, "Failed to open import/export page"); }
        }

        private void OpenActivityDestination()
        {
            if (SelectedActivity == null)
                return;

            var destination = ActivityLogsViewModel.BuildDestinationKey(SelectedActivity.Action);
            switch (destination)
            {
                case "Rentals":
                case "Reservations":
                    OpenRentalsWorkflow();
                    break;
                case "ImportExport":
                    OpenImportExportWorkflow();
                    break;
                default:
                    OpenItemsWorkflow();
                    break;
            }
        }

        private void UpdateSelectedRecordSummary()
        {
            SelectedRecordSummary = _selectedRecordKind switch
            {
                DashboardRecordKind.CommonItem when SelectedCommonlyUsedItem != null => DescribeItem("Common item", SelectedCommonlyUsedItem),
                DashboardRecordKind.CheckedOutItem when SelectedCheckedOutItem != null => DescribeItem("Checked out", SelectedCheckedOutItem),
                DashboardRecordKind.IncompleteItem when SelectedIncompleteItem != null => DescribeItem("Issue", SelectedIncompleteItem),
                DashboardRecordKind.Rental when SelectedRental != null => $"Rental: {SelectedRental.ItemNumber} for {SelectedRental.CustomerName} | due {SelectedRental.DueDate:yyyy-MM-dd}",
                DashboardRecordKind.Activity when SelectedActivity != null => DescribeActivity(SelectedActivity),
                _ => BuildFallbackSelectedRecordSummary()
            };
            OnPropertyChanged(nameof(SelectedRecordSummary));
        }

        private string BuildFallbackSelectedRecordSummary()
        {
            if (SelectedCommonlyUsedItem != null)
                return DescribeItem("Common item", SelectedCommonlyUsedItem);
            if (SelectedCheckedOutItem != null)
                return DescribeItem("Checked out", SelectedCheckedOutItem);
            if (SelectedIncompleteItem != null)
                return DescribeItem("Issue", SelectedIncompleteItem);
            if (SelectedRental != null)
                return $"Rental: {SelectedRental.ItemNumber} for {SelectedRental.CustomerName} | due {SelectedRental.DueDate:yyyy-MM-dd}";
            if (SelectedActivity != null)
                return DescribeActivity(SelectedActivity);

            _selectedRecordKind = DashboardRecordKind.None;
            return "Select or double-click a row to open the related workflow.";
        }

        private void NotifySelectionStateChanged()
        {
            OnPropertyChanged(nameof(HasSelectedCommonItem));
            OnPropertyChanged(nameof(HasSelectedCheckedOutItem));
            OnPropertyChanged(nameof(HasSelectedIncompleteItem));
            OnPropertyChanged(nameof(HasSelectedRental));
            OnPropertyChanged(nameof(HasSelectedActivity));

            OpenSelectedCommonItemCommand.NotifyCanExecuteChanged();
            OpenSelectedCheckedOutItemCommand.NotifyCanExecuteChanged();
            OpenSelectedIncompleteItemCommand.NotifyCanExecuteChanged();
            OpenSelectedRentalCommand.NotifyCanExecuteChanged();
            OpenActivityDestinationCommand.NotifyCanExecuteChanged();
            ToggleSelectedCommonItemCommand.NotifyCanExecuteChanged();
            CheckInSelectedItemCommand.NotifyCanExecuteChanged();
            ReturnSelectedRentalCommand.NotifyCanExecuteChanged();
        }

        private void ClearCommonSelectionIfMissing()
        {
            if (SelectedCommonlyUsedItem != null && CommonlyUsedItems.All(item => item.ItemID != SelectedCommonlyUsedItem.ItemID))
                SelectedCommonlyUsedItem = null;
        }

        private void ClearCheckedOutSelectionIfMissing()
        {
            if (SelectedCheckedOutItem != null && CheckedOutItems.All(item => item.ItemID != SelectedCheckedOutItem.ItemID))
                SelectedCheckedOutItem = null;
        }

        private void ClearIncompleteSelectionIfMissing()
        {
            if (SelectedIncompleteItem != null && IncompleteItems.All(item => item.ItemID != SelectedIncompleteItem.ItemID))
                SelectedIncompleteItem = null;
        }

        private void ClearRentalSelectionIfMissing()
        {
            if (SelectedRental != null && RentedItems.All(rental => rental.RentalID != SelectedRental.RentalID))
                SelectedRental = null;
        }

        private void ClearActivitySelectionIfMissing()
        {
            if (SelectedActivity != null && RecentActivity.All(log => log.LogID != SelectedActivity.LogID))
                SelectedActivity = null;
        }

        private void ClearDashboardStatsAfterLoadFailure()
        {
            StatCards.Clear();
        }

        private void ClearRecentActivityAfterLoadFailure()
        {
            RecentActivity.Clear();
            SelectedActivity = null;
            OnPropertyChanged(nameof(OperationsSummary));
        }

        private void ClearCheckedOutItemsAfterLoadFailure()
        {
            CheckedOutItems.Clear();
            SelectedCheckedOutItem = null;
            OnPropertyChanged(nameof(OperationsSummary));
        }

        private void ClearRentedItemsAfterLoadFailure()
        {
            RentedItems.Clear();
            SelectedRental = null;
            OnPropertyChanged(nameof(OperationsSummary));
        }

        private void ClearCommonlyUsedItemsAfterLoadFailure()
        {
            CommonlyUsedItems.Clear();
            SelectedCommonlyUsedItem = null;
        }

        private void ClearIncompleteItemsAfterLoadFailure()
        {
            IncompleteItems.Clear();
            SelectedIncompleteItem = null;
            OnPropertyChanged(nameof(OperationsSummary));
        }

        private static string DescribeItem(string prefix, ItemModel item)
        {
            var status = item.IsCheckedOut ? "checked out" : "available";
            var issue = item.IsIncomplete ? " | issue noted" : string.Empty;
            return $"{prefix}: {item.ItemNumber} - {item.Name} | {item.Location} | {status}{issue}";
        }

        private static string DescribeActivity(ActivityLog activity)
        {
            var destination = ActivityLogsViewModel.BuildDestinationName(ActivityLogsViewModel.BuildDestinationKey(activity.Action));
            return $"Activity: {activity.Timestamp:yyyy-MM-dd HH:mm} | {activity.UserName} | open {destination} | {activity.Action}";
        }

        private static string ValueOrNotRecorded(string? value) => string.IsNullOrWhiteSpace(value) ? "Not recorded" : value;

        private async Task PrintCheckedOutItemsAsync()
        {
            try
            {
                var currentUser = await _userService.GetCurrentUserAsync();
                var userName = currentUser?.UserName ?? "Unknown";
                var doc = GenerateCheckedOutItemsDocument(userName);
                ShowDashboardPrintPreview(doc, $"Checked Out Items - {userName}", "Dashboard checked-out item handoff");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to print checked-out items");
            }
        }

        private async Task PrintDashboardSnapshotAsync()
        {
            try
            {
                var currentUser = await _userService.GetCurrentUserAsync();
                var userName = currentUser?.UserName ?? "Unknown";
                var doc = GenerateDashboardSnapshotDocument(userName);
                ShowDashboardPrintPreview(doc, $"Dashboard Snapshot - {DateTime.Now:yyyy-MM-dd HH:mm}", "Dashboard operations snapshot");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to print dashboard snapshot");
            }
        }

        private void ShowDashboardPrintPreview(System.Windows.Documents.FlowDocument document, string title, string description)
        {
            var dialogService = _dialogService
                ?? (System.Windows.Application.Current as InventoryManagementApp.App)?.Host.Services.GetService<IDialogService>();

            if (dialogService == null)
            {
                _logger.LogWarning("Dashboard print preview service is not available for {Title}", title);
                return;
            }

            dialogService.ShowPrintPreview(document, title, description);
        }

        private System.Windows.Documents.FlowDocument GenerateCheckedOutItemsDocument(string userName)
        {
            var doc = CreatePrintDocument("Checked Out Items", userName);
            AddItemTable(doc, CheckedOutItems, "Checked-out inventory", includeHolder: true);
            AddTotal(doc, $"Total Items: {CheckedOutItems.Count}");
            return doc;
        }

        private System.Windows.Documents.FlowDocument GenerateDashboardSnapshotDocument(string userName)
        {
            var doc = CreatePrintDocument("Dashboard Operations Snapshot", userName);

            AddSummaryParagraph(doc, OperationsSummary);
            AddItemTable(doc, CommonlyUsedItems.Take(10), "Commonly used items", includeUsage: true);
            AddItemTable(doc, CheckedOutItems.Take(25), "Checked-out items", includeHolder: true);
            AddRentalTable(doc, RentedItems.Take(25), "Active rentals");
            AddItemTable(doc, IncompleteItems.Take(15), "Items with issues", includeNotes: true);

            return doc;
        }

        private System.Windows.Documents.FlowDocument CreatePrintDocument(string title, string userName)
        {
            var doc = new System.Windows.Documents.FlowDocument
            {
                PagePadding = new System.Windows.Thickness(40),
                FontFamily = new System.Windows.Media.FontFamily("Calibri"),
                FontSize = 12
            };

            doc.Blocks.Add(new System.Windows.Documents.Paragraph(new System.Windows.Documents.Bold(new System.Windows.Documents.Run(title)))
            {
                FontSize = 20,
                TextAlignment = System.Windows.TextAlignment.Center,
                Margin = new System.Windows.Thickness(0, 0, 0, 10)
            });

            doc.Blocks.Add(new System.Windows.Documents.Paragraph(new System.Windows.Documents.Run($"User: {userName}"))
            {
                FontSize = 14,
                Margin = new System.Windows.Thickness(0, 0, 0, 5)
            });

            doc.Blocks.Add(new System.Windows.Documents.Paragraph(new System.Windows.Documents.Run($"Date: {DateTime.Now:yyyy-MM-dd HH:mm}"))
            {
                FontSize = 14,
                Margin = new System.Windows.Thickness(0, 0, 0, 20)
            });

            return doc;
        }

        private static void AddSummaryParagraph(System.Windows.Documents.FlowDocument doc, string summary)
        {
            doc.Blocks.Add(new System.Windows.Documents.Paragraph(new System.Windows.Documents.Run(summary))
            {
                FontSize = 12,
                FontWeight = System.Windows.FontWeights.Bold,
                Margin = new System.Windows.Thickness(0, 0, 0, 12)
            });
        }

        private void AddItemTable(System.Windows.Documents.FlowDocument doc, IEnumerable<ItemModel> items, string title, bool includeHolder = false, bool includeUsage = false, bool includeNotes = false)
        {
            var itemList = items.ToList();
            AddSectionTitle(doc, title);

            var table = CreateTable(4);
            var headerRow = CreateHeaderRow();
            AddTableCell(headerRow, "Item #", true);
            AddTableCell(headerRow, "Name", true);
            AddTableCell(headerRow, includeHolder ? "Holder" : includeUsage ? "Use" : "Location", true);
            AddTableCell(headerRow, includeNotes ? "Notes" : "Status", true);
            table.RowGroups[0].Rows.Add(headerRow);

            foreach (var item in itemList)
            {
                var row = new System.Windows.Documents.TableRow();
                AddTableCell(row, item.ItemNumber);
                AddTableCell(row, item.Name);
                AddTableCell(row, includeHolder ? item.CheckedOutBy : includeUsage ? item.CheckoutCount.ToString() : item.Location);
                AddTableCell(row, includeNotes ? item.MissingComponentsNotes : (item.IsCheckedOut ? "Checked out" : "Available"));
                table.RowGroups[1].Rows.Add(row);
            }

            doc.Blocks.Add(table);
            AddTotal(doc, $"Rows: {itemList.Count}");
        }

        private void AddRentalTable(System.Windows.Documents.FlowDocument doc, IEnumerable<RentalModel> rentals, string title)
        {
            var rentalList = rentals.ToList();
            AddSectionTitle(doc, title);

            var table = CreateTable(4);
            var headerRow = CreateHeaderRow();
            AddTableCell(headerRow, "Item #", true);
            AddTableCell(headerRow, "Customer", true);
            AddTableCell(headerRow, "Start", true);
            AddTableCell(headerRow, "Due", true);
            table.RowGroups[0].Rows.Add(headerRow);

            foreach (var rental in rentalList)
            {
                var row = new System.Windows.Documents.TableRow();
                AddTableCell(row, rental.ItemNumber);
                AddTableCell(row, rental.CustomerName);
                AddTableCell(row, rental.RentalDate.ToString("yyyy-MM-dd"));
                AddTableCell(row, rental.DueDate.ToString("yyyy-MM-dd"));
                table.RowGroups[1].Rows.Add(row);
            }

            doc.Blocks.Add(table);
            AddTotal(doc, $"Rows: {rentalList.Count}");
        }

        private static void AddSectionTitle(System.Windows.Documents.FlowDocument doc, string title)
        {
            doc.Blocks.Add(new System.Windows.Documents.Paragraph(new System.Windows.Documents.Bold(new System.Windows.Documents.Run(title)))
            {
                FontSize = 14,
                Margin = new System.Windows.Thickness(0, 12, 0, 6)
            });
        }

        private static System.Windows.Documents.Table CreateTable(int columnCount)
        {
            var table = new System.Windows.Documents.Table
            {
                CellSpacing = 0,
                BorderBrush = System.Windows.Media.Brushes.Black,
                BorderThickness = new System.Windows.Thickness(1),
                Margin = new System.Windows.Thickness(0, 0, 0, 6)
            };

            for (var i = 0; i < columnCount; i++)
                table.Columns.Add(new System.Windows.Documents.TableColumn { Width = new System.Windows.GridLength(1, System.Windows.GridUnitType.Star) });

            table.RowGroups.Add(new System.Windows.Documents.TableRowGroup());
            table.RowGroups.Add(new System.Windows.Documents.TableRowGroup());
            return table;
        }

        private static System.Windows.Documents.TableRow CreateHeaderRow()
        {
            return new System.Windows.Documents.TableRow
            {
                Background = System.Windows.Media.Brushes.LightGray
            };
        }

        private static void AddTotal(System.Windows.Documents.FlowDocument doc, string text)
        {
            doc.Blocks.Add(new System.Windows.Documents.Paragraph(new System.Windows.Documents.Run(text))
            {
                FontSize = 12,
                FontWeight = System.Windows.FontWeights.Bold,
                Margin = new System.Windows.Thickness(0, 4, 0, 8)
            });
        }

        private void AddTableCell(System.Windows.Documents.TableRow row, string? text, bool isHeader = false)
        {
            var cell = new System.Windows.Documents.TableCell(new System.Windows.Documents.Paragraph(new System.Windows.Documents.Run(text ?? string.Empty)))
            {
                BorderBrush = System.Windows.Media.Brushes.Black,
                BorderThickness = new System.Windows.Thickness(1),
                Padding = new System.Windows.Thickness(5)
            };
            if (isHeader)
                cell.FontWeight = System.Windows.FontWeights.Bold;
            row.Cells.Add(cell);
        }
    }
}
