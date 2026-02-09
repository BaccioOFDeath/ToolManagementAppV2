using System.Windows;
using System.Windows.Controls;
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
    }
}
