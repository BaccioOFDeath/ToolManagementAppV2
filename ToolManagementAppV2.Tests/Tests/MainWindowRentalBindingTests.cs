using System.Windows.Controls;
using System.Windows.Data;
using ToolManagementAppV2;
using ToolManagementAppV2.ViewModels;
using Xunit;

namespace ToolManagementAppV2.Tests.Tests
{
    public class MainWindowRentalBindingTests
    {
        [Fact]
        public void RentalsList_BindsToActiveRentalsCollection()
        {
            var window = new MainWindow();
            var vm = Assert.IsType<MainViewModel>(window.DataContext);

            Assert.True(BindingOperations.IsDataBound(window.RentalsList, ItemsControl.ItemsSourceProperty));
            Assert.Same(vm.ActiveRentals, window.RentalsList.ItemsSource);
        }
    }
}
