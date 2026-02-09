using System.Windows;
using System.Windows.Controls;
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
    }
}
