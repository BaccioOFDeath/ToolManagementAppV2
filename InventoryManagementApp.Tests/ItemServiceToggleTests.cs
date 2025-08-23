using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Data;
using System.Linq;
using Dapper;
using InventoryManagementApp.Data;
using InventoryManagementApp.Interfaces;
using InventoryManagementApp.Models.Domain;
using InventoryManagementApp.Services.Items;
using Xunit;

public class ItemServiceToggleTests
{
    private sealed class DummyItemRepository : IItemRepository
    {
        public IAsyncEnumerable<ItemModel> GetPageAsync(ItemFilter filter, ItemPage page, CancellationToken ct) => AsyncEnumerable.Empty<ItemModel>();
        public Task<int> CountAsync(ItemFilter filter, CancellationToken ct) => Task.FromResult(0);
        public Task SaveChangesAsync(IEnumerable<ItemModel> changes, CancellationToken ct) => Task.CompletedTask;
    }

    private sealed class DummyUserContext : IUserContext
    {
        public User? CurrentUser { get; set; }
        public event EventHandler<User?>? UserChanged;
        public bool IsAdmin => CurrentUser?.IsAdmin ?? false;
        public string UserName => CurrentUser?.UserName ?? string.Empty;
        public string Role => IsAdmin ? "Admin" : "User";
    }

    private sealed class NonAdminAuthorizationService : IAuthorizationService
    {
        public bool IsAdmin => false;
        public void EnsureAdmin() => throw new UnauthorizedAccessException();
    }

    private sealed class AdminAuthorizationService : IAuthorizationService
    {
        public bool IsAdmin => true;
        public void EnsureAdmin() { }
    }

