using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using InventoryManagementApp.Data;
using InventoryManagementApp.Interfaces;
using InventoryManagementApp.Models.Domain;

namespace InventoryManagementApp.Views.Windows
{
    public partial class RentalItemPickerWindow : Window
    {
        readonly IItemService _itemService;
        readonly int _excludedItemId;

        public RentalItemPickerWindow(IItemService itemService, string title, int excludedItemId = 0)
        {
            InitializeComponent();
            _itemService = itemService;
            _excludedItemId = excludedItemId;
            Title = title;
            TitleText.Text = title;
            Loaded += async (_, _) => await LoadItemsAsync();
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
                    if (item.ItemID != _excludedItemId && item.QuantityOnHand > 0)
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
    }
}
