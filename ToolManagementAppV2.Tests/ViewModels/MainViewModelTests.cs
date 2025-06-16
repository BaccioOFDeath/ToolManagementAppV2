using System.IO;
using System.Linq;
using ToolManagementAppV2.Models.Domain;
using ToolManagementAppV2.Services.Core;
using ToolManagementAppV2.Services.Customers;
using ToolManagementAppV2.Services.Rentals;
using ToolManagementAppV2.Services.Settings;
using ToolManagementAppV2.Services.Tools;
using ToolManagementAppV2.Services.Users;
using ToolManagementAppV2.Interfaces;
using ToolManagementAppV2.ViewModels;
using Xunit;

namespace ToolManagementAppV2.Tests.ViewModels
{
    public class MainViewModelTests
    {
        [Fact]
        public void SearchCommand_FiltersToolsBySearchTerm()
        {
            var dbPath = Path.GetTempFileName();
            try
            {
                var db = new DatabaseService(dbPath);
                IToolService toolService = new ToolService(db);
                IUserService userService = new UserService(db);
                ICustomerService customerService = new CustomerService(db);
                IRentalService rentalService = new RentalService(db);
                ISettingsService settingsService = new SettingsService(db);

                toolService.AddTool(new Tool { ToolNumber = "T1", NameDescription = "Hammer" });
                toolService.AddTool(new Tool { ToolNumber = "T2", NameDescription = "Saw" });

                var vm = new MainViewModel(toolService, userService, customerService, rentalService, settingsService);

                vm.SearchTerm = "Ham";
                vm.SearchCommand.Execute(null);

                Assert.Single(vm.SearchResults);
                Assert.Equal("Hammer", vm.SearchResults.First().NameDescription);
            }
            finally
            {
                if (File.Exists(dbPath))
                    File.Delete(dbPath);
            }
        }

        [Fact]
        public void SearchCommand_EmptyTerm_ReturnsAllTools()
        {
            var dbPath = Path.GetTempFileName();
            try
            {
                var db = new DatabaseService(dbPath);
                IToolService toolService = new ToolService(db);
                IUserService userService = new UserService(db);
                ICustomerService customerService = new CustomerService(db);
                IRentalService rentalService = new RentalService(db);
                ISettingsService settingsService = new SettingsService(db);

                toolService.AddTool(new Tool { ToolNumber = "T1", NameDescription = "Hammer" });
                toolService.AddTool(new Tool { ToolNumber = "T2", NameDescription = "Saw" });

                var vm = new MainViewModel(toolService, userService, customerService, rentalService, settingsService);

                vm.SearchTerm = string.Empty;
                vm.SearchCommand.Execute(null);

                Assert.Equal(2, vm.SearchResults.Count);
            }
            finally
            {
                if (File.Exists(dbPath))
                    File.Delete(dbPath);
            }
        }

        [Fact]
        public void AddCustomerCommand_AddsCustomerAndClearsFields()
        {
            var dbPath = Path.GetTempFileName();
            try
            {
                var db = new DatabaseService(dbPath);
                IToolService toolService = new ToolService(db);
                IUserService userService = new UserService(db);
                ICustomerService customerService = new CustomerService(db);
                IRentalService rentalService = new RentalService(db);
                ISettingsService settingsService = new SettingsService(db);

                var vm = new MainViewModel(toolService, userService, customerService, rentalService, settingsService);

                vm.NewCustomerName = "Acme";
                vm.NewCustomerEmail = "info@acme.com";
                vm.NewCustomerContact = "John";
                vm.NewCustomerPhone = "111";
                vm.NewCustomerMobile = "222";
                vm.NewCustomerAddress = "Addr";

                vm.AddCustomerCommand.Execute(null);

                Assert.Single(vm.Customers);
                var added = vm.Customers.First();
                Assert.Equal("Acme", added.Company);
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
        public void UpdateCustomerCommand_UpdatesSelectedCustomerFromBoundFields()
        {
            var dbPath = Path.GetTempFileName();
            try
            {
                var db = new DatabaseService(dbPath);
                IToolService toolService = new ToolService(db);
                IUserService userService = new UserService(db);
                ICustomerService customerService = new CustomerService(db);
                IRentalService rentalService = new RentalService(db);
                ISettingsService settingsService = new SettingsService(db);

                customerService.AddCustomer(new Customer { Company = "Old" });
                var existing = customerService.GetAllCustomers().First();

                var vm = new MainViewModel(toolService, userService, customerService, rentalService, settingsService);
                vm.SelectedCustomer = vm.Customers.First();

                vm.NewCustomerName = "New";
                vm.NewCustomerEmail = "new@a.com";
                vm.NewCustomerContact = "Bob";
                vm.NewCustomerPhone = "123";
                vm.NewCustomerMobile = "456";
                vm.NewCustomerAddress = "Addr2";

                vm.UpdateCustomerCommand.Execute(null);

                var updated = customerService.GetCustomerByID(existing.CustomerID);
                Assert.Equal("New", updated.Company);
                Assert.Equal("new@a.com", updated.Email);
                Assert.Equal("Bob", updated.Contact);
                Assert.Equal("123", updated.Phone);
                Assert.Equal("456", updated.Mobile);
                Assert.Equal("Addr2", updated.Address);
            }
            finally
            {
                if (File.Exists(dbPath))
                    File.Delete(dbPath);
            }
        }
    }
}
