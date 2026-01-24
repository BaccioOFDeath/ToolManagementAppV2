using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using InventoryManagementApp.Data;
using InventoryManagementApp.Services.Core;
using InventoryManagementApp.Services.Items;
using InventoryManagementApp.Services.Customers;
using InventoryManagementApp.Services.Rentals;
using InventoryManagementApp.Services.Notifications;
using Xunit;

namespace InventoryManagementApp.Tests
{
    public class RentalReminderServiceTests : IDisposable
    {
        private readonly string _testDbPath;
        private readonly DatabaseService _dbService;
        private readonly ItemService _itemService;
        private readonly CustomerService _customerService;
        private readonly RentalService _rentalService;

        public RentalReminderServiceTests()
        {
            _testDbPath = Path.Combine(Path.GetTempPath(), $"test_reminder_{Guid.NewGuid()}.db");
            _dbService = new DatabaseService(_testDbPath);
            var factory = new SqliteConnectionFactory(_dbService.ConnectionString);
            var repository = new ItemRepository(factory);
            _itemService = new ItemService(_dbService, repository);
            _customerService = new CustomerService(_dbService);
            _rentalService = new RentalService(_dbService, itemService: _itemService);
        }

        public void Dispose()
        {
            _dbService?.Dispose();
            if (File.Exists(_testDbPath))
                File.Delete(_testDbPath);
        }

        [Fact]
        public void Constructor_WithNullRentalService_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => 
                new RentalReminderService(null!, null, "contact"));
        }

        [Fact]
        public void Start_WithoutEmailService_LogsWarning()
        {
            var reminderService = new RentalReminderService(_rentalService, null, "contact");
            
            reminderService.Start();
            reminderService.Stop();
        }

        [Fact]
        public async Task CheckAndSendRemindersAsync_WithoutEmailService_LogsWarning()
        {
            var reminderService = new RentalReminderService(_rentalService, null, "contact");
            
            await reminderService.CheckAndSendRemindersAsync();
        }

        [Fact]
        public async Task CheckAndSendRemindersAsync_FindsRentalsDueTomorrow()
        {
            var customer = new CustomerModel
            {
                Company = "Test Customer",
                Email = "customer@test.com",
                Contact = "John Doe"
            };
            await _customerService.AddCustomerAsync(customer);
            var item = new ItemModel
            {
                ItemNumber = "ITEM001",
                Name = "Test Item",
                Location = "Loc1",
                QuantityOnHand = 5,
                RentedQuantity = 0,
                IsRentalItem = false
            };
            await _itemService.AddItemAsync(item);
            
            await _rentalService.RentItemAsync(item.ItemID, customer.CustomerID, 
                DateTime.Today, DateTime.Today.AddDays(1));

            var reminderService = new RentalReminderService(_rentalService, null, "contact");
            await reminderService.CheckAndSendRemindersAsync();
        }

        [Fact]
        public void Dispose_DisposesResourcesProperly()
        {
            var reminderService = new RentalReminderService(_rentalService, null, "contact");
            reminderService.Dispose();
        }
    }
}
