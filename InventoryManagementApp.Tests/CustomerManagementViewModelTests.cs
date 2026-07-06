using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Documents;
using InventoryManagementApp.Interfaces;
using InventoryManagementApp.Models.ImportExport;
using InventoryManagementApp.ViewModels;
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
            Assert.Equal("Showing all customers", vm.CustomerFilterStatus);
        }

        [Fact]
        public async Task SearchCustomersCommand_UsesServiceSearchForFilteredDirectoryAndStatus()
        {
            var service = new StubCustomerService();
            service.Customers.Add(new CustomerModel { CustomerID = 2, Company = "Beta Builders", Contact = "Blair", Phone = "555-2000" });
            service.Customers.Add(new CustomerModel { CustomerID = 1, Company = "Alpha Supply", Contact = "Alex", Email = "alpha@example.test" });
            service.Customers.Add(new CustomerModel { CustomerID = 3, Company = "Gamma Tools", Contact = "Gale", Mobile = "555-3000" });
            var dialog = new StubDialogService();
            var vm = new CustomerManagementViewModel(service, dialog);
            await vm.LoadCustomersAsync();

            vm.CustomerSearchTerm = "555";
            await vm.SearchCustomersCommand.ExecuteAsync(null);

            Assert.Equal(1, service.SearchCustomersCallCount);
            Assert.Equal("555", service.LastSearchTerm);
            Assert.Equal(2, vm.Customers.Count);
            Assert.Equal(2, vm.CustomerDirectoryMatchCount);
            Assert.Equal(0, vm.CustomerDirectoryOmittedCount);
            Assert.False(vm.IsCustomerDirectoryWindowCapped);
            Assert.Equal("Beta Builders", vm.Customers[0].Company);
            Assert.Equal("Gamma Tools", vm.Customers[1].Company);
            Assert.Equal("Filtered by \"555\"", vm.CustomerFilterStatus);
            Assert.Contains("2 visible customers ready", vm.CustomerPrintSummary);
            Assert.True(vm.PrintCustomerDirectoryCommand.CanExecute(null));
        }

        [Fact]
        public async Task LoadCustomersAsync_BoundsLargeCustomerDirectoryAndKeepsFullMatchContext()
        {
            var service = new StubCustomerService();
            for (var i = 1; i <= 620; i++)
            {
                service.Customers.Add(new CustomerModel
                {
                    CustomerID = i,
                    Company = $"Customer {i:000}",
                    Contact = $"Contact {i:000}",
                    Phone = $"555-{i:0000}",
                    Email = $"customer{i:000}@example.test"
                });
            }

            var dialog = new StubDialogService();
            var vm = new CustomerManagementViewModel(service, dialog);

            await vm.LoadCustomersAsync();

            Assert.Equal(500, vm.Customers.Count);
            Assert.Equal(620, vm.CustomerDirectoryMatchCount);
            Assert.Equal(500, vm.CustomerDirectoryVisibleCount);
            Assert.Equal(120, vm.CustomerDirectoryOmittedCount);
            Assert.True(vm.IsCustomerDirectoryWindowCapped);
            Assert.Equal("500 of 620 customers shown", vm.CustomerResultsSummary);
            Assert.Contains("first 500 of 620 shown", vm.CustomerFilterStatus);
            Assert.Contains("Showing first 500 of 620 matching customers", vm.CustomerVisibleWindowSummary);
            Assert.Contains("Print preview includes the first 250 of 500 shown customers; 620 matched", vm.CustomerPrintSummary);
            Assert.DoesNotContain(vm.Customers, c => c.CustomerID == 620);

            vm.PrintCustomerDirectoryCommand.Execute(null);

            var text = new TextRange(dialog.LastPrintPreviewDocument!.ContentStart, dialog.LastPrintPreviewDocument.ContentEnd).Text;
            Assert.Contains("Matched: 620", text);
            Assert.Contains("Visible: 500", text);
            Assert.Contains("Printed: 250", text);
            Assert.Contains("Omitted: 370", text);
            Assert.Contains("120 additional matching customers are outside the live grid", text);
            Assert.DoesNotContain("Customer 620", text);
        }

        [Fact]
        public async Task LoadCustomersAsync_WhenRefreshFails_PreservesRowsSelectionAndExplainsState()
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

            Assert.Single(vm.Customers);
            Assert.Equal(1, vm.SelectedCustomer?.CustomerID);
            Assert.Equal("1 customer shown", vm.CustomerResultsSummary);
            Assert.Equal("Showing all customers", vm.CustomerFilterStatus);
            Assert.Contains("1 visible customer ready", vm.CustomerPrintSummary);
            Assert.Equal("Alpha", vm.NewCustomerName);
            Assert.True(vm.UpdateCustomerCommand.CanExecute(null));
            Assert.True(vm.PrintSelectedCustomerCommand.CanExecute(null));
            Assert.True(vm.PrintCustomerDirectoryCommand.CanExecute(null));
            Assert.Equal("Customer Load Failed", dialog.LastInfoTitle);
            Assert.Contains("Existing customer rows were kept when available", dialog.LastInfoMessage);
        }

        [Fact]
        public async Task SearchCustomersCommand_WhenRefreshFails_PreservesRowsSelectionAndExplainsState()
        {
            var service = new StubCustomerService();
            service.Customers.Add(new CustomerModel { CustomerID = 1, Company = "Alpha", Contact = "Alex" });
            service.Customers.Add(new CustomerModel { CustomerID = 2, Company = "Beta", Contact = "Blair" });
            var dialog = new StubDialogService();
            var vm = new CustomerManagementViewModel(service, dialog);
            await vm.LoadCustomersAsync();
            vm.SelectedCustomer = vm.Customers.First(c => c.CustomerID == 2);
            vm.CustomerSearchTerm = "Alpha";

            service.ThrowOnSearchCustomers = true;
            await vm.SearchCustomersCommand.ExecuteAsync(null);

            Assert.Equal(2, vm.Customers.Count);
            Assert.Equal(2, vm.SelectedCustomer?.CustomerID);
            Assert.Equal("2 customers shown for \"Alpha\"", vm.CustomerResultsSummary);
            Assert.Equal("Filtered by \"Alpha\"", vm.CustomerFilterStatus);
            Assert.True(vm.OpenCustomerDetailsCommand.CanExecute(null));
            Assert.True(vm.CopySelectedCustomerCommand.CanExecute(null));
            Assert.True(vm.PrintCustomerDirectoryCommand.CanExecute(null));
            Assert.Equal("Customer Search Failed", dialog.LastInfoTitle);
            Assert.Contains("Existing customer rows were kept when available", dialog.LastInfoMessage);
        }

        [Fact]
        public async Task PrintCustomerDirectory_BoundsLargeDirectoryAndAddsHandoffContext()
        {
            var service = new StubCustomerService();
            for (var i = 1; i <= 260; i++)
            {
                service.Customers.Add(new CustomerModel
                {
                    CustomerID = i,
                    Company = $"Customer {i:000}",
                    Contact = $"Contact {i:000}",
                    Phone = $"555-{i:0000}",
                    Mobile = $"555-9{i:000}",
                    Email = $"customer{i:000}@example.test",
                    Address = $"{i} Warehouse Road"
                });
            }

            var dialog = new StubDialogService();
            var vm = new CustomerManagementViewModel(service, dialog);
            await vm.LoadCustomersAsync();

            vm.PrintCustomerDirectoryCommand.Execute(null);

            Assert.Equal("Customer Directory", dialog.LastPrintPreviewTitle);
            Assert.Contains("large-directory limits", dialog.LastPrintPreviewDescription);
            Assert.NotNull(dialog.LastPrintPreviewDocument);
            var text = new TextRange(dialog.LastPrintPreviewDocument!.ContentStart, dialog.LastPrintPreviewDocument.ContentEnd).Text;
            Assert.Contains("Matched: 260", text);
            Assert.Contains("Visible: 260", text);
            Assert.Contains("Printed: 250", text);
            Assert.Contains("Omitted: 10", text);
            Assert.Contains("Large directory limit", text);
            Assert.Contains("Review note", text);
            Assert.DoesNotContain("Customer 260", text);
            Assert.Contains("Print preview includes the first 250 of 260 visible customers", vm.CustomerPrintSummary);
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
            public bool ThrowOnSearchCustomers { get; set; }
            public bool ThrowAfterAddCustomer { get; set; }
            public bool ThrowAfterUpdateCustomer { get; set; }
            public bool ThrowAfterDeleteCustomer { get; set; }
            public int SearchCustomersCallCount { get; private set; }
            public string? LastSearchTerm { get; private set; }

            public Task AddCustomerAsync(CustomerModel customer, CancellationToken cancellationToken = default)
            {
                Customers.Add(customer);
                return ThrowAfterAddCustomer
                    ? Task.FromException(new InvalidOperationException("add handoff failed"))
                    : Task.CompletedTask;
            }

            public Task UpdateCustomerAsync(CustomerModel customer, CancellationToken cancellationToken = default)
            {
                var idx = Customers.FindIndex(c => c.CustomerID == customer.CustomerID);
                if (idx >= 0) Customers[idx] = customer;

                return ThrowAfterUpdateCustomer
                    ? Task.FromException(new InvalidOperationException("update handoff failed"))
                    : Task.CompletedTask;
            }

            public Task DeleteCustomerAsync(int customerID, CancellationToken cancellationToken = default)
            {
                Customers.RemoveAll(c => c.CustomerID == customerID);
                return ThrowAfterDeleteCustomer
                    ? Task.FromException(new InvalidOperationException("delete handoff failed"))
                    : Task.CompletedTask;
            }

            public Task<CustomerModel?> GetCustomerByIDAsync(int customerID, CancellationToken cancellationToken = default)
                => Task.FromResult<CustomerModel?>(Customers.First(c => c.CustomerID == customerID));

            public Task<List<CustomerModel>> GetAllCustomersAsync(CancellationToken cancellationToken = default)
            {
                if (ThrowOnGetAllCustomers)
                    return Task.FromException<List<CustomerModel>>(new InvalidOperationException("database offline"));

                return Task.FromResult(Customers.ToList());
            }

            public Task<int> CountCustomersAsync(CancellationToken cancellationToken = default)
                => Task.FromResult(Customers.Count);

            public Task<List<CustomerModel>> SearchCustomersAsync(string searchTerm, CancellationToken cancellationToken = default)
            {
                SearchCustomersCallCount++;
                LastSearchTerm = searchTerm;

                if (ThrowOnSearchCustomers)
                    return Task.FromException<List<CustomerModel>>(new InvalidOperationException("search offline"));

                return Task.FromResult(Customers.Where(c =>
                    Contains(c.Company, searchTerm) ||
                    Contains(c.Email, searchTerm) ||
                    Contains(c.Contact, searchTerm) ||
                    Contains(c.Phone, searchTerm) ||
                    Contains(c.Mobile, searchTerm) ||
                    Contains(c.Address, searchTerm)).ToList());
            }

            public Task<CustomerImportResult> ImportCustomersFromCsvAsync(string filePath, IDictionary<string, string> map, CancellationToken cancellationToken = default)
                => Task.FromResult(new CustomerImportResult());

            public Task ExportCustomersToCsvAsync(string filePath, CancellationToken cancellationToken = default)
                => Task.CompletedTask;

            public Task<int> ImportCustomersAsync(string filePath, IDataImporter<CustomerModel> importer, CancellationToken cancellationToken = default)
                => Task.FromResult(0);

            public Task ExportCustomersAsync(string filePath, IDataExporter<CustomerModel> exporter, CancellationToken cancellationToken = default)
                => Task.CompletedTask;

            private static bool Contains(string? value, string searchTerm)
                => value?.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) == true;
        }

        private sealed class StubDialogService : IDialogService
        {
            public CustomerModel? AddCustomerResult { get; set; }
            public CustomerModel? EditCustomerResult { get; set; }
            public string? LastInfoMessage { get; private set; }
            public string? LastInfoTitle { get; private set; }
            public FlowDocument? LastPrintPreviewDocument { get; private set; }
            public string? LastPrintPreviewTitle { get; private set; }
            public string? LastPrintPreviewDescription { get; private set; }

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
            public (CustomerModel customer, DateTime dueDate)? ShowRentItemDialog(ItemModel item, IEnumerable<CustomerModel> customers) => null;
            public CustomerModel? ShowAddCustomerDialog() => AddCustomerResult;
            public CustomerModel? ShowEditCustomerDialog(CustomerModel customer) => EditCustomerResult;
            public void ShowRentalsFilter(ManageRentalsViewModel viewModel) { }
            public void ShowRentalHistory(ItemModel item, IEnumerable<RentalModel> history) { }
            public Dictionary<string, string>? ShowImportMapping(IEnumerable<string> headers, IEnumerable<string> properties, IEnumerable<string>? requiredPropertyNames = null) => null;
            public Func<ItemModel, IEnumerable<string>>? ShowImageImportMapping() => null;

            public void ShowPrintPreview(FlowDocument document, string title, string description)
            {
                LastPrintPreviewDocument = document;
                LastPrintPreviewTitle = title;
                LastPrintPreviewDescription = description;
            }

            public void ShowPrintLabelDialog() { }
        }
    }
}
