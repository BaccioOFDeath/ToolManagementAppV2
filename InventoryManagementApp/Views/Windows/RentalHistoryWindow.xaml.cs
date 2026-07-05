using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using InventoryManagementApp.ViewModels.Rental;
using InventoryManagementApp.Utilities.Extensions;

namespace InventoryManagementApp.Views.Windows
{
    public partial class RentalHistoryWindow : Window
    {
        public RentalHistoryWindow()
        {
            InitializeComponent();
            PreviewKeyDown += RentalHistoryWindow_PreviewKeyDown;
            this.DisposeDataContextOnUnload();
        }

        public RentalHistoryWindow(RentalHistoryViewModel viewModel) : this()
        {
            DataContext = viewModel;
        }

        private void RentalHistoryWindow_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.F)
            {
                HistorySearchBar.Focus();
                e.Handled = true;
                return;
            }

            if (DataContext is not RentalHistoryViewModel vm)
                return;

            if (vm.IsFiltering && IsRentalHistoryActionShortcut(e))
            {
                e.Handled = true;
                return;
            }

            if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.D)
            {
                if (vm.OpenDetailsCommand.CanExecute(null))
                    vm.OpenDetailsCommand.Execute(null);
                e.Handled = true;
                return;
            }

            if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.E)
            {
                if (vm.ExportCsvCommand.CanExecute(null))
                    vm.ExportCsvCommand.Execute(null);
                e.Handled = true;
                return;
            }

            if (Keyboard.Modifiers == ModifierKeys.None && e.Key == Key.Escape)
            {
                if (vm.CloseCommand.CanExecute(null))
                    vm.CloseCommand.Execute(null);
                e.Handled = true;
            }
        }

        private static bool IsRentalHistoryActionShortcut(KeyEventArgs e)
        {
            if (Keyboard.Modifiers == ModifierKeys.Control)
                return e.Key is Key.D or Key.E;

            return Keyboard.Modifiers == ModifierKeys.None && e.Key == Key.Enter;
        }

        private void HistoryRow_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (DataContext is not RentalHistoryViewModel vm)
                return;

            if (vm.IsFiltering)
            {
                e.Handled = true;
                return;
            }

            if (vm.OpenDetailsCommand.CanExecute(null))
            {
                vm.OpenDetailsCommand.Execute(null);
                e.Handled = true;
            }
        }

        private void HistoryRow_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (DataContext is RentalHistoryViewModel { IsFiltering: true })
            {
                e.Handled = true;
                return;
            }

            if (sender is DataGridRow row && !row.IsSelected)
            {
                row.IsSelected = true;
                e.Handled = true;
            }
        }
    }
}
