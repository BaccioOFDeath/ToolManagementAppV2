using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using InventoryManagementApp.Data;
using InventoryManagementApp.Interfaces;
using InventoryManagementApp.Models.Domain;
using InventoryManagementApp.Models;
using InventoryManagementApp.Utilities;
using InventoryManagementApp.Utilities.Helpers;
using Microsoft.Extensions.Logging;
using Application = System.Windows.Application;

namespace InventoryManagementApp.ViewModels
{
    public partial class ItemsViewModel : ObservableObject, IDisposable
    {
        private readonly IItemService _itemService;
        private readonly MemoryBudget _memoryBudget;
        private readonly IDialogService _dialogService;
        private readonly IRentalService _rentalService;
        private readonly ISettingsService _settingsService;
        private readonly ILogger<ItemsViewModel> _logger;
        private CancellationTokenSource _filterCts = new();
        private bool _disposed;
        private readonly List<ItemModel> _pendingEdits = new();

        public IncrementalLoadingCollection<ItemModel> Items { get; }

        public IAsyncRelayCommand EditItemCommand { get; }
        public IRelayCommand ViewDetailsCommand { get; }
        public IAsyncRelayCommand OpenRentalHistoryCommand { get; }
        public IAsyncRelayCommand NewItemCommand { get; }
        public IAsyncRelayCommand<IList> DeleteItemsCommand { get; }
        public IAsyncRelayCommand CommitChangesCommand { get; }

        public IReadOnlyCollection<ItemModel> PendingEdits => _pendingEdits;

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

        private bool GetVisible(ItemDetailField field) => VisibleFields.TryGetValue(field, out var v) && v;

        public bool ShowImage => GetVisible(ItemDetailField.Image);
        public bool ShowName => GetVisible(ItemDetailField.Name);
        public bool ShowItemNumber => GetVisible(ItemDetailField.ItemNumber);
        public bool ShowPartNumber => GetVisible(ItemDetailField.PartNumber);
        public bool ShowBrand => GetVisible(ItemDetailField.Brand);
        public bool ShowQuantityOnHand => GetVisible(ItemDetailField.QuantityOnHand);
        public bool ShowLocation => GetVisible(ItemDetailField.Location);
        public bool ShowPrice => GetVisible(ItemDetailField.Price);
        public bool ShowNotes => GetVisible(ItemDetailField.Notes);

        [ObservableProperty]
        private ItemModel? selectedItem;

        [ObservableProperty]
        private string filter = string.Empty;

        [ObservableProperty]
        private SortOption selectedSortOption;

        [ObservableProperty]
        private int pageSize = 200;

        public ObservableCollection<SortOption> SortOptions { get; }

        public ItemsViewModel(IItemService itemService, MemoryBudget memoryBudget, IDialogService dialogService, IRentalService rentalService, ISettingsService settingsService, ILogger<ItemsViewModel> logger)
        {
            _itemService = itemService;
            _memoryBudget = memoryBudget;
            _dialogService = dialogService;
            _rentalService = rentalService;
            _settingsService = settingsService;
            _logger = logger;
            Items = new IncrementalLoadingCollection<ItemModel>(LoadPageAsync, PageSize);
            SortOptions = new ObservableCollection<SortOption>(new[]
            {
                new SortOption(SortField.Name, SortDirection.Ascending, "Name Asc"),
                new SortOption(SortField.Name, SortDirection.Descending, "Name Desc"),
                new SortOption(SortField.ItemNumber, SortDirection.Ascending, "Number Asc"),
                new SortOption(SortField.ItemNumber, SortDirection.Descending, "Number Desc"),
                new SortOption(SortField.QuantityOnHand, SortDirection.Ascending, "Qty Asc"),
                new SortOption(SortField.QuantityOnHand, SortDirection.Descending, "Qty Desc"),
                new SortOption(SortField.UpdatedAt, SortDirection.Ascending, "Updated Asc"),
                new SortOption(SortField.UpdatedAt, SortDirection.Descending, "Updated Desc")
            });
            selectedSortOption = SortOptions[0];
            VisibleFields = Enum.GetValues<ItemDetailField>().ToDictionary(f => f, _ => true);
            _memoryBudget.SteadyExceeded += OnSteadyExceeded;
            _memoryBudget.PeakExceeded += OnPeakExceeded;

            EditItemCommand = new AsyncRelayCommand(ct => EditItemAsync(ct));
            ViewDetailsCommand = new RelayCommand(ViewDetails);
            OpenRentalHistoryCommand = new AsyncRelayCommand(ct => OpenRentalHistoryAsync(ct));
            NewItemCommand = new AsyncRelayCommand(ct => NewItemAsync(ct));
            DeleteItemsCommand = new AsyncRelayCommand<IList>(DeleteItemsAsync);
            CommitChangesCommand = new AsyncRelayCommand(ct => CommitChangesAsync(ct));
        }

