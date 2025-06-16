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
        public void AddToolCommand_PersistsNewToolValues()
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

                vm.NewTool.ToolNumber = "TN1";
                vm.NewTool.NameDescription = "Hammer";
                vm.NewTool.PartNumber = "PN1";
                vm.NewTool.Brand = "BrandA";
                vm.NewTool.Location = "Shelf";
                vm.NewTool.QuantityOnHand = 5;
                vm.NewTool.Supplier = "ABC";
                vm.NewTool.Notes = "Note";

                vm.AddToolCommand.Execute(null);

                var tools = toolService.GetAllTools();
                Assert.Single(tools);
                var tool = tools.First();
                Assert.Equal("TN1", tool.ToolNumber);
                Assert.Equal("Hammer", tool.NameDescription);
                Assert.Equal("PN1", tool.PartNumber);
                Assert.Equal("BrandA", tool.Brand);
                Assert.Equal("Shelf", tool.Location);
                Assert.Equal(5, tool.QuantityOnHand);
                Assert.Equal("ABC", tool.Supplier);
                Assert.Equal("Note", tool.Notes);
            }
            finally
            {
                if (File.Exists(dbPath))
                    File.Delete(dbPath);
            }
        }

        [Fact]
        public void AddCustomerCommand_PersistsNewCustomerValues()
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
                IToolService toolService = new ToolService(db);
                IUserService userService = new UserService(db);
                ICustomerService customerService = new CustomerService(db);
                IRentalService rentalService = new RentalService(db);
                ISettingsService settingsService = new SettingsService(db);

                customerService.AddCustomer(new Customer { Company = "Old" });
                var existing = customerService.GetAllCustomers().First();

                var vm = new MainViewModel(toolService, userService, customerService, rentalService, settingsService);
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
