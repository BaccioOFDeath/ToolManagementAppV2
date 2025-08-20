using System.IO;
using System.Windows.Controls;
using System.Collections.Generic;
using ToolManagementAppV2.Services.Core;
using ToolManagementAppV2.Services.Customers;
using ToolManagementAppV2.ViewModels;
using ToolManagementAppV2.Views.Pages;
using ToolManagementAppV2.Views.Windows;
using ToolManagementAppV2.Interfaces;
using Xunit;

namespace ToolManagementAppV2.Tests.Tests
{
    public class CustomersPageBindingTests
    {
        [Fact]
        public void SearchBox_TwoWayBindsToCustomerSearchTerm()
        {
            var dbPath = Path.GetTempFileName();
            try
            {
                var db = new DatabaseService(dbPath);
                var customerService = new CustomerService(db);
                var vm = new CustomerManagementViewModel(customerService, new StubDialogService());
                var page = new CustomersPage { DataContext = vm };

                var grid = (Grid)page.Content;
                var border = (Border)grid.Children[0];
                var stack = (StackPanel)border.Child;
                var box = (TextBox)stack.Children[0];

                box.Text = "Alpha";
                Assert.Equal("Alpha", vm.CustomerSearchTerm);

                vm.CustomerSearchTerm = "Beta";
                Assert.Equal("Beta", box.Text);
            }
            finally
            {
                if (File.Exists(dbPath))
                    File.Delete(dbPath);
            }
        }

        [Fact]
        public async System.Threading.Tasks.Task DataGrid_ShowsContactNames()
        {
            var dbPath = Path.GetTempFileName();
            try
            {
                var db = new DatabaseService(dbPath);
                var customerService = new CustomerService(db);
                // Add a customer with a contact name
                customerService.AddCustomer(new CustomerModel { Company = "ACME", Contact = "John Doe" });
                var vm = new CustomerManagementViewModel(customerService, new StubDialogService());
                await vm.LoadCustomersAsync();

                var page = new CustomersPage { DataContext = vm };
                var grid = (Grid)page.Content;
                var border = (Border)grid.Children[1];
                var dataGrid = (DataGrid)border.Child;

                dataGrid.UpdateLayout();
                var cell = (TextBlock)dataGrid.Columns[1].GetCellContent(dataGrid.Items[0]);
                Assert.Equal("John Doe", cell.Text);
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
        public ItemModel? ShowEditToolDialog(ItemModel tool) => null;
        public void ShowToolDetails(ItemModel tool) { }
        public (CustomerModel customer, DateTime dueDate)? ShowRentToolDialog(ItemModel tool, IEnumerable<CustomerModel> customers) => null;
        public CustomerModel? ShowAddCustomerDialog() => null;
        public void ShowRentalsFilter(ToolManagementAppV2.ViewModels.ManageRentalsViewModel viewModel) { }
        public void ShowRentalHistory(ItemModel tool, System.Collections.Generic.IEnumerable<RentalModel> history) { }
        public System.Collections.Generic.Dictionary<string, string>? ShowImportMapping(System.Collections.Generic.IEnumerable<string> headers, System.Collections.Generic.IEnumerable<string> properties) => null;
        public System.Func<ItemModel, System.Collections.Generic.IEnumerable<string>>? ShowImageImportMapping() => null;
        public void ShowPrintPreview(System.Windows.Documents.FlowDocument document, string title, string description) { }
        public void ShowPrintLabelDialog() { }
    }
}
