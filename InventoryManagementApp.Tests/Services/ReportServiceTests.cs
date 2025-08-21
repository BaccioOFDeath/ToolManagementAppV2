using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Documents;
using Xunit;
using InventoryManagementApp.Models.Domain;
using InventoryManagementApp.Services.Core;
using InventoryManagementApp.Services.Items;
using InventoryManagementApp.Services.Rentals;
using InventoryManagementApp.Services.Users;
using InventoryManagementApp.Services.Customers;
using InventoryManagementApp.Utilities.Helpers;

namespace InventoryManagementApp.Tests.Services
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
                var itemService = new ItemService(db);
                var customerService = new CustomerService(db);
                var userContext = new ApplicationUserContext();
                var userService = new UserService(db, userContext);
                var rentalService = new RentalService(db, itemService);
                var activityLogService = new ActivityLogService(db);
                var reportService = new ReportService(itemService, rentalService, activityLogService, customerService, userService);

                var item = new ItemModel
                {
                    ItemNumber = "T1",
                    NameDescription = "Hammer",
                    Location = "Loc",
                    Brand = "Brand",
                    PartNumber = "PN",
                    QuantityOnHand = 1,
                    RentedQuantity = 0
                };
                itemService.AddItem(item);

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

                rentalService.RentItem(item.ItemID, customer.CustomerID, DateTime.Today, DateTime.Today.AddDays(1));

                var task = Task.Run(() => reportService.GenerateSummaryReport().Result);
                Assert.True(task.Wait(TimeSpan.FromSeconds(5)), "GenerateSummaryReport deadlocked.");
                var doc = task.Result;
                var text = new TextRange(doc.ContentStart, doc.ContentEnd).Text;
                Assert.Contains($"Total {LabelProvider.Instance.ItemLabelPlural}: 1", text);
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
                var itemService = new ItemService(db);
                var customerService = new CustomerService(db);
                var userContext = new ApplicationUserContext();
                var userService = new UserService(db, userContext);
                var rentalService = new RentalService(db, itemService);
                var activityLogService = new ActivityLogService(db);
                var reportService = new ReportService(itemService, rentalService, activityLogService, customerService, userService);

                var item = new ItemModel
                {
                    ItemNumber = "T1",
                    NameDescription = "Hammer",
                    Location = "Loc",
                    Brand = "Brand",
                    PartNumber = "PN",
                    QuantityOnHand = 1,
                    RentedQuantity = 0
                };
                itemService.AddItem(item);

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

                rentalService.RentItem(item.ItemID, customer.CustomerID, DateTime.Today, DateTime.Today.AddDays(1));

                var doc = await reportService.GenerateSummaryReport();
                var text = new TextRange(doc.ContentStart, doc.ContentEnd).Text;
                Assert.Contains($"Total {LabelProvider.Instance.ItemLabelPlural}: 1", text);
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
