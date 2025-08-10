using System.IO;
using System.Linq;
using ToolManagementAppV2.Services.Core;
using ToolManagementAppV2.Services.Customers;
using ToolManagementAppV2.Interfaces;
using ToolManagementAppV2.ViewModels;
using Xunit;

namespace ToolManagementAppV2.Tests.ViewModels
{
    public class RentalManagementViewModelTests
    {
        [Fact]
        public void AddCustomerCommand_PersistsNewCustomerValues()
        {
            var dbPath = Path.GetTempFileName();
            try
            {
                var db = new DatabaseService(dbPath);
                ICustomerService customerService = new CustomerService(db);
                var vm = new RentalManagementViewModel(customerService);
                vm.NewCustomerName = "ACME";
                vm.NewCustomerEmail = "a@b.com";
                vm.NewCustomerContact = "John";
                vm.NewCustomerPhone = "123";
                vm.NewCustomerMobile = "456";
                vm.NewCustomerAddress = "Addr";
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
                Assert.True(string.IsNullOrEmpty(vm.NewCustomerName));
                Assert.True(string.IsNullOrEmpty(vm.NewCustomerEmail));
                Assert.True(string.IsNullOrEmpty(vm.NewCustomerContact));
                Assert.True(string.IsNullOrEmpty(vm.NewCustomerPhone));
                Assert.True(string.IsNullOrEmpty(vm.NewCustomerMobile));
                Assert.True(string.IsNullOrEmpty(vm.NewCustomerAddress));
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
    }
}
