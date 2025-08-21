using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Data.SQLite;
using InventoryManagementApp.Models;
using InventoryManagementApp.Models.Domain;
using InventoryManagementApp.Models.ImportExport;
using InventoryManagementApp.Services.Core;
using InventoryManagementApp.Services.Customers;
using InventoryManagementApp.Services.Rentals;
using InventoryManagementApp.Services.Items;
using InventoryManagementApp.Interfaces;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using InventoryManagementApp.Services.Users;
using InventoryManagementApp.Tests;

namespace InventoryManagementApp.Tests.Services
{
    public class RentalServiceTests
    {
        class StubUserContext : IUserContext
        {
            public User? CurrentUser { get; set; }
            public event EventHandler<User?>? UserChanged;
            public bool IsAdmin => CurrentUser?.IsAdmin ?? false;
            public string UserName => CurrentUser?.UserName ?? string.Empty;
            public string Role => CurrentUser?.Role ?? string.Empty;
        }
        [Fact]
        public void GetRentalHistoryForItem_ReturnsHistory()
        {
            var dbPath = Path.GetTempFileName();
            try
            {
                var db = new DatabaseService(dbPath);
                IItemService toolService = new ItemService(db);
                ICustomerService customerService = new CustomerService(db);
                IRentalService rentalService = new RentalService(db, toolService);

                var tool = new ItemModel { ItemNumber = "T1", NameDescription = "Hammer", QuantityOnHand = 5 };
                toolService.AddItem(tool);
                var addedTool = toolService.GetAllItems().First();

                var cust = new Customer { Company = "Acme" };
                customerService.AddCustomer(cust);
                var addedCust = customerService.GetAllCustomers().First();

                rentalService.RentItem(addedTool.ItemID, addedCust.CustomerID, DateTime.Today, DateTime.Today.AddDays(1));
                rentalService.RentItem(addedTool.ItemID, addedCust.CustomerID, DateTime.Today.AddDays(2), DateTime.Today.AddDays(3));

                var history = rentalService.GetRentalHistoryForItem(addedTool.ItemID);
                Assert.Equal(2, history.Count);
                Assert.True(history[0].RentalDate > history[1].RentalDate);
            }
            finally
            {
                if (File.Exists(dbPath))
                    File.Delete(dbPath);
            }
        }

        [Fact]
        public void RentItem_NoAvailability_Throws()
        {
            var dbPath = Path.GetTempFileName();
            try
            {
                var db = new DatabaseService(dbPath);
                IItemService toolService = new ItemService(db);
                ICustomerService customerService = new CustomerService(db);
                IRentalService rentalService = new RentalService(db, toolService);

                var tool = new ItemModel { ItemNumber = "T1", NameDescription = "Hammer", QuantityOnHand = 0 };
                toolService.AddItem(tool);
                var addedTool = toolService.GetAllItems().First();

                customerService.AddCustomer(new Customer { Company = "Acme" });
                var cust = customerService.GetAllCustomers().First();

                Assert.Throws<InvalidOperationException>(() =>
                    rentalService.RentItem(addedTool.ItemID, cust.CustomerID, DateTime.Today, DateTime.Today.AddDays(1)));
                Assert.Empty(rentalService.GetAllRentals());
            }
            finally
            {
                if (File.Exists(dbPath))
                    File.Delete(dbPath);
            }
        }

        [Fact]
        public void RentItem_NoAvailability_Throws_WithHelper()
        {
            var dbPath = Path.GetTempFileName();
            try
            {
                var db = new DatabaseService(dbPath);
                IItemService toolService = new ItemService(db);
                ICustomerService customerService = new CustomerService(db);
                IRentalService rentalService = new RentalService(db, toolService);

                var tool = new ItemModel { ItemNumber = "T2", NameDescription = "Wrench", QuantityOnHand = 0 };
                toolService.AddItem(tool);
                var addedTool = toolService.GetAllItems().First();

                customerService.AddCustomer(new Customer { Company = "Beta" });
                var cust = customerService.GetAllCustomers().First();

                Assert.Throws<InvalidOperationException>(() =>
                    rentalService.RentItem(addedTool.ItemID, cust.CustomerID, DateTime.Today, DateTime.Today.AddDays(1)));
                Assert.Empty(rentalService.GetAllRentals());
            }
            finally
            {
                if (File.Exists(dbPath))
                    File.Delete(dbPath);
            }
        }

