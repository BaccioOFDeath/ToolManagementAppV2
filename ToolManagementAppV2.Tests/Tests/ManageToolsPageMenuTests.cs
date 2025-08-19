using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Controls;
using ToolManagementAppV2.Interfaces;
using ToolManagementAppV2.Services.Core;
using ToolManagementAppV2.Services.Tools;
using ToolManagementAppV2.Services.Customers;
using ToolManagementAppV2.Services.Rentals;
using ToolManagementAppV2.ViewModels;
using ToolManagementAppV2.Views.Pages;
using ToolManagementAppV2.Views.Windows;
using Xunit;

namespace ToolManagementAppV2.Tests.Tests
{
    public class ManageToolsPageMenuTests
    {
        [Fact]
        public void ContextMenu_BindsToSecondaryCommands()
        {
            var dbPath = Path.GetTempFileName();
            try
            {
                var db = new DatabaseService(dbPath);
                var toolService = new ToolService(db);
                var customerService = new CustomerService(db);
                var rentalService = new RentalService(db, toolService);
                var dialog = new StubDialogService();
                var vm = new ToolManagementViewModel(toolService, customerService, rentalService, dialog);
                var page = new ManageToolsPage { DataContext = vm };

                var grid = (Grid)page.Content;
                var border = (Border)grid.Children[0];
                var innerGrid = (Grid)border.Child;
                var listView = (ListView)innerGrid.Children[1];
                var menu = listView.ContextMenu;
                var items = menu.Items.OfType<MenuItem>().ToArray();

                Assert.Equal(vm.OpenRentalsCommand, items[0].Command);
                Assert.Equal(vm.DeleteToolCommand, items[1].Command);
            }
            finally
            {
                if (File.Exists(dbPath))
                    File.Delete(dbPath);
            }
        }

        [Fact]
        public void PrimaryButtons_BindToPrimaryCommands()
        {
            var dbPath = Path.GetTempFileName();
            try
            {
                var db = new DatabaseService(dbPath);
                var toolService = new ToolService(db);
                var customerService = new CustomerService(db);
                var rentalService = new RentalService(db, toolService);
                var dialog = new StubDialogService();
                var vm = new ToolManagementViewModel(toolService, customerService, rentalService, dialog);
                var page = new ManageToolsPage { DataContext = vm };

                var grid = (Grid)page.Content;
                var border = (Border)grid.Children[0];
                var innerGrid = (Grid)border.Child;
                var stack = (StackPanel)innerGrid.Children[2];

                Assert.Equal(3, stack.Children.Count);
                Assert.Equal(vm.EditToolCommand, ((Button)stack.Children[0]).Command);
                Assert.Equal(vm.ViewDetailsCommand, ((Button)stack.Children[1]).Command);
                Assert.Equal(vm.NewToolCommand, ((Button)stack.Children[2]).Command);
            }
            finally
            {
                if (File.Exists(dbPath))
                    File.Delete(dbPath);
            }
        }
        class StubDialogService : IDialogService
        {
            public void ShowInfo(string message, string title) { }
            public bool ShowConfirmation(string message, string title) => true;
            public ToolModel? ShowEditToolDialog(ToolModel tool) => null;
            public void ShowToolDetails(ToolModel tool) { }
            public (CustomerModel customer, DateTime dueDate)? ShowRentToolDialog(ToolModel tool, IEnumerable<CustomerModel> customers) => null;
            public CustomerModel? ShowAddCustomerDialog() => null;
            public void ShowRentalsFilter(ToolManagementAppV2.ViewModels.ManageRentalsViewModel viewModel) { }
            public void ShowRentalHistory(ToolModel tool, System.Collections.Generic.IEnumerable<RentalModel> history) { }
            public System.Collections.Generic.Dictionary<string, string>? ShowImportMapping(System.Collections.Generic.IEnumerable<string> headers, System.Collections.Generic.IEnumerable<string> properties) => null;
            public System.Func<ToolModel, System.Collections.Generic.IEnumerable<string>>? ShowImageImportMapping() => null;
            public void ShowPrintPreview(System.Windows.Documents.FlowDocument document, string title, string description) { }
            public void ShowPrintLabelDialog() { }
        }
    }
}
