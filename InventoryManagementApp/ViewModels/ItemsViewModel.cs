using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using InventoryManagementApp.Data;
using InventoryManagementApp.Interfaces;
using InventoryManagementApp.Messages;
using InventoryManagementApp.Models.Domain;
using InventoryManagementApp.Models;
using InventoryManagementApp.Utilities;
using InventoryManagementApp.Utilities.Helpers;
using InventoryManagementApp.Services.MobileCapture;
using InventoryManagementApp.Services.Items;
using InventoryManagementApp.Views.Windows;
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
        private readonly IUserContext? _userContext;
        private readonly MobileCaptureService? _mobileCaptureService;
        private readonly ILogger<ItemsViewModel> _logger;
        private readonly ItemThumbnailCache? _thumbnailCache;
        private CancellationTokenSource _filterCts = new();
        private CancellationTokenSource _loadCts = new();
        private CancellationTokenSource _imageScanCts = new();
        private CancellationTokenSource _thumbnailCts = new();
        private readonly SemaphoreSlim _initializeGate = new(1, 1);
        private readonly HashSet<ItemModel> _observedItems = new();
        private Task _imageStatusRefreshTask = Task.CompletedTask;
        private bool _hasInitialized;
        private int _cacheStale;
        private bool _disposed;
        private bool _suppressViewOptionRefresh;
        private readonly List<ItemModel> _pendingEdits = new();
        private static readonly string[] ItemImageExtensions = [".jpg", ".jpeg", ".png", ".bmp", ".gif"];
        private const int DefaultInteractivePageSize = 40;
        private const int MinInteractivePageSize = 20;
        private const int MaxInteractivePageSize = 60;

        public IncrementalLoadingCollection<ItemModel> Items { get; }

        private bool _isInitializing;
        public bool IsInitializing
        {
            get => _isInitializing;
            private set
            {
                if (SetProperty(ref _isInitializing, value))
                    OnPropertyChanged(nameof(IsDirectoryBusy));
            }
        }

        public bool IsDirectoryBusy => IsInitializing || Items.IsLoading;

        public IAsyncRelayCommand SearchCommand { get; }
        public IAsyncRelayCommand EditItemCommand { get; }
        public IRelayCommand ViewDetailsCommand { get; }
        public IAsyncRelayCommand OpenRentalHistoryCommand { get; }
        public IAsyncRelayCommand NewItemCommand { get; }
        public IAsyncRelayCommand OpenMobileCaptureCommand { get; }
        public IAsyncRelayCommand<ItemModel?> DeleteSelectedItemCommand { get; }
        public IAsyncRelayCommand<IList> DeleteItemsCommand { get; }
        public IAsyncRelayCommand CommitChangesCommand { get; }

        public IReadOnlyCollection<ItemModel> PendingEdits => _pendingEdits;

        private int _missingImageCount;
        public int MissingImageCount
        {
            get => _missingImageCount;
            private set => SetProperty(ref _missingImageCount, value);
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
        private int pageSize = DefaultInteractivePageSize;

        public ObservableCollection<SortOption> SortOptions { get; }

        public ItemsViewModel(IItemService itemService, MemoryBudget memoryBudget, IDialogService dialogService, IRentalService rentalService, ISettingsService settingsService, ILogger<ItemsViewModel> logger, MobileCaptureService? mobileCaptureService = null, IUserContext? userContext = null, ItemThumbnailCache? thumbnailCache = null)
        {
            _itemService = itemService;
            _memoryBudget = memoryBudget;
            _dialogService = dialogService;
            _rentalService = rentalService;
            _settingsService = settingsService;
            _mobileCaptureService = mobileCaptureService;
            _userContext = userContext;
            _logger = logger;
            _thumbnailCache = thumbnailCache;
            Items = new IncrementalLoadingCollection<ItemModel>(LoadPageAsync, PageSize, OnCollectionBatchApplied);
            SearchCommand = new AsyncRelayCommand(StartFilterAsync);
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
            if (SortOptions.Count > 0)
                selectedSortOption = SortOptions[0];
            VisibleFields = Enum.GetValues<ItemDetailField>().ToDictionary(f => f, _ => true);
            _memoryBudget.SteadyExceeded += OnSteadyExceeded;
            _memoryBudget.PeakExceeded += OnPeakExceeded;
            _settingsService.ItemDetailVisibilityChanged += OnItemDetailVisibilityChanged;
            if (_userContext is not null)
                _userContext.UserChanged += OnCurrentUserChanged;

            EditItemCommand = new AsyncRelayCommand(ct => EditItemAsync(ct));
            ViewDetailsCommand = new RelayCommand(ViewDetails);
            OpenRentalHistoryCommand = new AsyncRelayCommand(ct => OpenRentalHistoryAsync(ct));
            NewItemCommand = new AsyncRelayCommand(ct => NewItemAsync(ct));
            OpenMobileCaptureCommand = new AsyncRelayCommand(ct => OpenMobileCaptureAsync(ct));
            DeleteSelectedItemCommand = new AsyncRelayCommand<ItemModel?>(DeleteSelectedItemAsync, item => item != null);
            DeleteItemsCommand = new AsyncRelayCommand<IList>(DeleteItemsAsync);
            CommitChangesCommand = new AsyncRelayCommand(ct => CommitChangesAsync(ct));
            Items.CollectionChanged += Items_CollectionChanged;
            ((INotifyPropertyChanged)Items).PropertyChanged += Items_PropertyChanged;
            WeakReferenceMessenger.Default.Register<DomainDataChangedMessage>(this, (_, message) =>
            {
                if (message.Includes(DomainDataScope.Items | DomainDataScope.Categories | DomainDataScope.Kits))
                    Interlocked.Exchange(ref _cacheStale, 1);
            });
        }

        partial void OnSelectedItemChanged(ItemModel? value)
        {
            DeleteSelectedItemCommand.NotifyCanExecuteChanged();
        }

        public async Task InitializeAsync(CancellationToken ct = default)
        {
            await _initializeGate.WaitAsync(ct).ConfigureAwait(false);
            if (_hasInitialized)
            {
                _initializeGate.Release();
                return;
            }

            IsInitializing = true;
            _suppressViewOptionRefresh = true;
            try
            {
                var psSetting = await _settingsService.GetSettingAsync("PageSize", ct).ConfigureAwait(false);
                if (int.TryParse(psSetting, out var ps) && ps > 0)
                {
                    PageSize = NormalizeInteractivePageSize(ps);
                    Items.PageSize = PageSize;
                }

                var filterSetting = await _settingsService.GetSettingAsync(GetLastFilterSettingKey(), ct).ConfigureAwait(false);
                if (!string.IsNullOrWhiteSpace(filterSetting))
                    Filter = filterSetting;

                var sortSetting = await _settingsService.GetSettingAsync("LastSort", ct).ConfigureAwait(false);
                if (!string.IsNullOrWhiteSpace(sortSetting))
                {
                    var parts = sortSetting.Split('|');
                    if (parts.Length >= 2)
                    {
                        if (Enum.TryParse(parts[0], out SortField sf) && Enum.TryParse(parts[1], out SortDirection sd))
                        {
                            var opt = SortOptions.FirstOrDefault(o => o.Field == sf && o.Direction == sd);
                            if (opt != default)
                                SelectedSortOption = opt;
                        }
                    }
                }

                var vis = await _settingsService.GetItemDetailVisibilityAsync(ct).ConfigureAwait(false);
                var complete = Enum.GetValues<ItemDetailField>().ToDictionary(f => f, f => vis.TryGetValue(f, out var v) ? v : true);
                VisibleFields = complete;
                if (vis.Count != complete.Count)
                    await _settingsService.SaveItemDetailVisibilityAsync(complete, ct).ConfigureAwait(false);
                _hasInitialized = true;
            }
            catch (OperationCanceledException)
            {
                _logger.LogDebug("Initialization canceled");
            }
            finally
            {
                _suppressViewOptionRefresh = false;
                IsInitializing = false;
                _initializeGate.Release();
            }
        }

        public async Task EnsureLoadedAsync(CancellationToken ct = default)
        {
            var timer = Stopwatch.StartNew();
            await InitializeAsync(ct).ConfigureAwait(false);
            if (Items.Count == 0 && Items.HasMoreItems)
            {
                await LoadMoreAsync(ct).ConfigureAwait(false);
                Interlocked.Exchange(ref _cacheStale, 0);
            }
            else if (Interlocked.Exchange(ref _cacheStale, 0) != 0)
                await RefreshAsync(ct).ConfigureAwait(false);

            _logger.LogInformation("Item directory ready with {ItemCount} rows in {ElapsedMilliseconds} ms", Items.Count, timer.ElapsedMilliseconds);
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
            var timer = Stopwatch.StartNew();
            try
            {
                var result = new List<ItemModel>();
                var pageInfo = new ItemPage(page, PageSize);
                var source = string.IsNullOrWhiteSpace(Filter)
                    ? _itemService.GetItemsAsync(pageInfo, SelectedSortOption.Field, SelectedSortOption.Direction, cancellationToken: ct)
                    : _itemService.SearchItemsAsync(Filter, pageInfo, SelectedSortOption.Field, SelectedSortOption.Direction, cancellationToken: ct);
                await foreach (var item in source.WithCancellation(ct).ConfigureAwait(false))
                {
                    result.Add(item);
                }
                _logger.LogDebug("Item query page {PageNumber} returned {ItemCount} rows in {ElapsedMilliseconds} ms", page, result.Count, timer.ElapsedMilliseconds);
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
                using var linked = CancellationTokenSource.CreateLinkedTokenSource(ct, _loadCts.Token);
                await Items.LoadMoreAsync(linked.Token).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                _logger.LogDebug("Incremental load canceled");
            }
            catch (Exception ex)
            {
                await ClearItemsAfterLoadMoreFailureAsync(ex).ConfigureAwait(false);
            }
        }

        public async Task RefreshAsync(CancellationToken ct = default)
        {
            _filterCts.Cancel();
            _filterCts.Dispose();
            _filterCts = new CancellationTokenSource();
            _loadCts.Cancel();
            _loadCts.Dispose();
            _loadCts = new CancellationTokenSource();

            using var linked = CancellationTokenSource.CreateLinkedTokenSource(ct, _loadCts.Token);
            var firstPage = await LoadPageAsync(1, linked.Token).ConfigureAwait(false);
            await InvokeOnUiThreadAsync(() =>
            {
                var selectedId = SelectedItem?.ItemID;
                Items.ResetWith(firstPage);
                SelectedItem = selectedId.HasValue
                    ? Items.FirstOrDefault(item => item.ItemID == selectedId.Value)
                    : null;
            }).ConfigureAwait(false);
            Interlocked.Exchange(ref _cacheStale, 0);
        }

        private Task StartFilterAsync()
        {
            _filterCts.Cancel();
            _filterCts.Dispose();
            _filterCts = new CancellationTokenSource();
            _loadCts.Cancel();
            _loadCts.Dispose();
            _loadCts = new CancellationTokenSource();
            return ApplyFilterAsync(_filterCts.Token);
        }

        partial void OnFilterChanged(string value)
        {
            if (_suppressViewOptionRefresh)
                return;

            _ = StartFilterAsync();
        }

        private async Task ApplyFilterAsync(CancellationToken token)
        {
            try
            {
                await Task.Delay(300, token).ConfigureAwait(false);
                using var linked = CancellationTokenSource.CreateLinkedTokenSource(token, _loadCts.Token);
                var firstPage = await LoadPageAsync(1, linked.Token).ConfigureAwait(false);
                InvokeOnUiThread(() => Items.ResetWith(firstPage));
                await _settingsService.SaveSettingAsync(GetLastFilterSettingKey(), Filter, token).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                _logger.LogDebug("Filter application canceled");
            }
            catch (Exception ex)
            {
                await ClearItemsAfterViewOptionFailureAsync("filter", ex).ConfigureAwait(false);
            }
        }

        private void OnSteadyExceeded(object? sender, EventArgs e) =>
            InvokeOnUiThread(() => Items.TrimToWindow(PageSize * 3));

        private void OnPeakExceeded(object? sender, EventArgs e) =>
            InvokeOnUiThread(Items.Reset);

        partial void OnSelectedSortOptionChanged(SortOption value)
        {
            if (_suppressViewOptionRefresh)
                return;

            _ = ApplySortAsync(value);
        }

        private async Task ApplySortAsync(SortOption value)
        {
            try
            {
                var firstPage = await LoadPageAsync(1, _loadCts.Token).ConfigureAwait(false);
                InvokeOnUiThread(() => Items.ResetWith(firstPage));
                await _settingsService.SaveSettingAsync("LastSort", $"{value.Field}|{value.Direction}").ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                _logger.LogDebug("Sort application canceled");
            }
            catch (Exception ex)
            {
                await ClearItemsAfterViewOptionFailureAsync("sort", ex).ConfigureAwait(false);
            }
        }

        partial void OnPageSizeChanged(int value)
        {
            var normalized = NormalizeInteractivePageSize(value);
            if (normalized != value)
            {
                PageSize = normalized;
                return;
            }

            if (_suppressViewOptionRefresh)
                return;

            _ = ApplyPageSizeAsync(value);
        }

        private static int NormalizeInteractivePageSize(int value) =>
            Math.Clamp(value, MinInteractivePageSize, MaxInteractivePageSize);

        private async Task ApplyPageSizeAsync(int value)
        {
            try
            {
                Items.PageSize = value;
                InvokeOnUiThread(() => Items.TrimToWindow(value * 3));
                var firstPage = await LoadPageAsync(1, _loadCts.Token).ConfigureAwait(false);
                InvokeOnUiThread(() => Items.ResetWith(firstPage));
                await _settingsService.SaveSettingAsync("PageSize", value.ToString()).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                _logger.LogDebug("Page size application canceled");
            }
            catch (Exception ex)
            {
                await ClearItemsAfterViewOptionFailureAsync("page size", ex).ConfigureAwait(false);
            }
        }

        private async Task ClearItemsAfterViewOptionFailureAsync(string optionName, Exception ex)
        {
            _logger.LogError(ex, "Failed to apply incremental item {OptionName}", optionName);
            await InvokeOnUiThreadAsync(() =>
            {
                Items.Reset();
                SelectedItem = null;
            }).ConfigureAwait(false);
            await _dialogService.ShowInfoAsync($"Failed to apply item {optionName}: {ex.Message} Visible item rows were cleared until reload succeeds.", "Error").ConfigureAwait(false);
        }

        private async Task ClearItemsAfterLoadMoreFailureAsync(Exception ex)
        {
            _logger.LogError(ex, "Failed to load more incremental item rows");
            await InvokeOnUiThreadAsync(() =>
            {
                Items.Reset();
                SelectedItem = null;
            }).ConfigureAwait(false);
            await _dialogService.ShowInfoAsync($"Failed to load more items: {ex.Message} Visible item rows were cleared until reload succeeds.", "Error").ConfigureAwait(false);
        }

        private async Task<bool> RefreshItemsAfterMutationFailureAsync(int? preferredItemId, CancellationToken cancellationToken)
        {
            try
            {
                var firstPage = await LoadPageAsync(1, cancellationToken).ConfigureAwait(false);
                await InvokeOnUiThreadAsync(() =>
                {
                    Items.ResetWith(firstPage);
                    SelectedItem = preferredItemId.HasValue
                        ? Items.FirstOrDefault(item => item.ItemID == preferredItemId.Value)
                        : null;
                }).ConfigureAwait(false);
                return true;
            }
            catch (OperationCanceledException)
            {
                _logger.LogDebug("Item mutation recovery refresh canceled");
                throw;
            }
            catch (Exception refreshEx)
            {
                _logger.LogError(refreshEx, "Failed to refresh incremental item list after mutation failure");
                await InvokeOnUiThreadAsync(() =>
                {
                    Items.Reset();
                    SelectedItem = null;
                }).ConfigureAwait(false);
                return false;
            }
        }

        private static string AppendItemMutationRefreshMessage(string message, bool refreshed) => refreshed
            ? $"{message} The item list has been refreshed in case saved state changed before the failure."
            : $"{message} The item list could not be refreshed, so visible item rows were cleared until reload succeeds.";

        private string GetLastFilterSettingKey()
        {
            var user = _userContext?.CurrentUser;
            if (user == null)
                return "LastFilter";

            if (user.UserID > 0)
                return $"LastFilter.User.{user.UserID}";

            var userName = user.UserName?.Trim();
            return string.IsNullOrWhiteSpace(userName)
                ? "LastFilter"
                : $"LastFilter.User.{userName}";
        }

        private void Item_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (sender is not ItemModel item) return;
            if (e.PropertyName == nameof(ItemModel.ImagePath) || e.PropertyName == nameof(ItemModel.ItemNumber))
            {
                item.Thumbnail = null;
                RefreshMissingImageCount();
                QueueThumbnailLoad(item);
            }

            if (e.PropertyName == nameof(ItemModel.QuantityOnHand) || e.PropertyName == nameof(ItemModel.Location) || e.PropertyName == nameof(ItemModel.Price))
            {
                if (!_pendingEdits.Contains(item))
                {
                    _pendingEdits.Add(item);
                    OnPropertyChanged(nameof(PendingEdits));
                }
            }
        }

        private void Items_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            var currentItems = Items.ToHashSet();
            foreach (var removed in _observedItems.Where(item => !currentItems.Contains(item)).ToArray())
            {
                removed.PropertyChanged -= Item_PropertyChanged;
                _observedItems.Remove(removed);
            }

            foreach (var added in currentItems.Where(item => !_observedItems.Contains(item)))
            {
                added.PropertyChanged += Item_PropertyChanged;
                _observedItems.Add(added);
                QueueThumbnailLoad(added);
            }

            RefreshMissingImageCount();
        }

        private void Items_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(IncrementalLoadingCollection<ItemModel>.IsLoading))
                OnPropertyChanged(nameof(IsDirectoryBusy));
        }

        public Task WaitForImageStatusRefreshAsync() => _imageStatusRefreshTask;

        private void RefreshMissingImageCount()
        {
            _imageScanCts.Cancel();
            _imageScanCts.Dispose();
            _imageScanCts = new CancellationTokenSource();
            var token = _imageScanCts.Token;
            var snapshot = Items.Select(item => new ImageStatusSnapshot(item.ImagePath, item.ItemNumber)).ToArray();
            _imageStatusRefreshTask = RefreshMissingImageCountAsync(snapshot, token);
        }

        private void OnCurrentUserChanged(object? sender, User? user)
        {
            Interlocked.Exchange(ref _cacheStale, 1);
            _hasInitialized = false;
            InvokeOnUiThread(() =>
            {
                _suppressViewOptionRefresh = true;
                try
                {
                    Filter = string.Empty;
                    Items.Reset();
                    SelectedItem = null;
                }
                finally
                {
                    _suppressViewOptionRefresh = false;
                }
            });
        }

        private async Task RefreshMissingImageCountAsync(ImageStatusSnapshot[] snapshot, CancellationToken token)
        {
            var timer = Stopwatch.StartNew();
            try
            {
                var missing = await Task.Run(() => snapshot.Count(ItemIsMissingImage), token).ConfigureAwait(false);
                token.ThrowIfCancellationRequested();
                await InvokeOnUiThreadAsync(() => MissingImageCount = missing).ConfigureAwait(false);
                _logger.LogInformation("Scanned {ItemCount} item image paths in {ElapsedMilliseconds} ms", snapshot.Length, timer.ElapsedMilliseconds);
            }
            catch (OperationCanceledException)
            {
            }
        }

        private static bool ItemIsMissingImage(ImageStatusSnapshot item)
        {
            if (HasUsableImagePath(item.ImagePath))
                return false;

            return !HasItemNumberImageFallback(item.ItemNumber);
        }

        private void QueueThumbnailLoad(ItemModel item)
        {
            if (_thumbnailCache is null || item.Thumbnail is not null || _thumbnailCts.IsCancellationRequested)
                return;

            _ = LoadThumbnailAsync(item, _thumbnailCts.Token);
        }

        private async Task LoadThumbnailAsync(ItemModel item, CancellationToken token)
        {
            var timer = Stopwatch.StartNew();
            try
            {
                var thumbnail = await _thumbnailCache!.GetAsync(item, token).ConfigureAwait(false);
                if (thumbnail is null || token.IsCancellationRequested)
                    return;
                await InvokeOnUiThreadAsync(() => item.Thumbnail = thumbnail).ConfigureAwait(false);
                if (timer.ElapsedMilliseconds >= 100)
                    _logger.LogInformation("Slow item thumbnail {ItemId} completed in {ElapsedMilliseconds} ms", item.ItemID, timer.ElapsedMilliseconds);
                else
                    _logger.LogDebug("Item {ItemId} thumbnail ready in {ElapsedMilliseconds} ms", item.ItemID, timer.ElapsedMilliseconds);
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex, "Unable to load thumbnail for item {ItemId}", item.ItemID);
            }
        }

        private void OnCollectionBatchApplied(int itemCount, TimeSpan elapsed)
        {
            _logger.LogInformation("Applied {ItemCount} item rows to the directory in one UI batch in {ElapsedMilliseconds} ms", itemCount, elapsed.TotalMilliseconds);
        }

        private sealed record ImageStatusSnapshot(string? ImagePath, string? ItemNumber);

        private static bool HasUsableImagePath(string? imagePath)
        {
            if (string.IsNullOrWhiteSpace(imagePath))
                return false;

            var resolved = PathHelper.GetAbsolutePath(imagePath, false) ?? AppAssetHelper.ResolveAssetPath(imagePath);
            return !string.IsNullOrWhiteSpace(resolved) && File.Exists(resolved);
        }

        private static bool HasItemNumberImageFallback(string? itemNumber)
        {
            if (string.IsNullOrWhiteSpace(itemNumber))
                return false;

            var trimmed = itemNumber.Trim();
            foreach (var extension in ItemImageExtensions)
            {
                var candidate = Path.Combine(AppAssetHelper.AssetsDirectoryName, AppAssetHelper.ItemImagesFolder, trimmed + extension);
                var resolved = PathHelper.GetAbsolutePath(candidate, false) ?? AppAssetHelper.ResolveAssetPath(candidate);
                if (!string.IsNullOrWhiteSpace(resolved) && File.Exists(resolved))
                    return true;
            }

            return false;
        }

        private async Task EditItemAsync(CancellationToken ct)
        {
            var selected = SelectedItem;
            if (selected == null) return;
            ItemModel? updated;
            try
            {
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
                    CheckedInBy = selected.CheckedInBy,
                    CheckedInTime = selected.CheckedInTime,
                    ImagePath = selected.ImagePath,
                    Price = selected.Price,
                    UpdatedAt = selected.UpdatedAt
                };
                updated = await _dialogService.ShowEditItemDialogAsync(clone).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                _logger.LogDebug("Edit item dialog canceled");
                return;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to open edit item dialog for item {ItemID}", selected.ItemID);
                await _dialogService.ShowInfoAsync($"Failed to open edit item dialog: {ex.Message}", "Error").ConfigureAwait(false);
                return;
            }
            if (updated == null) return;
            try
            {
                await _itemService.UpdateItemAsync(updated, ct).ConfigureAwait(false);
                await InvokeOnUiThreadAsync(() => ApplySuccessfulItemEdit(selected, updated)).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                _logger.LogDebug("Edit item canceled");
            }
            catch (Exception ex)
            {
                var refreshed = await RefreshItemsAfterMutationFailureAsync(updated.ItemID, ct).ConfigureAwait(false);
                await _dialogService.ShowInfoAsync($"Failed to update {LabelProvider.Instance.ItemLabelSingular.ToLower()}: {AppendItemMutationRefreshMessage(ex.Message, refreshed)}", "Error").ConfigureAwait(false);
            }
        }

        private void ApplySuccessfulItemEdit(ItemModel original, ItemModel updated)
        {
            var index = Items.IndexOf(original);
            if (index < 0)
                index = Items.ToList().FindIndex(item => item.ItemID == updated.ItemID);

            if (index >= 0)
                Items[index] = updated;

            _pendingEdits.Remove(original);
            _pendingEdits.Remove(updated);
            OnPropertyChanged(nameof(PendingEdits));
            SelectedItem = updated;
            RefreshMissingImageCount();
        }

        private void ViewDetails()
        {
            var item = SelectedItem;
            if (item == null) return;
            try
            {
                _dialogService.ShowItemDetails(item);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to open item details for item {ItemID}", item.ItemID);
                _dialogService.ShowInfo($"Failed to open item details: {ex.Message}", "Error");
            }
        }

        private async Task OpenRentalHistoryAsync(CancellationToken ct)
        {
            var item = SelectedItem;
            if (item == null) return;
            try
            {
                var history = await _rentalService.GetRentalHistoryForItemAsync(item.ItemID).ConfigureAwait(false);
                _dialogService.ShowRentalHistory(item, history);
            }
            catch (OperationCanceledException)
            {
                _logger.LogDebug("Rental history load canceled");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to open incremental rental history for item {ItemID}", item.ItemID);
                await _dialogService.ShowInfoAsync($"Failed to load rental history: {ex.Message}", "Error").ConfigureAwait(false);
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
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to open new item dialog");
                await _dialogService.ShowInfoAsync($"Failed to open new item dialog: {ex.Message}", "Error").ConfigureAwait(false);
                return;
            }
            if (item == null) return;
            try
            {
                await _itemService.AddItemAsync(item, ct).ConfigureAwait(false);
                InvokeOnUiThread(Items.Reset);
                await Items.LoadMoreAsync(ct).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                _logger.LogDebug("New item creation canceled");
            }
            catch (Exception ex)
            {
                var refreshed = await RefreshItemsAfterMutationFailureAsync(item.ItemID > 0 ? item.ItemID : null, ct).ConfigureAwait(false);
                await _dialogService.ShowInfoAsync($"Failed to create {LabelProvider.Instance.ItemLabelSingular.ToLower()}: {AppendItemMutationRefreshMessage(ex.Message, refreshed)}", "Error").ConfigureAwait(false);
            }
        }

        private async Task OpenMobileCaptureAsync(CancellationToken ct)
        {
            if (_mobileCaptureService == null)
            {
                await _dialogService.ShowInfoAsync("Mobile capture is not available in this application session.", "Mobile Capture").ConfigureAwait(false);
                return;
            }

            try
            {
                var session = await _mobileCaptureService.StartSessionAsync(ct).ConfigureAwait(false);
                await InvokeOnUiThreadAsync(() =>
                {
                    var window = new MobileCaptureWindow(session);
                    try { window.Owner = Application.Current?.MainWindow; }
                    catch (Exception ex) { _logger.LogError(ex, "Failed to set owner for MobileCaptureWindow"); }
                    window.ShowDialog();
                }).ConfigureAwait(false);

                InvokeOnUiThread(Items.Reset);
                await Items.LoadMoreAsync(ct).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                _logger.LogDebug("Mobile capture start canceled");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to start mobile capture");
                await _dialogService.ShowInfoAsync($"Failed to start mobile capture: {ex.Message}", "Mobile Capture").ConfigureAwait(false);
            }
        }

        private async Task DeleteItemsAsync(IList? items, CancellationToken ct)
        {
            if (items == null || items.Count == 0) return;
            var firstItem = items[0] as ItemModel;
            var message = items.Count == 1
                ? (firstItem != null
                    ? $"Delete {LabelProvider.Instance.ItemLabelSingular.ToLower()} '{firstItem.Name}'?"
                    : "Delete selected item?")
                : $"Delete {items.Count} {LabelProvider.Instance.ItemLabelPlural.ToLower()}?";
            var confirm = await _dialogService.ShowConfirmationAsync(message, "Confirm Delete").ConfigureAwait(false);
            if (!confirm) return;
            var toRemove = items.Cast<ItemModel>().ToList();
            try
            {
                foreach (var item in toRemove)
                {
                    await _itemService.DeleteItemAsync(item.ItemID, ct).ConfigureAwait(false);
                    await InvokeOnUiThreadAsync(() => Items.Remove(item)).ConfigureAwait(false);
                }
                await InvokeOnUiThreadAsync(() =>
                {
                    if (SelectedItem != null && toRemove.Contains(SelectedItem))
                        SelectedItem = null;
                }).ConfigureAwait(false);
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
                var refreshed = await RefreshItemsAfterMutationFailureAsync(toRemove.FirstOrDefault()?.ItemID, ct).ConfigureAwait(false);
                await _dialogService.ShowInfoAsync($"Failed to delete {LabelProvider.Instance.ItemLabelPlural.ToLower()}: {AppendItemMutationRefreshMessage(ex.Message, refreshed)}", "Error").ConfigureAwait(false);
            }
        }

        private Task DeleteSelectedItemAsync(ItemModel? item, CancellationToken ct)
        {
            if (item == null)
                return Task.CompletedTask;

            return DeleteItemsAsync(new List<ItemModel> { item }, ct);
        }

        private async Task CommitChangesAsync(CancellationToken ct)
        {
            if (_pendingEdits.Count == 0) return;
            var edits = _pendingEdits.ToList();
            try
            {
                await _itemService.SaveChangesAsync(edits, ct).ConfigureAwait(false);
                _pendingEdits.Clear();
                OnPropertyChanged(nameof(PendingEdits));
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
                var refreshed = await RefreshItemsAfterMutationFailureAsync(edits.FirstOrDefault()?.ItemID, ct).ConfigureAwait(false);
                if (refreshed)
                {
                    _pendingEdits.Clear();
                    OnPropertyChanged(nameof(PendingEdits));
                }
                await _dialogService.ShowInfoAsync($"Failed to save changes: {AppendItemMutationRefreshMessage(ex.Message, refreshed)}", "Error").ConfigureAwait(false);
            }
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            _memoryBudget.SteadyExceeded -= OnSteadyExceeded;
            _memoryBudget.PeakExceeded -= OnPeakExceeded;
            _settingsService.ItemDetailVisibilityChanged -= OnItemDetailVisibilityChanged;
            if (_userContext is not null)
                _userContext.UserChanged -= OnCurrentUserChanged;
            Items.CollectionChanged -= Items_CollectionChanged;
            ((INotifyPropertyChanged)Items).PropertyChanged -= Items_PropertyChanged;
            foreach (var item in _observedItems)
                item.PropertyChanged -= Item_PropertyChanged;
            _observedItems.Clear();
            WeakReferenceMessenger.Default.UnregisterAll(this);
            var dispatcher = Application.Current?.Dispatcher;
            if (dispatcher == null || dispatcher.CheckAccess())
                Items.Reset();
            else
                dispatcher.Invoke(Items.Reset);
            _filterCts.Cancel();
            _filterCts.Dispose();
            _loadCts.Cancel();
            _loadCts.Dispose();
            _imageScanCts.Cancel();
            _imageScanCts.Dispose();
            _thumbnailCts.Cancel();
            _thumbnailCts.Dispose();
            _initializeGate.Dispose();
        }

        private static void InvokeOnUiThread(Action action)
        {
            var dispatcher = Application.Current?.Dispatcher;
            if (dispatcher == null || dispatcher.CheckAccess())
                action();
            else
                dispatcher.Invoke(action);
        }

        private static Task InvokeOnUiThreadAsync(Action action)
        {
            var dispatcher = Application.Current?.Dispatcher;
            if (dispatcher == null || dispatcher.CheckAccess())
            {
                action();
                return Task.CompletedTask;
            }

            return dispatcher.InvokeAsync(action).Task;
        }
    }

    public class IncrementalLoadingCollection<T> : ObservableCollection<T>
    {
        private readonly Func<int, CancellationToken, Task<IList<T>>> _loader;
        private readonly Action<int, TimeSpan>? _batchApplied;
        private int _pageSize;
        private int _page;
        private bool _deferNotifications;
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

        public IncrementalLoadingCollection(Func<int, CancellationToken, Task<IList<T>>> loader, int pageSize, Action<int, TimeSpan>? batchApplied = null)
        {
            _loader = loader;
            _pageSize = pageSize;
            _batchApplied = batchApplied;
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
                var result = items.ToList();
                var dispatcher = Application.Current?.Dispatcher;
                if (dispatcher == null || dispatcher.CheckAccess())
                {
                    AddRange(result);
                }
                else
                {
                    await dispatcher.InvokeAsync(() => AddRange(result));
                }
                _page = next;
                if (result.Count < _pageSize)
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

        public void ResetWith(IList<T> items)
        {
            ReplaceAll(items);
            _page = items.Count > 0 ? 1 : 0;
            HasMoreItems = items.Count == _pageSize;
        }

        public void AddRange(IEnumerable<T> items)
        {
            var batch = items as IList<T> ?? items.ToList();
            if (batch.Count == 0)
                return;

            var timer = Stopwatch.StartNew();
            _deferNotifications = true;
            try
            {
                foreach (var item in batch)
                    Items.Add(item);
            }
            finally
            {
                _deferNotifications = false;
            }
            RaiseResetNotifications();
            _batchApplied?.Invoke(batch.Count, timer.Elapsed);
        }

        private void ReplaceAll(IEnumerable<T> items)
        {
            var batch = items as IList<T> ?? items.ToList();
            var timer = Stopwatch.StartNew();
            _deferNotifications = true;
            try
            {
                Items.Clear();
                foreach (var item in batch)
                    Items.Add(item);
            }
            finally
            {
                _deferNotifications = false;
            }
            RaiseResetNotifications();
            _batchApplied?.Invoke(batch.Count, timer.Elapsed);
        }

        protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            if (!_deferNotifications)
                base.OnCollectionChanged(e);
        }

        protected override void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            if (!_deferNotifications)
                base.OnPropertyChanged(e);
        }

        private void RaiseResetNotifications()
        {
            base.OnPropertyChanged(new PropertyChangedEventArgs(nameof(Count)));
            base.OnPropertyChanged(new PropertyChangedEventArgs("Item[]"));
            base.OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        public void TrimToWindow(int max)
        {
            if (Count <= max) return;
            while (Count > max && Count > 0)
                RemoveAt(0);
        }
    }
}
