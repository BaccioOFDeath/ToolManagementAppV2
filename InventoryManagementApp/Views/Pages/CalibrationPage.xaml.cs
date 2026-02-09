using System.Windows;
using System.Windows.Controls;
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
    }
}
