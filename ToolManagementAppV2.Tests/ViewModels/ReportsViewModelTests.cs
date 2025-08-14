using System;
using System.Data;
using System.IO;
using System.Linq;
using ToolManagementAppV2.Models.Domain;
using ToolManagementAppV2.Services.Core;
using ToolManagementAppV2.Services.Customers;
using ToolManagementAppV2.Services.Rentals;
using ToolManagementAppV2.Services.Tools;
using ToolManagementAppV2.Services.Users;
using ToolManagementAppV2.ViewModels;
using Xunit;
using System.Threading.Tasks;

namespace ToolManagementAppV2.Tests.ViewModels
{
    public class ReportsViewModelTests
    {
        [Fact]
        public async Task ReportsViewModel_ReportsPage_Binding_Wiring()
        {
            var dbPath = Path.GetTempFileName();
            try
            {
                var db = new DatabaseService(dbPath);
                var toolService = new ToolService(db);
                var customerService = new CustomerService(db);
                var userService = new UserService(db, new ApplicationUserContext());
                var rentalService = new RentalService(db, toolService);
                var activityService = new ActivityLogService(db);
                var reportService = new ReportService(toolService, rentalService, activityService, customerService, userService);

                var tool = new Tool { ToolNumber = "T1", QuantityOnHand = 1 };
                toolService.AddTool(tool);

                var customer = new Customer { Company = "C1" };
                customerService.AddCustomer(customer);
                var cust = customerService.GetAllCustomers().First();

                rentalService.RentTool(tool.ToolID, cust.CustomerID, DateTime.Today, DateTime.Today.AddDays(1));

                var user = new User { UserName = "user" };
                userService.AddUser(user);

                var vm = new ReportsViewModel(reportService);

                Assert.False(vm.RunReportCommand.CanExecute(null));

                vm.SelectedReport = "Summary";
                Assert.True(vm.RunReportCommand.CanExecute(null));

                await vm.RunReportCommand.ExecuteAsync(null);
                Assert.NotNull(vm.ReportResults);
                Assert.Contains("Total Tools: 1",
                    vm.ReportResults.Rows.Cast<DataRow>().Select(r => r[0]?.ToString()));

                vm.SelectedReport = null;
                Assert.False(vm.RunReportCommand.CanExecute(null));

                vm.SelectedReport = "Inventory";
                Assert.True(vm.RunReportCommand.CanExecute(null));

                await vm.RunReportCommand.ExecuteAsync(null);
                Assert.Contains("Tool ID:",
                    vm.ReportResults.Rows.Cast<DataRow>().Select(r => r[0]?.ToString()));
            }
            finally
            {
                if (File.Exists(dbPath))
                    File.Delete(dbPath);
            }
        }
    }
}
