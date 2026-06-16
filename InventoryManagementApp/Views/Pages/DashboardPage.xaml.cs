using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using InventoryManagementApp.Models;
using InventoryManagementApp.Models.Domain;
using InventoryManagementApp.ViewModels;

namespace InventoryManagementApp.Views.Pages
{
    public partial class DashboardPage : Page
    {
        private CancellationTokenSource? _loadCts;

        public DashboardPage()
        {
            InitializeComponent();
            Loaded += DashboardPage_Loaded;
            Unloaded += DashboardPage_Unloaded;
            PreviewKeyDown += DashboardPage_PreviewKeyDown;
        }

        private async void DashboardPage_Loaded(object sender, RoutedEventArgs e)
        {
            Focus();

            if (DataContext is DashboardViewModel vm)
            {
                _loadCts = new CancellationTokenSource();
                try
                {
                    await vm.LoadAsync(_loadCts.Token);
                }
                catch (OperationCanceledException)
                {
                }
            }
        }

        private void DashboardPage_Unloaded(object sender, RoutedEventArgs e)
        {
            _loadCts?.Cancel();
            _loadCts?.Dispose();
            _loadCts = null;
        }

        private void DashboardPage_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (DataContext is not DashboardViewModel vm)
                return;

            if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.P)
            {
                vm.PrintDashboardSnapshotCommand.Execute(null);
                e.Handled = true;
                return;
            }

            if (Keyboard.Modifiers == (ModifierKeys.Control | ModifierKeys.Shift) && e.Key == Key.P)
            {
                vm.PrintCheckedOutItemsCommand.Execute(null);
                e.Handled = true;
                return;
            }

            if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.I)
            {
                vm.OpenItemsCommand.Execute(null);
                e.Handled = true;
                return;
            }

            if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.R)
            {
                vm.OpenRentalsCommand.Execute(null);
                e.Handled = true;
                return;
            }

            if (Keyboard.Modifiers == ModifierKeys.None && e.Key == Key.Enter)
            {
                OpenFocusedRow(vm);
                e.Handled = true;
            }
        }

        private void CommonItemsGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (DataContext is DashboardViewModel vm)
                vm.OpenSelectedCommonItemCommand.Execute(null);
        }

        private void CheckedOutItemsGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (DataContext is DashboardViewModel vm)
                vm.OpenSelectedCheckedOutItemCommand.Execute(null);
        }

        private void RentedItemsGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (DataContext is DashboardViewModel vm)
                vm.OpenSelectedRentalCommand.Execute(null);
        }

        private void RecentActivityGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (DataContext is DashboardViewModel vm)
                vm.OpenActivityDestinationCommand.Execute(null);
        }

        private void IncompleteItemsGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (DataContext is DashboardViewModel vm)
                vm.OpenSelectedIncompleteItemCommand.Execute(null);
        }

        private void DashboardGrid_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            var row = FindAncestor<DataGridRow>(e.OriginalSource as DependencyObject);
            if (row == null)
                return;

            row.IsSelected = true;
            row.Focus();

            if (DataContext is not DashboardViewModel vm)
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
            if (Keyboard.FocusedElement is DependencyObject focusedElement && FindAncestor<DataGrid>(focusedElement) is DataGrid grid)
            {
                switch (grid.Name)
                {
                    case nameof(CommonItemsGrid):
                        vm.OpenSelectedCommonItemCommand.Execute(null);
                        return;
                    case nameof(CheckedOutItemsGrid):
                        vm.OpenSelectedCheckedOutItemCommand.Execute(null);
                        return;
                    case nameof(RentedItemsGrid):
                        vm.OpenSelectedRentalCommand.Execute(null);
                        return;
                    case nameof(IncompleteItemsGrid):
                        vm.OpenSelectedIncompleteItemCommand.Execute(null);
                        return;
                    case nameof(RecentActivityGrid):
                        vm.OpenActivityDestinationCommand.Execute(null);
                        return;
                }
            }

            vm.OpenItemsCommand.Execute(null);
        }

        private static T? FindAncestor<T>(DependencyObject? current) where T : DependencyObject
        {
            while (current != null)
            {
                if (current is T match)
                    return match;

                current = VisualTreeHelper.GetParent(current);
            }

            return null;
        }
    }
}