    private static async Task InitializeAsync(DatabaseService db)
    {
        using var conn = db.CreateConnection();
        var cmd = conn.CreateCommand();
        cmd.CommandText = @"CREATE TABLE Items (
            ItemID INTEGER PRIMARY KEY AUTOINCREMENT,
            ItemNumber TEXT,
            NameDescription TEXT,
            Location TEXT,
            Brand TEXT,
            PartNumber TEXT,
            Supplier TEXT,
            PurchasedDate TEXT,
            Notes TEXT,
            Keywords TEXT,
            AvailableQuantity INTEGER,
            RentedQuantity INTEGER,
            IsRentalItem INTEGER,
            Price NUMERIC NOT NULL DEFAULT 0,
            ImagePath TEXT,
            IsCheckedOut INTEGER,
            CheckedOutBy TEXT,
            CheckedOutTime TEXT,
            CheckedInBy TEXT,
            CheckedInTime TEXT,
            IsPowered INTEGER,
            UpdatedAt TEXT
        );";
        cmd.ExecuteNonQuery();
        await conn.ExecuteAsync("INSERT INTO Items (ItemNumber, NameDescription, AvailableQuantity, RentedQuantity, IsRentalItem, IsCheckedOut, IsPowered, UpdatedAt) VALUES (@ItemNumber,@Name,1,0,0,0,0,@UpdatedAt)", new { ItemNumber = "A1", Name = "Saw", UpdatedAt = DateTime.UtcNow });
        await conn.ExecuteAsync("INSERT INTO Items (ItemNumber, NameDescription, AvailableQuantity, RentedQuantity, IsRentalItem, IsCheckedOut, IsPowered, UpdatedAt) VALUES (@ItemNumber,@Name,1,0,1,0,0,@UpdatedAt)", new { ItemNumber = "B1", Name = "Drill", UpdatedAt = DateTime.UtcNow });
    }

    [Fact]
    public async Task NonAdminUserCanToggleStatusAndQuantityUpdates()
    {
        var dbPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".db");
        await using var db = new DatabaseService(dbPath);
        await InitializeAsync(db);
        var userContext = new DummyUserContext { CurrentUser = new User { UserID = 1, UserName = "user1", IsAdmin = false } };
        var service = new ItemService(db, new DummyItemRepository(), new NonAdminAuthorizationService(), userContext: userContext);

        var checkout = await service.ToggleItemCheckOutStatusAsync(1, CancellationToken.None);
        Assert.True(checkout);

        using (var conn = db.CreateConnection())
        {
            var record = await conn.QuerySingleAsync("SELECT IsCheckedOut, AvailableQuantity, CheckedOutBy, CheckedInBy, CheckedInTime FROM Items WHERE ItemID=1");
            Assert.Equal(1L, record.IsCheckedOut);
            Assert.Equal(0L, record.AvailableQuantity);
            Assert.Equal("user1", (string)record.CheckedOutBy);
            Assert.Null(record.CheckedInBy);
            Assert.Null(record.CheckedInTime);
        }

        var checkin = await service.ToggleItemCheckOutStatusAsync(1, CancellationToken.None);
        Assert.True(checkin);

        using (var conn = db.CreateConnection())
        {
            var record = await conn.QuerySingleAsync("SELECT IsCheckedOut, AvailableQuantity, CheckedOutBy, CheckedInBy, CheckedInTime FROM Items WHERE ItemID=1");
            Assert.Equal(0L, record.IsCheckedOut);
            Assert.Equal(1L, record.AvailableQuantity);
            Assert.Null(record.CheckedOutBy);
            Assert.Equal("user1", (string)record.CheckedInBy);
            Assert.NotNull(record.CheckedInTime);
        }


        File.Delete(dbPath);
    }

    [Fact]
    public async Task NonAdminCannotCheckInItemCheckedOutByAnotherUser()
    {
        var dbPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".db");
        await using var db = new DatabaseService(dbPath);
        await InitializeAsync(db);
        var userContext1 = new DummyUserContext { CurrentUser = new User { UserID = 1, UserName = "user1", IsAdmin = false } };
        var service1 = new ItemService(db, new DummyItemRepository(), new NonAdminAuthorizationService(), userContext: userContext1);
        var checkout = await service1.ToggleItemCheckOutStatusAsync(1, CancellationToken.None);
        Assert.True(checkout);

        var userContext2 = new DummyUserContext { CurrentUser = new User { UserID = 2, UserName = "user2", IsAdmin = false } };
        var service2 = new ItemService(db, new DummyItemRepository(), new NonAdminAuthorizationService(), userContext: userContext2);
        var attempt = await service2.ToggleItemCheckOutStatusAsync(1, CancellationToken.None);
        Assert.False(attempt);

        File.Delete(dbPath);
    }

    [Fact]
    public async Task AdminCanCheckInItemCheckedOutByAnotherUser()
    {
        var dbPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".db");
        await using var db = new DatabaseService(dbPath);
        await InitializeAsync(db);
        var userContext1 = new DummyUserContext { CurrentUser = new User { UserID = 1, UserName = "user1", IsAdmin = false } };
        var service1 = new ItemService(db, new DummyItemRepository(), new NonAdminAuthorizationService(), userContext: userContext1);
        var checkout = await service1.ToggleItemCheckOutStatusAsync(1, CancellationToken.None);
        Assert.True(checkout);

        var adminContext = new DummyUserContext { CurrentUser = new User { UserID = 3, UserName = "admin", IsAdmin = true } };
        var adminService = new ItemService(db, new DummyItemRepository(), new AdminAuthorizationService(), userContext: adminContext);
        var checkin = await adminService.ToggleItemCheckOutStatusAsync(1, CancellationToken.None);
        Assert.True(checkin);

        using (var conn = db.CreateConnection())
        {
            var record = await conn.QuerySingleAsync("SELECT IsCheckedOut, AvailableQuantity, CheckedOutBy, CheckedInBy, CheckedInTime FROM Items WHERE ItemID=1");
            Assert.Equal(0L, record.IsCheckedOut);
            Assert.Equal(1L, record.AvailableQuantity);
            Assert.Null(record.CheckedOutBy);
            Assert.Equal("admin", (string)record.CheckedInBy);
            Assert.NotNull(record.CheckedInTime);
        }

        File.Delete(dbPath);
    }

    [Fact]
    public async Task RentalItemCannotBeToggled()
    {
        var dbPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".db");
        await using var db = new DatabaseService(dbPath);
        await InitializeAsync(db);
        var userContext = new DummyUserContext { CurrentUser = new User { UserID = 1, UserName = "user1", IsAdmin = false } };
        var service = new ItemService(db, new DummyItemRepository(), new NonAdminAuthorizationService(), userContext: userContext);

        var result = await service.ToggleItemCheckOutStatusAsync(2, CancellationToken.None);
        Assert.False(result);

        using (var conn = db.CreateConnection())
        {
            var record = await conn.QuerySingleAsync("SELECT IsCheckedOut FROM Items WHERE ItemID=2");
            Assert.Equal(0L, record.IsCheckedOut);
        }

        File.Delete(dbPath);
    }

    [Fact]
    public async Task GetItemsCheckedOutByAsyncExcludesRentalItems()
    {
        var dbPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".db");
        await using var db = new DatabaseService(dbPath);
        await InitializeAsync(db);
        var userContext = new DummyUserContext { CurrentUser = new User { UserID = 1, UserName = "user1", IsAdmin = false } };
        var service = new ItemService(db, new DummyItemRepository(), new NonAdminAuthorizationService(), userContext: userContext);

        await service.ToggleItemCheckOutStatusAsync(1, CancellationToken.None);

        using (var conn = db.CreateConnection())
        {
            await conn.ExecuteAsync("UPDATE Items SET IsCheckedOut=1, CheckedOutBy='user1' WHERE ItemID=2");
        }

        var items = await service.GetItemsCheckedOutByAsync("user1", CancellationToken.None);
        Assert.Single(items);
        Assert.Equal(1, items[0].ItemID);

        File.Delete(dbPath);
    }
}
