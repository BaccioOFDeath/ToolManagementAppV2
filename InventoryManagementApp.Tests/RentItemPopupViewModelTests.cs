using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using InventoryManagementApp.Interfaces;
using InventoryManagementApp.ViewModels.Rental;
using InventoryManagementApp.Models.ImportExport;
using InventoryManagementApp.ViewModels;
using System.Windows.Documents;

namespace InventoryManagementApp.Tests
{
    public class RentItemPopupViewModelTests
    {
        [Fact]
        public async Task AddCustomerCommand_InsertsAndSelectsCustomer()
        {
            var existing = new CustomerModel { CustomerID = 1, Company = "Old" };
            var newCustomer = new CustomerModel { CustomerID = 2, Company = "New" };
            var cs = new RecordingCustomerService();
            var ds = new StubDialogService { AddCustomerDialogResult = newCustomer };
            var vm = new RentItemPopupViewModel(new ItemModel(), new[] { existing }, cs, ds);

            await vm.AddCustomerCommand.ExecuteAsync(null);

            Assert.Equal(2, vm.Customers.Count);
            Assert.Same(newCustomer, vm.SelectedCustomer);
            Assert.Same(newCustomer, cs.AddedCustomer);
        }

        [Fact]
        public void CheckOutCommand_BecomesExecutableWhenCustomerIsSelected()
        {
            var customer = new CustomerModel { CustomerID = 1, Company = "Customer" };
            var vm = new RentItemPopupViewModel(new ItemModel(), new[] { customer }, new RecordingCustomerService(), new StubDialogService());

            Assert.False(vm.CheckOutCommand.CanExecute(null));

            vm.SelectedCustomer = customer;

            Assert.True(vm.CheckOutCommand.CanExecute(null));
        }

        private sealed class RecordingCustomerService : ICustomerService
        {
            public CustomerModel? AddedCustomer { get; private set; }
            public Task AddCustomerAsync(CustomerModel customer, CancellationToken cancellationToken = default)
            {
                AddedCustomer = customer;
                return Task.CompletedTask;
            }
            public Task UpdateCustomerAsync(CustomerModel customer, CancellationToken cancellationToken = default) => throw new NotImplementedException();
            public Task DeleteCustomerAsync(int customerID, CancellationToken cancellationToken = default) => throw new NotImplementedException();
            public Task<CustomerModel?> GetCustomerByIDAsync(int customerID, CancellationToken cancellationToken = default) => throw new NotImplementedException();
            public Task<List<CustomerModel>> GetAllCustomersAsync(CancellationToken cancellationToken = default) => throw new NotImplementedException();
            public Task<int> CountCustomersAsync(CancellationToken cancellationToken = default) => throw new NotImplementedException();
            public Task<List<CustomerModel>> SearchCustomersAsync(string searchTerm, CancellationToken cancellationToken = default) => throw new NotImplementedException();
            public Task<CustomerImportResult> ImportCustomersFromCsvAsync(string filePath, IDictionary<string, string> map, CancellationToken cancellationToken = default) => throw new NotImplementedException();
            public Task ExportCustomersToCsvAsync(string filePath, CancellationToken cancellationToken = default) => throw new NotImplementedException();
            public Task<int> ImportCustomersAsync(string filePath, IDataImporter<CustomerModel> importer, CancellationToken cancellationToken = default) => throw new NotImplementedException();
            public Task ExportCustomersAsync(string filePath, IDataExporter<CustomerModel> exporter, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        }

        private sealed class StubDialogService : IDialogService
        {
            public CustomerModel? AddCustomerDialogResult { get; set; }
            public CustomerModel? ShowAddCustomerDialog() => AddCustomerDialogResult;
            public CustomerModel? ShowEditCustomerDialog(CustomerModel customer) => null;
            public void ShowInfo(string message, string title) { }
            public bool ShowConfirmation(string message, string title) => true;
            public ItemModel? ShowEditItemDialog(ItemModel item) => null;
            public void ShowItemDetails(ItemModel item) { }
            public (CustomerModel customer, DateTime dueDate)? ShowRentItemDialog(ItemModel item, IEnumerable<CustomerModel> customers) => null;
            public void ShowRentalsFilter(ManageRentalsViewModel viewModel) { }
            public void ShowRentalHistory(ItemModel item, IEnumerable<RentalModel> history) { }
            public Dictionary<string, string>? ShowImportMapping(IEnumerable<string> headers, IEnumerable<string> properties, IEnumerable<string>? requiredPropertyNames = null) => null;
            public Func<ItemModel, IEnumerable<string>>? ShowImageImportMapping() => null;
            public void ShowPrintPreview(FlowDocument document, string title, string description) { }
            public void ShowPrintLabelDialog() { }
        }
    }
}
