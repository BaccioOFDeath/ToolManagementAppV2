using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using InventoryManagementApp.Data;
using InventoryManagementApp.Interfaces;
using InventoryManagementApp.Models.Domain;
using InventoryManagementApp.Utilities;

namespace InventoryManagementApp.ViewModels
{
    public partial class ItemsViewModel : ObservableObject, IDisposable
    {
        private readonly IItemService _itemService;
        private readonly IItemRepository _itemRepository;
        private readonly MemoryBudget _memoryBudget;
        private CancellationTokenSource _filterCts = new();
        private bool _disposed;
        private readonly List<ItemModel> _pendingEdits = new();

        private const int PageSize = 200;
        public IncrementalLoadingCollection<ItemModel> Items { get; }

        public IRelayCommand EditItemCommand { get; }
        public IRelayCommand ViewDetailsCommand { get; }
        public IRelayCommand OpenRentalHistoryCommand { get; }
        public IRelayCommand NewItemCommand { get; }
        public IAsyncRelayCommand SaveChangesCommand { get; }

        public IReadOnlyCollection<ItemModel> PendingEdits => _pendingEdits;

        [ObservableProperty]
        private ItemModel? selectedItem;

        [ObservableProperty]
        private string filter = string.Empty;

        public ItemsViewModel(IItemService itemService, IItemRepository itemRepository, MemoryBudget memoryBudget)
        {
            _itemService = itemService;
            _itemRepository = itemRepository;
            _memoryBudget = memoryBudget;
            Items = new IncrementalLoadingCollection<ItemModel>(LoadPageAsync, PageSize);
            _memoryBudget.ThresholdExceeded += OnThresholdExceeded;

            EditItemCommand = new RelayCommand(() => { /* Edit item placeholder */ });
            ViewDetailsCommand = new RelayCommand(() => { /* View details placeholder */ });
            OpenRentalHistoryCommand = new RelayCommand(() => { /* Open rental history placeholder */ });
            NewItemCommand = new RelayCommand(() => { /* New item placeholder */ });
            SaveChangesCommand = new AsyncRelayCommand(ct => SaveChangesAsync(ct));
        }

        private async Task<IList<ItemModel>> LoadPageAsync(int page, CancellationToken ct)
        {
            var result = new List<ItemModel>();
            var pageInfo = new ItemPage(page, PageSize);
            var source = string.IsNullOrWhiteSpace(Filter)
                ? _itemService.GetItemsAsync(pageInfo, ct)
                : _itemService.SearchItemsAsync(Filter, pageInfo, ct);
            await foreach (var item in source.ConfigureAwait(false))
            {
                item.PropertyChanged += Item_PropertyChanged;
                result.Add(item);
            }
            return result;
        }

        public Task LoadMoreAsync(CancellationToken ct = default) => Items.LoadMoreAsync(ct);

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
            }
            catch (TaskCanceledException)
            {
                return;
            }
            Items.Reset();
            await Items.LoadMoreAsync(token).ConfigureAwait(false);
        }

        private void OnThresholdExceeded(object? sender, EventArgs e) => Items.TrimToWindow(PageSize * 3);

        private void Item_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (sender is not ItemModel item) return;
            if (e.PropertyName == nameof(ItemModel.QuantityOnHand) || e.PropertyName == nameof(ItemModel.Location) || e.PropertyName == nameof(ItemModel.Price))
            {
                if (!_pendingEdits.Contains(item))
                    _pendingEdits.Add(item);
            }
        }

        private async Task SaveChangesAsync(CancellationToken ct)
        {
            if (_pendingEdits.Count == 0) return;
            await _itemRepository.SaveChangesAsync(_pendingEdits, ct).ConfigureAwait(false);
            _pendingEdits.Clear();
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            _memoryBudget.ThresholdExceeded -= OnThresholdExceeded;
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
        private readonly int _pageSize;
        private int _page;
        private readonly SemaphoreSlim _gate = new(1, 1);
        public bool HasMoreItems { get; private set; } = true;

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
