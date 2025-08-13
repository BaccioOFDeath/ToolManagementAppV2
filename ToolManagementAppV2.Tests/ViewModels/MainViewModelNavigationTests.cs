using System.IO;
using System.Linq;
using ToolManagementAppV2.Models.Domain;
using ToolManagementAppV2.Services.Core;
using ToolManagementAppV2.Services.Tools;
using ToolManagementAppV2.Services.Users;
using ToolManagementAppV2.Services.Customers;
using ToolManagementAppV2.ViewModels;
using ToolManagementAppV2.Interfaces;
using ToolManagementAppV2.Views;
using ToolManagementAppV2.Services.Rentals;
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
                IRentalService rentalService = new RentalService(db);
                var activityLogService = new ActivityLogService(db);

                var vm = new MainViewModel(toolService, userService, customerService, rentalService, new StubFileDialogService(), activityLogService);

                Assert.NotNull(vm.ToolManagement);
                Assert.NotNull(vm.UserManagement);
                Assert.NotNull(vm.CustomerManagement);
                Assert.NotNull(vm.ManageRentals);
                Assert.NotNull(vm.ImportExport);
                Assert.NotNull(vm.ActivityLogs);
                Assert.NotNull(vm.Reports);
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
                IRentalService rentalService = new RentalService(db);
                var activityLogService = new ActivityLogService(db);

                var vm = new MainViewModel(toolService, userService, customerService, rentalService, new StubFileDialogService(), activityLogService);
                vm.OpenDashboardCommand.Execute(null);

                var page = Assert.IsType<DashboardPage>(vm.CurrentPage);
                Assert.IsType<DashboardViewModel>(page.DataContext);
            }
            finally
            {
                if (File.Exists(dbPath))
                    File.Delete(dbPath);
            }
        }

        [Fact]
        public void OpenImportExportCommand_SetsDataContext()
        {
            var dbPath = Path.GetTempFileName();
            try
            {
                var db = new DatabaseService(dbPath);
                IToolService toolService = new ToolService(db);
                IUserService userService = new UserService(db);
                ICustomerService customerService = new CustomerService(db);
                IRentalService rentalService = new RentalService(db);
                var activityLogService = new ActivityLogService(db);

                var vm = new MainViewModel(toolService, userService, customerService, rentalService, new StubFileDialogService(), activityLogService);
                vm.OpenImportExportCommand.Execute(null);

                var page = Assert.IsType<ImportExportPage>(vm.CurrentPage);
                Assert.IsType<ImportExportViewModel>(page.DataContext);
            }
            finally
            {
                if (File.Exists(dbPath))
                    File.Delete(dbPath);
            }
        }

        [Fact]
        public void OpenActivityLogsCommand_LoadsLogs()
        {
            var dbPath = Path.GetTempFileName();
            try
            {
                var db = new DatabaseService(dbPath);
                var activityLogService = new ActivityLogService(db);
                activityLogService.LogAction(1, "user", "action");
                IToolService toolService = new ToolService(db);
                IUserService userService = new UserService(db);
                ICustomerService customerService = new CustomerService(db);
                IRentalService rentalService = new RentalService(db);

                var vm = new MainViewModel(toolService, userService, customerService, rentalService, new StubFileDialogService(), activityLogService);
                vm.OpenActivityLogsCommand.Execute(null);

                var page = Assert.IsType<ActivityLogsPage>(vm.CurrentPage);
                var logsVm = Assert.IsType<ActivityLogsViewModel>(page.DataContext);
                Assert.NotEmpty(logsVm.Logs);
            }
            finally
            {
                if (File.Exists(dbPath))
                    File.Delete(dbPath);
            }
        }

        [Fact]
        public void OpenReportsCommand_SetsDataContext()
        {
            var dbPath = Path.GetTempFileName();
            try
            {
                var db = new DatabaseService(dbPath);
                var activityLogService = new ActivityLogService(db);
                IToolService toolService = new ToolService(db);
                IUserService userService = new UserService(db);
                ICustomerService customerService = new CustomerService(db);
                IRentalService rentalService = new RentalService(db);

                var vm = new MainViewModel(toolService, userService, customerService, rentalService, new StubFileDialogService(), activityLogService);
                vm.OpenReportsCommand.Execute(null);

                var page = Assert.IsType<ReportsPage>(vm.CurrentPage);
                Assert.IsType<ReportsViewModel>(page.DataContext);
            }
            finally
            {
                if (File.Exists(dbPath))
                    File.Delete(dbPath);
            }
        }

        [Fact]
        public void OpenRentalsCommand_NavigatesToManageRentalsPage()
        {
            var dbPath = Path.GetTempFileName();
            try
            {
                var db = new DatabaseService(dbPath);
                IToolService toolService = new ToolService(db);
                IUserService userService = new UserService(db);
                ICustomerService customerService = new CustomerService(db);
                IRentalService rentalService = new RentalService(db);
                var activityLogService = new ActivityLogService(db);

                var vm = new MainViewModel(toolService, userService, customerService, rentalService, new StubFileDialogService(), activityLogService);
                vm.OpenRentalsCommand.Execute(null);

                var page = Assert.IsType<ManageRentalsPage>(vm.CurrentPage);
                Assert.IsType<ManageRentalsViewModel>(page.DataContext);
            }
            finally
            {
                if (File.Exists(dbPath))
                    File.Delete(dbPath);
            }
        }

        [Fact]
        public void GlobalSearchCommand_NavigatesAndExecutesSearch()
        {
            var dbPath = Path.GetTempFileName();
            try
            {
                var db = new DatabaseService(dbPath);
                IToolService toolService = new ToolService(db);
                IUserService userService = new UserService(db);
                ICustomerService customerService = new CustomerService(db);
                IRentalService rentalService = new RentalService(db);
                var activityLogService = new ActivityLogService(db);

                toolService.AddTool(new Tool { ToolNumber = "T1", NameDescription = "Hammer" });
                toolService.AddTool(new Tool { ToolNumber = "T2", NameDescription = "Saw" });

                var vm = new MainViewModel(toolService, userService, customerService, rentalService, new StubFileDialogService(), activityLogService);
                vm.GlobalSearchText = "Ham";

                vm.GlobalSearchCommand.Execute(null);

                var page = Assert.IsType<ToolSearchPage>(vm.CurrentPage);
                Assert.Equal("Ham", vm.ToolManagement.SearchText);
                Assert.Empty(vm.GlobalSearchText);
                Assert.Single(vm.ToolManagement.SearchResults);
                Assert.Equal("Hammer", vm.ToolManagement.SearchResults.First().NameDescription);
            }
            finally
            {
                if (File.Exists(dbPath))
                    File.Delete(dbPath);
            }
        }

        [Fact]
        public void OpenImportMappingWindowCommand_NoFile_Returns()
        {
            var dbPath = Path.GetTempFileName();
            try
            {
                var db = new DatabaseService(dbPath);
                IToolService toolService = new ToolService(db);
                IUserService userService = new UserService(db);
                ICustomerService customerService = new CustomerService(db);
                IRentalService rentalService = new RentalService(db);
                var activityLogService = new ActivityLogService(db);

                var vm = new MainViewModel(toolService, userService, customerService, rentalService, new StubFileDialogService(), activityLogService);
                vm.OpenImportMappingWindowCommand.Execute(null);
            }
            finally
            {
                if (File.Exists(dbPath))
                    File.Delete(dbPath);
            }
        }

        [Fact]
        public void OpenImageImportMappingWindowCommand_NotNull()
        {
            var dbPath = Path.GetTempFileName();
            try
            {
                var db = new DatabaseService(dbPath);
                IToolService toolService = new ToolService(db);
                IUserService userService = new UserService(db);
                ICustomerService customerService = new CustomerService(db);
                IRentalService rentalService = new RentalService(db);
                var activityLogService = new ActivityLogService(db);

                var vm = new MainViewModel(toolService, userService, customerService, rentalService, new StubFileDialogService(), activityLogService);
                Assert.NotNull(vm.OpenImageImportMappingWindowCommand);
            }
            finally
            {
                if (File.Exists(dbPath))
                    File.Delete(dbPath);
            }
        }
    }
}

class StubFileDialogService : ToolManagementAppV2.Interfaces.IFileDialogService
{
    public string OpenFile(string filter) => null;
    public string SaveFile(string filter) => null;
}
