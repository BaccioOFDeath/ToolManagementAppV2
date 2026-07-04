using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using InventoryManagementApp.Models;
using InventoryManagementApp.Models.Domain;
using InventoryManagementApp.ViewModels;

namespace InventoryManagementApp.Views.Pages
{
    public partial class DashboardPage : Page
    {
        private CancellationTokenSource? _loadCts;
        private bool _isLoadingDashboard;
        private DashboardViewModel? _loadedDashboardViewModel;
        private bool _hasLoadedDashboardForViewModel;

        public DashboardPage()
        {
            InitializeComponent();
            Loaded += DashboardPage_Loaded;
            Unloaded += DashboardPage_Unloaded;
            DataContextChanged += DashboardPage_DataContextChanged;
            PreviewKeyDown += DashboardPage_PreviewKeyDown;
        }

        private async void DashboardPage_Loaded(object sender, RoutedEventArgs e)
        {
            Focus();

            if (DataContext is not DashboardViewModel vm)
                return;

            if (ReferenceEquals(_loadedDashboardViewModel, vm) && _hasLoadedDashboardForViewModel)
                return;

            _loadedDashboardViewModel = vm;
            _hasLoadedDashboardForViewModel = true;
            await LoadDashboardAsync("Loading dashboard data...");
        }

        private void DashboardPage_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (!ReferenceEquals(_loadedDashboardViewModel, e.NewValue))
            {
                _loadedDashboardViewModel = e.NewValue as DashboardViewModel;
                _hasLoadedDashboardForViewModel = false;
            }
        }

        private async void DashboardLoadRetryButton_Click(object sender, RoutedEventArgs e)
        {
            _hasLoadedDashboardForViewModel = false;
            await LoadDashboardAsync("Refreshing dashboard data...");
        }

        private async Task LoadDashboardAsync(string loadingMessage)
        {
            if (_isLoadingDashboard || DataContext is not DashboardViewModel vm)
                return;

            _isLoadingDashboard = true;
            _loadCts?.Cancel();
            _loadCts?.Dispose();
            _loadCts = new CancellationTokenSource();
            var token = _loadCts.Token;
            var previousCursor = Cursor;

            SetDashboardLoadStatus(loadingMessage, showRetry: false);
            Cursor = Cursors.Wait;

            try
            {
                await Dispatcher.Yield(DispatcherPriority.Background);
                await vm.LoadAsync(token);
                SetDashboardLoadStatus(null, showRetry: false);
            }
            catch (OperationCanceledException)
            {
                if (IsLoaded)
                    SetDashboardLoadStatus("Dashboard refresh was cancelled before it finished.", showRetry: true);
            }
            catch (Exception)
            {
                SetDashboardLoadStatus("Dashboard data could not be loaded. Check the database connection, then retry.", showRetry: true);
            }
            finally
            {
                Cursor = previousCursor;
                _isLoadingDashboard = false;
                DashboardLoadRetryButton.IsEnabled = DashboardLoadRetryButton.Visibility == Visibility.Visible;
            }
        }

        private void SetDashboardLoadStatus(string? message, bool showRetry)
        {
            var hasMessage = !string.IsNullOrWhiteSpace(message);
            DashboardLoadStatusBanner.Visibility = hasMessage ? Visibility.Visible : Visibility.Collapsed;
            DashboardLoadRetryButton.Visibility = showRetry ? Visibility.Visible : Visibility.Collapsed;
            DashboardLoadRetryButton.IsEnabled = showRetry && !_isLoadingDashboard;
            DashboardLoadStatusText.Text = message ?? string.Empty;
        }

        private void DashboardPage_Unloaded(object sender, RoutedEventArgs e)
        {
            _loadCts?.Cancel();
            _loadCts?.Dispose();
            _loadCts = null;
            _isLoadingDashboard = false;
            Cursor = null;
        }

        private void DashboardPage_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (DataContext is not DashboardViewModel vm)
                return;

