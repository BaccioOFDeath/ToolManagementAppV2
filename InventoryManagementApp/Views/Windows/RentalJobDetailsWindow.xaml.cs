using System.Windows;
using InventoryManagementApp.ViewModels;

namespace InventoryManagementApp.Views.Windows
{
    public partial class RentalJobDetailsWindow : Window
    {
        public RentalJobDetailsWindow(ManageRentalsViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
