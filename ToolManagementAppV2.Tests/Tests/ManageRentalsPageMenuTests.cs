using System.IO;
using System.Linq;
using System.Windows.Controls;
using ToolManagementAppV2.Services.Core;
using ToolManagementAppV2.Services.Rentals;
using ToolManagementAppV2.ViewModels;
using ToolManagementAppV2.Views;
using Xunit;

namespace ToolManagementAppV2.Tests.Tests
{
    public class ManageRentalsPageMenuTests
    {
        [Fact]
        public void ContextMenu_BindsToViewModelCommands()
        {
            var dbPath = Path.GetTempFileName();
            try
            {
                var db = new DatabaseService(dbPath);
                var rentalService = new RentalService(db);
                var vm = new ManageRentalsViewModel(rentalService);
                var page = new ManageRentalsPage { DataContext = vm };

                var grid = (Grid)page.Content;
                var border = (Border)grid.Children[1];
                var dataGrid = (DataGrid)border.Child;
                var menu = dataGrid.ContextMenu;
                var items = menu.Items.OfType<MenuItem>().ToArray();

                Assert.Equal(vm.CheckInCommand, items[0].Command);
                Assert.Equal(vm.ExtendCommand, items[1].Command);
                Assert.Equal(vm.OpenHistoryCommand, items[2].Command);
                Assert.Equal(vm.PrintRentalCommand, items[3].Command);
                Assert.Equal(vm.DeleteRentalCommand, items[4].Command);
            }
            finally
            {
                if (File.Exists(dbPath))
                    File.Delete(dbPath);
            }
        }

        [Fact]
        public void Toolbar_CheckInButton_BindsToCheckInCommand()
        {
            var dbPath = Path.GetTempFileName();
            try
            {
                var db = new DatabaseService(dbPath);
                var rentalService = new RentalService(db);
                var vm = new ManageRentalsViewModel(rentalService);
                var page = new ManageRentalsPage { DataContext = vm };

                var grid = (Grid)page.Content;
                var stack = (StackPanel)grid.Children[0];
                var toolbar = (ToolBar)stack.Children[1];
                var button = (Button)toolbar.Items[0];

                Assert.Equal(vm.CheckInCommand, button.Command);
            }
            finally
            {
                if (File.Exists(dbPath))
                    File.Delete(dbPath);
            }
        }
    }
}
