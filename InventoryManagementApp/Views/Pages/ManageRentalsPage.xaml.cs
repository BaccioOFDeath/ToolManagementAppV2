using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using InventoryManagementApp.ViewModels;

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
        }

        private async void ManageRentalsPage_Loaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is ManageRentalsViewModel vm)
            {
                await vm.LoadRentalsAsync();
            }
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

        private static void SelectRowForContextMenu(object sender, MouseButtonEventArgs e)
        {
            if (sender is DataGridRow row && !row.IsSelected)
            {
                row.IsSelected = true;
                e.Handled = true;
            }
        }
    }
}
