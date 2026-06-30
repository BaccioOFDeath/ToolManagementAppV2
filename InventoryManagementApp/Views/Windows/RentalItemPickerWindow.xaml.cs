using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using InventoryManagementApp.Data;
using InventoryManagementApp.Interfaces;
using InventoryManagementApp.Models.Domain;

namespace InventoryManagementApp.Views.Windows
{
    public partial class RentalItemPickerWindow : Window
    {
        readonly IItemService _itemService;
        readonly int _excludedItemId;
        readonly DispatcherTimer _searchTimer;
        bool _isLoaded;

        public RentalItemPickerWindow(IItemService itemService, string title, int excludedItemId = 0)
        {
            InitializeComponent();
            _itemService = itemService;
            _excludedItemId = excludedItemId;
            _searchTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(300) };
            _searchTimer.Tick += async (_, _) =>
            {
                _searchTimer.Stop();
                await LoadItemsAsync();
            };
            Title = title;
            TitleText.Text = title;
            Loaded += async (_, _) =>
            {
                _isLoaded = true;
                SearchBox.Focus();
                await LoadItemsAsync();
            };
        }

        public ItemModel? SelectedItem { get; private set; }

        async Task LoadItemsAsync()
        {
            try
            {
                StatusText.Text = "Loading available rental items...";
                var term = SearchBox.Text?.Trim();
                var items = new List<ItemModel>();
                var page = new ItemPage(1, 100);
                var source = string.IsNullOrWhiteSpace(term)
                    ? _itemService.GetItemsAsync(page, SortField.Name, SortDirection.Ascending, isRentalItem: true)
                    : _itemService.SearchItemsAsync(term, page, SortField.Name, SortDirection.Ascending, isRentalItem: true);

                await foreach (var item in source)
                {
                    if (IsAvailableForRentalPick(item))
                        items.Add(item);
                }

                ItemsGrid.ItemsSource = items;
                StatusText.Text = items.Count == 1 ? "1 available item shown." : $"{items.Count} available items shown.";
            }
            catch (Exception ex)
            {
                ItemsGrid.ItemsSource = Array.Empty<ItemModel>();
                StatusText.Text = $"Unable to load items: {ex.Message}";
            }
        }

        private async void Find_Click(object sender, RoutedEventArgs e)
        {
            _searchTimer.Stop();
            await LoadItemsAsync();
        }

        private void SearchBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            if (!_isLoaded)
                return;

            _searchTimer.Stop();
            _searchTimer.Start();
        }

        private async void SearchBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Enter)
                return;

            e.Handled = true;
            _searchTimer.Stop();
            await LoadItemsAsync();
        }

        private void UseItem_Click(object sender, RoutedEventArgs e)
        {
            SelectCurrentItem();
        }

        private void ItemsGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            SelectCurrentItem();
        }

        void SelectCurrentItem()
        {
            if (ItemsGrid.SelectedItem is not ItemModel item)
            {
                StatusText.Text = "Select an available item first.";
                return;
            }

            SelectedItem = item;
            DialogResult = true;
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        bool IsAvailableForRentalPick(ItemModel item)
        {
            return item.ItemID != _excludedItemId
                && item.IsRentalItem
                && !item.IsIncomplete
                && !item.IsCheckedOut
                && item.QuantityOnHand > 0;
        }
    }
}