        [Fact]
        public void ReturnItem_InvalidRentalID_Throws()
        {
            var dbPath = Path.GetTempFileName();
            try
            {
                var db = new DatabaseService(dbPath);
                var toolService = new ItemService(db);
                IRentalService rentalService = new RentalService(db, toolService);

                Assert.Throws<InvalidOperationException>(() => rentalService.ReturnItem(1, DateTime.Today));
            }
            finally
            {
                if (File.Exists(dbPath))
                    File.Delete(dbPath);
            }
        }

        [Fact]
        public void ReturnItem_InvalidRentalID_Throws_WithHelper()
        {
            var dbPath = Path.GetTempFileName();
            try
            {
                var db = new DatabaseService(dbPath);
                var toolService = new ItemService(db);
                IRentalService rentalService = new RentalService(db, toolService);

                Assert.Throws<InvalidOperationException>(() => rentalService.ReturnItem(1, DateTime.Today));
            }
            finally
            {
                if (File.Exists(dbPath))
                    File.Delete(dbPath);
            }
        }

        [Fact]
        public void ExtendRental_InvalidRentalID_Throws()
        {
            var dbPath = Path.GetTempFileName();
            try
            {
                var db = new DatabaseService(dbPath);
                var toolService = new ItemService(db);
                var rentalService = new RentalService(db, toolService);

                Assert.Throws<InvalidOperationException>(() =>
                    rentalService.ExtendRental(1, DateTime.Today.AddDays(1)));
            }
            finally
            {
                if (File.Exists(dbPath))
                    File.Delete(dbPath);
            }
        }

        [Fact]
        public void ExtendRental_ReturnedRental_Throws()
        {
            var dbPath = Path.GetTempFileName();
            try
            {
                var db = new DatabaseService(dbPath);
                var toolService = new ItemService(db);
                var customerService = new CustomerService(db);
                var rentalService = new RentalService(db, toolService);

                var tool = new ItemModel { ItemNumber = "T1", NameDescription = "Hammer", QuantityOnHand = 1 };
                toolService.AddItem(tool);
                var addedTool = toolService.GetAllItems().First();

                customerService.AddCustomer(new Customer { Company = "Acme" });
                var cust = customerService.GetAllCustomers().First();

                rentalService.RentItem(addedTool.ItemID, cust.CustomerID, DateTime.Today, DateTime.Today.AddDays(1));
                var rental = rentalService.GetAllRentals().First();

                rentalService.ReturnItem(rental.RentalID, DateTime.Today);

                Assert.Throws<InvalidOperationException>(() =>
                    rentalService.ExtendRental(rental.RentalID, DateTime.Today.AddDays(2)));
            }
            finally
            {
                if (File.Exists(dbPath))
                    File.Delete(dbPath);
            }
        }

        [Fact]
        public void DeleteRental_RemovesRecord()
        {
            var dbPath = Path.GetTempFileName();
            try
            {
                var db = new DatabaseService(dbPath);
                var toolService = new ItemService(db);
                var customerService = new CustomerService(db);
                var rentalService = new RentalService(db, toolService);

                var tool = new ItemModel { ItemNumber = "T1", NameDescription = "Hammer", QuantityOnHand = 1 };
                toolService.AddItem(tool);
                var addedTool = toolService.GetAllItems().First();

                customerService.AddCustomer(new Customer { Company = "Acme" });
                var cust = customerService.GetAllCustomers().First();

                rentalService.RentItem(addedTool.ItemID, cust.CustomerID, DateTime.Today, DateTime.Today.AddDays(1));
                var rental = rentalService.GetAllRentals().First();

                rentalService.DeleteRental(rental.RentalID);

                Assert.Empty(rentalService.GetAllRentals());
            }
            finally
            {
                if (File.Exists(dbPath))
                    File.Delete(dbPath);
            }
        }

