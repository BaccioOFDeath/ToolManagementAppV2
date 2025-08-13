using System.IO;
using ToolManagementAppV2;
using ToolManagementAppV2.Interfaces;
using ToolManagementAppV2.Models.Domain;
using ToolManagementAppV2.Services.Core;
using ToolManagementAppV2.Services.Customers;
using ToolManagementAppV2.Services.Rentals;
using ToolManagementAppV2.Services.Tools;
using ToolManagementAppV2.Services.Users;
using ToolManagementAppV2.ViewModels;
using ToolManagementAppV2.Views;
using Xunit;

namespace ToolManagementAppV2.Tests.Tests
{
    public class NavigationCommandsTests
    {
        [Fact]
        public void OpenSearchToolsCommand_NavigatesToToolSearchPage()
        {
            var window = new MainWindow();
            var vm = Assert.IsType<MainViewModel>(window.DataContext);

            vm.OpenSearchToolsCommand.Execute(null);

            Assert.IsType<ToolSearchPage>(vm.CurrentPage);
        }

        [Fact]
        public void OpenSearchToolsCommand_LoadsToolsAndSetsDataContext()
        {
            var dbPath = Path.GetTempFileName();
            try
            {
                var db = new DatabaseService(dbPath);
                IToolService toolService = new ToolService(db);
                IUserService userService = new UserService(db, new ApplicationUserContext());
                ICustomerService customerService = new CustomerService(db);
                IRentalService rentalService = new RentalService(db);
                var activityLogService = new ActivityLogService(db);
                toolService.AddTool(new Tool { ToolNumber = "T1", NameDescription = "Hammer" });

                var vm = new MainViewModel(toolService, userService, customerService, rentalService, new StubFileDialogService(), activityLogService);
                vm.OpenSearchToolsCommand.Execute(null);

                var page = Assert.IsType<ToolSearchPage>(vm.CurrentPage);
                Assert.Same(vm.ToolManagement, page.DataContext);
                Assert.Same(vm.ToolManagement.HandTools, page.HandToolsList.ItemsSource);
                Assert.NotEmpty(vm.ToolManagement.HandTools);
            }
            finally
            {
                if (File.Exists(dbPath))
                    File.Delete(dbPath);
            }
        }
    }
}

class StubFileDialogService : IFileDialogService
{
    public string OpenFile(string filter) => null;
    public string SaveFile(string filter) => null;
}
