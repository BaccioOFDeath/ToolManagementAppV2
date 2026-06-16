using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using InventoryManagementApp.ViewModels;

namespace InventoryManagementApp.Views.Pages
{
    public partial class KitManagementPage : Page
    {
        public KitManagementPage()
        {
            InitializeComponent();
            Loaded += KitManagementPage_Loaded;
            PreviewKeyDown += KitManagementPage_PreviewKeyDown;
        }

        private async void KitManagementPage_Loaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is KitManagementViewModel vm)
            {
                await vm.LoadKitsCommand.ExecuteAsync(null);
            }
        }

        private async void KitManagementPage_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (DataContext is not KitManagementViewModel vm)
                return;

            if (e.Key == Key.Enter && vm.OpenKitDetailsCommand.CanExecute(null))
            {
                vm.OpenKitDetailsCommand.Execute(null);
                e.Handled = true;
            }
            else if (e.Key == Key.F5 && vm.RefreshCommand.CanExecute(null))
            {
                await vm.RefreshCommand.ExecuteAsync(null);
                e.Handled = true;
            }
            else if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.P && vm.PrintSelectedKitCommand.CanExecute(null))
            {
                vm.PrintSelectedKitCommand.Execute(null);
                e.Handled = true;
            }
            else if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.D && vm.PrintKitListCommand.CanExecute(null))
            {
                vm.PrintKitListCommand.Execute(null);
                e.Handled = true;
            }
        }

        private void KitRow_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (DataContext is KitManagementViewModel vm && vm.OpenKitDetailsCommand.CanExecute(null))
            {
                vm.OpenKitDetailsCommand.Execute(null);
                e.Handled = true;
            }
        }

        private async void KitItemRow_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (DataContext is KitManagementViewModel vm && vm.EditKitItemCommand.CanExecute(null))
            {
                await vm.EditKitItemCommand.ExecuteAsync(null);
                e.Handled = true;
            }
        }

        private void DataGridRow_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is DataGridRow row)
            {
                row.IsSelected = true;
                var dataGrid = FindParent<DataGrid>(row);
                if (dataGrid != null)
                    dataGrid.SelectedItem = row.Item;
            }
        }

        private static T? FindParent<T>(DependencyObject child) where T : DependencyObject
        {
            var parent = VisualTreeHelper.GetParent(child);
            while (parent != null)
            {
                if (parent is T typedParent)
                    return typedParent;
                parent = VisualTreeHelper.GetParent(parent);
            }
            return null;
        }
    }
}