        [Fact]
        public void DeleteRental_InvalidRentalID_Throws()
        {
            var dbPath = Path.GetTempFileName();
            try
            {
                var db = new DatabaseService(dbPath);
                var toolService = new ItemService(db);
                var rentalService = new RentalService(db, toolService);

                Assert.Throws<InvalidOperationException>(() => rentalService.DeleteRental(1));
            }
            finally
            {
                if (File.Exists(dbPath))
                    File.Delete(dbPath);
            }
        }

        [Fact]
        public void GetActiveRentals_ReturnsCustomerAndToolDetails()
        {
            var dbPath = Path.GetTempFileName();
            try
            {
                var db = new DatabaseService(dbPath);
                var toolService = new ItemService(db);
                var customerService = new CustomerService(db);
                var rentalService = new RentalService(db, toolService);

                var tool = new ItemModel { ItemNumber = "T1", NameDescription = "Hammer", QuantityOnHand = 1, Location = "A1", ImagePath = "path" };
                toolService.AddItem(tool);
                var addedTool = toolService.GetAllItems().First();

                var customer = new Customer { Company = "Acme", Contact = "Bob", Email = "b@c.com", Phone = "111", Mobile = "222", Address = "Addr" };
                customerService.AddCustomer(customer);
                var addedCust = customerService.GetAllCustomers().First();

                rentalService.RentItem(addedTool.ItemID, addedCust.CustomerID, DateTime.Today, DateTime.Today.AddDays(1));

                var rentals = rentalService.GetActiveRentals();
                var r = rentals.First();
                Assert.Equal("A1", r.ItemLocation);
                Assert.Equal("path", r.ImagePath);
                Assert.Equal("Bob", r.CustomerContact);
                Assert.Equal("b@c.com", r.CustomerEmail);
                Assert.Equal("111", r.CustomerPhone);
                Assert.Equal("222", r.CustomerMobile);
                Assert.Equal("Addr", r.CustomerAddress);
            }
            finally
            {
                if (File.Exists(dbPath))
                    File.Delete(dbPath);
            }
        }

        [Fact]
        public void RentAndReturnItem_UpdatesQuantities()
        {
            var dbPath = Path.GetTempFileName();
            try
            {
                var db = new DatabaseService(dbPath);
                var toolService = new ItemService(db);
                var customerService = new CustomerService(db);
                var rentalService = new RentalService(db, toolService);

                toolService.AddItem(new ItemModel { ItemNumber = "T1", NameDescription = "Hammer", QuantityOnHand = 1 });
                var tool = toolService.GetAllItems().First();

                customerService.AddCustomer(new Customer { Company = "Acme" });
                var cust = customerService.GetAllCustomers().First();

                rentalService.RentItem(tool.ItemID, cust.CustomerID, DateTime.Today, DateTime.Today.AddDays(1));

                var rented = toolService.GetItemByID(tool.ItemID);
                Assert.Equal(0, rented.QuantityOnHand);
                Assert.Equal(1, rented.RentedQuantity);

                var rental = rentalService.GetAllRentals().First();
                rentalService.ReturnItem(rental.RentalID, DateTime.Today);

                var returned = toolService.GetItemByID(tool.ItemID);
                Assert.Equal(1, returned.QuantityOnHand);
                Assert.Equal(0, returned.RentedQuantity);
            }
            finally
            {
                if (File.Exists(dbPath))
                    File.Delete(dbPath);
            }
        }

        [Fact]
        public void ExtendRental_Failure_RollsBack()
        {
            var dbPath = Path.GetTempFileName();
            try
            {
                var db = new DatabaseService(dbPath);
                var toolService = new ItemService(db);
                var customerService = new CustomerService(db);
                var rentalService = new RentalService(db, toolService);

                toolService.AddItem(new ItemModel { ItemNumber = "T1", NameDescription = "Hammer", QuantityOnHand = 1 });
                var tool = toolService.GetAllItems().First();

                customerService.AddCustomer(new Customer { Company = "Acme" });
                var cust = customerService.GetAllCustomers().First();

                rentalService.RentItem(tool.ItemID, cust.CustomerID, DateTime.Today.AddDays(-2), DateTime.Today.AddDays(-1));
                var rental = rentalService.GetAllRentals().First();
                var originalDue = rental.DueDate;

                var failingItemService = new FailingItemService();
                var rentalService2 = new RentalService(db, failingItemService);

                Assert.Throws<InvalidOperationException>(() =>
                    rentalService2.ExtendRental(rental.RentalID, DateTime.Today.AddDays(1)));

                var after = rentalService2.GetAllRentals().First();
                Assert.Equal(originalDue, after.DueDate);

                var toolAfter = toolService.GetItemByID(tool.ItemID);
                Assert.Equal(0, toolAfter.QuantityOnHand);
                Assert.Equal(1, toolAfter.RentedQuantity);
            }
            finally
            {
                if (File.Exists(dbPath))
                    File.Delete(dbPath);
            }
        }

