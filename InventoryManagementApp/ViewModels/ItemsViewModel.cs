using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using InventoryManagementApp.Data;
using InventoryManagementApp.Interfaces;
using InventoryManagementApp.Models.Domain;
using InventoryManagementApp.Utilities;
using InventoryManagementApp.Utilities.Extensions;

namespace InventoryManagementApp.ViewModels
{
    public class ItemsViewModel : ObservableObject, IDisposable
    {
        private readonly IItemService _itemService;
        private readonly MemoryBudget _memoryBudget;
        private readonly ObservableCollection<ItemModel> _items = new();
        public ReadOnlyObservableCollection<ItemModel> Items { get; }
        private int _currentPage = 1;
        private const int PageSize = 200;

        public ItemsViewModel(IItemService itemService, MemoryBudget memoryBudget)
        {
            _itemService = itemService;
            _memoryBudget = memoryBudget;
            Items = new(_items);
            _memoryBudget.ThresholdExceeded += OnThresholdExceeded;
        }

        public async Task LoadPageAsync(int page, CancellationToken cancellationToken = default)
        {
            _currentPage = page;
            var list = new List<ItemModel>();
            await foreach (var item in _itemService.GetItemsAsync(new ItemPage(page, PageSize), cancellationToken))
                list.Add(item);
            _items.AddRange(list);
            TrimToWindow();
        }

        private void OnThresholdExceeded(object? sender, EventArgs e) => TrimToWindow();

        private void TrimToWindow()
        {
            var max = PageSize * 3;
            if (_items.Count <= max) return;
            var start = Math.Max(0, (_currentPage - 2) * PageSize);
            var trimmed = _items.Skip(start).Take(max).ToList();
            _items.ReplaceRange(trimmed);
        }

        public void Dispose()
        {
            _memoryBudget.ThresholdExceeded -= OnThresholdExceeded;
        }
    }
}
