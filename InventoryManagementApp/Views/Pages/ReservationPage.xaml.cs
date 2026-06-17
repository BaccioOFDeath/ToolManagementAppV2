using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using InventoryManagementApp.ViewModels;

namespace InventoryManagementApp.Views.Pages
{
    public partial class ReservationPage : Page
    {
        public ReservationPage()
        {
            InitializeComponent();
            Loaded += ReservationPage_Loaded;
            PreviewKeyDown += ReservationPage_PreviewKeyDown;
        }

        private async void ReservationPage_Loaded(object sender, RoutedEventArgs e)
        {
            Focus();
            if (DataContext is ReservationManagementViewModel vm)
            {
                await vm.LoadReservationsCommand.ExecuteAsync(null);
            }

            FocusFirstSearchBox();
        }

        private void ReservationRow_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (DataContext is ReservationManagementViewModel vm && vm.OpenReservationDetailsCommand.CanExecute(null))
            {
                vm.OpenReservationDetailsCommand.Execute(null);
            }
        }

        private void ReservationRow_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            SelectRowForContextMenu(sender, e);
        }

        private void ReservationPage_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (DataContext is not ReservationManagementViewModel vm)
                return;

            if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.F)
            {
                FocusFirstSearchBox();
                e.Handled = true;
                return;
            }

            if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.N)
            {
                vm.AddReservationCommand.Execute(null);
                e.Handled = true;
                return;
            }

            if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.P)
            {
                vm.PrintReservationDirectoryCommand.Execute(null);
                e.Handled = true;
                return;
            }

            if (Keyboard.Modifiers == (ModifierKeys.Control | ModifierKeys.Shift) && e.Key == Key.P && vm.PrintReservationHandoffCommand.CanExecute(null))
            {
                vm.PrintReservationHandoffCommand.Execute(null);
                e.Handled = true;
                return;
            }

            if (!IsTextInputFocused() && Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.C && vm.CopyReservationHandoffCommand.CanExecute(null))
            {
                vm.CopyReservationHandoffCommand.Execute(null);
                e.Handled = true;
                return;
            }

            if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.D && vm.OpenReservationDetailsCommand.CanExecute(null))
            {
                vm.OpenReservationDetailsCommand.Execute(null);
                e.Handled = true;
                return;
            }

            if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.Enter && vm.ConfirmReservationCommand.CanExecute(null))
            {
                vm.ConfirmReservationCommand.Execute(null);
                e.Handled = true;
                return;
            }

            if (Keyboard.Modifiers == (ModifierKeys.Control | ModifierKeys.Shift) && e.Key == Key.Enter && vm.FulfillReservationCommand.CanExecute(null))
            {
                vm.FulfillReservationCommand.Execute(null);
                e.Handled = true;
                return;
            }

            if (Keyboard.Modifiers == ModifierKeys.None && e.Key == Key.Enter && vm.OpenReservationDetailsCommand.CanExecute(null))
            {
                vm.OpenReservationDetailsCommand.Execute(null);
                e.Handled = true;
                return;
            }

            if (Keyboard.Modifiers == ModifierKeys.None && e.Key == Key.Delete && vm.CancelReservationCommand.CanExecute(null))
            {
                vm.CancelReservationCommand.Execute(null);
                e.Handled = true;
            }
        }

        private void FocusFirstSearchBox()
        {
            var searchBox = FindDescendant<TextBox>(this);
            if (searchBox == null)
                return;

            searchBox.Focus();
            searchBox.SelectAll();
        }

        private void SelectRowForContextMenu(object sender, MouseButtonEventArgs e)
        {
            var row = sender as DataGridRow ?? FindAncestor<DataGridRow>(e.OriginalSource as DependencyObject);
            if (row == null)
                return;

            row.IsSelected = true;
            row.Focus();
        }

        private static bool IsTextInputFocused()
        {
            return Keyboard.FocusedElement is TextBoxBase or PasswordBox;
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

        private static T? FindDescendant<T>(DependencyObject current) where T : DependencyObject
        {
            for (var index = 0; index < VisualTreeHelper.GetChildrenCount(current); index++)
            {
                var child = VisualTreeHelper.GetChild(current, index);
                if (child is T match)
                    return match;

                var nested = FindDescendant<T>(child);
                if (nested != null)
                    return nested;
            }

            return null;
        }
    }
}