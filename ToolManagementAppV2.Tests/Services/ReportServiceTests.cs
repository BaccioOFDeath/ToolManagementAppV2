using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Documents;
using Xunit;
using ToolManagementAppV2.Models.Domain;
using ToolManagementAppV2.Services.Core;
using ToolManagementAppV2.Services.Tools;
using ToolManagementAppV2.Services.Rentals;
using ToolManagementAppV2.Services.Users;
using ToolManagementAppV2.Services.Customers;

namespace ToolManagementAppV2.Tests.Services
{
    public class ReportServiceTests
    {
        [Fact]
        public void GenerateSummaryReport_TaskRun_ReturnsSummary()
        {
            var dbPath = Path.GetTempFileName();
            try
            {
                var db = new DatabaseService(dbPath);
                var toolService = new ItemService(db);
                var customerService = new CustomerService(db);
                var userContext = new ApplicationUserContext();
                var userService = new UserService(db, userContext);
                var rentalService = new RentalService(db, toolService);
                var activityLogService = new ActivityLogService(db);
                var reportService = new ReportService(toolService, rentalService, activityLogService, customerService, userService);

                var tool = new ItemModel
                {
                    ToolNumber = "T1",
                    NameDescription = "Hammer",
                    Location = "Loc",
                    Brand = "Brand",
                    PartNumber = "PN",
                    QuantityOnHand = 1,
                    RentedQuantity = 0
                };
                toolService.AddTool(tool);

                var customer = new Customer
                {
                    Company = "Comp",
                    Contact = "John",
                    Phone = "123",
                    Email = "john@example.com",
                    Address = "Addr"
                };
                customerService.AddCustomer(customer);

                var user = new User
                {
                    UserName = "user",
                    PasswordHash = "Strong1!",
                    IsAdmin = false
                };
                userService.AddUser(user);

                rentalService.RentTool(tool.ToolID, customer.CustomerID, DateTime.Today, DateTime.Today.AddDays(1));

                var task = Task.Run(() => reportService.GenerateSummaryReport().Result);
                Assert.True(task.Wait(TimeSpan.FromSeconds(5)), "GenerateSummaryReport deadlocked.");
                var doc = task.Result;
                var text = new TextRange(doc.ContentStart, doc.ContentEnd).Text;
                Assert.Contains("Total Tools: 1", text);
                Assert.Contains("Total Rentals (History): 1", text);
                Assert.Contains("Active Rentals: 1", text);
                Assert.Contains("Total Customers: 1", text);
                Assert.Contains("Total Users: 1", text);
            }
            finally
            {
                if (File.Exists(dbPath))
                    File.Delete(dbPath);
            }
        }

        [Fact]
        public async Task GenerateSummaryReport_Await_ReturnsSummary()
        {
            var dbPath = Path.GetTempFileName();
            try
            {
                var db = new DatabaseService(dbPath);
                var toolService = new ItemService(db);
                var customerService = new CustomerService(db);
                var userContext = new ApplicationUserContext();
                var userService = new UserService(db, userContext);
                var rentalService = new RentalService(db, toolService);
                var activityLogService = new ActivityLogService(db);
                var reportService = new ReportService(toolService, rentalService, activityLogService, customerService, userService);

                var tool = new ItemModel
                {
                    ToolNumber = "T1",
                    NameDescription = "Hammer",
                    Location = "Loc",
                    Brand = "Brand",
                    PartNumber = "PN",
                    QuantityOnHand = 1,
                    RentedQuantity = 0
                };
                toolService.AddTool(tool);

                var customer = new Customer
                {
                    Company = "Comp",
                    Contact = "John",
                    Phone = "123",
                    Email = "john@example.com",
                    Address = "Addr"
                };
                customerService.AddCustomer(customer);

                var user = new User
                {
                    UserName = "user",
                    PasswordHash = "Strong1!",
                    IsAdmin = false
                };
                userService.AddUser(user);

                rentalService.RentTool(tool.ToolID, customer.CustomerID, DateTime.Today, DateTime.Today.AddDays(1));

                var doc = await reportService.GenerateSummaryReport();
                var text = new TextRange(doc.ContentStart, doc.ContentEnd).Text;
                Assert.Contains("Total Tools: 1", text);
                Assert.Contains("Total Rentals (History): 1", text);
                Assert.Contains("Active Rentals: 1", text);
                Assert.Contains("Total Customers: 1", text);
                Assert.Contains("Total Users: 1", text);
            }
            finally
            {
                if (File.Exists(dbPath))
                    File.Delete(dbPath);
            }
        }
    }
}
