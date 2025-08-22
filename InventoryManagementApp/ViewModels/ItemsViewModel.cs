using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using InventoryManagementApp.Data;
using InventoryManagementApp.Interfaces;
using InventoryManagementApp.Models.Domain;

namespace InventoryManagementApp.ViewModels
{
    public partial class ItemsViewModel : ObservableObject
    {
        private readonly IItemService _itemService;
        private readonly TimeSpan _filterDelay = TimeSpan.FromMilliseconds(300);
        private CancellationTokenSource _filterCts = new();

        public IncrementalLoadingCollection<ItemModel> Items { get; }

        [ObservableProperty]
        private ItemModel? _selectedItem;

        [ObservableProperty]
        private string _filter = string.Empty;

        public IAsyncRelayCommand LoadMoreItemsCommand { get; }
        public IAsyncRelayCommand OpenRentalsCommand { get; }
        public IAsyncRelayCommand NewItemCommand { get; }
        public IAsyncRelayCommand EditItemCommand { get; }
        public IAsyncRelayCommand<IList> DeleteItemsCommand { get; }
        public IRelayCommand ViewDetailsCommand { get; }
        public IAsyncRelayCommand OpenRentalHistoryCommand { get; }

        public ItemsViewModel(IItemService itemService)
        {
            _itemService = itemService;
            Items = new IncrementalLoadingCollection<ItemModel>(200, LoadPageAsync);
            LoadMoreItemsCommand = new AsyncRelayCommand(ct => Items.LoadMoreAsync(ct));
            OpenRentalsCommand = new AsyncRelayCommand(ct => Task.CompletedTask, () => SelectedItem != null);
            NewItemCommand = new AsyncRelayCommand(ct => Task.CompletedTask);
            EditItemCommand = new AsyncRelayCommand(ct => Task.CompletedTask, () => SelectedItem != null);
            DeleteItemsCommand = new AsyncRelayCommand<IList>(_ => Task.CompletedTask);
            ViewDetailsCommand = new RelayCommand(() => { }, () => SelectedItem != null);
            OpenRentalHistoryCommand = new AsyncRelayCommand(ct => Task.CompletedTask, () => SelectedItem != null);
        }

        partial void OnSelectedItemChanged(ItemModel? value)
        {
            (OpenRentalsCommand as AsyncRelayCommand)?.NotifyCanExecuteChanged();
            (EditItemCommand as AsyncRelayCommand)?.NotifyCanExecuteChanged();
            (ViewDetailsCommand as RelayCommand)?.NotifyCanExecuteChanged();
            (OpenRentalHistoryCommand as AsyncRelayCommand)?.NotifyCanExecuteChanged();
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
                await Task.Delay(_filterDelay, token).ConfigureAwait(true);
                Items.Reset();
                await Items.LoadMoreAsync(token).ConfigureAwait(true);
            }
            catch (OperationCanceledException) { }
        }

        private async Task<IReadOnlyList<ItemModel>> LoadPageAsync(int pageNumber, int pageSize, CancellationToken ct)
        {
            var page = new ItemPage(pageNumber, pageSize);
            var list = new List<ItemModel>();
            var source = string.IsNullOrWhiteSpace(Filter)
                ? _itemService.GetItemsAsync(page, ct)
                : _itemService.SearchItemsAsync(Filter, page, ct);

            await foreach (var item in source.WithCancellation(ct).ConfigureAwait(false))
                list.Add(item);

            return list;
        }

        public Task LoadMoreAsync(CancellationToken ct = default) => Items.LoadMoreAsync(ct);
    }

    public class IncrementalLoadingCollection<T> : ObservableCollection<T>
    {
        private readonly Func<int, int, CancellationToken, Task<IReadOnlyList<T>>> _loader;
        private bool _isLoading;
        private int _currentPage;
        private readonly int _pageSize;

        public IncrementalLoadingCollection(int pageSize, Func<int, int, CancellationToken, Task<IReadOnlyList<T>>> loader)
        {
            _pageSize = pageSize;
            _loader = loader;
        }

        public async Task LoadMoreAsync(CancellationToken ct = default)
        {
            if (_isLoading) return;
            _isLoading = true;
            try
            {
                var items = await _loader(_currentPage + 1, _pageSize, ct).ConfigureAwait(true);
                foreach (var item in items)
                    Add(item);
                if (items.Count > 0)
                    _currentPage++;
            }
            finally
            {
                _isLoading = false;
            }
        }

        public void Reset()
        {
            _currentPage = 0;
            Clear();
        }
    }
}
