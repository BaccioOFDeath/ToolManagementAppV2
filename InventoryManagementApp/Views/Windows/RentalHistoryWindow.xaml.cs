using System.Windows;
using InventoryManagementApp.ViewModels.Rental;
using InventoryManagementApp.Utilities.Extensions;

namespace InventoryManagementApp.Views.Windows
{
    public partial class RentalHistoryWindow : Window
    {
        public RentalHistoryWindow()
        {
            InitializeComponent();
            this.DisposeDataContextOnUnload();
        }

        public RentalHistoryWindow(RentalHistoryViewModel viewModel) : this()
        {
            DataContext = viewModel;
        }
    }
}
