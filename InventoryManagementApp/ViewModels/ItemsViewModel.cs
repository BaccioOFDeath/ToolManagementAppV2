using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using InventoryManagementApp.Data;
using InventoryManagementApp.Interfaces;
using InventoryManagementApp.Models.Domain;
using InventoryManagementApp.Utilities;

namespace InventoryManagementApp.ViewModels
{
    public partial class ItemsViewModel : ObservableObject, IDisposable
    {
        private readonly IItemService _itemService;
        private readonly MemoryBudget _memoryBudget;
        private CancellationTokenSource _filterCts = new();

        private const int PageSize = 200;
        public IncrementalLoadingCollection<ItemModel> Items { get; }

        [ObservableProperty]
        private ItemModel? selectedItem;

        [ObservableProperty]
        private string filter = string.Empty;

        public ItemsViewModel(IItemService itemService, MemoryBudget memoryBudget)
        {
            _itemService = itemService;
            _memoryBudget = memoryBudget;
            Items = new IncrementalLoadingCollection<ItemModel>(LoadPageAsync, PageSize);
            _memoryBudget.ThresholdExceeded += OnThresholdExceeded;
        }

        private async Task<IList<ItemModel>> LoadPageAsync(int page, CancellationToken ct)
        {
            var result = new List<ItemModel>();
            var pageInfo = new ItemPage(page, PageSize);
            var source = string.IsNullOrWhiteSpace(Filter)
                ? _itemService.GetItemsAsync(pageInfo, ct)
                : _itemService.SearchItemsAsync(Filter, pageInfo, ct);
            await foreach (var item in source.ConfigureAwait(false))
                result.Add(item);
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

        public void Dispose()
        {
            _memoryBudget.ThresholdExceeded -= OnThresholdExceeded;
            _filterCts.Cancel();
            _filterCts.Dispose();
        }
    }

    public class IncrementalLoadingCollection<T> : ObservableCollection<T>
    {
        private readonly Func<int, CancellationToken, Task<IList<T>>> _loader;
        private readonly int _pageSize;
        private int _page;
        public bool HasMoreItems { get; private set; } = true;

        public IncrementalLoadingCollection(Func<int, CancellationToken, Task<IList<T>>> loader, int pageSize)
        {
            _loader = loader;
            _pageSize = pageSize;
        }

        public async Task LoadMoreAsync(CancellationToken ct = default)
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
