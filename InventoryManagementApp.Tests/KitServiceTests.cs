using System;
using System.Threading.Tasks;
using Xunit;
using InventoryManagementApp.Services.Kits;
using InventoryManagementApp.Services.Core;
using InventoryManagementApp.Models.Domain;
using InventoryManagementApp.Interfaces;
using Moq;

namespace InventoryManagementApp.Tests
{
    public class KitServiceTests : IDisposable
    {
        private readonly DatabaseService _databaseService;
        private readonly KitService _kitService;
        private readonly Mock<IUserContext> _userContextMock;

        public KitServiceTests()
        {
            var testDbPath = $"test_kit_{Guid.NewGuid()}.db";
            _databaseService = new DatabaseService(testDbPath);
            _userContextMock = new Mock<IUserContext>();
            _userContextMock.Setup(x => x.CurrentUser).Returns(new User { UserID = 1, UserName = "TestUser" });
            _kitService = new KitService(_databaseService, _userContextMock.Object);
        }

        public void Dispose()
        {
            _databaseService?.Dispose();
        }

        [Fact]
        public async Task CreateKit_ShouldSucceed()
        {
            var kit = new Kit
            {
                KitNumber = "KIT-001",
                Name = "Test Kit",
                Description = "A test kit",
                Category = "Testing",
                IsActive = true
            };

            var id = await _kitService.CreateKitAsync(kit);

            Assert.True(id > 0);
        }

        [Fact]
        public async Task GetAllKits_ShouldReturnList()
        {
            var kit = new Kit
            {
                KitNumber = "KIT-002",
                Name = "Another Kit",
                IsActive = true
            };
            await _kitService.CreateKitAsync(kit);

            var kits = await _kitService.GetAllKitsAsync();

            Assert.NotEmpty(kits);
        }

        [Fact]
        public async Task GetActiveKits_ShouldReturnActiveOnly()
        {
            var activeKit = new Kit
            {
                KitNumber = "KIT-003",
                Name = "Active Kit",
                IsActive = true
            };
            await _kitService.CreateKitAsync(activeKit);

            var activeKits = await _kitService.GetActiveKitsAsync();

            Assert.NotEmpty(activeKits);
        }

        [Fact]
        public async Task UpdateKit_ShouldSucceed()
        {
            var kit = new Kit
            {
                KitNumber = "KIT-004",
                Name = "Update Kit",
                IsActive = true
            };
            var id = await _kitService.CreateKitAsync(kit);
            kit.KitID = id;
            kit.Name = "Updated Kit Name";

            var result = await _kitService.UpdateKitAsync(kit);

            Assert.True(result);
        }

        [Fact]
        public async Task AddKitItem_ShouldSucceed()
        {
            var kit = new Kit
            {
                KitNumber = "KIT-005",
                Name = "Kit with Items",
                IsActive = true
            };
            var kitId = await _kitService.CreateKitAsync(kit);

            var kitItem = new KitItem
            {
                KitID = kitId,
                ItemID = 1,
                Quantity = 2,
                IsOptional = false
            };

            var itemId = await _kitService.AddKitItemAsync(kitItem);

            Assert.True(itemId > 0);
        }

        [Fact]
        public async Task GetKitItems_ShouldReturnItems()
        {
            var kit = new Kit
            {
                KitNumber = "KIT-006",
                Name = "Kit with Multiple Items",
                IsActive = true
            };
            var kitId = await _kitService.CreateKitAsync(kit);

            var kitItem = new KitItem
            {
                KitID = kitId,
                ItemID = 1,
                Quantity = 1,
                IsOptional = false
            };
            await _kitService.AddKitItemAsync(kitItem);

            var items = await _kitService.GetKitItemsAsync(kitId);

            Assert.NotEmpty(items);
        }

        [Fact]
        public async Task RemoveKitItem_ShouldSucceed()
        {
            var kit = new Kit
            {
                KitNumber = "KIT-007",
                Name = "Kit to Remove Item",
                IsActive = true
            };
            var kitId = await _kitService.CreateKitAsync(kit);

            var kitItem = new KitItem
            {
                KitID = kitId,
                ItemID = 1,
                Quantity = 1,
                IsOptional = false
            };
            var itemId = await _kitService.AddKitItemAsync(kitItem);

            var result = await _kitService.RemoveKitItemAsync(itemId);

            Assert.True(result);
        }

        [Fact]
        public async Task DeleteKit_ShouldSucceed()
        {
            var kit = new Kit
            {
                KitNumber = "KIT-008",
                Name = "Kit to Delete",
                IsActive = true
            };
            var id = await _kitService.CreateKitAsync(kit);

            var result = await _kitService.DeleteKitAsync(id);

            Assert.True(result);
        }
    }
}
