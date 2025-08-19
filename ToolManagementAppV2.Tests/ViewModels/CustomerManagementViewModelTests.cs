using System.IO;
using System.Linq;
using ToolManagementAppV2.Services.Core;
using ToolManagementAppV2.Services.Customers;
using ToolManagementAppV2.Interfaces;
using ToolManagementAppV2.Models.Domain;
using ToolManagementAppV2.ViewModels;
using Xunit;
using System.Threading.Tasks;

namespace ToolManagementAppV2.Tests.ViewModels
{
    public class CustomerManagementViewModelTests
    {
        [Fact]
        public async Task AddCustomerCommand_UsesDialogValues()
        {
            var dbPath = Path.GetTempFileName();
            try
            {
                var db = new DatabaseService(dbPath);
                ICustomerService customerService = new CustomerService(db);
                var dialog = new StubDialogService
                {
                    AddCustomerResult = new CustomerModel
                    {
                        Company = "ACME",
                        Email = "a@b.com",
                        Contact = "John",
                        Phone = "123",
                        Mobile = "456",
                        Address = "Addr"
                    }
                };
                var vm = new CustomerManagementViewModel(customerService, dialog);
                await vm.AddCustomerCommand.ExecuteAsync(null);
                var customers = customerService.GetAllCustomers();
                Assert.Single(customers);
                var c = customers.First();
                Assert.Equal("ACME", c.Company);
                Assert.Equal("a@b.com", c.Email);
                Assert.Equal("John", c.Contact);
                Assert.Equal("123", c.Phone);
                Assert.Equal("456", c.Mobile);
                Assert.Equal("Addr", c.Address);
            }
            finally
            {
                if (File.Exists(dbPath))
                    File.Delete(dbPath);
            }
        }

        [Fact]
        public async Task AddCustomerCommand_CancelledDialog_DoesNotAdd()
        {
            var dbPath = Path.GetTempFileName();
            try
            {
                var db = new DatabaseService(dbPath);
                ICustomerService customerService = new CustomerService(db);
                var dialog = new StubDialogService { AddCustomerResult = null };
                var vm = new CustomerManagementViewModel(customerService, dialog);
                await vm.AddCustomerCommand.ExecuteAsync(null);
                var customers = customerService.GetAllCustomers();
                Assert.Empty(customers);
            }
            finally
            {
                if (File.Exists(dbPath))
                    File.Delete(dbPath);
            }
        }

        [Fact]
        public async Task AddCustomerCommand_ClearsNewCustomerFields()
        {
            var dbPath = Path.GetTempFileName();
            try
            {
                var db = new DatabaseService(dbPath);
                ICustomerService customerService = new CustomerService(db);
                var dialog = new StubDialogService
                {
                    AddCustomerResult = new CustomerModel { Company = "ACME" }
                };
                var vm = new CustomerManagementViewModel(customerService, dialog);

                vm.NewCustomerName = "Temp";
                vm.NewCustomerEmail = "e";
                vm.NewCustomerContact = "c";
                vm.NewCustomerPhone = "p";
                vm.NewCustomerMobile = "m";
                vm.NewCustomerAddress = "a";

                await vm.AddCustomerCommand.ExecuteAsync(null);

                Assert.Equal(string.Empty, vm.NewCustomerName);
                Assert.Equal(string.Empty, vm.NewCustomerEmail);
                Assert.Equal(string.Empty, vm.NewCustomerContact);
                Assert.Equal(string.Empty, vm.NewCustomerPhone);
                Assert.Equal(string.Empty, vm.NewCustomerMobile);
                Assert.Equal(string.Empty, vm.NewCustomerAddress);
            }
            finally
            {
                if (File.Exists(dbPath))
                    File.Delete(dbPath);
            }
        }