        public async Task InitializeAsync(CancellationToken ct = default)
        {
            try
            {
                var psSetting = await _settingsService.GetSettingAsync("PageSize", ct).ConfigureAwait(false);
                if (int.TryParse(psSetting, out var ps) && ps > 0)
                {
                    pageSize = ps;
                    Items.PageSize = ps;
                }

                var filterSetting = await _settingsService.GetSettingAsync("LastFilter", ct).ConfigureAwait(false);
                if (!string.IsNullOrEmpty(filterSetting))
                    filter = filterSetting;

                var sortSetting = await _settingsService.GetSettingAsync("LastSort", ct).ConfigureAwait(false);
                if (!string.IsNullOrEmpty(sortSetting))
                {
                    var parts = sortSetting.Split('|');
                    if (parts.Length == 2 && Enum.TryParse(parts[0], out SortField sf) && Enum.TryParse(parts[1], out SortDirection sd))
                    {
                        var opt = SortOptions.FirstOrDefault(o => o.Field == sf && o.Direction == sd);
                        if (opt != default)
                            selectedSortOption = opt;
                    }
                }

                var vis = await _settingsService.GetItemDetailVisibilityAsync(ct).ConfigureAwait(false);
                var complete = Enum.GetValues<ItemDetailField>().ToDictionary(f => f, f => vis.TryGetValue(f, out var v) ? v : true);
                VisibleFields = complete;
                _settingsService.ItemDetailVisibilityChanged += OnItemDetailVisibilityChanged;
            }
            catch (OperationCanceledException)
            {
                _logger.LogDebug("Initialization canceled");
            }
        }

        void OnItemDetailVisibilityChanged(object? sender, IDictionary<ItemDetailField, bool> visibility)
        {
            try
            {
                var complete = Enum.GetValues<ItemDetailField>().ToDictionary(f => f, f => visibility.TryGetValue(f, out var v) ? v : true);
                VisibleFields = complete;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to handle visibility change");
            }
        }

        private async Task<IList<ItemModel>> LoadPageAsync(int page, CancellationToken ct)
        {
            try
            {
                var result = new List<ItemModel>();
                var pageInfo = new ItemPage(page, PageSize);
                var source = string.IsNullOrWhiteSpace(Filter)
                    ? _itemService.GetItemsAsync(pageInfo, SelectedSortOption.Field, SelectedSortOption.Direction, isRentalItem: false, cancellationToken: ct)
                    : _itemService.SearchItemsAsync(Filter, pageInfo, SelectedSortOption.Field, SelectedSortOption.Direction, isRentalItem: false, cancellationToken: ct);
                await foreach (var item in source.ConfigureAwait(false))
                {
                    item.PropertyChanged += Item_PropertyChanged;
                    result.Add(item);
                }
                return result;
            }
            catch (OperationCanceledException)
            {
                _logger.LogDebug("Page load canceled");
                return Array.Empty<ItemModel>();
            }
        }

        public async Task LoadMoreAsync(CancellationToken ct = default)
        {
            try
            {
                await Items.LoadMoreAsync(ct).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                _logger.LogDebug("Incremental load canceled");
            }
        }

        partial void OnFilterChanged(string value)
        {
            _filterCts.Cancel();
            _filterCts.Dispose();
            _filterCts = new CancellationTokenSource();
            _ = ApplyFilterAsync(_filterCts.Token);
        }

        private async Task ApplyFilterAsync(CancellationToken token)
        {
            try
            {
                await Task.Delay(300, token).ConfigureAwait(false);
                Items.Reset();
                await Items.LoadMoreAsync(token).ConfigureAwait(false);
                await _settingsService.SaveSettingAsync("LastFilter", Filter, token).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                _logger.LogDebug("Filter application canceled");
            }
        }

        private void OnSteadyExceeded(object? sender, EventArgs e) => Items.TrimToWindow(PageSize * 3);

        private void OnPeakExceeded(object? sender, EventArgs e) => Items.Reset();

        partial void OnSelectedSortOptionChanged(SortOption value) => _ = ApplySortAsync(value);

        private async Task ApplySortAsync(SortOption value)
        {
            Items.Reset();
            await Items.LoadMoreAsync().ConfigureAwait(false);
            await _settingsService.SaveSettingAsync("LastSort", $"{value.Field}|{value.Direction}").ConfigureAwait(false);
        }

        partial void OnPageSizeChanged(int value) => _ = ApplyPageSizeAsync(value);

