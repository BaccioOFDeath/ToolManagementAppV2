using System.IO;
using System.Windows.Controls;
using System.Collections.Generic;
using ToolManagementAppV2.Services.Core;
using ToolManagementAppV2.Services.Customers;
using ToolManagementAppV2.ViewModels;
using ToolManagementAppV2.Views;
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
    }

    class StubDialogService : IDialogService
    {
        public void ShowInfo(string message, string title) { }
        public bool ShowConfirmation(string message, string title) => false;
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
        public void ShowScannerStatus() { }
    }
}