        [Fact]
        public async Task RentItemAsync_LogsActivity()
        {
            var dbPath = Path.GetTempFileName();
            try
            {
                var db = new DatabaseService(dbPath);
                var ctx = new StubUserContext { CurrentUser = new User { UserID = 1, UserName = "tester", IsAdmin = true } };
                var auth = new AllowAllAuthorizationService();
                var logService = new ActivityLogService(db);
                var itemService = new ItemService(db, auth, null, logService, ctx);
                var customerService = new CustomerService(db, auth);
                var rentalService = new RentalService(db, auth, toolService, null, logService, ctx);

                await itemService.AddItemAsync(new ItemModel { ItemNumber = "T1", NameDescription = "Hammer", QuantityOnHand = 1, RentedQuantity = 0 });
                await customerService.AddCustomerAsync(new Customer { Company = "Acme" });
                var item = (await itemService.GetAllItemsAsync()).First();
                var cust = (await customerService.GetAllCustomersAsync()).First();

                await rentalService.RentItemAsync(item.ItemID, cust.CustomerID, DateTime.Today, DateTime.Today.AddDays(1));

                var logs = await logService.GetRecentLogsAsync();
                Assert.Contains(logs.Value, l => l.Action.Contains("Rented item"));
            }
            finally
            {
                if (File.Exists(dbPath))
                    File.Delete(dbPath);
            }
        }

        class FailingItemService : IItemService
        {
            public Task<List<int>> ImportItemsFromCsvAsync(string filePath, IDictionary<string, string> map, CancellationToken cancellationToken) => throw new NotImplementedException();
            public Task ExportItemsToCsvAsync(string filePath, CancellationToken cancellationToken = default) => Task.CompletedTask;
            public Task<List<ItemModel>> GetAllItemsAsync(CancellationToken cancellationToken = default) => Task.FromResult(new List<ItemModel>());
            public Task AddItemAsync(ItemModel item, CancellationToken cancellationToken = default) => throw new NotImplementedException();
            public Task UpdateItemAsync(ItemModel item, CancellationToken cancellationToken = default) => throw new NotImplementedException();
            public Task DeleteItemAsync(int toolID, CancellationToken cancellationToken = default) => throw new NotImplementedException();
            public Task<ItemModel?> GetItemByIDAsync(int toolID, CancellationToken cancellationToken = default) => throw new NotImplementedException();
            public Task<List<ItemModel>> SearchItemsAsync(string? searchText, CancellationToken cancellationToken = default) => Task.FromResult(new List<ItemModel>());
            public Task<bool> ToggleItemCheckOutStatusAsync(int toolID, string currentUser, CancellationToken cancellationToken = default) => throw new NotImplementedException();
            public Task<List<ItemModel>> GetItemsCheckedOutByAsync(string userName, CancellationToken cancellationToken = default) => Task.FromResult(new List<ItemModel>());
            public Task UpdateItemImageAsync(int toolID, string imagePath, CancellationToken cancellationToken = default) => throw new NotImplementedException();
            public Task<ImageImportResult> ImportItemImagesAsync(string folderPath, Func<ItemModel, IEnumerable<string>> keySelector, IProgress<ImageImportProgress>? progress = null, CancellationToken cancellationToken = default) => Task.FromResult(new ImageImportResult());
            public Task UpdateItemQuantitiesAsync(int toolID, int qtyChange, bool isRental, SQLiteConnection? conn = null, SQLiteTransaction? tx = null, CancellationToken cancellationToken = default)
                => throw new InvalidOperationException("fail");
            public Task<string> GenerateNextItemNumberAsync(CancellationToken cancellationToken = default) => Task.FromResult("T1");
        }
    }
}
