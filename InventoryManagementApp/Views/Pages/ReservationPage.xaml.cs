using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using InventoryManagementApp.ViewModels;

namespace InventoryManagementApp.Views.Pages
{
    public partial class ReservationPage : Page
    {
        public ReservationPage()
        {
            InitializeComponent();
            Loaded += ReservationPage_Loaded;
        }

        private async void ReservationPage_Loaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is ReservationManagementViewModel vm)
            {
                await vm.LoadReservationsCommand.ExecuteAsync(null);
            }
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
            if (sender is DataGridRow row && !row.IsSelected)
            {
                row.IsSelected = true;
                row.Focus();
            }
        }
    }
}
