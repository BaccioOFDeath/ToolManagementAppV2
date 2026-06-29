using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using InventoryManagementApp.Models.Domain;
using InventoryManagementApp.ViewModels;
namespace InventoryManagementApp.Views.Pages
{
    /// <summary>
    /// Interaction logic for ManageRentalsPage.xaml
    /// </summary>
    public partial class ManageRentalsPage : Page
    {
        const double CompactHeightThreshold = 650;
        bool _isCompactHeight;

        public ManageRentalsPage()
        {
            InitializeComponent();
            Loaded += ManageRentalsPage_Loaded;
            SizeChanged += ManageRentalsPage_SizeChanged;
            PreviewKeyDown += ManageRentalsPage_PreviewKeyDown;
        }

        private async void ManageRentalsPage_Loaded(object sender, RoutedEventArgs e)
        {
            Focus();
            if (DataContext is ManageRentalsViewModel vm)
            {
                await vm.LoadRentalsAsync();
            }

            SearchTextBox.Focus();
            SearchTextBox.SelectAll();
            UpdateCompactHeightMode();
        }

        private void ManageRentalsPage_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            UpdateCompactHeightMode();
        }

        private void UpdateCompactHeightMode()
        {
            var compactHeight = ActualHeight > 0 && ActualHeight < CompactHeightThreshold;
            if (compactHeight == _isCompactHeight)
                return;

            _isCompactHeight = compactHeight;
            RentalStatsStrip.Visibility = compactHeight ? Visibility.Collapsed : Visibility.Visible;
            RentalStatsRow.Height = compactHeight ? new GridLength(0) : GridLength.Auto;
            RentalMainRow.Height = compactHeight ? new GridLength(1.6, GridUnitType.Star) : new GridLength(1.55, GridUnitType.Star);
            RentalSplitterRow.Height = compactHeight ? new GridLength(6) : new GridLength(8);
            RequestQueueRow.Height = compactHeight ? new GridLength(1.15, GridUnitType.Star) : new GridLength(1.25, GridUnitType.Star);
            RequestDetailPanel.Visibility = compactHeight ? Visibility.Collapsed : Visibility.Visible;
            RequestDetailSplitter.Visibility = compactHeight ? Visibility.Collapsed : Visibility.Visible;
            RequestListColumn.Width = compactHeight ? new GridLength(1, GridUnitType.Star) : new GridLength(1.65, GridUnitType.Star);
            RequestDetailSplitterColumn.Width = compactHeight ? new GridLength(0) : new GridLength(8);
            RequestDetailColumn.Width = compactHeight ? new GridLength(0) : new GridLength(1.05, GridUnitType.Star);
            RequestDetailColumn.MinWidth = compactHeight ? 0 : 260;
        }

        private void RentalRow_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (DataContext is ManageRentalsViewModel vm && vm.OpenRentalDetailsCommand.CanExecute(null))
            {
                UiActionGuard.Run(this, "Rentals", () => vm.OpenRentalDetailsCommand.Execute(null));
            }
        }

        private void RentalRow_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            SelectRowForContextMenu(sender, e);
        }

        private void RequestRow_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (DataContext is ManageRentalsViewModel vm && vm.OpenRequestDetailsCommand.CanExecute(null))
            {
                UiActionGuard.Run(this, "Rentals", () => vm.OpenRequestDetailsCommand.Execute(null));
            }
        }

        private void RequestRow_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            SelectRowForContextMenu(sender, e);
        }

        private void ManageRentalsPage_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (DataContext is not ManageRentalsViewModel vm)
                return;

            if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.F)
            {
                SearchTextBox.Focus();
                SearchTextBox.SelectAll();
                e.Handled = true;
                return;
            }

            if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.P)
            {
                UiActionGuard.Run(this, "Rentals", () => vm.PrintSearchResultsCommand.Execute(null));
                e.Handled = true;
                return;
            }

            if (Keyboard.Modifiers == (ModifierKeys.Control | ModifierKeys.Shift) && e.Key == Key.P)
            {
                UiActionGuard.Run(this, "Rentals", () => vm.PrintCheckedOutCommand.Execute(null));
                e.Handled = true;
                return;
            }

            if (Keyboard.Modifiers == (ModifierKeys.Control | ModifierKeys.Shift) && e.Key == Key.R)
            {
                UiActionGuard.Run(this, "Rentals", () => vm.PrintRequestsCommand.Execute(null));
                e.Handled = true;
                return;
            }

            if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.D)
            {
                OpenFocusedDetails(vm);
                e.Handled = true;
                return;
            }

            if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.H && vm.OpenHistoryCommand.CanExecute(null))
            {
                UiActionGuard.RunAsync(this, "Rentals", async () => await vm.OpenHistoryCommand.ExecuteAsync(null));
                e.Handled = true;
                return;
            }

            if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.I && vm.CheckInCommand.CanExecute(null))
            {
                UiActionGuard.RunAsync(this, "Rentals", async () => await vm.CheckInCommand.ExecuteAsync(null));
                e.Handled = true;
                return;
            }

            if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.E && vm.ExtendCommand.CanExecute(null))
            {
                UiActionGuard.RunAsync(this, "Rentals", async () => await vm.ExtendCommand.ExecuteAsync(null));
                e.Handled = true;
                return;
            }

            if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.R && vm.PlaceRequestCommand.CanExecute(null))
            {
                UiActionGuard.RunAsync(this, "Rentals", async () => await vm.PlaceRequestCommand.ExecuteAsync(null));
                e.Handled = true;
                return;
            }

            if (Keyboard.Modifiers == ModifierKeys.None && e.Key == Key.Enter)
            {
                OpenFocusedDetails(vm);
                e.Handled = true;
                return;
            }

            if (Keyboard.Modifiers == ModifierKeys.None && e.Key == Key.Delete && vm.DeleteRentalCommand.CanExecute(null))
            {
                UiActionGuard.RunAsync(this, "Rentals", async () => await vm.DeleteRentalCommand.ExecuteAsync(null));
                e.Handled = true;
            }
        }

        private void OpenFocusedDetails(ManageRentalsViewModel vm)
        {
            UiActionGuard.Run(this, "Rentals", () =>
            {
                if (Keyboard.FocusedElement is DependencyObject focusedElement && GridContextMenuSelection.FindAncestor<System.Windows.Controls.DataGrid>(focusedElement) is System.Windows.Controls.DataGrid grid)
                {
                    if (grid.SelectedItem is Reservation && vm.OpenRequestDetailsCommand.CanExecute(null))
                    {
                        vm.OpenRequestDetailsCommand.Execute(null);
                        return;
                    }
                }

                if (vm.OpenRentalDetailsCommand.CanExecute(null))
                    vm.OpenRentalDetailsCommand.Execute(null);
                else if (vm.OpenRequestDetailsCommand.CanExecute(null))
                    vm.OpenRequestDetailsCommand.Execute(null);
            });
        }

        private void SelectRowForContextMenu(object sender, MouseButtonEventArgs e)
        {
            var row = GridContextMenuSelection.SelectRow(sender, e);
            if (row == null)
                return;

            if (DataContext is ManageRentalsViewModel vm)
            {
                if (row.Item is RentalModel rental)
                    vm.SelectedRental = rental;
                else if (row.Item is Reservation request)
                    vm.SelectedRequest = request;
            }
        }
    }
}
