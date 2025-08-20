using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ToolManagementAppV2.Models.Domain;
using ToolManagementAppV2.Services.Core;
using ToolManagementAppV2.Services.Customers;
using ToolManagementAppV2.Services.Rentals;
using ToolManagementAppV2.Services.Tools;
using Xunit;

namespace ToolManagementAppV2.Tests.Services
{
    public class RentalServiceConcurrentTests
    {
        [Fact]
        public void RentTool_ConcurrentRequests_MaintainsQuantities()
        {
            var dbPath = Path.GetTempFileName();
            try
            {
                var db = new DatabaseService(dbPath);
                var toolService = new ItemService(db);
                var customerService = new CustomerService(db);
                var rentalService = new RentalService(db, toolService);

                toolService.AddTool(new ItemModel { ItemNumber = "T1", NameDescription = "Hammer", QuantityOnHand = 1 });
                var tool = toolService.GetAllTools().First();

                customerService.AddCustomer(new Customer { Company = "Acme" });
                var cust = customerService.GetAllCustomers().First();

                var barrier = new Barrier(2);
                var t1 = Task.Run(() =>
                {
                    barrier.SignalAndWait();
                    try
                    {
                        rentalService.RentTool(tool.ItemID, cust.CustomerID, DateTime.Today, DateTime.Today.AddDays(1));
                    }
                    catch { }
                });
                var t2 = Task.Run(() =>
                {
                    barrier.SignalAndWait();
                    try
                    {
                        rentalService.RentTool(tool.ItemID, cust.CustomerID, DateTime.Today, DateTime.Today.AddDays(1));
                    }
                    catch { }
                });

                Task.WaitAll(t1, t2);

                var updated = toolService.GetToolByID(tool.ItemID);
                Assert.Equal(0, updated.QuantityOnHand);
                Assert.Equal(1, updated.RentedQuantity);
                Assert.Single(rentalService.GetAllRentals());
            }
            finally
            {
                if (File.Exists(dbPath)) File.Delete(dbPath);
            }
        }
    }
}