        [Fact]
        public void SelectedCustomer_PopulatesNewCustomerFields()
        {
            var dbPath = Path.GetTempFileName();
            try
            {
                var db = new DatabaseService(dbPath);
                ICustomerService customerService = new CustomerService(db);
                customerService.AddCustomer(new ToolManagementAppV2.Models.Domain.Customer
                {
                    Company = "ACME",
                    Email = "a@b.com",
                    Contact = "John",
                    Phone = "123",
                    Mobile = "456",
                    Address = "Addr"
                });
                var existing = customerService.GetAllCustomers().First();
                var vm = new CustomerManagementViewModel(customerService, new StubDialogService());
                vm.SelectedCustomer = existing;

                Assert.Equal("ACME", vm.NewCustomerName);
                Assert.Equal("a@b.com", vm.NewCustomerEmail);
                Assert.Equal("John", vm.NewCustomerContact);
                Assert.Equal("123", vm.NewCustomerPhone);
                Assert.Equal("456", vm.NewCustomerMobile);
                Assert.Equal("Addr", vm.NewCustomerAddress);
            }
            finally
            {
                if (File.Exists(dbPath))
                    File.Delete(dbPath);
            }
        }

        [Fact]
        public void SelectedCustomer_Null_ClearsNewCustomerFields()
        {
            var dbPath = Path.GetTempFileName();
            try
            {
                var db = new DatabaseService(dbPath);
                ICustomerService customerService = new CustomerService(db);
                customerService.AddCustomer(new ToolManagementAppV2.Models.Domain.Customer
                {
                    Company = "ACME",
                    Email = "a@b.com",
                    Contact = "John",
                    Phone = "123",
                    Mobile = "456",
                    Address = "Addr"
                });
                var existing = customerService.GetAllCustomers().First();
                var vm = new CustomerManagementViewModel(customerService, new StubDialogService());
                vm.SelectedCustomer = existing;
                vm.SelectedCustomer = null;

                Assert.Equal(string.Empty, vm.NewCustomerName);
                Assert.Equal(string.Empty, vm.NewCustomerEmail);
                Assert.Equal(string.Empty, vm.NewCustomerContact);
                Assert.Equal(string.Empty, vm.NewCustomerPhone);
                Assert.Equal(string.Empty, vm.NewCustomerMobile);
                Assert.Equal(string.Empty, vm.NewCustomerAddress);
            }
            finally
            {
                if (File.Exists(dbPath))
                    File.Delete(dbPath);
            }
        }

        [Fact]
        public async Task UpdateCustomerCommand_UpdatesSelectedCustomer()
        {
            var dbPath = Path.GetTempFileName();
            try
            {
                var db = new DatabaseService(dbPath);
                ICustomerService customerService = new CustomerService(db);
                customerService.AddCustomer(new ToolManagementAppV2.Models.Domain.Customer { Company = "Old" });
                var existing = customerService.GetAllCustomers().First();
                var vm = new CustomerManagementViewModel(customerService, new StubDialogService());
                vm.SelectedCustomer = existing;
                vm.NewCustomerName = "New";
                vm.NewCustomerEmail = "e@e.com";
                vm.NewCustomerContact = "Bob";
                vm.NewCustomerPhone = "9";
                vm.NewCustomerMobile = "8";
                vm.NewCustomerAddress = "Addr";
                await vm.UpdateCustomerCommand.ExecuteAsync(null);
                var updated = customerService.GetCustomerByID(existing.CustomerID);
                Assert.Equal("New", updated.Company);
                Assert.Equal("e@e.com", updated.Email);
                Assert.Equal("Bob", updated.Contact);
                Assert.Equal("9", updated.Phone);
                Assert.Equal("8", updated.Mobile);
                Assert.Equal("Addr", updated.Address);
            }
            finally
            {
                if (File.Exists(dbPath))
                    File.Delete(dbPath);
            }
        }

