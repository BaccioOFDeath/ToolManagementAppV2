using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Data.SQLite;
using ToolManagementAppV2.Models;
using ToolManagementAppV2.Models.Domain;
using ToolManagementAppV2.Models.ImportExport;
using ToolManagementAppV2.Services.Core;
using ToolManagementAppV2.Services.Customers;
using ToolManagementAppV2.Services.Rentals;
using ToolManagementAppV2.Services.Tools;
using ToolManagementAppV2.Interfaces;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using ToolManagementAppV2.Services.Users;
using ToolManagementAppV2.Tests;

namespace ToolManagementAppV2.Tests.Services
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
        public void GetRentalHistoryForTool_ReturnsHistory()
        {
            var dbPath = Path.GetTempFileName();
            try
            {
                var db = new DatabaseService(dbPath);
                IItemService toolService = new ItemService(db);
                ICustomerService customerService = new CustomerService(db);
                IRentalService rentalService = new RentalService(db, toolService);

                var tool = new ItemModel { ToolNumber = "T1", NameDescription = "Hammer", QuantityOnHand = 5 };
                toolService.AddTool(tool);
                var addedTool = toolService.GetAllTools().First();

                var cust = new Customer { Company = "Acme" };
                customerService.AddCustomer(cust);
                var addedCust = customerService.GetAllCustomers().First();

                rentalService.RentTool(addedTool.ToolID, addedCust.CustomerID, DateTime.Today, DateTime.Today.AddDays(1));
                rentalService.RentTool(addedTool.ToolID, addedCust.CustomerID, DateTime.Today.AddDays(2), DateTime.Today.AddDays(3));

                var history = rentalService.GetRentalHistoryForTool(addedTool.ToolID);
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
        public void RentTool_NoAvailability_Throws()
        {
            var dbPath = Path.GetTempFileName();
            try
            {
                var db = new DatabaseService(dbPath);
                IItemService toolService = new ItemService(db);
                ICustomerService customerService = new CustomerService(db);
                IRentalService rentalService = new RentalService(db, toolService);

                var tool = new ItemModel { ToolNumber = "T1", NameDescription = "Hammer", QuantityOnHand = 0 };
                toolService.AddTool(tool);
                var addedTool = toolService.GetAllTools().First();

                customerService.AddCustomer(new Customer { Company = "Acme" });
                var cust = customerService.GetAllCustomers().First();

                Assert.Throws<InvalidOperationException>(() =>
                    rentalService.RentTool(addedTool.ToolID, cust.CustomerID, DateTime.Today, DateTime.Today.AddDays(1)));
                Assert.Empty(rentalService.GetAllRentals());
            }
            finally
            {
                if (File.Exists(dbPath))
                    File.Delete(dbPath);
            }
        }

        [Fact]
        public void RentTool_NoAvailability_Throws_WithHelper()
        {
            var dbPath = Path.GetTempFileName();
            try
            {
                var db = new DatabaseService(dbPath);
                IItemService toolService = new ItemService(db);
                ICustomerService customerService = new CustomerService(db);
                IRentalService rentalService = new RentalService(db, toolService);

                var tool = new ItemModel { ToolNumber = "T2", NameDescription = "Wrench", QuantityOnHand = 0 };
                toolService.AddTool(tool);
                var addedTool = toolService.GetAllTools().First();

                customerService.AddCustomer(new Customer { Company = "Beta" });
                var cust = customerService.GetAllCustomers().First();

                Assert.Throws<InvalidOperationException>(() =>
                    rentalService.RentTool(addedTool.ToolID, cust.CustomerID, DateTime.Today, DateTime.Today.AddDays(1)));
                Assert.Empty(rentalService.GetAllRentals());
            }
            finally
            {
                if (File.Exists(dbPath))
                    File.Delete(dbPath);
            }
        }

        [Fact]
        public void ReturnTool_InvalidRentalID_Throws()
        {
            var dbPath = Path.GetTempFileName();
            try
            {
                var db = new DatabaseService(dbPath);
                var toolService = new ItemService(db);
                IRentalService rentalService = new RentalService(db, toolService);

                Assert.Throws<InvalidOperationException>(() => rentalService.ReturnTool(1, DateTime.Today));
            }
            finally
            {
                if (File.Exists(dbPath))
                    File.Delete(dbPath);
            }
        }

        [Fact]
        public void ReturnTool_InvalidRentalID_Throws_WithHelper()
        {
            var dbPath = Path.GetTempFileName();
            try
            {
                var db = new DatabaseService(dbPath);
                var toolService = new ItemService(db);
                IRentalService rentalService = new RentalService(db, toolService);

                Assert.Throws<InvalidOperationException>(() => rentalService.ReturnTool(1, DateTime.Today));
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

                var tool = new ItemModel { ToolNumber = "T1", NameDescription = "Hammer", QuantityOnHand = 1 };
                toolService.AddTool(tool);
                var addedTool = toolService.GetAllTools().First();

                customerService.AddCustomer(new Customer { Company = "Acme" });
                var cust = customerService.GetAllCustomers().First();

                rentalService.RentTool(addedTool.ToolID, cust.CustomerID, DateTime.Today, DateTime.Today.AddDays(1));
                var rental = rentalService.GetAllRentals().First();

                rentalService.ReturnTool(rental.RentalID, DateTime.Today);

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

                var tool = new ItemModel { ToolNumber = "T1", NameDescription = "Hammer", QuantityOnHand = 1 };
                toolService.AddTool(tool);
                var addedTool = toolService.GetAllTools().First();

                customerService.AddCustomer(new Customer { Company = "Acme" });
                var cust = customerService.GetAllCustomers().First();

                rentalService.RentTool(addedTool.ToolID, cust.CustomerID, DateTime.Today, DateTime.Today.AddDays(1));
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

                var tool = new ItemModel { ToolNumber = "T1", NameDescription = "Hammer", QuantityOnHand = 1, Location = "A1", ToolImagePath = "path" };
                toolService.AddTool(tool);
                var addedTool = toolService.GetAllTools().First();

                var customer = new Customer { Company = "Acme", Contact = "Bob", Email = "b@c.com", Phone = "111", Mobile = "222", Address = "Addr" };
                customerService.AddCustomer(customer);
                var addedCust = customerService.GetAllCustomers().First();

                rentalService.RentTool(addedTool.ToolID, addedCust.CustomerID, DateTime.Today, DateTime.Today.AddDays(1));

                var rentals = rentalService.GetActiveRentals();
                var r = rentals.First();
                Assert.Equal("A1", r.ToolLocation);
                Assert.Equal("path", r.ToolImagePath);
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
        public void RentAndReturnTool_UpdatesQuantities()
        {
            var dbPath = Path.GetTempFileName();
            try
            {
                var db = new DatabaseService(dbPath);
                var toolService = new ItemService(db);
                var customerService = new CustomerService(db);
                var rentalService = new RentalService(db, toolService);

                toolService.AddTool(new ItemModel { ToolNumber = "T1", NameDescription = "Hammer", QuantityOnHand = 1 });
                var tool = toolService.GetAllTools().First();

                customerService.AddCustomer(new Customer { Company = "Acme" });
                var cust = customerService.GetAllCustomers().First();

                rentalService.RentTool(tool.ToolID, cust.CustomerID, DateTime.Today, DateTime.Today.AddDays(1));

                var rented = toolService.GetToolByID(tool.ToolID);
                Assert.Equal(0, rented.QuantityOnHand);
                Assert.Equal(1, rented.RentedQuantity);

                var rental = rentalService.GetAllRentals().First();
                rentalService.ReturnTool(rental.RentalID, DateTime.Today);

                var returned = toolService.GetToolByID(tool.ToolID);
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

                toolService.AddTool(new ItemModel { ToolNumber = "T1", NameDescription = "Hammer", QuantityOnHand = 1 });
                var tool = toolService.GetAllTools().First();

                customerService.AddCustomer(new Customer { Company = "Acme" });
                var cust = customerService.GetAllCustomers().First();

                rentalService.RentTool(tool.ToolID, cust.CustomerID, DateTime.Today.AddDays(-2), DateTime.Today.AddDays(-1));
                var rental = rentalService.GetAllRentals().First();
                var originalDue = rental.DueDate;

                var failingItemService = new FailingItemService();
                var rentalService2 = new RentalService(db, failingItemService);

                Assert.Throws<InvalidOperationException>(() =>
                    rentalService2.ExtendRental(rental.RentalID, DateTime.Today.AddDays(1)));

                var after = rentalService2.GetAllRentals().First();
                Assert.Equal(originalDue, after.DueDate);

                var toolAfter = toolService.GetToolByID(tool.ToolID);
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
        public async Task RentToolAsync_LogsActivity()
        {
            var dbPath = Path.GetTempFileName();
            try
            {
                var db = new DatabaseService(dbPath);
                var ctx = new StubUserContext { CurrentUser = new User { UserID = 1, UserName = "tester", IsAdmin = true } };
                var auth = new AllowAllAuthorizationService();
                var logService = new ActivityLogService(db);
                var toolService = new ItemService(db, auth, null, logService, ctx);
                var customerService = new CustomerService(db, auth);
                var rentalService = new RentalService(db, auth, toolService, null, logService, ctx);

                await toolService.AddToolAsync(new ItemModel { ToolNumber = "T1", NameDescription = "Hammer", QuantityOnHand = 1, RentedQuantity = 0 });
                await customerService.AddCustomerAsync(new Customer { Company = "Acme" });
                var tool = (await toolService.GetAllToolsAsync()).First();
                var cust = (await customerService.GetAllCustomersAsync()).First();

                await rentalService.RentToolAsync(tool.ToolID, cust.CustomerID, DateTime.Today, DateTime.Today.AddDays(1));

                var logs = await logService.GetRecentLogsAsync();
                Assert.Contains(logs.Value, l => l.Action.Contains("Rented tool"));
            }
            finally
            {
                if (File.Exists(dbPath))
                    File.Delete(dbPath);
            }
        }

        class FailingItemService : IItemService
        {
            public Task<List<int>> ImportToolsFromCsvAsync(string filePath, IDictionary<string, string> map, CancellationToken cancellationToken) => throw new NotImplementedException();
            public Task ExportToolsToCsvAsync(string filePath, CancellationToken cancellationToken = default) => Task.CompletedTask;
            public Task<List<ItemModel>> GetAllToolsAsync(CancellationToken cancellationToken = default) => Task.FromResult(new List<ItemModel>());
            public Task AddToolAsync(ItemModel tool, CancellationToken cancellationToken = default) => throw new NotImplementedException();
            public Task UpdateToolAsync(ItemModel tool, CancellationToken cancellationToken = default) => throw new NotImplementedException();
            public Task DeleteToolAsync(int toolID, CancellationToken cancellationToken = default) => throw new NotImplementedException();
            public Task<ItemModel?> GetToolByIDAsync(int toolID, CancellationToken cancellationToken = default) => throw new NotImplementedException();
            public Task<List<ItemModel>> SearchToolsAsync(string? searchText, CancellationToken cancellationToken = default) => Task.FromResult(new List<ItemModel>());
            public Task<bool> ToggleToolCheckOutStatusAsync(int toolID, string currentUser, CancellationToken cancellationToken = default) => throw new NotImplementedException();
            public Task<List<ItemModel>> GetToolsCheckedOutByAsync(string userName, CancellationToken cancellationToken = default) => Task.FromResult(new List<ItemModel>());
            public Task UpdateToolImageAsync(int toolID, string imagePath, CancellationToken cancellationToken = default) => throw new NotImplementedException();
            public Task<ImageImportResult> ImportToolImagesAsync(string folderPath, Func<ItemModel, IEnumerable<string>> keySelector, IProgress<ImageImportProgress>? progress = null, CancellationToken cancellationToken = default) => Task.FromResult(new ImageImportResult());
            public Task UpdateToolQuantitiesAsync(int toolID, int qtyChange, bool isRental, SQLiteConnection? conn = null, SQLiteTransaction? tx = null, CancellationToken cancellationToken = default)
                => throw new InvalidOperationException("fail");
            public Task<string> GenerateNextToolNumberAsync(CancellationToken cancellationToken = default) => Task.FromResult("T1");
        }
    }
}
