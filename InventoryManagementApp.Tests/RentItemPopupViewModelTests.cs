using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using InventoryManagementApp.Interfaces;
using InventoryManagementApp.ViewModels.Rental;
using InventoryManagementApp.Models.ImportExport;
using InventoryManagementApp.ViewModels;
using InventoryManagementApp.Services.Core;
using InventoryManagementApp.Services.Settings;
using System.Windows.Documents;
using System.IO;

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

        [Fact]
        public void CustomerSearchText_FiltersByMobileAndAddress()
        {
            var first = new CustomerModel
            {
                CustomerID = 1,
                Company = "North Yard",
                Contact = "Alex Stone",
                Email = "alex@example.com",
                Phone = "111-111",
                Mobile = "022-555-9911",
                Address = "7 Shelf Road"
            };
            var second = new CustomerModel
            {
                CustomerID = 2,
                Company = "South Workshop",
                Contact = "Morgan Lee",
                Email = "morgan@example.com",
                Phone = "222-222",
                Mobile = "027-000-0000",
                Address = "18 Bench Lane"
            };
            var vm = new RentItemPopupViewModel(new ItemModel(), new[] { first, second }, new RecordingCustomerService(), new StubDialogService());

            vm.CustomerSearchText = "bench";

            var match = Assert.Single(vm.FilteredCustomers);
            Assert.Same(second, match);
            Assert.Same(second, vm.SelectedCustomer);
            Assert.Equal("1 of 2 customers shown", vm.CustomerCountSummary);
        }

        [Fact]
        public void ClearCustomerSearch_RestoresTheFullCustomerList()
        {
            var first = new CustomerModel { CustomerID = 1, Company = "North Yard", Mobile = "022-555-9911" };
            var second = new CustomerModel { CustomerID = 2, Company = "South Workshop", Mobile = "027-000-0000" };
            var vm = new RentItemPopupViewModel(new ItemModel(), new[] { first, second }, new RecordingCustomerService(), new StubDialogService());
            vm.CustomerSearchText = "022";

            vm.ClearCustomerSearchCommand.Execute(null);

            Assert.Equal(2, vm.FilteredCustomers.Count);
            Assert.Contains(first, vm.FilteredCustomers);
            Assert.Contains(second, vm.FilteredCustomers);
            Assert.Equal("2 customers available", vm.CustomerCountSummary);
        }

        [Fact]
        public void Confirm_UsesSelectedCustomerAndSelectedDueDate()
        {
            var customer = new CustomerModel { CustomerID = 3, Company = "Ready Rentals" };
            var dueDate = new DateTime(2026, 6, 25);
            var vm = new RentItemPopupViewModel(new ItemModel(), new[] { customer }, new RecordingCustomerService(), new StubDialogService())
            {
                SelectedCustomer = customer,
                SelectedDueDate = dueDate
            };

            vm.CheckOutCommand.Execute(null);

            Assert.Same(customer, vm.SelectedCustomerResult);
            Assert.Equal(dueDate, vm.SelectedDueDateResult);
        }

        [Fact]
        public void SetRentalDaysCommand_AcceptsQuickDayIntegerParameter()
        {
            var vm = new RentItemPopupViewModel(new ItemModel(), Array.Empty<CustomerModel>(), new RecordingCustomerService(), new StubDialogService());

            vm.SetRentalDaysCommand.Execute(14);

            Assert.Equal(14, vm.RentalDays);
            Assert.Equal(DateTime.Today.AddDays(14), vm.SelectedDueDate);
        }

        [Fact]
        public async Task LoadQuickRentalDaysAsync_UsesConfiguredButtonsAndFirstDefault()
        {
            var dbPath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.db");
            try
            {
                await using var db = new DatabaseService(dbPath);
                var settings = new SettingsService(db);
                var rentalConfig = new RentalConfigurationService(settings);
                await rentalConfig.SetQuickRentalDaysAsync(new[] { 5, 10, 21 });

                var vm = new RentItemPopupViewModel(new ItemModel(), Array.Empty<CustomerModel>(), new RecordingCustomerService(), new StubDialogService(), rentalConfig);

                await vm.LoadQuickRentalDaysAsync();

                Assert.Equal(new[] { 5, 10, 21 }, vm.QuickRentalDays);
                Assert.Equal(5, vm.RentalDays);
                Assert.Equal(DateTime.Today.AddDays(5), vm.SelectedDueDate);
            }
            finally
            {
                if (File.Exists(dbPath))
                {
                    File.Delete(dbPath);
                }
            }
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
