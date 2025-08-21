using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Windows.Controls;
using InventoryManagementApp.Services.Core;
using InventoryManagementApp.Services.Items;
using InventoryManagementApp.Services.Customers;
using InventoryManagementApp.Services.Rentals;
using InventoryManagementApp.ViewModels;
using InventoryManagementApp.Views.Pages;
using InventoryManagementApp.Views.Windows;
using InventoryManagementApp.Interfaces;
using InventoryManagementApp.Models.Domain;
using Xunit;

namespace InventoryManagementApp.Tests.Tests
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
                var itemService = new ItemService(db);
                var customerService = new CustomerService(db);
                var rentalService = new RentalService(db);

                var item = new ItemModel { ItemNumber = "T1" };
                itemService.AddItem(item);
                var customer = new Customer { Company = "C1" };
                customerService.AddCustomer(customer);
                var cust = customerService.GetAllCustomers().First();

                rentalService.RentItem(item.ItemID, cust.CustomerID, DateTime.Today, DateTime.Today.AddDays(1));

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
        public ItemModel? ShowEditItemDialog(ItemModel item) => null;
        public void ShowItemDetails(ItemModel item) { }
        public (CustomerModel customer, DateTime dueDate)? ShowRentItemDialog(ItemModel item, IEnumerable<CustomerModel> customers) => null;
        public CustomerModel? ShowAddCustomerDialog() => null;
        public void ShowRentalsFilter(ManageRentalsViewModel viewModel) { }
        public void ShowRentalHistory(ItemModel item, IEnumerable<RentalModel> history) { }
        public Dictionary<string, string>? ShowImportMapping(IEnumerable<string> headers, IEnumerable<string> properties) => null;
        public Func<ItemModel, IEnumerable<string>>? ShowImageImportMapping() => null;
        public void ShowPrintPreview(System.Windows.Documents.FlowDocument document, string title, string description) { }
        public void ShowPrintLabelDialog() { }
    }
}