        private async Task ApplyPageSizeAsync(int value)
        {
            Items.PageSize = value;
            Items.TrimToWindow(value * 3);
            Items.Reset();
            await Items.LoadMoreAsync().ConfigureAwait(false);
            await _settingsService.SaveSettingAsync("PageSize", value.ToString()).ConfigureAwait(false);
        }

        private void Item_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (sender is not ItemModel item) return;
            if (e.PropertyName == nameof(ItemModel.QuantityOnHand) || e.PropertyName == nameof(ItemModel.Location) || e.PropertyName == nameof(ItemModel.Price))
            {
                if (!_pendingEdits.Contains(item))
                    _pendingEdits.Add(item);
            }
        }

        private async Task EditItemAsync(CancellationToken ct)
        {
            if (SelectedItem == null) return;
            ItemModel? updated;
            try
            {
                var clone = new ItemModel
                {
                    ItemID = SelectedItem.ItemID,
                    ItemNumber = SelectedItem.ItemNumber,
                    PartNumber = SelectedItem.PartNumber,
                    Name = SelectedItem.Name,
                    Brand = SelectedItem.Brand,
                    Location = SelectedItem.Location,
                    QuantityOnHand = SelectedItem.QuantityOnHand,
                    RentedQuantity = SelectedItem.RentedQuantity,
                    Supplier = SelectedItem.Supplier,
                    PurchasedDate = SelectedItem.PurchasedDate,
                    Notes = SelectedItem.Notes,
                    Keywords = SelectedItem.Keywords,
                    IsPowered = SelectedItem.IsPowered,
                    IsRentalItem = SelectedItem.IsRentalItem,
                    IsCheckedOut = SelectedItem.IsCheckedOut,
                    CheckedOutBy = SelectedItem.CheckedOutBy,
                    CheckedOutTime = SelectedItem.CheckedOutTime,
                    CheckedInBy = SelectedItem.CheckedInBy,
                    CheckedInTime = SelectedItem.CheckedInTime,
                    ImagePath = SelectedItem.ImagePath,
                    Price = SelectedItem.Price,
                    UpdatedAt = SelectedItem.UpdatedAt
                };
                updated = await _dialogService.ShowEditItemDialogAsync(clone).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                _logger.LogDebug("Edit item dialog canceled");
                return;
            }
            catch
            {
                return;
            }
            if (updated == null) return;
            try
            {
                await _itemService.UpdateItemAsync(updated, ct).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                _logger.LogDebug("Edit item canceled");
            }
            catch
            {
            }
        }

        private void ViewDetails()
        {
            if (SelectedItem == null) return;
            try
            {
                _dialogService.ShowItemDetails(SelectedItem);
            }
            catch
            {
            }
        }

        private async Task OpenRentalHistoryAsync(CancellationToken ct)
        {
            if (SelectedItem == null) return;
            try
            {
                var history = await _rentalService.GetRentalHistoryForItemAsync(SelectedItem.ItemID).ConfigureAwait(false);
                _dialogService.ShowRentalHistory(SelectedItem, history);
            }
            catch (OperationCanceledException)
            {
                _logger.LogDebug("Rental history load canceled");
            }
            catch
            {
            }
        }

        private async Task NewItemAsync(CancellationToken ct)
        {
            ItemModel? item;
            try
            {
                item = await _dialogService.ShowEditItemDialogAsync(new ItemModel()).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                _logger.LogDebug("New item dialog canceled");
                return;
            }
            catch
            {
                return;
            }
            if (item == null) return;
            try
            {
                await _itemService.AddItemAsync(item, ct).ConfigureAwait(false);
                Items.Reset();
                await Items.LoadMoreAsync(ct).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                _logger.LogDebug("New item creation canceled");
            }
            catch
            {
            }
        }