        [Fact]
        public async Task SearchCustomersCommand_FiltersCustomers()
        {
            var dbPath = Path.GetTempFileName();
            try
            {
                var db = new DatabaseService(dbPath);
                ICustomerService customerService = new CustomerService(db);
                customerService.AddCustomer(new ToolManagementAppV2.Models.Domain.Customer { Company = "Alpha" });
                customerService.AddCustomer(new ToolManagementAppV2.Models.Domain.Customer { Company = "Beta" });
                var vm = new CustomerManagementViewModel(customerService, new StubDialogService());
                vm.CustomerSearchTerm = "Beta";
                await vm.SearchCustomersCommand.ExecuteAsync(null);
                Assert.Single(vm.Customers);
                Assert.Equal("Beta", vm.Customers.First().Company);
            }
            finally
            {
                if (File.Exists(dbPath))
                    File.Delete(dbPath);
            }
        }

        [Fact]
        public async Task DeleteCustomerCommand_RemovesSelectedCustomer()
        {
            var dbPath = Path.GetTempFileName();
            try
            {
                var db = new DatabaseService(dbPath);
                ICustomerService customerService = new CustomerService(db);
                customerService.AddCustomer(new ToolManagementAppV2.Models.Domain.Customer { Company = "ACME" });
                customerService.AddCustomer(new ToolManagementAppV2.Models.Domain.Customer { Company = "Beta" });
                var vm = new CustomerManagementViewModel(customerService, new StubDialogService());
                await vm.SearchCustomersCommand.ExecuteAsync(null);
                vm.SelectedCustomer = vm.Customers.First(c => c.Company == "ACME");
                await vm.DeleteCustomerCommand.ExecuteAsync(null);
                var remaining = customerService.GetAllCustomers();
                Assert.Single(remaining);
                Assert.DoesNotContain(remaining, c => c.Company == "ACME");
                Assert.Single(vm.Customers);
                Assert.DoesNotContain(vm.Customers, c => c.Company == "ACME");
            }
            finally
            {
                if (File.Exists(dbPath))
                    File.Delete(dbPath);
            }
        }

        [Fact]
        public void DeleteCustomerCommand_CanExecuteDependsOnSelection()
        {
            var dbPath = Path.GetTempFileName();
            try
            {
                var db = new DatabaseService(dbPath);
                ICustomerService customerService = new CustomerService(db);
                var vm = new CustomerManagementViewModel(customerService, new StubDialogService());
                Assert.False(vm.DeleteCustomerCommand.CanExecute(null));
                vm.SelectedCustomer = new ToolManagementAppV2.Models.Domain.Customer { Company = "ACME" };
                Assert.True(vm.DeleteCustomerCommand.CanExecute(null));
                vm.SelectedCustomer = null;
                Assert.False(vm.DeleteCustomerCommand.CanExecute(null));
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
        public CustomerModel? AddCustomerResult;

        public void ShowInfo(string message, string title) { }
        public bool ShowConfirmation(string message, string title) => false;
        public ToolModel? ShowEditToolDialog(ToolModel tool) => null;
        public void ShowToolDetails(ToolModel tool) { }
        public (CustomerModel customer, DateTime dueDate)? ShowRentToolDialog(ToolModel tool, IEnumerable<CustomerModel> customers) => null;
        public CustomerModel? ShowAddCustomerDialog() => AddCustomerResult;
        public void ShowRentalsFilter(ToolManagementAppV2.ViewModels.ManageRentalsViewModel viewModel) { }
        public void ShowRentalHistory(ToolModel tool, System.Collections.Generic.IEnumerable<RentalModel> history) { }
        public System.Collections.Generic.Dictionary<string, string>? ShowImportMapping(System.Collections.Generic.IEnumerable<string> headers, System.Collections.Generic.IEnumerable<string> properties) => null;
        public System.Func<ToolModel, System.Collections.Generic.IEnumerable<string>>? ShowImageImportMapping() => null;
        public void ShowPrintPreview(System.Windows.Documents.FlowDocument document, string title, string description) { }
        public void ShowPrintLabelDialog() { }
    }
}
