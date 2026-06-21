using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
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

        private void KitManagementPage_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (DataContext is not KitManagementViewModel vm)
                return;

            if (e.Key == Key.Enter && vm.OpenKitDetailsCommand.CanExecute(null))
            {
                UiActionGuard.Run(this, "Kit Management", () => vm.OpenKitDetailsCommand.Execute(null));
                e.Handled = true;
            }
            else if (e.Key == Key.F5 && vm.RefreshCommand.CanExecute(null))
            {
                UiActionGuard.RunAsync(this, "Kit Management", async () => await vm.RefreshCommand.ExecuteAsync(null));
                e.Handled = true;
            }
            else if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.P && vm.PrintSelectedKitCommand.CanExecute(null))
            {
                UiActionGuard.Run(this, "Kit Management", () => vm.PrintSelectedKitCommand.Execute(null));
                e.Handled = true;
            }
            else if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.D && vm.PrintKitListCommand.CanExecute(null))
            {
                UiActionGuard.Run(this, "Kit Management", () => vm.PrintKitListCommand.Execute(null));
                e.Handled = true;
            }
        }

        private void KitRow_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (DataContext is KitManagementViewModel vm && vm.OpenKitDetailsCommand.CanExecute(null))
            {
                UiActionGuard.Run(this, "Kit Management", () => vm.OpenKitDetailsCommand.Execute(null));
                e.Handled = true;
            }
        }

        private void KitItemRow_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (DataContext is KitManagementViewModel vm && vm.EditKitItemCommand.CanExecute(null))
            {
                UiActionGuard.RunAsync(this, "Kit Management", async () => await vm.EditKitItemCommand.ExecuteAsync(null));
                e.Handled = true;
            }
        }

        private void DataGridRow_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            GridContextMenuSelection.SelectRow(sender, e);
        }
    }
}
