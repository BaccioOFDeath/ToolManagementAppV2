using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using InventoryManagementApp.ViewModels;

namespace InventoryManagementApp.Views.Pages
{
    public partial class CalibrationPage : Page
    {
        public CalibrationPage()
        {
            InitializeComponent();
            Loaded += CalibrationPage_Loaded;
        }

        private async void CalibrationPage_Loaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is CalibrationManagementViewModel vm)
            {
                await vm.LoadCalibrationCommand.ExecuteAsync(null);
            }
        }

        private void CalibrationRow_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (DataContext is CalibrationManagementViewModel vm && vm.OpenCalibrationDetailsCommand.CanExecute(null))
            {
                UiActionGuard.Run(this, "Calibration", () => vm.OpenCalibrationDetailsCommand.Execute(null));
            }
        }

        private void CalibrationRow_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            GridContextMenuSelection.SelectRow(sender, e);
        }
    }
}
