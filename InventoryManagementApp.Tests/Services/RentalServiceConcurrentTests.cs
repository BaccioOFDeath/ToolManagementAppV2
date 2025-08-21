using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using InventoryManagementApp.Models.Domain;
using InventoryManagementApp.Services.Core;
using InventoryManagementApp.Services.Customers;
using InventoryManagementApp.Services.Rentals;
using InventoryManagementApp.Services.Items;
using Xunit;

namespace InventoryManagementApp.Tests.Services
{
    public class RentalServiceConcurrentTests
    {
        [Fact]
        public void RentItem_ConcurrentRequests_MaintainsQuantities()
        {
            var dbPath = Path.GetTempFileName();
            try
            {
                var db = new DatabaseService(dbPath);
                var itemService = new ItemService(db);
                var customerService = new CustomerService(db);
                var rentalService = new RentalService(db, itemService);

                itemService.AddItem(new ItemModel { ItemNumber = "T1", NameDescription = "Hammer", QuantityOnHand = 1 });
                var item = itemService.GetAllItems().First();

                customerService.AddCustomer(new Customer { Company = "Acme" });
                var cust = customerService.GetAllCustomers().First();

                var barrier = new Barrier(2);
                var t1 = Task.Run(() =>
                {
                    barrier.SignalAndWait();
                    try
                    {
                        rentalService.RentItem(item.ItemID, cust.CustomerID, DateTime.Today, DateTime.Today.AddDays(1));
                    }
                    catch { }
                });
                var t2 = Task.Run(() =>
                {
                    barrier.SignalAndWait();
                    try
                    {
                        rentalService.RentItem(item.ItemID, cust.CustomerID, DateTime.Today, DateTime.Today.AddDays(1));
                    }
                    catch { }
                });

                Task.WaitAll(t1, t2);

                var updated = itemService.GetItemByID(item.ItemID);
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

