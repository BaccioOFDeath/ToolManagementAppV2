using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using InventoryManagementApp.Models.Domain;
using InventoryManagementApp.ViewModels;
using RentalModel = InventoryManagementApp.Models.Domain.Rental;

namespace InventoryManagementApp.Views.Pages
{
    /// <summary>
    /// Interaction logic for ManageRentalsPage.xaml
    /// </summary>
    public partial class ManageRentalsPage : Page
    {
        public ManageRentalsPage()
        {
            InitializeComponent();
            Loaded += ManageRentalsPage_Loaded;
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
        }

        private void RentalRow_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (DataContext is ManageRentalsViewModel vm && vm.OpenRentalDetailsCommand.CanExecute(null))
            {
                vm.OpenRentalDetailsCommand.Execute(null);
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
                vm.OpenRequestDetailsCommand.Execute(null);
            }
        }

        private void RequestRow_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            SelectRowForContextMenu(sender, e);
        }

        private void ManageRentalsPage_PreviewKeyDown(object sender, KeyEventArgs e)
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
                vm.PrintSearchResultsCommand.Execute(null);
                e.Handled = true;
                return;
            }

            if (Keyboard.Modifiers == (ModifierKeys.Control | ModifierKeys.Shift) && e.Key == Key.P)
            {
                vm.PrintCheckedOutCommand.Execute(null);
                e.Handled = true;
                return;
            }

            if (Keyboard.Modifiers == (ModifierKeys.Control | ModifierKeys.Shift) && e.Key == Key.R)
            {
                vm.PrintRequestsCommand.Execute(null);
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
                vm.OpenHistoryCommand.Execute(null);
                e.Handled = true;
                return;
            }

            if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.I && vm.CheckInCommand.CanExecute(null))
            {
                vm.CheckInCommand.Execute(null);
                e.Handled = true;
                return;
            }

            if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.E && vm.ExtendCommand.CanExecute(null))
            {
                vm.ExtendCommand.Execute(null);
                e.Handled = true;
                return;
            }

            if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.R && vm.PlaceRequestCommand.CanExecute(null))
            {
                vm.PlaceRequestCommand.Execute(null);
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
                vm.DeleteRentalCommand.Execute(null);
                e.Handled = true;
            }
        }

        private static void OpenFocusedDetails(ManageRentalsViewModel vm)
        {
            if (Keyboard.FocusedElement is DependencyObject focusedElement && FindAncestor<DataGrid>(focusedElement) is DataGrid grid)
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
        }

        private void SelectRowForContextMenu(object sender, MouseButtonEventArgs e)
        {
            var row = sender as DataGridRow ?? FindAncestor<DataGridRow>(e.OriginalSource as DependencyObject);
            if (row == null)
                return;

            row.IsSelected = true;
            row.Focus();

            if (DataContext is ManageRentalsViewModel vm)
            {
                if (row.Item is RentalModel rental)
                    vm.SelectedRental = rental;
                else if (row.Item is Reservation request)
                    vm.SelectedRequest = request;
            }
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
