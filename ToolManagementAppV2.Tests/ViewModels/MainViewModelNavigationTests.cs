using System.IO;
using ToolManagementAppV2.Services.Core;
using ToolManagementAppV2.Services.Tools;
using ToolManagementAppV2.Services.Users;
using ToolManagementAppV2.Services.Customers;
using ToolManagementAppV2.ViewModels;
using ToolManagementAppV2.Interfaces;
using ToolManagementAppV2.Views;
using Xunit;

namespace ToolManagementAppV2.Tests.ViewModels
{
    public class MainViewModelNavigationTests
    {
        [Fact]
        public void Constructor_ComposesSubViewModels()
        {
            var dbPath = Path.GetTempFileName();
            try
            {
                var db = new DatabaseService(dbPath);
                IToolService toolService = new ToolService(db);
                IUserService userService = new UserService(db);
                ICustomerService customerService = new CustomerService(db);

                var vm = new MainViewModel(toolService, userService, customerService);

                Assert.NotNull(vm.ToolManagement);
                Assert.NotNull(vm.UserManagement);
                Assert.NotNull(vm.RentalManagement);
            }
            finally
            {
                if (File.Exists(dbPath))
                    File.Delete(dbPath);
            }
        }

        [Fact]
        public void OpenDashboardCommand_NavigatesToDashboardPage()
        {
            var dbPath = Path.GetTempFileName();
            try
            {
                var db = new DatabaseService(dbPath);
                IToolService toolService = new ToolService(db);
                IUserService userService = new UserService(db);
                ICustomerService customerService = new CustomerService(db);

                var vm = new MainViewModel(toolService, userService, customerService);
                vm.OpenDashboardCommand.Execute(null);

                Assert.IsType<DashboardPage>(vm.CurrentPage);
            }
            finally
            {
                if (File.Exists(dbPath))
                    File.Delete(dbPath);
            }
        }
    }
}
