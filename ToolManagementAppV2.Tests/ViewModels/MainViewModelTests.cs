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
    }
}
