using System.IO;
using CommunityToolkit.Mvvm.Input;
using ToolManagementAppV2.Interfaces;
using ToolManagementAppV2.Services.Core;
using ToolManagementAppV2.Services.Tools;
using ToolManagementAppV2.Services.Users;
using ToolManagementAppV2.Services.Customers;
using ToolManagementAppV2.Services.Rentals;
using ToolManagementAppV2.ViewModels;
using Xunit;

namespace ToolManagementAppV2.Tests.ViewModels
{
    public class DashboardViewModelTests
    {
        [Fact]
        public void Constructor_LoadsStatsAndActivity_AndCommandsExecute()
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
                activityLogService.LogAction(1, "user", "action");

                bool newTool = false, rentals = false, import = false;

                var vm = new DashboardViewModel(
                    toolService,
                    rentalService,
                    customerService,
                    userService,
                    activityLogService,
                    new RelayCommand(() => newTool = true),
                    new RelayCommand(() => rentals = true),
                    new RelayCommand(() => import = true));

                Assert.NotEmpty(vm.StatCards);
                Assert.NotEmpty(vm.RecentActivity);

                vm.NewToolCommand.Execute(null);
                vm.OpenRentalsCommand.Execute(null);
                vm.OpenImportExportCommand.Execute(null);

                Assert.True(newTool);
                Assert.True(rentals);
                Assert.True(import);
            }
            finally
            {
                if (File.Exists(dbPath))
                    File.Delete(dbPath);
            }
        }
    }
}
