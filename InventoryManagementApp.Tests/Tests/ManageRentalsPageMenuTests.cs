using System.IO;
using System.Linq;
using System.Windows.Controls;
using System.Collections.Generic;
using InventoryManagementApp.Services.Core;
using InventoryManagementApp.Services.Rentals;
using InventoryManagementApp.ViewModels;
using InventoryManagementApp.Views.Pages;
using InventoryManagementApp.Views.Windows;
using InventoryManagementApp.Interfaces;
using Xunit;

namespace InventoryManagementApp.Tests.Tests
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
                var vm = new ManageRentalsViewModel(rentalService, new StubDialogService());
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
                var vm = new ManageRentalsViewModel(rentalService, new StubDialogService());
                var page = new ManageRentalsPage { DataContext = vm };

                var grid = (Grid)page.Content;
                var toolbar = (ToolBar)grid.Children[0];
                var button = (Button)toolbar.Items[1];

                Assert.Equal(vm.CheckInCommand, button.Command);
            }
            finally
            {
                if (File.Exists(dbPath))
                    File.Delete(dbPath);
            }
        }

        [Fact]
        public void Toolbar_FilterButton_BindsToOpenFilterWindowCommand()
        {
            var dbPath = Path.GetTempFileName();
            try
            {
                var db = new DatabaseService(dbPath);
                var rentalService = new RentalService(db);
                var vm = new ManageRentalsViewModel(rentalService, new StubDialogService());
                var page = new ManageRentalsPage { DataContext = vm };

                var grid = (Grid)page.Content;
                var toolbar = (ToolBar)grid.Children[0];
                var button = (Button)toolbar.Items[0];

                Assert.Equal(vm.OpenFilterWindowCommand, button.Command);
            }
            finally
            {
                if (File.Exists(dbPath))
                    File.Delete(dbPath);
            }
        }
    }

    class StubDialogService : IDialogService
    {
        public void ShowInfo(string message, string title) { }
        public bool ShowConfirmation(string message, string title) => false;
        public ItemModel? ShowEditItemDialog(ItemModel item) => null;
        public void ShowItemDetails(ItemModel item) { }
        public (CustomerModel customer, DateTime dueDate)? ShowRentItemDialog(ItemModel item, IEnumerable<CustomerModel> customers) => null;
        public CustomerModel? ShowAddCustomerDialog() => null;
        public void ShowRentalsFilter(InventoryManagementApp.ViewModels.ManageRentalsViewModel viewModel) { }
        public void ShowRentalHistory(ItemModel item, System.Collections.Generic.IEnumerable<RentalModel> history) { }
        public System.Collections.Generic.Dictionary<string, string>? ShowImportMapping(System.Collections.Generic.IEnumerable<string> headers, System.Collections.Generic.IEnumerable<string> properties) => null;
        public System.Func<ItemModel, System.Collections.Generic.IEnumerable<string>>? ShowImageImportMapping() => null;
        public void ShowPrintPreview(System.Windows.Documents.FlowDocument document, string title, string description) { }
        public void ShowPrintLabelDialog() { }
    }
}
