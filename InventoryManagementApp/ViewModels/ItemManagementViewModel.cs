using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;
using InventoryManagementApp.Data;
using InventoryManagementApp.Interfaces;
using InventoryManagementApp.Models;
using InventoryManagementApp.Utilities;
using InventoryManagementApp.Utilities.Extensions;
using InventoryManagementApp.Services;
using InventoryManagementApp.Services.Printing;
using InventoryManagementApp.Services.Settings;
using InventoryManagementApp.Utilities.Helpers;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace InventoryManagementApp.ViewModels
{
    public class ItemManagementViewModel : ObservableObject, IDisposable
    {
        private readonly IItemService _itemService;
        private readonly ICustomerService _customerService;
        private readonly IRentalService _rentalService;
        private readonly IDialogService _dialogService;
        private readonly ISettingsService _settingsService;
        private readonly ILogger<ItemManagementViewModel> _logger;

        public ObservableCollection<ItemModel> Items { get; } = new();
        public ObservableCollection<ItemModel> SearchResults { get; } = new();
        public ObservableCollection<ItemModel> CheckedOutItems { get; } = new();

        public string SearchResultsSummary => SearchResults.Count == 1 ? "1 result" : $"{SearchResults.Count} results";
        public string CheckedOutSummary => CheckedOutItems.Count == 1 ? "1 checked out" : $"{CheckedOutItems.Count} checked out";

        /// <summary>
        /// List of available item categories derived from distinct brands
        /// in the current item set; rebuilt whenever items are loaded or filtered.
        /// </summary>
        public ObservableCollection<string> Categories { get; } = new();

        private ItemModel _newItem = new();
        public ItemModel NewItem
        {
            get => _newItem;
            set => SetProperty(ref _newItem, value);
        }

        private string _searchTerm = string.Empty;
        public string SearchTerm
        {
            get => _searchTerm;
            set => SetProperty(ref _searchTerm, value);
        }

        private ItemModel? _selectedItem;
        public ItemModel? SelectedItem
        {
            get => _selectedItem;
            set
            {
                if (SetProperty(ref _selectedItem, value))
                {
                    ((AsyncRelayCommand)OpenRentalsCommand).NotifyCanExecuteChanged();
                    ((AsyncRelayCommand)EditItemCommand).NotifyCanExecuteChanged();
                    ((RelayCommand)ViewDetailsCommand).NotifyCanExecuteChanged();
                    ((AsyncRelayCommand)OpenRentalHistoryCommand).NotifyCanExecuteChanged();
                }
            }
        }

        private string _selectedCategory = "All";

        /// <summary>
        /// Currently selected category used to filter <see cref="SearchResults"/>.
        /// Changing the value triggers <see cref="SearchCommand"/> to reapply the filter.
        /// </summary>
        public string SelectedCategory
        {
            get => _selectedCategory;
            set
            {
                if (SetProperty(ref _selectedCategory, value))
                {
                    _searchCts?.Cancel();
                    _searchCts?.Dispose();
                    _searchCts = new CancellationTokenSource();
                    SearchCommand.Execute(null);
                }
            }
        }

        public IAsyncRelayCommand SearchCommand { get; }
        public IAsyncRelayCommand NewItemCommand { get; }
        public IAsyncRelayCommand EditItemCommand { get; }
        public IAsyncRelayCommand<IList?> DeleteItemsCommand { get; }
        public IAsyncRelayCommand OpenRentalsCommand { get; }
        public IRelayCommand ViewDetailsCommand { get; }
        public IRelayCommand<ItemModel?> OpenItemCardCommand { get; }
        public IAsyncRelayCommand OpenRentalHistoryCommand { get; }
        public IAsyncRelayCommand<ItemModel> RentItemCommand { get; }
        public IAsyncRelayCommand<ItemModel> ToggleCheckOutCommand { get; }
        public Func<Task>? OpenRentalReturnWorkflowAsync { get; set; }

        readonly IDispatcherTimer _searchDebounceTimer;
        CancellationTokenSource? _searchCts = new();

        bool _suppressItemsChanged;
        bool _disposed;
        const int PageSize = 50;

        // Writable for TwoWay binding from XAML. Mirrors SearchTerm and triggers search.
        private string _searchText = string.Empty;
        public string SearchText
        {
            get => _searchText;
            set
            {
                if (SetProperty(ref _searchText, value))
                {
                    SearchTerm = value;
                    _searchCts?.Cancel();
                    _searchDebounceTimer.Stop();
                    _searchDebounceTimer.Start();
                }
            }
        }

        public ItemManagementViewModel(IItemService itemService,
                                       ICustomerService customerService,
                                       IRentalService rentalService,
                                       IDialogService dialogService,
                                       ISettingsService settingsService,
                                       ILogger<ItemManagementViewModel>? logger = null,
                                       IDispatcherTimer? searchDebounceTimer = null)
        {
            _itemService = itemService;
            _customerService = customerService;
            _rentalService = rentalService;
            _dialogService = dialogService;
            _settingsService = settingsService;
            _logger = logger ?? NullLogger<ItemManagementViewModel>.Instance;
            SearchCommand = new AsyncRelayCommand(FilterItemsAsync);
            _searchDebounceTimer = searchDebounceTimer ?? new DispatcherTimerWrapper { Interval = TimeSpan.FromMilliseconds(300) };
            _searchDebounceTimer.Tick += OnSearchDebounceTimerTick;
            NewItemCommand = new AsyncRelayCommand(ct => AddItemAsync(ct));
            EditItemCommand = new AsyncRelayCommand(ct => EditItemAsync(ct), () => SelectedItem != null);
            DeleteItemsCommand = new AsyncRelayCommand<IList?>(DeleteItemsAsync);
            OpenRentalsCommand = new AsyncRelayCommand(ct => OpenRentalsAsync(ct), () => SelectedItem != null);
            ViewDetailsCommand = new RelayCommand(ViewDetails, () => SelectedItem != null);
            OpenItemCardCommand = new RelayCommand<ItemModel?>(OpenItemCard);
            OpenRentalHistoryCommand = new AsyncRelayCommand(OpenRentalHistoryAsync, () => SelectedItem != null);
            RentItemCommand = new AsyncRelayCommand<ItemModel>(RentItemAsync);
            ToggleCheckOutCommand = new AsyncRelayCommand<ItemModel>(ToggleCheckOutAsync);
            // Ensure no duplicate event subscriptions when the view model is
            // constructed multiple times or the collection persists across
            // instances.
            Items.CollectionChanged -= Items_CollectionChanged;
            Items.CollectionChanged += Items_CollectionChanged;
            SearchResults.CollectionChanged -= SearchResults_CollectionChanged;
            SearchResults.CollectionChanged += SearchResults_CollectionChanged;
            CheckedOutItems.CollectionChanged -= CheckedOutItems_CollectionChanged;
            CheckedOutItems.CollectionChanged += CheckedOutItems_CollectionChanged;
        }

        Dictionary<ItemDetailField, bool> _visibleFields = new();
        public Dictionary<ItemDetailField, bool> VisibleFields
        {
            get => _visibleFields;
            private set
            {
                if (SetProperty(ref _visibleFields, value))
                {
                    OnPropertyChanged(nameof(ShowImage));
                    OnPropertyChanged(nameof(ShowName));
                    OnPropertyChanged(nameof(ShowItemNumber));
                    OnPropertyChanged(nameof(ShowPartNumber));
                    OnPropertyChanged(nameof(ShowBrand));
                    OnPropertyChanged(nameof(ShowQuantityOnHand));
                    OnPropertyChanged(nameof(ShowLocation));
                    OnPropertyChanged(nameof(ShowPrice));
                    OnPropertyChanged(nameof(ShowNotes));
                }
            }
        }

        public bool ShowImage => VisibleFields.TryGetValue(ItemDetailField.Image, out var v) && v;
        public bool ShowName => VisibleFields.TryGetValue(ItemDetailField.Name, out var v) && v;
        public bool ShowItemNumber => VisibleFields.TryGetValue(ItemDetailField.ItemNumber, out var v) && v;
        public bool ShowPartNumber => VisibleFields.TryGetValue(ItemDetailField.PartNumber, out var v) && v;
        public bool ShowBrand => VisibleFields.TryGetValue(ItemDetailField.Brand, out var v) && v;
        public bool ShowQuantityOnHand => VisibleFields.TryGetValue(ItemDetailField.QuantityOnHand, out var v) && v;
        public bool ShowLocation => VisibleFields.TryGetValue(ItemDetailField.Location, out var v) && v;
        public bool ShowPrice => VisibleFields.TryGetValue(ItemDetailField.Price, out var v) && v;
        public bool ShowNotes => VisibleFields.TryGetValue(ItemDetailField.Notes, out var v) && v;

        private double _cardScale = 1.0;
        public double CardScale
        {
            get => _cardScale;
            private set
            {
                if (SetProperty(ref _cardScale, value))
                {
                    OnPropertyChanged(nameof(CardWidth));
                    OnPropertyChanged(nameof(CardImageWidth));
                    OnPropertyChanged(nameof(CardImageHeight));
                }
            }
        }

        public double CardWidth => Math.Round(240 * CardScale);
        public double CardImageWidth => Math.Round(220 * CardScale);
        public double CardImageHeight => Math.Round(200 * CardScale);

        bool _initialized;
        public async Task InitializeAsync()
        {
            if (_initialized) return;
            var vis = await _settingsService.GetItemDetailVisibilityAsync().ConfigureAwait(false);
            var complete = Enum.GetValues<ItemDetailField>().ToDictionary(f => f, f => vis.TryGetValue(f, out var v) ? v : true);
            VisibleFields = complete;
            CardScale = await _settingsService.GetItemCardSizeAsync().ConfigureAwait(false);
            _settingsService.ItemDetailVisibilityChanged += OnItemDetailVisibilityChanged;
            _settingsService.ItemCardSizeChanged += OnItemCardSizeChanged;
            _initialized = true;
            _searchCts?.Cancel();
            _searchCts?.Dispose();
            _searchCts = new CancellationTokenSource();
            await SearchCommand.ExecuteAsync(null);
        }

        async void OnItemDetailVisibilityChanged(object? sender, IDictionary<ItemDetailField, bool> visibility)
        {
            try
            {
                var complete = Enum.GetValues<ItemDetailField>().ToDictionary(f => f, f => visibility.TryGetValue(f, out var v) ? v : true);
                VisibleFields = complete;
                _searchCts?.Cancel();
                _searchCts?.Dispose();
                _searchCts = new CancellationTokenSource();
                await SearchCommand.ExecuteAsync(null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to handle visibility change");
            }
        }

        void OnItemCardSizeChanged(object? sender, double size)
        {
            if (size <= 0.2)
                return;
            CardScale = size;
        }

        void OnSearchDebounceTimerTick(object? s, EventArgs e)
        {
            _searchDebounceTimer.Stop();
            _searchCts?.Dispose();
            _searchCts = new CancellationTokenSource();
            SearchCommand.Execute(null);
        }

        public async Task LoadItemsAsync(ItemPage page)
        {
            _suppressItemsChanged = true;
            try
            {
                var list = new List<ItemModel>();
                await foreach (var item in _itemService.GetItemsAsync(page, SortField.Name, SortDirection.Ascending, isRentalItem: false)
                    .WithCancellation(CancellationToken.None))
                    list.Add(item);
                Items.ReplaceRange(list);
                SearchResults.ReplaceRange(list);
                LoadCategories(Items);
                await RefreshCheckedOutItemsAsync(CancellationToken.None);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load item directory");
                ClearItemStateAfterLoadFailure();
                await _dialogService.ShowInfoAsync($"Failed to load {LabelProvider.Instance.ItemLabelPlural.ToLower()}: {ex.Message} Visible item rows were cleared until reload succeeds.", "Error");
            }
            finally
            {
                _suppressItemsChanged = false;
            }
        }

        /// <summary>
        /// Applies text and category filters to the item list.
        /// Invoked by <see cref="SearchCommand"/> whenever the search text or
        /// <see cref="SelectedCategory"/> changes and recomputes <see cref="Categories"/>.
        /// </summary>
        async Task FilterItemsAsync()
        {
            var cancellationToken = _searchCts?.Token ?? CancellationToken.None;
            if (cancellationToken.IsCancellationRequested)
                return;

            var term = string.IsNullOrWhiteSpace(SearchTerm) ? string.Empty : SearchTerm.Trim();
            var page = new ItemPage(1, PageSize);
            var list = new List<ItemModel>();

            SearchResults.Clear();

            try
            {
                if (!string.IsNullOrWhiteSpace(term))
                {
                    await foreach (var item in _itemService.SearchItemsAsync(term, page, SortField.Name, SortDirection.Ascending, isRentalItem: false, cancellationToken: cancellationToken)
                        .WithCancellation(cancellationToken))
                        list.Add(item);
                }
                else
                {
                    await foreach (var item in _itemService.GetItemsAsync(page, SortField.Name, SortDirection.Ascending, isRentalItem: false, cancellationToken: cancellationToken)
                        .WithCancellation(cancellationToken))
                        list.Add(item);
                }
            }
            catch (OperationCanceledException ex)
            {
                _logger.LogDebug(ex, "Item search cancelled");
                return;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to search item directory");
                ClearItemStateAfterLoadFailure();
                await _dialogService.ShowInfoAsync($"Failed to search {LabelProvider.Instance.ItemLabelPlural.ToLower()}: {ex.Message} Visible item rows were cleared until reload succeeds.", "Error");
                return;
            }

            if (!string.IsNullOrWhiteSpace(SelectedCategory) && SelectedCategory != "All")
            {
                list = list.Where(t => string.Equals(t.Brand, SelectedCategory, StringComparison.OrdinalIgnoreCase)).ToList();
            }

            if (string.IsNullOrWhiteSpace(term))
            {
                Items.ReplaceRange(list);
                SearchResults.ReplaceRange(list);
                LoadCategories(list, suppressSearch: true);
                await RefreshCheckedOutItemsAsync(cancellationToken);
            }
            else
            {
                SearchResults.ReplaceRange(list);
                await RefreshCheckedOutItemsAsync(cancellationToken);
            }
        }

        async Task AddItemAsync(CancellationToken cancellationToken)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(NewItem.ItemNumber))
                    NewItem.ItemNumber = await _itemService.GenerateNextItemNumberAsync(cancellationToken);
                await _itemService.AddItemAsync(NewItem, cancellationToken);
                await LoadItemsAsync(new ItemPage(1, PageSize));
                await FilterItemsAsync();
                NewItem = new ItemModel();
            }
            catch (OperationCanceledException)
            {
                // Ignore cancellation
            }
            catch (UnauthorizedAccessException)
            {
                await _dialogService.ShowInfoAsync($"You are not authorized to add {LabelProvider.Instance.ItemLabelPlural.ToLower()}.", "Unauthorized");
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogError(ex, "Failed to add {ItemLabelSingular} due to invalid operation", LabelProvider.Instance.ItemLabelSingular);
                await _dialogService.ShowInfoAsync(ex.Message, "Error");
            }
            catch (ArgumentException ex)
            {
                _logger.LogError(ex, "Failed to add {ItemLabelSingular} due to invalid argument", LabelProvider.Instance.ItemLabelSingular);
                await _dialogService.ShowInfoAsync(ex.Message, "Error");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to add {ItemLabelSingular}", LabelProvider.Instance.ItemLabelSingular);
                await _dialogService.ShowInfoAsync($"Failed to add {LabelProvider.Instance.ItemLabelSingular.ToLower()}: {ex.Message}", "Error");
            }
        }

        async Task EditItemAsync(CancellationToken cancellationToken)
        {
            var selected = SelectedItem;
            if (selected == null) return;

            var clone = CloneItemForEdit(selected);
            var updated = await _dialogService.ShowEditItemDialogAsync(clone);
            if (updated == null) return;

            try
            {
                var itemId = updated.ItemID;
                await _itemService.UpdateItemAsync(updated, cancellationToken);
                await LoadItemsAsync(new ItemPage(1, PageSize));
                await FilterItemsAsync();
                SelectedItem = SearchResults.FirstOrDefault(t => t.ItemID == itemId)
                    ?? Items.FirstOrDefault(t => t.ItemID == itemId)
                    ?? selected;
            }
            catch (OperationCanceledException)
            {
                // Ignore cancellation
            }
            catch (UnauthorizedAccessException)
            {
                await _dialogService.ShowInfoAsync($"You are not authorized to update {LabelProvider.Instance.ItemLabelPlural.ToLower()}.", "Unauthorized");
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogError(ex, "Failed to update {ItemLabelSingular} {ItemID} due to invalid operation", LabelProvider.Instance.ItemLabelSingular, selected.ItemID);
                await _dialogService.ShowInfoAsync(ex.Message, "Error");
            }
            catch (ArgumentException ex)
            {
                _logger.LogError(ex, "Failed to update {ItemLabelSingular} {ItemID} due to invalid argument", LabelProvider.Instance.ItemLabelSingular, selected.ItemID);
                await _dialogService.ShowInfoAsync(ex.Message, "Error");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update {ItemLabelSingular} {ItemID}", LabelProvider.Instance.ItemLabelSingular, selected.ItemID);
                await _dialogService.ShowInfoAsync($"Failed to update {LabelProvider.Instance.ItemLabelSingular.ToLower()}: {ex.Message}", "Error");
            }
        }

        void ViewDetails()
        {
            var item = SelectedItem;
            if (item == null) return;
            _dialogService.ShowItemDetails(item);
        }

        void OpenItemCard(ItemModel? item)
        {
            if (item == null)
                return;

            SelectedItem = item;
            ViewDetails();
        }

        async Task OpenRentalHistoryAsync()
        {
            var item = SelectedItem;
            if (item == null) return;
            try
            {
                var history = await _rentalService.GetRentalHistoryForItemAsync(item.ItemID);
                _dialogService.ShowRentalHistory(item, history);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to open rental history for {ItemLabelSingular} {ItemID}", LabelProvider.Instance.ItemLabelSingular, item.ItemID);
                await _dialogService.ShowInfoAsync($"Failed to load rental history: {ex.Message}", "Error");
            }
        }

        async Task DeleteItemsAsync(IList? items, CancellationToken cancellationToken)
        {
            if (items == null || items.Count == 0) return;
            var message = items.Count == 1 && items[0] is ItemModel { Name: { } name }
                ? $"Delete {LabelProvider.Instance.ItemLabelSingular.ToLower()} '{name}'?"
                : $"Delete {items.Count} {LabelProvider.Instance.ItemLabelPlural.ToLower()}?";
            var confirm = await _dialogService.ShowConfirmationAsync(message, "Confirm Delete").ConfigureAwait(false);
            if (!confirm)
                return;

            try
            {
                foreach (ItemModel item in items)
                {
                    await _itemService.DeleteItemAsync(item.ItemID, cancellationToken);
                }
                await LoadItemsAsync(new ItemPage(1, PageSize));
                await FilterItemsAsync();
                SelectedItem = null;
            }
            catch (OperationCanceledException)
            {
                // Ignore cancellation
            }
            catch (UnauthorizedAccessException)
            {
                await _dialogService.ShowInfoAsync($"You are not authorized to delete {LabelProvider.Instance.ItemLabelPlural.ToLower()}.", "Unauthorized");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete {ItemLabelPlural}", LabelProvider.Instance.ItemLabelPlural);
                await _dialogService.ShowInfoAsync($"Failed to delete {LabelProvider.Instance.ItemLabelPlural.ToLower()}: {ex.Message}", "Error");
            }
        }

        async Task OpenRentalsAsync(CancellationToken cancellationToken)
        {
            var item = SelectedItem;
            if (item == null) return;

            try
            {
                var customers = await _customerService.GetAllCustomersAsync(cancellationToken);
                var result = _dialogService.ShowRentItemDialog(item, customers);
                if (result != null)
                {
                    var (customer, dueDate) = result.Value;
                    await _rentalService.RentItemAsync(item.ItemID,
                        customer.CustomerID,
                        DateTime.Today,
                        dueDate);
                    await PromptToPrintRentalHandoffAsync(item, customer, dueDate);
                    await ReloadItemsAfterRentalAsync(item.ItemID, cancellationToken);
                }
            }
            catch (OperationCanceledException)
            {
                // Ignore cancellation
            }
            catch (UnauthorizedAccessException)
            {
                await _dialogService.ShowInfoAsync($"You are not authorized to rent {LabelProvider.Instance.ItemLabelPlural.ToLower()}.", "Unauthorized");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to rent {ItemLabelSingular} {ItemID}", LabelProvider.Instance.ItemLabelSingular, item.ItemID);
                await RefreshItemsAfterWorkflowFailureAsync(item.ItemID, cancellationToken);
                await _dialogService.ShowInfoAsync($"Failed to rent {LabelProvider.Instance.ItemLabelSingular.ToLower()}: {ex.Message} The item list has been refreshed in case the rental was saved before the failure.", "Error");
            }
        }

        private async Task RentItemAsync(ItemModel? item, CancellationToken cancellationToken)
        {
            if (item == null) return;
            try
            {
                var customers = await _customerService.GetAllCustomersAsync(cancellationToken);
                var result = _dialogService.ShowRentItemDialog(item, customers);
                if (result != null)
                {
                    var (customer, dueDate) = result.Value;
                    await _rentalService.RentItemAsync(item.ItemID, customer.CustomerID, DateTime.Today, dueDate);
                    await PromptToPrintRentalHandoffAsync(item, customer, dueDate);
                    await ReloadItemsAfterRentalAsync(item.ItemID, cancellationToken);
                }
            }
            catch (OperationCanceledException)
            {
            }
            catch (UnauthorizedAccessException)
            {
                await _dialogService.ShowInfoAsync($"You are not authorized to rent {LabelProvider.Instance.ItemLabelPlural.ToLower()}.", "Unauthorized");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to rent {ItemLabelSingular} {ItemID}", LabelProvider.Instance.ItemLabelSingular, item.ItemID);
                await RefreshItemsAfterWorkflowFailureAsync(item.ItemID, cancellationToken);
                await _dialogService.ShowInfoAsync($"Failed to rent {LabelProvider.Instance.ItemLabelSingular.ToLower()}: {ex.Message} The item list has been refreshed in case the rental was saved before the failure.", "Error");
            }
        }

        private Task ReloadItemsAfterRentalAsync(int itemId, CancellationToken cancellationToken)
        {
            return ReloadItemsAfterItemWorkflowAsync(itemId, cancellationToken);
        }

        private async Task PromptToPrintRentalHandoffAsync(ItemModel item, CustomerModel customer, DateTime dueDate)
        {
            var print = await _dialogService.ShowConfirmAsync(
                "Print Rental Handoff",
                $"Rental saved for {ValueOrNotRecorded(customer.Company)}.{Environment.NewLine}{Environment.NewLine}Print the picking slip for shelf collection now?");
            if (!print)
                return;

            var printInvoice = await IsRentalInvoiceEnabledAsync().ConfigureAwait(false);
            var rental = await FindNewActiveRentalAsync(item, customer, dueDate)
                ?? BuildRentalHandoffFallback(item, customer, dueDate);
            var printService = new RentalPrintingService("Equipment Rentals", "", "");
            var rentalTitle = rental.RentalID > 0 ? rental.RentalID.ToString() : item.ItemNumber;

            _dialogService.ShowPrintPreview(
                printService.GeneratePickingSlip(rental),
                $"Picking Slip - Rental {rentalTitle}",
                "Shelf picking slip");
            if (printInvoice)
            {
                _dialogService.ShowPrintPreview(
                    printService.GenerateInvoice(rental, dailyRate: 25.00m, lateFee: 0),
                    $"Invoice - Rental {rentalTitle}",
                    "Customer rental copy");
            }
        }

        private async Task<bool> IsRentalInvoiceEnabledAsync()
        {
            try
            {
                return await new RentalConfigurationService(_settingsService).GetInvoiceEnabledAsync().ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to read rental invoice setting.");
                return false;
            }
        }

        private async Task<RentalModel?> FindNewActiveRentalAsync(ItemModel item, CustomerModel customer, DateTime dueDate)
        {
            try
            {
                var activeRentals = await _rentalService.GetActiveRentalsAsync().ConfigureAwait(false);
                return activeRentals
                    .Where(r => r.ItemID == item.ItemID
                        && r.CustomerID == customer.CustomerID
                        && r.DueDate.Date == dueDate.Date
                        && string.Equals(r.Status, "Rented", StringComparison.OrdinalIgnoreCase))
                    .OrderByDescending(r => r.RentalID)
                    .FirstOrDefault();
            }
            catch
            {
                return null;
            }
        }

        private static RentalModel BuildRentalHandoffFallback(ItemModel item, CustomerModel customer, DateTime dueDate)
        {
            return new RentalModel
            {
                ItemID = item.ItemID,
                CustomerID = customer.CustomerID,
                RentalDate = DateTime.Today,
                DueDate = dueDate,
                Status = "Rented",
                ItemNumber = item.ItemNumber,
                ItemLocation = item.Location,
                CustomerName = customer.Company,
                CustomerContact = customer.Contact,
                CustomerEmail = customer.Email,
                CustomerPhone = string.IsNullOrWhiteSpace(customer.Phone) ? customer.Mobile : customer.Phone,
                CustomerMobile = customer.Mobile,
                CustomerAddress = customer.Address
            };
        }

        private static string ValueOrNotRecorded(string? value) => string.IsNullOrWhiteSpace(value) ? "Not recorded" : value;

        private Task ReloadItemsAfterCheckoutAsync(ItemModel item, CancellationToken cancellationToken)
        {
            return ReloadItemAfterCheckoutAsync(item, cancellationToken);
        }

        private async Task RefreshItemsAfterWorkflowFailureAsync(int itemId, CancellationToken cancellationToken)
        {
            try
            {
                await ReloadItemsAfterItemWorkflowAsync(itemId, cancellationToken);
            }
            catch (Exception refreshEx)
            {
                _logger.LogError(refreshEx, "Failed to refresh items after workflow failure for item {ItemID}", itemId);
            }
        }

        private async Task ReloadItemsAfterItemWorkflowAsync(int itemId, CancellationToken cancellationToken)
        {
            var previousSelection = SelectedItem;

            await LoadItemsAsync(new ItemPage(1, PageSize));
            await FilterItemsAsync();

            SelectedItem = SearchResults.FirstOrDefault(t => t.ItemID == itemId)
                ?? Items.FirstOrDefault(t => t.ItemID == itemId)
                ?? CheckedOutItems.FirstOrDefault(t => t.ItemID == itemId)
                ?? previousSelection;
        }

        private async Task ReloadItemAfterCheckoutAsync(ItemModel item, CancellationToken cancellationToken)
        {
            var itemId = item.ItemID;
            var previousSelection = SelectedItem;
            var refreshed = await _itemService.GetItemByIDAsync(itemId, cancellationToken).ConfigureAwait(false);

            if (refreshed != null)
            {
                var existingRows = new[] { item }
                    .Concat(Items)
                    .Concat(SearchResults)
                    .Concat(CheckedOutItems)
                    .Where(t => t.ItemID == itemId)
                    .Distinct()
                    .ToList();

                if (existingRows.Count == 0)
                {
                    SearchResults.Add(refreshed);
                }
                else
                {
                    foreach (var row in existingRows)
                        ApplyItemState(row, refreshed);
                }
            }
            else
            {
                await LoadItemsAsync(new ItemPage(1, PageSize));
                await FilterItemsAsync();
            }

            await RefreshCheckedOutItemsAsync(cancellationToken);

            SelectedItem = SearchResults.FirstOrDefault(t => t.ItemID == itemId)
                ?? Items.FirstOrDefault(t => t.ItemID == itemId)
                ?? CheckedOutItems.FirstOrDefault(t => t.ItemID == itemId)
                ?? previousSelection;
        }

        private async Task ToggleCheckOutAsync(ItemModel? item, CancellationToken cancellationToken)
        {
            if (item == null) return;
            try
            {
                if (await HandleRentedItemCheckInRequestAsync(item, cancellationToken))
                    return;

                var result = await _itemService.ToggleItemCheckOutStatusAsync(item.ItemID, cancellationToken).ConfigureAwait(false);
                if (!result)
                {
                    await ReloadItemsAfterCheckoutAsync(item, cancellationToken);
                    await _dialogService.ShowInfoAsync("Check-out status could not be updated. The item may have been changed by another user; the list has been refreshed.", "Check-out Status");
                    return;
                }

                await ReloadItemsAfterCheckoutAsync(item, cancellationToken);
            }
            catch (OperationCanceledException)
            {
            }
            catch (UnauthorizedAccessException)
            {
                await _dialogService.ShowInfoAsync("You are not authorized to update check-out status.", "Unauthorized");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to toggle check out status for item {ItemID}", item.ItemID);
                await RefreshItemsAfterWorkflowFailureAsync(item.ItemID, cancellationToken);
                await _dialogService.ShowInfoAsync($"Failed to update check-out status: {ex.Message} The item list has been refreshed in case the check-out status changed before the failure.", "Error");
            }
        }

        private async Task<bool> HandleRentedItemCheckInRequestAsync(ItemModel item, CancellationToken cancellationToken)
        {
            if (item.IsCheckedOut || !item.HasRentedStock)
                return false;

            await ReloadItemsAfterCheckoutAsync(item, cancellationToken);

            var refreshed = SelectedItem?.ItemID == item.ItemID ? SelectedItem : item;
            if (refreshed.IsCheckedOut || !refreshed.HasRentedStock)
                return false;

            const string title = "Return Rental";
            var message =
                $"{ValueOrNotRecorded(refreshed.ItemNumber)} is currently rented out, not checked out.{Environment.NewLine}{Environment.NewLine}" +
                "Rental returns must be completed from the Rentals screen so the customer rental and stock counts stay together.";

            if (OpenRentalReturnWorkflowAsync == null)
            {
                await _dialogService.ShowInfoAsync(message, title);
                return true;
            }

            var openRentals = await _dialogService.ShowConfirmAsync(
                title,
                $"{message}{Environment.NewLine}{Environment.NewLine}Open Rentals now to return it?");
            if (openRentals)
                await OpenRentalReturnWorkflowAsync();

            return true;
        }

        private static ItemModel CloneItemForEdit(ItemModel source)
        {
            return new ItemModel
            {
                ItemID = source.ItemID,
                ItemNumber = source.ItemNumber,
                PartNumber = source.PartNumber,
                Name = source.Name,
                Brand = source.Brand,
                Location = source.Location,
                Price = source.Price,
                QuantityOnHand = source.QuantityOnHand,
                RentedQuantity = source.RentedQuantity,
                Supplier = source.Supplier,
                PurchasedDate = source.PurchasedDate,
                Notes = source.Notes,
                Keywords = source.Keywords,
                IsPowered = source.IsPowered,
                IsRentalItem = source.IsRentalItem,
                IsCheckedOut = source.IsCheckedOut,
                CheckedOutBy = source.CheckedOutBy,
                CheckedOutTime = source.CheckedOutTime,
                CheckedInBy = source.CheckedInBy,
                CheckedInTime = source.CheckedInTime,
                ImagePath = source.ImagePath,
                UpdatedAt = source.UpdatedAt,
                IsIncomplete = source.IsIncomplete,
                MissingComponentsNotes = source.MissingComponentsNotes,
                IssuesNotes = source.IssuesNotes,
                CheckoutCount = source.CheckoutCount
            };
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

        private void ClearItemStateAfterLoadFailure()
        {
            _suppressItemsChanged = true;
            try
            {
                Items.Clear();
                SearchResults.Clear();
                CheckedOutItems.Clear();
                Categories.ReplaceRange(new[] { "All" });
                _selectedCategory = "All";
                OnPropertyChanged(nameof(SelectedCategory));
                SelectedItem = null;
                OnPropertyChanged(nameof(SearchResultsSummary));
                OnPropertyChanged(nameof(CheckedOutSummary));
            }
            finally
            {
                _suppressItemsChanged = false;
            }
        }

        void LoadCategories(IEnumerable<ItemModel> items, bool suppressSearch = false)
        {
            var categories = items.Select(t => t.Brand)
                                   .Where(b => !string.IsNullOrWhiteSpace(b))
                                   .Distinct()
                                   .OrderBy(b => b)
                                   .ToList();
            categories.Insert(0, "All");
            Categories.ReplaceRange(categories);

            if (!Categories.Contains(SelectedCategory))
            {
                if (suppressSearch)
                {
                    _selectedCategory = "All";
                    OnPropertyChanged(nameof(SelectedCategory));
                }
                else
                {
                    SelectedCategory = "All";
                }
            }
        }

        async Task RefreshCheckedOutItemsAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                var checkedOutItemsTask = _itemService.GetCheckedOutItemsAsync(cancellationToken);
                if (checkedOutItemsTask == null)
                {
                    ReplaceCheckedOutItemsFromLoadedItems();
                    return;
                }

                var checkedOutItems = await checkedOutItemsTask;
                if (checkedOutItems == null)
                {
                    ReplaceCheckedOutItemsFromLoadedItems();
                    return;
                }

                CheckedOutItems.ReplaceRange(checkedOutItems
                    .OrderByDescending(t => t.CheckedOutTime ?? DateTime.MinValue)
                    .ThenBy(t => t.Name)
                    .ToList());
            }
            catch (OperationCanceledException)
            {
            }
            catch (NullReferenceException)
            {
                ReplaceCheckedOutItemsFromLoadedItems();
            }
        }

        void ReplaceCheckedOutItemsFromLoadedItems()
        {
            var checkedOutItems = Items.Where(t => t.IsCheckedOut)
                                       .OrderByDescending(t => t.CheckedOutTime ?? DateTime.MinValue)
                                       .ThenBy(t => t.Name)
                                       .ToList();
            CheckedOutItems.ReplaceRange(checkedOutItems);
        }

        void Items_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            if (_suppressItemsChanged)
                return;

            LoadCategories(Items);
            _ = RefreshCheckedOutItemsAsync();
        }

        void SearchResults_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            OnPropertyChanged(nameof(SearchResultsSummary));
        }

        void CheckedOutItems_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            OnPropertyChanged(nameof(CheckedOutSummary));
        }

        /// <inheritdoc />
        public void Dispose()
        {
            if (_disposed)
                return;
            _disposed = true;

            _searchDebounceTimer.Tick -= OnSearchDebounceTimerTick;
            _searchDebounceTimer.Stop();

            var cts = Interlocked.Exchange(ref _searchCts, null);
            try
            {
                cts?.Cancel();
            }
            catch (ObjectDisposedException)
            {
            }
            cts?.Dispose();

            Items.CollectionChanged -= Items_CollectionChanged;
            SearchResults.CollectionChanged -= SearchResults_CollectionChanged;
            CheckedOutItems.CollectionChanged -= CheckedOutItems_CollectionChanged;
            _settingsService.ItemDetailVisibilityChanged -= OnItemDetailVisibilityChanged;
            _settingsService.ItemCardSizeChanged -= OnItemCardSizeChanged;
        }
    }
}
