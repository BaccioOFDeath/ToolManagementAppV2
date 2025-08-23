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
using InventoryManagementApp.Data;
using InventoryManagementApp.Interfaces;
using InventoryManagementApp.Models;
using InventoryManagementApp.Utilities;
using InventoryManagementApp.Utilities.Extensions;
using InventoryManagementApp.Services;
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
        public IAsyncRelayCommand<IList> DeleteItemsCommand { get; }
        public IAsyncRelayCommand OpenRentalsCommand { get; }
        public IRelayCommand ViewDetailsCommand { get; }
        public IAsyncRelayCommand OpenRentalHistoryCommand { get; }
        public IAsyncRelayCommand<ItemModel> RentItemCommand { get; }
        public IAsyncRelayCommand<ItemModel> ToggleCheckOutCommand { get; }

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
            DeleteItemsCommand = new AsyncRelayCommand<IList>(DeleteItemsAsync);
            OpenRentalsCommand = new AsyncRelayCommand(ct => OpenRentalsAsync(ct), () => SelectedItem != null);
            ViewDetailsCommand = new RelayCommand(ViewDetails, () => SelectedItem != null);
            OpenRentalHistoryCommand = new AsyncRelayCommand(OpenRentalHistoryAsync, () => SelectedItem != null);
            RentItemCommand = new AsyncRelayCommand<ItemModel>(RentItemAsync);
            ToggleCheckOutCommand = new AsyncRelayCommand<ItemModel>(ToggleCheckOutAsync);
            // Ensure no duplicate event subscriptions when the view model is
            // constructed multiple times or the collection persists across
            // instances.
            Items.CollectionChanged -= Items_CollectionChanged;
            Items.CollectionChanged += Items_CollectionChanged;
        }

        Dictionary<ItemDetailField, bool> _visibleFields = new();
        public Dictionary<ItemDetailField, bool> VisibleFields
        {
            get => _visibleFields;
            private set => SetProperty(ref _visibleFields, value);
        }

        bool _initialized;
        public async Task InitializeAsync()
        {
            if (_initialized) return;
            var vis = await _settingsService.GetItemDetailVisibilityAsync().ConfigureAwait(false);
            VisibleFields = new Dictionary<ItemDetailField, bool>(vis);
            _settingsService.ItemDetailVisibilityChanged += OnItemDetailVisibilityChanged;
            _initialized = true;
        }

        void OnItemDetailVisibilityChanged(object? sender, IDictionary<ItemDetailField, bool> e)
        {
            VisibleFields = new Dictionary<ItemDetailField, bool>(e);
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
                await foreach (var item in _itemService.GetItemsAsync(page, SortField.Name, SortDirection.Ascending, isRentalItem: false))
                    list.Add(item);
                Items.ReplaceRange(list);
                SearchResults.ReplaceRange(list);
                LoadCategories(Items);
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
            var term = string.IsNullOrWhiteSpace(SearchTerm) ? string.Empty : SearchTerm.Trim();
            var page = new ItemPage(1, PageSize);
            var list = new List<ItemModel>();

            if (!string.IsNullOrEmpty(term))
            {
                await foreach (var item in _itemService.SearchItemsAsync(term, page, SortField.Name, SortDirection.Ascending, isRentalItem: false, cancellationToken: cancellationToken))
                    list.Add(item);
            }
            else
            {
                await foreach (var item in _itemService.GetItemsAsync(page, SortField.Name, SortDirection.Ascending, isRentalItem: false, cancellationToken: cancellationToken))
                    list.Add(item);
            }

            if (!string.IsNullOrEmpty(SelectedCategory) && SelectedCategory != "All")
            {
                list = list.Where(t => string.Equals(t.Brand, SelectedCategory, StringComparison.OrdinalIgnoreCase)).ToList();
            }

            Items.ReplaceRange(list);
            SearchResults.ReplaceRange(list);
            LoadCategories(list, suppressSearch: true);
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
        }

        async Task EditItemAsync(CancellationToken cancellationToken)
        {
            var selected = SelectedItem;
            if (selected == null) return;

            var clone = new ItemModel
            {
                ItemID = selected.ItemID,
                ItemNumber = selected.ItemNumber,
                PartNumber = selected.PartNumber,
                Name = selected.Name,
                Brand = selected.Brand,
                Location = selected.Location,
                QuantityOnHand = selected.QuantityOnHand,
                RentedQuantity = selected.RentedQuantity,
                Supplier = selected.Supplier,
                PurchasedDate = selected.PurchasedDate,
                Notes = selected.Notes,
                Keywords = selected.Keywords,
                IsPowered = selected.IsPowered,
                IsRentalItem = selected.IsRentalItem,
                IsCheckedOut = selected.IsCheckedOut,
                CheckedOutBy = selected.CheckedOutBy,
                CheckedOutTime = selected.CheckedOutTime,
                ImagePath = selected.ImagePath
            };

            var updated = await _dialogService.ShowEditItemDialogAsync(clone);
            if (updated == null) return;

            try
            {
                await _itemService.UpdateItemAsync(updated, cancellationToken);
                await LoadItemsAsync(new ItemPage(1, PageSize));
                await FilterItemsAsync();
                SelectedItem = Items.FirstOrDefault(t => t.ItemID == updated.ItemID);
            }
            catch (OperationCanceledException)
            {
                // Ignore cancellation
            }
            catch (UnauthorizedAccessException)
            {
                await _dialogService.ShowInfoAsync($"You are not authorized to update {LabelProvider.Instance.ItemLabelPlural.ToLower()}.", "Unauthorized");
            }
        }

        void ViewDetails()
        {
            var item = SelectedItem;
            if (item == null) return;
            _dialogService.ShowItemDetails(item);
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
            }
        }

        async Task DeleteItemsAsync(IList items, CancellationToken cancellationToken)
        {
            if (items == null || items.Count == 0) return;
            string message = items.Count == 1
                ? $"Delete {LabelProvider.Instance.ItemLabelSingular.ToLower()} '{((ItemModel)items[0]).Name}'?"
                : $"Delete {items.Count} {LabelProvider.Instance.ItemLabelPlural.ToLower()}?";
            var confirm = await _dialogService.ShowConfirmationAsync(message, "Confirm Delete");
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
                    await LoadItemsAsync(new ItemPage(1, PageSize));
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
                await _dialogService.ShowInfoAsync($"Failed to rent {LabelProvider.Instance.ItemLabelSingular.ToLower()}: {ex.Message}", "Error");
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
                    await LoadItemsAsync(new ItemPage(1, PageSize));
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
                await _dialogService.ShowInfoAsync($"Failed to rent {LabelProvider.Instance.ItemLabelSingular.ToLower()}: {ex.Message}", "Error");
            }
        }

        private async Task ToggleCheckOutAsync(ItemModel? item, CancellationToken cancellationToken)
        {
            if (item == null) return;
            try
            {
                var result = await _itemService.ToggleItemCheckOutStatusAsync(item.ItemID, cancellationToken).ConfigureAwait(false);
                if (!result) return;
                var checkedOut = !item.IsCheckedOut;
                item.IsCheckedOut = checkedOut;
                item.QuantityOnHand += checkedOut ? -1 : 1;
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
                await _dialogService.ShowInfoAsync($"Failed to update check-out status: {ex.Message}", "Error");
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

        void Items_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            if (_suppressItemsChanged)
                return;

            LoadCategories(Items);
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
            _settingsService.ItemDetailVisibilityChanged -= OnItemDetailVisibilityChanged;
        }
    }
}
