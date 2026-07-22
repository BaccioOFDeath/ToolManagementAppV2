using System;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using InventoryManagementApp.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Application = System.Windows.Application;

namespace InventoryManagementApp.Views.Pages
{
    public partial class ManageItemsPage : Page
    {
        private CancellationTokenSource _loadCts = new();
        private ItemsViewModel? _loadedViewModel;

        public ManageItemsPage()
        {
            InitializeComponent();
            DataContext = ((App)Application.Current).Host.Services.GetRequiredService<ItemsViewModel>();
            Loaded += ManageItemsPage_Loaded;
            Unloaded += ManageItemsPage_Unloaded;
            DataContextChanged += ManageItemsPage_DataContextChanged;
            PreviewKeyDown += ManageItemsPage_PreviewKeyDown;
        }

        private void ManageItemsPage_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (!ReferenceEquals(e.NewValue, _loadedViewModel))
                _loadedViewModel = null;
        }

        private void DataGridRow_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (IsItemDirectoryBusy())
            {
                return;
            }

            GridContextMenuSelection.SelectRow(sender, e);
        }

        private void DataGridRow_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (IsItemDirectoryBusy())
            {
                e.Handled = true;
                return;
            }

            if (GridContextMenuSelection.SelectRow(sender, e) == null)
                return;

            if (DataContext is ItemsViewModel vm && vm.ViewDetailsCommand.CanExecute(null))
            {
                UiActionGuard.Run(this, "Item Details", () => vm.ViewDetailsCommand.Execute(null));
                e.Handled = true;
            }
        }

        private async void ItemDirectoryGrid_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if (e.VerticalChange <= 0 || e.VerticalOffset < e.ExtentHeight - e.ViewportHeight - 2)
                return;

            if (DataContext is not ItemsViewModel vm || vm.IsDirectoryBusy || !vm.Items.HasMoreItems)
                return;

            try
            {
                await vm.LoadMoreAsync(_loadCts.Token);
            }
            catch (OperationCanceledException)
            {
            }
        }

        private async void ManageItemsPage_Loaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is not ItemsViewModel vm)
            {
                vm = ((App)Application.Current).Host.Services.GetRequiredService<ItemsViewModel>();
                DataContext = vm;
            }

            if (ReferenceEquals(_loadedViewModel, vm))
                return;

            _loadedViewModel = vm;

            if (vm.IsDirectoryBusy)
                return;

            try
            {
                await System.Windows.Threading.Dispatcher.Yield(System.Windows.Threading.DispatcherPriority.Background);
                await vm.EnsureLoadedAsync(_loadCts.Token);
            }
            catch (OperationCanceledException)
            {
                if (ReferenceEquals(_loadedViewModel, vm))
                    _loadedViewModel = null;
            }
        }

        private void ManageItemsPage_Unloaded(object sender, RoutedEventArgs e)
        {
            _loadCts.Cancel();
            _loadedViewModel = null;
            _loadCts.Dispose();
            _loadCts = new CancellationTokenSource();
        }

        private void ManageItemsPage_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (DataContext is not ItemsViewModel vm)
                return;

            if (IsItemDirectoryBusy())
            {
                if (IsManagedDirectoryShortcut(e))
                    e.Handled = true;
                return;
            }

            if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.N && vm.NewItemCommand.CanExecute(null))
            {
                UiActionGuard.RunAsync(this, "Manage Items", async () => await vm.NewItemCommand.ExecuteAsync(null));
                e.Handled = true;
                return;
            }

            if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.M && vm.OpenMobileCaptureCommand.CanExecute(null))
            {
                UiActionGuard.RunAsync(this, "Manage Items", async () => await vm.OpenMobileCaptureCommand.ExecuteAsync(null));
                e.Handled = true;
                return;
            }

            if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.E && vm.EditItemCommand.CanExecute(null))
            {
                UiActionGuard.RunAsync(this, "Manage Items", async () => await vm.EditItemCommand.ExecuteAsync(null));
                e.Handled = true;
                return;
            }

            if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.D && vm.ViewDetailsCommand.CanExecute(null))
            {
                UiActionGuard.Run(this, "Manage Items", () => vm.ViewDetailsCommand.Execute(null));
                e.Handled = true;
                return;
            }

            if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.H && vm.OpenRentalHistoryCommand.CanExecute(null))
            {
                UiActionGuard.RunAsync(this, "Manage Items", async () => await vm.OpenRentalHistoryCommand.ExecuteAsync(null));
                e.Handled = true;
                return;
            }

            if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.S && vm.CommitChangesCommand.CanExecute(null))
            {
                UiActionGuard.RunAsync(this, "Manage Items", async () => await vm.CommitChangesCommand.ExecuteAsync(null));
                e.Handled = true;
                return;
            }

            if (Keyboard.Modifiers == ModifierKeys.None && e.Key == Key.Delete && vm.DeleteItemsCommand.CanExecute(ItemDirectoryGrid.SelectedItems))
            {
                UiActionGuard.RunAsync(this, "Manage Items", async () => await vm.DeleteItemsCommand.ExecuteAsync(ItemDirectoryGrid.SelectedItems));
                e.Handled = true;
                return;
            }

            if (Keyboard.Modifiers == ModifierKeys.None && e.Key == Key.Enter && vm.ViewDetailsCommand.CanExecute(null))
            {
                UiActionGuard.Run(this, "Manage Items", () => vm.ViewDetailsCommand.Execute(null));
                e.Handled = true;
            }
        }

        private static bool IsManagedDirectoryShortcut(KeyEventArgs e)
        {
            if (Keyboard.Modifiers == ModifierKeys.Control)
            {
                return e.Key is Key.N or Key.M or Key.E or Key.D or Key.H or Key.S;
            }

            return Keyboard.Modifiers == ModifierKeys.None && e.Key is Key.Delete or Key.Enter;
        }

        private bool IsItemDirectoryBusy()
        {
            return DataContext is ItemsViewModel { IsDirectoryBusy: true };
        }
    }
}
