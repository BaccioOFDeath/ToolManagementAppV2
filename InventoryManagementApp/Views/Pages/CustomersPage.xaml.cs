using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using InventoryManagementApp.ViewModels;

namespace InventoryManagementApp.Views.Pages
{
    public partial class CustomersPage : Page
    {
        public CustomersPage()
        {
            InitializeComponent();
            Loaded += CustomersPage_Loaded;
        }

        private async void CustomersPage_Loaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is CustomerManagementViewModel vm)
            {
                await vm.LoadCustomersAsync();
            }
        }

        private void CustomerRow_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (DataContext is CustomerManagementViewModel vm && vm.OpenCustomerDetailsCommand.CanExecute(null))
            {
                UiActionGuard.Run(this, "Customers", () => vm.OpenCustomerDetailsCommand.Execute(null));
            }
        }

        private void CustomerRow_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is DataGridRow row && !row.IsSelected)
            {
                row.IsSelected = true;
                row.Focus();
            }
        }
    }
}
