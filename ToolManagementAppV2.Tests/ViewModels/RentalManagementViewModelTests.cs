using System.IO;
using System.Linq;
using ToolManagementAppV2.Services.Core;
using ToolManagementAppV2.Services.Customers;
using ToolManagementAppV2.Interfaces;
using ToolManagementAppV2.Models.Domain;
using ToolManagementAppV2.ViewModels;
using Xunit;

namespace ToolManagementAppV2.Tests.ViewModels
{
    public class RentalManagementViewModelTests
    {
        [Fact]
        public void AddCustomerCommand_UsesDialogValues()
        {
            var dbPath = Path.GetTempFileName();
            try
            {
                var db = new DatabaseService(dbPath);
                ICustomerService customerService = new CustomerService(db);
                var vm = new RentalManagementViewModel(customerService)
                {
                    AddCustomerDialog = () => new CustomerModel
                    {
                        Company = "ACME",
                        Email = "a@b.com",
                        Contact = "John",
                        Phone = "123",
                        Mobile = "456",
                        Address = "Addr"
                    }
                };
                vm.AddCustomerCommand.Execute(null);
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
        public void AddCustomerCommand_CancelledDialog_DoesNotAdd()
        {
            var dbPath = Path.GetTempFileName();
            try
            {
                var db = new DatabaseService(dbPath);
                ICustomerService customerService = new CustomerService(db);
                var vm = new RentalManagementViewModel(customerService)
                {
                    AddCustomerDialog = () => null
                };
                vm.AddCustomerCommand.Execute(null);
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
                var vm = new RentalManagementViewModel(customerService);
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
        public void UpdateCustomerCommand_UpdatesSelectedCustomer()
        {
            var dbPath = Path.GetTempFileName();
            try
            {
                var db = new DatabaseService(dbPath);
                ICustomerService customerService = new CustomerService(db);
                customerService.AddCustomer(new ToolManagementAppV2.Models.Domain.Customer { Company = "Old" });
                var existing = customerService.GetAllCustomers().First();
                var vm = new RentalManagementViewModel(customerService);
                vm.SelectedCustomer = existing;
                vm.NewCustomerName = "New";
                vm.NewCustomerEmail = "e@e.com";
                vm.NewCustomerContact = "Bob";
                vm.NewCustomerPhone = "9";
                vm.NewCustomerMobile = "8";
                vm.NewCustomerAddress = "Addr";
                vm.UpdateCustomerCommand.Execute(null);
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
        public void SearchCustomersCommand_FiltersCustomers()
        {
            var dbPath = Path.GetTempFileName();
            try
            {
                var db = new DatabaseService(dbPath);
                ICustomerService customerService = new CustomerService(db);
                customerService.AddCustomer(new ToolManagementAppV2.Models.Domain.Customer { Company = "Alpha" });
                customerService.AddCustomer(new ToolManagementAppV2.Models.Domain.Customer { Company = "Beta" });
                var vm = new RentalManagementViewModel(customerService);
                vm.CustomerSearchTerm = "Beta";
                vm.SearchCustomersCommand.Execute(null);
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
        public void DeleteCustomerCommand_RemovesSelectedCustomer()
        {
            var dbPath = Path.GetTempFileName();
            try
            {
                var db = new DatabaseService(dbPath);
                ICustomerService customerService = new CustomerService(db);
                customerService.AddCustomer(new ToolManagementAppV2.Models.Domain.Customer { Company = "ACME" });
                customerService.AddCustomer(new ToolManagementAppV2.Models.Domain.Customer { Company = "Beta" });
                var vm = new RentalManagementViewModel(customerService);
                vm.SearchCustomersCommand.Execute(null);
                vm.SelectedCustomer = vm.Customers.First(c => c.Company == "ACME");
                vm.DeleteCustomerCommand.Execute(null);
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
                var vm = new RentalManagementViewModel(customerService);
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
}
