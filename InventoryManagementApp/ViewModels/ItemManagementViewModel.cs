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
using InventoryManagementApp.Interfaces;
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

        private ItemModel _selectedItem;
        public ItemModel SelectedItem
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
                    _searchCts.Cancel();
                    _searchCts.Dispose();
                    _searchCts = new CancellationTokenSource();
                    SearchCommand.Execute();
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

        readonly IDispatcherTimer _searchDebounceTimer;
        CancellationTokenSource _searchCts = new();

        bool _suppressItemsChanged;
        bool _disposed;

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
                    _searchCts.Cancel();
                    _searchDebounceTimer.Stop();
                    _searchDebounceTimer.Start();
                }
            }
        }

        public ItemManagementViewModel(IItemService itemService,
                                       ICustomerService customerService,
                                       IRentalService rentalService,
                                       IDialogService dialogService,
                                       ILogger<ItemManagementViewModel>? logger = null,
                                       IDispatcherTimer? searchDebounceTimer = null)
        {
            _itemService = itemService;
            _customerService = customerService;
            _rentalService = rentalService;
            _dialogService = dialogService;
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
            // Ensure no duplicate event subscriptions when the view model is
            // constructed multiple times or the collection persists across
            // instances.
            Items.CollectionChanged -= Items_CollectionChanged;
            Items.CollectionChanged += Items_CollectionChanged;
        }

        void OnSearchDebounceTimerTick(object? s, EventArgs e)
        {
            _searchDebounceTimer.Stop();
            _searchCts.Dispose();
            _searchCts = new CancellationTokenSource();
            SearchCommand.Execute();
        }

        public async Task LoadItemsAsync()
        {
            _suppressItemsChanged = true;
            try
            {
                var all = await _itemService.GetAllItemsAsync();
                Items.ReplaceRange(all);
                SearchResults.ReplaceRange(all);
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
            var cancellationToken = _searchCts.Token;
            var term = string.IsNullOrWhiteSpace(SearchTerm) ? string.Empty : SearchTerm.Trim();
            IEnumerable<ItemModel> source;

            if (!string.IsNullOrEmpty(term))
            {
                source = await _itemService.SearchItemsAsync(term, cancellationToken);
            }
            else
            {
                if (Items.Count == 0)
                {
                    var all = await _itemService.GetAllItemsAsync();
                    Items.ReplaceRange(all);
                }
                source = Items;
            }

            if (!string.IsNullOrEmpty(SelectedCategory) && SelectedCategory != "All")
            {
                source = source.Where(t => string.Equals(t.Brand, SelectedCategory, StringComparison.OrdinalIgnoreCase));
            }

            SearchResults.ReplaceRange(source);
            LoadCategories(source, suppressSearch: true);
        }

        async Task AddItemAsync(CancellationToken cancellationToken)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(NewItem.ItemNumber))
                    NewItem.ItemNumber = await _itemService.GenerateNextItemNumberAsync(cancellationToken);
                await _itemService.AddItemAsync(NewItem, cancellationToken);
                await LoadItemsAsync();
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
            if (SelectedItem == null) return;

            var clone = new ItemModel
            {
                ItemID = SelectedItem.ItemID,
                ItemNumber = SelectedItem.ItemNumber,
                PartNumber = SelectedItem.PartNumber,
                NameDescription = SelectedItem.NameDescription,
                Brand = SelectedItem.Brand,
                Location = SelectedItem.Location,
                QuantityOnHand = SelectedItem.QuantityOnHand,
                RentedQuantity = SelectedItem.RentedQuantity,
                Supplier = SelectedItem.Supplier,
                PurchasedDate = SelectedItem.PurchasedDate,
                Notes = SelectedItem.Notes,
                Keywords = SelectedItem.Keywords,
                IsPowered = SelectedItem.IsPowered,
                IsCheckedOut = SelectedItem.IsCheckedOut,
                CheckedOutBy = SelectedItem.CheckedOutBy,
                CheckedOutTime = SelectedItem.CheckedOutTime,
                ImagePath = SelectedItem.ImagePath
            };

            var updated = await _dialogService.ShowEditItemDialogAsync(clone);
            if (updated == null) return;

            try
            {
                await _itemService.UpdateItemAsync(updated, cancellationToken);
                await LoadItemsAsync();
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
            if (SelectedItem == null) return;
            _dialogService.ShowItemDetails(SelectedItem);
        }

        async Task OpenRentalHistoryAsync()
        {
            if (SelectedItem == null) return;
            try
            {
                var history = await _rentalService.GetRentalHistoryForItemAsync(SelectedItem.ItemID);
                _dialogService.ShowRentalHistory(SelectedItem, history);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to open rental history for {ItemLabelSingular} {ItemID}", LabelProvider.Instance.ItemLabelSingular, SelectedItem.ItemID);
            }
        }

        async Task DeleteItemsAsync(IList items, CancellationToken cancellationToken)
        {
            if (items == null || items.Count == 0) return;
            string message = items.Count == 1
                ? $"Delete {LabelProvider.Instance.ItemLabelSingular.ToLower()} '{((ItemModel)items[0]).NameDescription}'?"
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
                await LoadItemsAsync();
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
            if (SelectedItem == null) return;

            try
            {
                var customers = await _customerService.GetAllCustomersAsync(cancellationToken);
                var result = _dialogService.ShowRentItemDialog(SelectedItem, customers);
                if (result != null)
                {
                    var (customer, dueDate) = result.Value;
                    await _rentalService.RentItemAsync(SelectedItem.ItemID,
                        customer.CustomerID,
                        DateTime.Today,
                        dueDate);
                    await LoadItemsAsync();
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
                _logger.LogError(ex, "Failed to rent {ItemLabelSingular} {ItemID}", LabelProvider.Instance.ItemLabelSingular, SelectedItem?.ItemID);
                await _dialogService.ShowInfoAsync($"Failed to rent {LabelProvider.Instance.ItemLabelSingular.ToLower()}: {ex.Message}", "Error");
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

            var cts = Interlocked.Exchange(ref _searchCts, null!);
            try
            {
                cts?.Cancel();
            }
            catch (ObjectDisposedException)
            {
            }
            cts?.Dispose();

            Items.CollectionChanged -= Items_CollectionChanged;
        }
    }
}