        private async Task DeleteItemsAsync(IList? items, CancellationToken ct)
        {
            if (items == null || items.Count == 0) return;
            var message = items.Count == 1
                ? $"Delete {LabelProvider.Instance.ItemLabelSingular.ToLower()} '{((ItemModel)items[0]).Name}'?"
                : $"Delete {items.Count} {LabelProvider.Instance.ItemLabelPlural.ToLower()}?";
            var confirm = await _dialogService.ShowConfirmationAsync(message, "Confirm Delete").ConfigureAwait(false);
            if (!confirm) return;
            try
            {
                var toRemove = items.Cast<ItemModel>().ToList();
                foreach (var item in toRemove)
                {
                    await _itemService.DeleteItemAsync(item.ItemID, ct).ConfigureAwait(false);
                    await Application.Current.Dispatcher.InvokeAsync(() => Items.Remove(item));
                }
                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    if (SelectedItem != null && toRemove.Contains(SelectedItem))
                        SelectedItem = null;
                });
            }
            catch (UnauthorizedAccessException)
            {
                await _dialogService.ShowInfoAsync($"You are not authorized to delete {LabelProvider.Instance.ItemLabelPlural.ToLower()}.", "Unauthorized").ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                _logger.LogDebug("Delete items canceled");
            }
            catch (Exception ex)
            {
                await _dialogService.ShowInfoAsync($"Failed to delete {LabelProvider.Instance.ItemLabelPlural.ToLower()}: {ex.Message}", "Error").ConfigureAwait(false);
            }
        }

        private async Task CommitChangesAsync(CancellationToken ct)
        {
            if (_pendingEdits.Count == 0) return;
            var edits = _pendingEdits.ToList();
            try
            {
                await _itemService.SaveChangesAsync(edits, ct).ConfigureAwait(false);
                _pendingEdits.Clear();
                foreach (var item in edits)
                {
                    var refreshed = await _itemService.GetItemByIDAsync(item.ItemID, ct).ConfigureAwait(false);
                    if (refreshed == null) continue;
                    item.PropertyChanged -= Item_PropertyChanged;
                    item.ItemNumber = refreshed.ItemNumber;
                    item.PartNumber = refreshed.PartNumber;
                    item.Name = refreshed.Name;
                    item.Brand = refreshed.Brand;
                    item.Location = refreshed.Location;
                    item.QuantityOnHand = refreshed.QuantityOnHand;
                    item.RentedQuantity = refreshed.RentedQuantity;
                    item.Supplier = refreshed.Supplier;
                    item.PurchasedDate = refreshed.PurchasedDate;
                    item.Notes = refreshed.Notes;
                    item.Keywords = refreshed.Keywords;
                    item.IsPowered = refreshed.IsPowered;
                    item.IsCheckedOut = refreshed.IsCheckedOut;
                    item.CheckedOutBy = refreshed.CheckedOutBy;
                    item.CheckedOutTime = refreshed.CheckedOutTime;
                    item.CheckedInBy = refreshed.CheckedInBy;
                    item.CheckedInTime = refreshed.CheckedInTime;
                    item.ImagePath = refreshed.ImagePath;
                    item.Price = refreshed.Price;
                    item.UpdatedAt = refreshed.UpdatedAt;
                    item.PropertyChanged += Item_PropertyChanged;
                }
                await _dialogService.ShowInfoAsync("Changes saved.", "Success").ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                _logger.LogDebug("Commit changes canceled");
            }
            catch (Exception ex)
            {
                await _dialogService.ShowInfoAsync($"Failed to save changes: {ex.Message}", "Error").ConfigureAwait(false);
            }
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            _memoryBudget.SteadyExceeded -= OnSteadyExceeded;
            _memoryBudget.PeakExceeded -= OnPeakExceeded;
            foreach (var item in Items)
                item.PropertyChanged -= Item_PropertyChanged;
            Items.Reset();
            _filterCts.Cancel();
            _filterCts.Dispose();
        }
    }

    public class IncrementalLoadingCollection<T> : ObservableCollection<T>
    {
        private readonly Func<int, CancellationToken, Task<IList<T>>> _loader;
        private int _pageSize;
        private int _page;
        private readonly SemaphoreSlim _gate = new(1, 1);
        public bool HasMoreItems { get; private set; } = true;
        private bool _isLoading;

        public bool IsLoading
        {
            get => _isLoading;
            private set
            {
                if (_isLoading == value) return;
                _isLoading = value;
                OnPropertyChanged(new PropertyChangedEventArgs(nameof(IsLoading)));
            }
        }

        public int PageSize
        {
            get => _pageSize;
            set => _pageSize = value;
        }

        public IncrementalLoadingCollection(Func<int, CancellationToken, Task<IList<T>>> loader, int pageSize)
        {
            _loader = loader;
            _pageSize = pageSize;
        }

        public async Task LoadMoreAsync(CancellationToken ct = default)
        {
            await _gate.WaitAsync(ct).ConfigureAwait(false);
            try
            {
                if (!HasMoreItems) return;
                IsLoading = true;
                var next = _page + 1;
                var items = await _loader(next, ct).ConfigureAwait(false);
                foreach (var item in items)
                    Add(item);
                _page = next;
                if (items.Count < _pageSize)
                    HasMoreItems = false;
            }
            finally
            {
                IsLoading = false;
                _gate.Release();
            }
        }

        public void Reset()
        {
            Clear();
            _page = 0;
            HasMoreItems = true;
        }

        public void TrimToWindow(int max)
        {
            if (Count <= max) return;
            while (Count > max)
                RemoveAt(0);
        }
    }
}