            if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.P)
            {
                if (!_isLoadingDashboard && vm.PrintDashboardSnapshotCommand.CanExecute(null))
                    UiActionGuard.RunAsync(this, "Dashboard", async () => await vm.PrintDashboardSnapshotCommand.ExecuteAsync(null));
                e.Handled = true;
                return;
            }

            if (Keyboard.Modifiers == (ModifierKeys.Control | ModifierKeys.Shift) && e.Key == Key.P)
            {
                if (!_isLoadingDashboard && vm.PrintCheckedOutItemsCommand.CanExecute(null))
                    UiActionGuard.RunAsync(this, "Dashboard", async () => await vm.PrintCheckedOutItemsCommand.ExecuteAsync(null));
                e.Handled = true;
                return;
            }

            if (_isLoadingDashboard)
                return;

            if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.I)
            {
                if (vm.OpenItemsCommand.CanExecute(null))
                    UiActionGuard.Run(this, "Dashboard", () => vm.OpenItemsCommand.Execute(null));
                e.Handled = true;
                return;
            }

            if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.R)
            {
                if (vm.OpenRentalsCommand.CanExecute(null))
                    UiActionGuard.Run(this, "Dashboard", () => vm.OpenRentalsCommand.Execute(null));
                e.Handled = true;
                return;
            }

            if (Keyboard.Modifiers == ModifierKeys.None && e.Key == Key.Enter)
            {
                UiActionGuard.Run(this, "Dashboard", () => OpenFocusedRow(vm));
                e.Handled = true;
            }
        }

        private void CommonItemsGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (_isLoadingDashboard || DataContext is not DashboardViewModel vm || !vm.OpenSelectedCommonItemCommand.CanExecute(null))
                return;

            UiActionGuard.Run(this, "Dashboard", () => vm.OpenSelectedCommonItemCommand.Execute(null));
        }

        private void CheckedOutItemsGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (_isLoadingDashboard || DataContext is not DashboardViewModel vm || !vm.OpenSelectedCheckedOutItemCommand.CanExecute(null))
                return;

            UiActionGuard.Run(this, "Dashboard", () => vm.OpenSelectedCheckedOutItemCommand.Execute(null));
        }

        private void RentedItemsGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (_isLoadingDashboard || DataContext is not DashboardViewModel vm || !vm.OpenSelectedRentalCommand.CanExecute(null))
                return;

            UiActionGuard.Run(this, "Dashboard", () => vm.OpenSelectedRentalCommand.Execute(null));
        }

        private void RecentActivityGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (_isLoadingDashboard || DataContext is not DashboardViewModel vm || !vm.OpenActivityDestinationCommand.CanExecute(null))
                return;

            UiActionGuard.Run(this, "Dashboard", () => vm.OpenActivityDestinationCommand.Execute(null));
        }

        private void IncompleteItemsGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (_isLoadingDashboard || DataContext is not DashboardViewModel vm || !vm.OpenSelectedIncompleteItemCommand.CanExecute(null))
                return;

            UiActionGuard.Run(this, "Dashboard", () => vm.OpenSelectedIncompleteItemCommand.Execute(null));
        }

        private void DashboardGrid_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (_isLoadingDashboard)
                return;

            var row = GridContextMenuSelection.SelectRow(sender, e);
            if (row == null || DataContext is not DashboardViewModel vm)
                return;

            switch (row.Item)
            {
                case ItemModel item when ReferenceEquals(sender, CommonItemsGrid):
                    vm.SelectedCommonlyUsedItem = item;
                    break;
                case ItemModel item when ReferenceEquals(sender, CheckedOutItemsGrid):
                    vm.SelectedCheckedOutItem = item;
                    break;
                case ItemModel item when ReferenceEquals(sender, IncompleteItemsGrid):
                    vm.SelectedIncompleteItem = item;
                    break;
                case RentalModel rental:
                    vm.SelectedRental = rental;
                    break;
                case ActivityLog activity:
                    vm.SelectedActivity = activity;
                    break;
            }
        }

        private static void OpenFocusedRow(DashboardViewModel vm)
        {
            if (Keyboard.FocusedElement is DependencyObject focusedElement && GridContextMenuSelection.FindAncestor<System.Windows.Controls.DataGrid>(focusedElement) is System.Windows.Controls.DataGrid grid)
            {
                switch (grid.Name)
                {
                    case nameof(CommonItemsGrid) when vm.OpenSelectedCommonItemCommand.CanExecute(null):
                        vm.OpenSelectedCommonItemCommand.Execute(null);
                        return;
                    case nameof(CheckedOutItemsGrid) when vm.OpenSelectedCheckedOutItemCommand.CanExecute(null):
                        vm.OpenSelectedCheckedOutItemCommand.Execute(null);
                        return;
                    case nameof(RentedItemsGrid) when vm.OpenSelectedRentalCommand.CanExecute(null):
                        vm.OpenSelectedRentalCommand.Execute(null);
                        return;
                    case nameof(IncompleteItemsGrid) when vm.OpenSelectedIncompleteItemCommand.CanExecute(null):
                        vm.OpenSelectedIncompleteItemCommand.Execute(null);
                        return;
                    case nameof(RecentActivityGrid) when vm.OpenActivityDestinationCommand.CanExecute(null):
                        vm.OpenActivityDestinationCommand.Execute(null);
                        return;
                }
            }

            if (vm.OpenItemsCommand.CanExecute(null))
                vm.OpenItemsCommand.Execute(null);
        }
    }
}
