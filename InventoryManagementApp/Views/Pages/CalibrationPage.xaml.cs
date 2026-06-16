using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using InventoryManagementApp.ViewModels;

namespace InventoryManagementApp.Views.Pages
{
    public partial class CalibrationPage : Page
    {
        public CalibrationPage()
        {
            InitializeComponent();
            Loaded += CalibrationPage_Loaded;
            PreviewKeyDown += CalibrationPage_PreviewKeyDown;
        }

        private async void CalibrationPage_Loaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is CalibrationManagementViewModel vm)
            {
                await vm.LoadCalibrationCommand.ExecuteAsync(null);
            }
        }

        private void CalibrationPage_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (DataContext is not CalibrationManagementViewModel vm)
            {
                return;
            }

            if ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control && e.Key == Key.F)
            {
                SearchBox.Focus();
                SearchBox.SelectAll();
                e.Handled = true;
                return;
            }

            if ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control && e.Key == Key.P)
            {
                if (vm.PrintCalibrationListCommand.CanExecute(null))
                {
                    vm.PrintCalibrationListCommand.Execute(null);
                    e.Handled = true;
                }
                return;
            }

            if ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control && e.Key == Key.C)
            {
                if (vm.CopySelectedCalibrationCommand.CanExecute(null))
                {
                    vm.CopySelectedCalibrationCommand.Execute(null);
                    e.Handled = true;
                }
                return;
            }

            if (e.Key == Key.Enter && vm.OpenCalibrationDetailsCommand.CanExecute(null))
            {
                vm.OpenCalibrationDetailsCommand.Execute(null);
                e.Handled = true;
            }
        }

        private void CalibrationRow_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (DataContext is CalibrationManagementViewModel vm && vm.OpenCalibrationDetailsCommand.CanExecute(null))
            {
                vm.OpenCalibrationDetailsCommand.Execute(null);
            }
        }

        private void CalibrationRow_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is DataGridRow row && !row.IsSelected)
            {
                row.Focus();
                row.IsSelected = true;
            }
        }
    }
}
