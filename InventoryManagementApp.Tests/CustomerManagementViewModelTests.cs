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

        [Fact]
        public async Task LoadCustomersAsync_WhenRefreshFails_ClearsStaleRowsSelectionAndExplainsState()
        {
            var service = new StubCustomerService();
            service.Customers.Add(new CustomerModel { CustomerID = 1, Company = "Alpha", Contact = "Alex" });
            var dialog = new StubDialogService();
            var vm = new CustomerManagementViewModel(service, dialog);
            await vm.LoadCustomersAsync();
            Assert.Single(vm.Customers);
            Assert.NotNull(vm.SelectedCustomer);

            service.ThrowOnGetAllCustomers = true;
            await vm.LoadCustomersAsync();

            Assert.Empty(vm.Customers);
            Assert.Null(vm.SelectedCustomer);
            Assert.Equal("0 customers shown", vm.CustomerResultsSummary);
            Assert.Equal(string.Empty, vm.NewCustomerName);
            Assert.False(vm.UpdateCustomerCommand.CanExecute(null));
            Assert.False(vm.PrintSelectedCustomerCommand.CanExecute(null));
            Assert.Equal("Customer Load Failed", dialog.LastInfoTitle);
            Assert.Contains("Customer rows were cleared until reload succeeds.", dialog.LastInfoMessage);
        }

        [Fact]
        public async Task SearchCustomersCommand_WhenRefreshFails_ClearsStaleRowsSelectionAndExplainsState()
        {
            var service = new StubCustomerService();
            service.Customers.Add(new CustomerModel { CustomerID = 1, Company = "Alpha", Contact = "Alex" });
            service.Customers.Add(new CustomerModel { CustomerID = 2, Company = "Beta", Contact = "Blair" });
            var dialog = new StubDialogService();
            var vm = new CustomerManagementViewModel(service, dialog);
            await vm.LoadCustomersAsync();
            vm.CustomerSearchTerm = "Alpha";

            service.ThrowOnGetAllCustomers = true;
            await vm.SearchCustomersCommand.ExecuteAsync(null);

            Assert.Empty(vm.Customers);
            Assert.Null(vm.SelectedCustomer);
            Assert.Equal("0 customers shown for \"Alpha\"", vm.CustomerResultsSummary);
            Assert.False(vm.OpenCustomerDetailsCommand.CanExecute(null));
            Assert.False(vm.CopySelectedCustomerCommand.CanExecute(null));
            Assert.Equal("Customer Search Failed", dialog.LastInfoTitle);
            Assert.Contains("Customer rows were cleared until reload succeeds.", dialog.LastInfoMessage);
        }

        [Fact]
        public async Task AddCustomerCommand_WhenMutationThrows_RefreshesRowsAndSelectsSavedCustomer()
        {
            var service = new StubCustomerService { ThrowAfterAddCustomer = true };
            service.Customers.Add(new CustomerModel { CustomerID = 1, Company = "Alpha" });
            var dialog = new StubDialogService { AddCustomerResult = new CustomerModel { CustomerID = 2, Company = "Beta" } };
            var vm = new CustomerManagementViewModel(service, dialog);
            await vm.LoadCustomersAsync();

            await vm.AddCustomerCommand.ExecuteAsync(null);

            Assert.Equal(2, vm.Customers.Count);
            Assert.Equal(2, vm.SelectedCustomer?.CustomerID);
            Assert.True(vm.UpdateCustomerCommand.CanExecute(null));
            Assert.Equal("Add Customer Failed", dialog.LastInfoTitle);
            Assert.Contains("Customer rows were refreshed from saved data where possible.", dialog.LastInfoMessage);
        }

        [Fact]
        public async Task DeleteCustomerCommand_WhenMutationThrows_RefreshesRowsAndClearsDeletedSelection()
        {
            var service = new StubCustomerService { ThrowAfterDeleteCustomer = true };
            service.Customers.Add(new CustomerModel { CustomerID = 1, Company = "Alpha" });
            service.Customers.Add(new CustomerModel { CustomerID = 2, Company = "Beta" });
            var dialog = new StubDialogService();
            var vm = new CustomerManagementViewModel(service, dialog);
            await vm.LoadCustomersAsync();
            vm.SelectedCustomer = vm.Customers.First(c => c.CustomerID == 1);

            await vm.DeleteCustomerCommand.ExecuteAsync(null);

            Assert.Single(vm.Customers);
            Assert.Equal(2, vm.Customers[0].CustomerID);
            Assert.Null(vm.SelectedCustomer);
            Assert.False(vm.DeleteCustomerCommand.CanExecute(null));
            Assert.Equal("Delete Customer Failed", dialog.LastInfoTitle);
            Assert.Contains("Customer rows were refreshed from saved data where possible.", dialog.LastInfoMessage);
        }

        private sealed class StubCustomerService : ICustomerService
        {
            public List<CustomerModel> Customers { get; } = new();
            public bool ThrowOnGetAllCustomers { get; set; }
            public bool ThrowAfterAddCustomer { get; set; }
            public bool ThrowAfterUpdateCustomer { get; set; }
            public bool ThrowAfterDeleteCustomer { get; set; }

            public Task AddCustomerAsync(CustomerModel customer, CancellationToken cancellationToken = default)
            {
                Customers.Add(customer);
                return ThrowAfterAddCustomer
                    ? Task.FromException(new System.InvalidOperationException("add handoff failed"))
                    : Task.CompletedTask;
            }
            public Task UpdateCustomerAsync(CustomerModel customer, CancellationToken cancellationToken = default)
            {
                var idx = Customers.FindIndex(c => c.CustomerID == customer.CustomerID);
                if (idx >= 0) Customers[idx] = customer;

                return ThrowAfterUpdateCustomer
                    ? Task.FromException(new System.InvalidOperationException("update handoff failed"))
                    : Task.CompletedTask;
            }
            public Task DeleteCustomerAsync(int customerID, CancellationToken cancellationToken = default)
            {
                Customers.RemoveAll(c => c.CustomerID == customerID);
                return ThrowAfterDeleteCustomer
                    ? Task.FromException(new System.InvalidOperationException("delete handoff failed"))
                    : Task.CompletedTask;
            }
            public Task<CustomerModel?> GetCustomerByIDAsync(int customerID, CancellationToken cancellationToken = default)
                => Task.FromResult<CustomerModel?>(Customers.First(c => c.CustomerID == customerID));
            public Task<List<CustomerModel>> GetAllCustomersAsync(CancellationToken cancellationToken = default)
            {
                if (ThrowOnGetAllCustomers)
                    return Task.FromException<List<CustomerModel>>(new System.InvalidOperationException("database offline"));

                return Task.FromResult(Customers.ToList());
            }
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
            public string? LastInfoMessage { get; private set; }
            public string? LastInfoTitle { get; private set; }
            public void ShowInfo(string message, string title)
            {
                LastInfoMessage = message;
                LastInfoTitle = title;
            }
            public Task ShowInfoAsync(string message, string title)
            {
                LastInfoMessage = message;
                LastInfoTitle = title;
                return Task.CompletedTask;
            }
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
            public System.Func<ItemModel, IEnumerable<string>>? ShowImageImportMapping() => null;
            public void ShowPrintPreview(FlowDocument document, string title, string description) { }
            public void ShowPrintLabelDialog() { }
        }
    }
}
