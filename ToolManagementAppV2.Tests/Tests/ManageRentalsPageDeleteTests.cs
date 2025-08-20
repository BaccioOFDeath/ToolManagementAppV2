using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Windows.Controls;
using ToolManagementAppV2.Services.Core;
using ToolManagementAppV2.Services.Tools;
using ToolManagementAppV2.Services.Customers;
using ToolManagementAppV2.Services.Rentals;
using ToolManagementAppV2.ViewModels;
using ToolManagementAppV2.Views.Pages;
using ToolManagementAppV2.Views.Windows;
using ToolManagementAppV2.Interfaces;
using ToolManagementAppV2.Models.Domain;
using Xunit;

namespace ToolManagementAppV2.Tests.Tests
{
    public class ManageRentalsPageDeleteTests
    {
        [Fact]
        public async System.Threading.Tasks.Task DeleteRental_RemovesFromDataGridAndClearsSelection()
        {
            var dbPath = Path.GetTempFileName();
            try
            {
                var db = new DatabaseService(dbPath);
                var toolService = new ItemService(db);
                var customerService = new CustomerService(db);
                var rentalService = new RentalService(db);

                var tool = new ItemModel { ToolNumber = "T1" };
                toolService.AddTool(tool);
                var customer = new Customer { Company = "C1" };
                customerService.AddCustomer(customer);
                var cust = customerService.GetAllCustomers().First();

                rentalService.RentTool(tool.ToolID, cust.CustomerID, DateTime.Today, DateTime.Today.AddDays(1));

                var vm = new ManageRentalsViewModel(rentalService, new StubDialogService());
                await vm.LoadRentalsAsync();

                var page = new ManageRentalsPage { DataContext = vm };
                var grid = (Grid)page.Content;
                var border = (Border)grid.Children[1];
                var dataGrid = (DataGrid)border.Child;

                dataGrid.SelectedItem = vm.Rentals.First();

                await vm.DeleteRentalCommand.ExecuteAsync(null);

                Assert.Equal(0, dataGrid.Items.Count);
                Assert.Null(dataGrid.SelectedItem);
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
        public void ShowRentalsFilter(ManageRentalsViewModel viewModel) { }
        public void ShowRentalHistory(ItemModel tool, IEnumerable<RentalModel> history) { }
        public Dictionary<string, string>? ShowImportMapping(IEnumerable<string> headers, IEnumerable<string> properties) => null;
        public Func<ItemModel, IEnumerable<string>>? ShowImageImportMapping() => null;
        public void ShowPrintPreview(System.Windows.Documents.FlowDocument document, string title, string description) { }
        public void ShowPrintLabelDialog() { }
    }
}
