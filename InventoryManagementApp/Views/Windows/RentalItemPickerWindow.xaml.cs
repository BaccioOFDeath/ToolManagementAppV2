using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
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
        int _loadVersion;
        bool _isLoaded;
        bool _isLoading;

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
            Loaded += RentalItemPickerWindow_Loaded;
            Unloaded += RentalItemPickerWindow_Unloaded;
            PreviewKeyDown += RentalItemPickerWindow_PreviewKeyDown;
            UpdatePickerState(isLoading: false, visibleItemCount: 0, showEmptyState: false);
        }

        public ItemModel? SelectedItem { get; private set; }

        async void RentalItemPickerWindow_Loaded(object sender, RoutedEventArgs e)
        {
            if (_isLoaded)
                return;

            _isLoaded = true;
            SearchBox.Focus();
            await Dispatcher.InvokeAsync(() => { }, DispatcherPriority.ContextIdle);
            await LoadItemsAsync();
        }

        void RentalItemPickerWindow_Unloaded(object sender, RoutedEventArgs e)
        {
            _searchTimer.Stop();
            _loadVersion++;
            _isLoading = false;
            UpdatePickerState(isLoading: false, visibleItemCount: ItemsGrid.Items.Count, showEmptyState: false);
        }

        async Task LoadItemsAsync()
        {
            var version = Interlocked.Increment(ref _loadVersion);
            var term = SearchBox.Text?.Trim();
            UpdatePickerState(isLoading: true, visibleItemCount: ItemsGrid.Items.Count, showEmptyState: false);

            try
            {
                var items = new List<ItemModel>();
                var page = new ItemPage(1, 100);
                var source = string.IsNullOrWhiteSpace(term)
                    ? _itemService.GetItemsAsync(page, SortField.Name, SortDirection.Ascending, isRentalItem: true)
                    : _itemService.SearchItemsAsync(term, page, SortField.Name, SortDirection.Ascending, isRentalItem: true);

                await foreach (var item in source)
                {
                    if (version != _loadVersion)
                        return;

                    if (IsAvailableForRentalPick(item))
                        items.Add(item);
                }

                if (version != _loadVersion)
                    return;

                ItemsGrid.ItemsSource = items;
                ItemsGrid.SelectedItem = null;
                var showEmptyState = items.Count == 0;
                UpdatePickerState(isLoading: false, visibleItemCount: items.Count, showEmptyState: showEmptyState);
            }
            catch (Exception ex)
            {
                if (version != _loadVersion)
                    return;

                ItemsGrid.ItemsSource = Array.Empty<ItemModel>();
                ItemsGrid.SelectedItem = null;
                UpdatePickerState(isLoading: false, visibleItemCount: 0, showEmptyState: true);
                StatusText.Text = $"Unable to load items: {ex.Message}";
                EmptyStateTitle.Text = "Unable to load items";
                EmptyStateMessage.Text = "Check the inventory data source, then try Find again.";
            }
        }

        void UpdatePickerState(bool isLoading, int visibleItemCount, bool showEmptyState)
        {
            _isLoading = isLoading;
            LoadingOverlay.Visibility = isLoading ? Visibility.Visible : Visibility.Collapsed;
            EmptyStatePanel.Visibility = !isLoading && showEmptyState ? Visibility.Visible : Visibility.Collapsed;
            FindButton.IsEnabled = !isLoading;
            UseItemButton.IsEnabled = !isLoading && ItemsGrid.SelectedItem is ItemModel;
            ItemsGrid.IsEnabled = !isLoading;

            if (isLoading)
            {
                StatusText.Text = "Loading available rental items...";
                ResultSummaryText.Text = "Loading";
                return;
            }

            ResultSummaryText.Text = visibleItemCount == 1 ? "1 item" : $"{visibleItemCount} items";
            if (showEmptyState)
                StatusText.Text = "No available rental items match the current search.";
            else
                StatusText.Text = visibleItemCount == 1 ? "1 available item shown." : $"{visibleItemCount} available items shown.";

            EmptyStateTitle.Text = "No available items";
            EmptyStateMessage.Text = "Try a different search term or clear the search to browse available rental inventory.";
        }

        private async void Find_Click(object sender, RoutedEventArgs e)
        {
            if (_isLoading)
                return;

            _searchTimer.Stop();
            await LoadItemsAsync();
        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!_isLoaded)
                return;

            _searchTimer.Stop();
            if (_isLoading)
                _loadVersion++;

            _searchTimer.Start();
        }

        private async void SearchBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Enter)
                return;

            e.Handled = true;
            if (_isLoading)
                return;

            _searchTimer.Stop();
            await LoadItemsAsync();
        }

        private void RentalItemPickerWindow_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.F)
            {
                e.Handled = true;
                SearchBox.Focus();
                SearchBox.SelectAll();
                return;
            }

            if (Keyboard.Modifiers == ModifierKeys.None && e.Key == Key.Escape)
            {
                e.Handled = true;
                DialogResult = false;
                return;
            }

            if (_isLoading && IsPickerActionShortcut(e))
            {
                e.Handled = true;
                return;
            }

            if (Keyboard.Modifiers == ModifierKeys.None && e.Key == Key.Enter)
            {
                e.Handled = true;
                SelectCurrentItem();
            }
        }

        static bool IsPickerActionShortcut(KeyEventArgs e)
        {
            return Keyboard.Modifiers == ModifierKeys.None && e.Key == Key.Enter;
        }

        private void UseItem_Click(object sender, RoutedEventArgs e)
        {
            SelectCurrentItem();
        }

        private void ItemsGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UseItemButton.IsEnabled = !_isLoading && ItemsGrid.SelectedItem is ItemModel;
        }

        private void ItemsGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (_isLoading)
            {
                e.Handled = true;
                return;
            }

            if (FindAncestor<DataGridRow>(e.OriginalSource as DependencyObject) is { DataContext: ItemModel item })
                ItemsGrid.SelectedItem = item;

            e.Handled = true;
            SelectCurrentItem();
        }

        void SelectCurrentItem()
        {
            if (_isLoading)
            {
                StatusText.Text = "Wait for available rental items to finish loading.";
                return;
            }

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

        static T? FindAncestor<T>(DependencyObject? current) where T : DependencyObject
        {
            while (current != null)
            {
                if (current is T match)
                    return match;

                current = VisualTreeHelper.GetParent(current);
            }

            return null;
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
