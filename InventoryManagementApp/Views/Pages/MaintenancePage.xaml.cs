using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using InventoryManagementApp.ViewModels;

namespace InventoryManagementApp.Views.Pages
{
    public partial class MaintenancePage : Page
    {
        public MaintenancePage()
        {
            InitializeComponent();
            Loaded += MaintenancePage_Loaded;
        }

        private async void MaintenancePage_Loaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is MaintenanceManagementViewModel vm)
            {
                await vm.LoadMaintenanceCommand.ExecuteAsync(null);
            }
        }

        private void MaintenanceRow_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (DataContext is MaintenanceManagementViewModel vm && vm.OpenMaintenanceDetailsCommand.CanExecute(null))
            {
                UiActionGuard.Run(this, "Maintenance", () => vm.OpenMaintenanceDetailsCommand.Execute(null));
            }
        }

        private void MaintenanceRow_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is DataGridRow row && !row.IsSelected)
            {
                row.IsSelected = true;
                e.Handled = true;
            }
        }
    }
}
