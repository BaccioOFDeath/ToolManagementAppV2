using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
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

        private void HistoryRow_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (DataContext is RentalHistoryViewModel vm && vm.OpenDetailsCommand.CanExecute(null))
            {
                vm.OpenDetailsCommand.Execute(null);
            }
        }

        private void HistoryRow_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is DataGridRow row && !row.IsSelected)
            {
                row.IsSelected = true;
                e.Handled = true;
            }
        }
    }
}