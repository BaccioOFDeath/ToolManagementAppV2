using System;
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
using System.Threading;

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
                IUserService userService = new UserService(db, new ApplicationUserContext());
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

        [Theory]
        [InlineData("toolService")]
        [InlineData("rentalService")]
        [InlineData("customerService")]
        [InlineData("userService")]
        [InlineData("activityLogService")]
        [InlineData("openManageToolsCommand")]
        [InlineData("openRentalsCommand")]
        [InlineData("openImportExportCommand")]
        public void Constructor_ThrowsArgumentNull(string nullParam)
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
                IRelayCommand manageCmd = new RelayCommand(() => { });
                IRelayCommand rentalsCmd = new RelayCommand(() => { });
                IRelayCommand importCmd = new RelayCommand(() => { });

                var ex = Assert.Throws<ArgumentNullException>(() => new DashboardViewModel(
                    nullParam == "toolService" ? null : toolService,
                    nullParam == "rentalService" ? null : rentalService,
                    nullParam == "customerService" ? null : customerService,
                    nullParam == "userService" ? null : userService,
                    nullParam == "activityLogService" ? null : activityLogService,
                    nullParam == "openManageToolsCommand" ? null : manageCmd,
                    nullParam == "openRentalsCommand" ? null : rentalsCmd,
                    nullParam == "openImportExportCommand" ? null : importCmd));

                Assert.Equal(nullParam, ex.ParamName);
            }
            finally
            {
                if (File.Exists(dbPath))
                    File.Delete(dbPath);
            }
        }

        [Fact]
        public async Task LoadStatsAsync_CanBeCancelled()
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
                var vm = new DashboardViewModel(toolService, rentalService, customerService, userService, activityLogService,
                    new RelayCommand(() => { }), new RelayCommand(() => { }), new RelayCommand(() => { }));
                vm.StatCards.Clear();
                var cts = new CancellationTokenSource();
                cts.Cancel();
                await vm.LoadStatsAsync(cts.Token);
                Assert.Empty(vm.StatCards);
            }
            finally
            {
                if (File.Exists(dbPath))
                    File.Delete(dbPath);
            }
        }
    }
}
