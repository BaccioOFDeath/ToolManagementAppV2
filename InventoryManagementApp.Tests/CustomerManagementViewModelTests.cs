using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Documents;
using InventoryManagementApp.Interfaces;
using InventoryManagementApp.ViewModels;
using InventoryManagementApp.Models.ImportExport;
using Xunit;

namespace InventoryManagementApp.Tests
{
    public class CustomerManagementViewModelTests
    {
        [Fact]
        public async Task AddCustomerCommand_AddsCustomer()
        {
            var service = new StubCustomerService();
            var dialog = new StubDialogService { AddCustomerResult = new CustomerModel { CustomerID = 1, Company = "A" } };
            var vm = new CustomerManagementViewModel(service, dialog);

            await vm.AddCustomerCommand.ExecuteAsync(null);

            Assert.Single(vm.Customers);
            Assert.Equal("A", vm.Customers[0].Company);
        }

        [Fact]
        public async Task EditCustomerCommand_EditsCustomer()
        {
            var service = new StubCustomerService();
            var existing = new CustomerModel { CustomerID = 1, Company = "A" };
            service.Customers.Add(existing);
            var dialog = new StubDialogService { EditCustomerResult = new CustomerModel { CustomerID = 1, Company = "B" } };
            var vm = new CustomerManagementViewModel(service, dialog);
            await vm.LoadCustomersAsync();
            vm.SelectedCustomer = vm.Customers.First();

            await vm.EditCustomerCommand.ExecuteAsync(null);

            Assert.Equal("B", vm.Customers[0].Company);
        }

        [Fact]
        public async Task DeleteCustomerCommand_RemovesCustomer()
        {
            var service = new StubCustomerService();
            var customer = new CustomerModel { CustomerID = 1, Company = "A" };
            service.Customers.Add(customer);
            var dialog = new StubDialogService();
            var vm = new CustomerManagementViewModel(service, dialog);
            await vm.LoadCustomersAsync();

            await vm.DeleteCustomerFromRowCommand.ExecuteAsync(vm.Customers.First());

            Assert.Empty(vm.Customers);
        }

        [Fact]
        public async Task ClearCustomerSearchCommand_ResetsSearch()
        {
            var service = new StubCustomerService();
            service.Customers.Add(new CustomerModel { CustomerID = 1, Company = "Alpha" });
            service.Customers.Add(new CustomerModel { CustomerID = 2, Company = "Beta" });
            var dialog = new StubDialogService();
            var vm = new CustomerManagementViewModel(service, dialog);
            await vm.LoadCustomersAsync();
            vm.CustomerSearchTerm = "Alpha";
            await vm.SearchCustomersCommand.ExecuteAsync(null);
            Assert.Single(vm.Customers);

            await vm.ClearCustomerSearchCommand.ExecuteAsync(null);

            Assert.Equal(2, vm.Customers.Count);
            Assert.Equal(string.Empty, vm.CustomerSearchTerm);
        }

        private sealed class StubCustomerService : ICustomerService
        {
            public List<CustomerModel> Customers { get; } = new();
            public Task AddCustomerAsync(CustomerModel customer, CancellationToken cancellationToken = default)
            {
                Customers.Add(customer);
                return Task.CompletedTask;
            }
            public Task UpdateCustomerAsync(CustomerModel customer, CancellationToken cancellationToken = default)
            {
                var idx = Customers.FindIndex(c => c.CustomerID == customer.CustomerID);
                if (idx >= 0) Customers[idx] = customer;
                return Task.CompletedTask;
            }
            public Task DeleteCustomerAsync(int customerID, CancellationToken cancellationToken = default)
            {
                Customers.RemoveAll(c => c.CustomerID == customerID);
                return Task.CompletedTask;
            }
            public Task<CustomerModel?> GetCustomerByIDAsync(int customerID, CancellationToken cancellationToken = default)
                => Task.FromResult<CustomerModel?>(Customers.First(c => c.CustomerID == customerID));
            public Task<List<CustomerModel>> GetAllCustomersAsync(CancellationToken cancellationToken = default)
                => Task.FromResult(Customers.ToList());
            public Task<int> CountCustomersAsync(CancellationToken cancellationToken = default)
                => Task.FromResult(Customers.Count);
            public Task<List<CustomerModel>> SearchCustomersAsync(string searchTerm, CancellationToken cancellationToken = default)
                => Task.FromResult(Customers.Where(c => c.Company?.Contains(searchTerm) == true).ToList());
            public Task<CustomerImportResult> ImportCustomersFromCsvAsync(string filePath, IDictionary<string, string> map, CancellationToken cancellationToken = default)
                => Task.FromResult(new CustomerImportResult());
            public Task ExportCustomersToCsvAsync(string filePath, CancellationToken cancellationToken = default)
                => Task.CompletedTask;
            public Task<int> ImportCustomersAsync(string filePath, IDataImporter<CustomerModel> importer, CancellationToken cancellationToken = default)
                => Task.FromResult(0);
            public Task ExportCustomersAsync(string filePath, IDataExporter<CustomerModel> exporter, CancellationToken cancellationToken = default)
                => Task.CompletedTask;
        }

        private sealed class StubDialogService : IDialogService
        {
            public CustomerModel? AddCustomerResult { get; set; }
            public CustomerModel? EditCustomerResult { get; set; }
            public void ShowInfo(string message, string title) { }
            public Task ShowInfoAsync(string message, string title) => Task.CompletedTask;
            public bool ShowConfirmation(string message, string title) => true;
            public Task<bool> ShowConfirmationAsync(string message, string title) => Task.FromResult(true);
            public ItemModel? ShowEditItemDialog(ItemModel item) => null;
            public Task<ItemModel?> ShowEditItemDialogAsync(ItemModel item) => Task.FromResult<ItemModel?>(null);
            public void ShowItemDetails(ItemModel item) { }
            public (CustomerModel customer, System.DateTime dueDate)? ShowRentItemDialog(ItemModel item, IEnumerable<CustomerModel> customers) => null;
            public CustomerModel? ShowAddCustomerDialog() => AddCustomerResult;
            public CustomerModel? ShowEditCustomerDialog(CustomerModel customer) => EditCustomerResult;
            public void ShowRentalsFilter(ManageRentalsViewModel viewModel) { }
            public void ShowRentalHistory(ItemModel item, IEnumerable<RentalModel> history) { }
            public Dictionary<string, string>? ShowImportMapping(IEnumerable<string> headers, IEnumerable<string> properties, IEnumerable<string>? requiredPropertyNames = null) => null;
            public Func<ItemModel, IEnumerable<string>>? ShowImageImportMapping() => null;
            public void ShowPrintPreview(FlowDocument document, string title, string description) { }
            public void ShowPrintLabelDialog() { }
        }
    }
}
