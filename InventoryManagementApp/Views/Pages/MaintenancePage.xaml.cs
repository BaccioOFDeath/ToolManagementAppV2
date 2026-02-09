using System.Windows;
using System.Windows.Controls;
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
    }
}
