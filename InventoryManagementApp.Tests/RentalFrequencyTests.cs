using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using InventoryManagementApp.Interfaces;
using InventoryManagementApp.Services.Core;
using InventoryManagementApp.Services.Items;
using InventoryManagementApp.Services.Rentals;
using InventoryManagementApp.Services.Customers;
using Xunit;

namespace InventoryManagementApp.Tests
{
    public class RentalFrequencyTests : IDisposable
    {
        private readonly string _testDbPath;
        private readonly DatabaseService _dbService;
        private readonly ItemService _itemService;
        private readonly CustomerService _customerService;
        private readonly RentalService _rentalService;

        public RentalFrequencyTests()
        {
            _testDbPath = Path.Combine(Path.GetTempPath(), $"test_rental_freq_{Guid.NewGuid()}.db");
            _dbService = new DatabaseService(_testDbPath);
            _itemService = new ItemService(_dbService);
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
        public async Task GetRentalFrequencyAsync_ReturnsItemsOrderedByRentalCount()
        {
            var customer = await _customerService.AddCustomerAsync("Test Customer", "test@test.com", "John Doe", "", "", "");
            
            var item1 = await _itemService.AddItemAsync("ITEM001", "Item 1", "Loc1", 5, 0, false);
            var item2 = await _itemService.AddItemAsync("ITEM002", "Item 2", "Loc2", 5, 0, false);
            var item3 = await _itemService.AddItemAsync("ITEM003", "Item 3", "Loc3", 5, 0, false);

            await _rentalService.RentItemAsync(item1, customer, DateTime.Today.AddDays(-30), DateTime.Today.AddDays(-23));
            await _rentalService.RentItemAsync(item1, customer, DateTime.Today.AddDays(-20), DateTime.Today.AddDays(-13));
            await _rentalService.RentItemAsync(item1, customer, DateTime.Today.AddDays(-10), DateTime.Today.AddDays(-3));
            
            await _rentalService.RentItemAsync(item2, customer, DateTime.Today.AddDays(-15), DateTime.Today.AddDays(-8));
            await _rentalService.RentItemAsync(item2, customer, DateTime.Today.AddDays(-5), DateTime.Today.AddDays(2));

            await _rentalService.RentItemAsync(item3, customer, DateTime.Today.AddDays(-7), DateTime.Today);

            var frequencies = await _rentalService.GetRentalFrequencyAsync(10);

            Assert.Equal(3, frequencies.Count);
            Assert.Equal("ITEM001", frequencies[0].ItemNumber);
            Assert.Equal(3, frequencies[0].RentalCount);
            Assert.Equal("ITEM002", frequencies[1].ItemNumber);
            Assert.Equal(2, frequencies[1].RentalCount);
            Assert.Equal("ITEM003", frequencies[2].ItemNumber);
            Assert.Equal(1, frequencies[2].RentalCount);
        }

        [Fact]
        public async Task GetRentalFrequencyAsync_WithTopN_ReturnsOnlyTopNItems()
        {
            var customer = await _customerService.AddCustomerAsync("Test Customer", "test@test.com", "John Doe", "", "", "");
            
            for (int i = 1; i <= 5; i++)
            {
                var item = await _itemService.AddItemAsync($"ITEM{i:D3}", $"Item {i}", "Loc", 10, 0, false);
                for (int j = 0; j < i; j++)
                {
                    await _rentalService.RentItemAsync(item, customer, 
                        DateTime.Today.AddDays(-j * 10), DateTime.Today.AddDays(-j * 10 + 5));
                }
            }

            var frequencies = await _rentalService.GetRentalFrequencyAsync(3);

            Assert.Equal(3, frequencies.Count);
            Assert.Equal("ITEM005", frequencies[0].ItemNumber);
            Assert.Equal(5, frequencies[0].RentalCount);
        }

        [Fact]
        public async Task GetRentalFrequencyAsync_WithNoRentals_ReturnsEmptyList()
        {
            await _itemService.AddItemAsync("ITEM001", "Item 1", "Loc1", 5, 0, false);
            await _itemService.AddItemAsync("ITEM002", "Item 2", "Loc2", 5, 0, false);

            var frequencies = await _rentalService.GetRentalFrequencyAsync(10);

            Assert.Empty(frequencies);
        }
    }
}
