using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;
using InventoryManagementApp.Data;
using InventoryManagementApp.Interfaces;
using InventoryManagementApp.Models;
using InventoryManagementApp.Models.Domain;
using InventoryManagementApp.Models.ImportExport;
using InventoryManagementApp.Services.Core;
using InventoryManagementApp.Services.Items;
using InventoryManagementApp.Services.Users;
using InventoryManagementApp.ViewModels;
using Microsoft.Data.Sqlite;
using Xunit;

public class DashboardViewModelTests
{
    private sealed class FakeItemRepository : IItemRepository
    {
        public int CountCalls { get; private set; }

        public IAsyncEnumerable<ItemModel> GetPageAsync(ItemFilter filter, ItemPage page, CancellationToken ct)
            => AsyncEnumerable.Empty<ItemModel>();

        public Task<int> CountAsync(ItemFilter filter, CancellationToken ct)
        {
            CountCalls++;
            return Task.FromResult(42);
        }

        public Task<ItemModel?> GetByIdAsync(int id, CancellationToken ct)
            => Task.FromResult<ItemModel?>(null);

        public Task SaveChangesAsync(IEnumerable<ItemModel> changes, CancellationToken ct)
            => Task.CompletedTask;

        public Task<int> InsertAsync(ItemModel item, CancellationToken ct) => Task.FromResult(0);
        public Task UpdateAsync(ItemModel item, CancellationToken ct) => Task.CompletedTask;
        public Task DeleteAsync(int itemID, CancellationToken ct) => Task.CompletedTask;
        public Task<bool> ToggleCheckOutStatusAsync(int itemID, string currentUser, bool isAdmin, CancellationToken ct) => Task.FromResult(false);
        public Task<List<ItemModel>> GetItemsCheckedOutByAsync(string userName, CancellationToken ct) => Task.FromResult(new List<ItemModel>());
        public Task<List<ItemModel>> GetCheckedOutItemsAsync(CancellationToken ct) => Task.FromResult(new List<ItemModel>());
        public Task UpdateItemImageAsync(int itemID, string imagePath, CancellationToken ct) => Task.CompletedTask;
    }

    private sealed class StubRentalService : IRentalService
    {
        public int CountCalls { get; private set; }
        public int GetCalls { get; private set; }
        public int ReturnCalls { get; private set; }
        public List<Rental> Rentals { get; } = new();
        public bool ReturnShouldThrow { get; set; }

        public Task<int> CountActiveRentalsAsync()
        {
            CountCalls++;
            return Task.FromResult(3);
        }

        public Task<List<Rental>> GetActiveRentalsAsync()
        {
            GetCalls++;
            return Task.FromResult(Rentals);
        }

        public Task ReturnItemAsync(int rentalID, DateTime returnDate)
        {
            ReturnCalls++;
            if (ReturnShouldThrow)
                throw new Exception("fail");
            return Task.CompletedTask;
        }

        public Task DeleteRentalAsync(int rentalID) => throw new NotImplementedException();
        public Task ExtendRentalAsync(int rentalID, DateTime newDueDate) => throw new NotImplementedException();
        public Task<List<Rental>> GetAllRentalsAsync() => throw new NotImplementedException();
        public Task<List<Rental>> GetOverdueRentalsAsync() => throw new NotImplementedException();
        public Task<List<Rental>> GetRentalHistoryForItemAsync(int itemID) => throw new NotImplementedException();
        public Task<List<Rental>> GetRentalHistoryForCustomerAsync(int customerID) => throw new NotImplementedException();
        public Task RentItemAsync(int itemID, int customerID, DateTime rentalDate, DateTime dueDate) => throw new NotImplementedException();
    }

    private sealed class StubCustomerService : ICustomerService
    {
        public int CountCalls { get; private set; }
        public int GetCalls { get; private set; }

        public Task<int> CountCustomersAsync(CancellationToken cancellationToken = default)
        {
            CountCalls++;
            return Task.FromResult(4);
        }

        public Task<List<Customer>> GetAllCustomersAsync(CancellationToken cancellationToken = default)
        {
            GetCalls++;
            return Task.FromResult(new List<Customer>());
        }

        public Task AddCustomerAsync(Customer customer, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task UpdateCustomerAsync(Customer customer, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task DeleteCustomerAsync(int customerID, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<Customer> GetCustomerByIDAsync(int customerID, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<List<Customer>> SearchCustomersAsync(string searchTerm, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<CustomerImportResult> ImportCustomersFromCsvAsync(string filePath, IDictionary<string, string> map, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task ExportCustomersToCsvAsync(string filePath, CancellationToken cancellationToken = default) => throw new NotImplementedException();
    }

    private sealed class StubUserService : IUserService
    {
        public int CountCalls { get; private set; }
        public int GetCalls { get; private set; }

        public Task<int> CountUsersAsync()
        {
            CountCalls++;
            return Task.FromResult(5);
        }

        public Task<List<User>> GetAllUsersAsync()
        {
            GetCalls++;
            return Task.FromResult(new List<User>());
        }

        public Task<User?> GetUserByIDAsync(int userID) => throw new NotImplementedException();
        public Task<(AuthenticationResult Result, User? User)> AuthenticateUserAsync(string userName, string password) => throw new NotImplementedException();
        public Task<User?> GetCurrentUserAsync() => throw new NotImplementedException();
        public Task AddUserAsync(User user) => throw new NotImplementedException();
        public Task UpdateUserAsync(User user) => throw new NotImplementedException();
        public Task<bool> TryDeleteUserAsync(int userID) => throw new NotImplementedException();
        public Task<bool> ChangeUserPasswordAsync(int userID, string newPassword) => throw new NotImplementedException();
    }

    private sealed class StubActivityLogService : ActivityLogService
    {
        public StubActivityLogService(DatabaseService db) : base(db) { }

        public override Task<Result<List<ActivityLog>>> GetRecentLogsAsync(int count = 50, CancellationToken cancellationToken = default)
            => Task.FromResult(new Result<List<ActivityLog>>(new List<ActivityLog>(), true));
    }

    private sealed class StubItemService : IItemService
    {
        public List<ItemModel> Items { get; } = new();
        public int CheckedOutCalls { get; private set; }
        public int ToggleCalls { get; private set; }
        public int LastToggledItemID { get; private set; }
        public bool ToggleResult { get; set; } = true;

        public Task<int> CountItemsAsync(ItemFilter filter, CancellationToken ct) => Task.FromResult(0);
        public Task<List<ItemModel>> GetCheckedOutItemsAsync(CancellationToken cancellationToken = default)
        {
            CheckedOutCalls++;
            return Task.FromResult(Items);
        }

        public Task AddItemAsync(ItemModel item, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task UpdateItemAsync(ItemModel item, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task DeleteItemAsync(int itemID, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<ItemModel?> GetItemByIDAsync(int itemID, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public IAsyncEnumerable<ItemModel> GetItemsAsync(ItemPage page, SortField sortField = SortField.Name, SortDirection sortDirection = SortDirection.Ascending, bool? isRentalItem = null, CancellationToken cancellationToken = default) => AsyncEnumerable.Empty<ItemModel>();
        public IAsyncEnumerable<ItemModel> SearchItemsAsync(string? searchText, ItemPage page, SortField sortField = SortField.Name, SortDirection sortDirection = SortDirection.Ascending, bool? isRentalItem = null, CancellationToken cancellationToken = default) => AsyncEnumerable.Empty<ItemModel>();
        public Task SaveChangesAsync(IEnumerable<ItemModel> changes, CancellationToken ct) => Task.CompletedTask;
        public Task<bool> ToggleItemCheckOutStatusAsync(int itemID, CancellationToken cancellationToken = default)
        {
            ToggleCalls++;
            LastToggledItemID = itemID;
            return Task.FromResult(ToggleResult);
        }
        public Task<List<ItemModel>> GetItemsCheckedOutByAsync(string userName, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task UpdateItemImageAsync(int itemID, string imagePath, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<List<int>> ImportItemsFromCsvAsync(string filePath, IDictionary<string, string> map, CancellationToken cancellationToken) => throw new NotImplementedException();
        public Task ExportItemsToCsvAsync(string filePath, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<ImageImportResult> ImportItemImagesAsync(string folderPath, Func<ItemModel, IEnumerable<string>> keySelector, IProgress<ImageImportProgress>? progress = null, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<string> GenerateNextItemNumberAsync(CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task UpdateItemQuantitiesAsync(int itemID, int qtyChange, bool isRental, SqliteConnection? conn = null, SqliteTransaction? tx = null, CancellationToken cancellationToken = default) => throw new NotImplementedException();
    }

    private sealed class SlowItemRepository : IItemRepository
    {
        public bool Canceled { get; private set; }

        public IAsyncEnumerable<ItemModel> GetPageAsync(ItemFilter filter, ItemPage page, CancellationToken ct)
            => AsyncEnumerable.Empty<ItemModel>();

        public async Task<int> CountAsync(ItemFilter filter, CancellationToken ct)
        {
            try
            {
                await Task.Delay(TimeSpan.FromSeconds(5), ct);
                return 0;
            }
            catch (OperationCanceledException)
            {
                Canceled = true;
                throw;
            }
        }

        public Task SaveChangesAsync(IEnumerable<ItemModel> changes, CancellationToken ct)
            => Task.CompletedTask;

        public Task<int> InsertAsync(ItemModel item, CancellationToken ct) => Task.FromResult(0);
        public Task UpdateAsync(ItemModel item, CancellationToken ct) => Task.CompletedTask;
        public Task DeleteAsync(int itemID, CancellationToken ct) => Task.CompletedTask;
        public Task<bool> ToggleCheckOutStatusAsync(int itemID, string currentUser, bool isAdmin, CancellationToken ct) => Task.FromResult(false);
        public Task<List<ItemModel>> GetItemsCheckedOutByAsync(string userName, CancellationToken ct) => Task.FromResult(new List<ItemModel>());
        public Task<List<ItemModel>> GetCheckedOutItemsAsync(CancellationToken ct) => Task.FromResult(new List<ItemModel>());
        public Task UpdateItemImageAsync(int itemID, string imagePath, CancellationToken ct) => Task.CompletedTask;
        public Task<ItemModel?> GetByIdAsync(int id, CancellationToken ct) => Task.FromResult<ItemModel?>(null);
    }

    private sealed class CancellableActivityLogService : ActivityLogService
    {
        public bool Canceled { get; private set; }

        public CancellableActivityLogService(DatabaseService db) : base(db) { }

        public override async Task<Result<List<ActivityLog>>> GetRecentLogsAsync(int count = 50, CancellationToken cancellationToken = default)
        {
            try
            {
                await Task.Delay(TimeSpan.FromSeconds(5), cancellationToken);
                return new Result<List<ActivityLog>>(new List<ActivityLog>(), true);
            }
            catch (OperationCanceledException)
            {
                Canceled = true;
                throw;
            }
        }
    }

    [Fact]
    public async Task LoadStatsAsync_UsesRepositoryCount()
    {
        using var db = new DatabaseService(":memory:");
        var repo = new FakeItemRepository();
        var itemService = new ItemService(db, repo);
        var rentalService = new StubRentalService();
        var customerService = new StubCustomerService();
        var userService = new StubUserService();
        var activityLogService = new StubActivityLogService(db);

        var vm = new DashboardViewModel(
            itemService,
            rentalService,
            customerService,
            userService,
            activityLogService,
            new RelayCommand(() => { }),
            new RelayCommand(() => { }),
            new RelayCommand(() => { }));

        var before = repo.CountCalls;
        await vm.LoadStatsAsync(CancellationToken.None);

        Assert.Equal(before + 1, repo.CountCalls);
        Assert.Equal(1, rentalService.CountCalls);
        Assert.Equal(0, rentalService.GetCalls);
        Assert.Equal(1, customerService.CountCalls);
        Assert.Equal(0, customerService.GetCalls);
        Assert.Equal(1, userService.CountCalls);
        Assert.Equal(0, userService.GetCalls);
        Assert.Contains(vm.StatCards, s => s.Title == "Total Items" && s.Value == "42");
    }

    [Fact]
    public async Task LoadAsync_Cancelled_DoesNotPopulateCollections()
    {
        using var db = new DatabaseService(":memory:");
        var repo = new SlowItemRepository();
        var itemService = new ItemService(db, repo);
        var rentalService = new StubRentalService();
        var customerService = new StubCustomerService();
        var userService = new StubUserService();
        var activityLogService = new CancellableActivityLogService(db);

        var vm = new DashboardViewModel(
            itemService,
            rentalService,
            customerService,
            userService,
            activityLogService,
            new RelayCommand(() => { }),
            new RelayCommand(() => { }),
            new RelayCommand(() => { }));

        using var cts = new CancellationTokenSource();
        var task = vm.LoadAsync(cts.Token);
        cts.Cancel();
        await task;

        Assert.True(repo.Canceled);
        Assert.True(activityLogService.Canceled);
        Assert.Empty(vm.StatCards);
        Assert.Empty(vm.RecentActivity);
    }

    [Fact]
    public async Task LoadCheckedOutItemsAsync_PopulatesCollection()
    {
        using var db = new DatabaseService(":memory:");
        var itemService = new StubItemService();
        itemService.Items.Add(new ItemModel { ItemNumber = "Y1", CheckedOutBy = "Bob" });
        var rentalService = new StubRentalService();
        var customerService = new StubCustomerService();
        var userService = new StubUserService();
        var activityLogService = new StubActivityLogService(db);

        var vm = new DashboardViewModel(
            itemService,
            rentalService,
            customerService,
            userService,
            activityLogService,
            new RelayCommand(() => { }),
            new RelayCommand(() => { }),
            new RelayCommand(() => { }));

        await vm.LoadCheckedOutItemsAsync(CancellationToken.None);

        Assert.Single(vm.CheckedOutItems);
        Assert.Equal("Y1", vm.CheckedOutItems[0].ItemNumber);
        Assert.Equal("Bob", vm.CheckedOutItems[0].CheckedOutBy);
        Assert.Equal(1, itemService.CheckedOutCalls);
    }

    [Fact]
    public async Task LoadAsync_PopulatesCheckedOutItems()
    {
        using var db = new DatabaseService(":memory:");
        var itemService = new StubItemService();
        itemService.Items.Add(new ItemModel { ItemNumber = "X1", CheckedOutBy = "Alice" });
        var rentalService = new StubRentalService();
        var customerService = new StubCustomerService();
        var userService = new StubUserService();
        var activityLogService = new StubActivityLogService(db);

        var vm = new DashboardViewModel(
            itemService,
            rentalService,
            customerService,
            userService,
            activityLogService,
            new RelayCommand(() => { }),
            new RelayCommand(() => { }),
            new RelayCommand(() => { }));

        await vm.LoadAsync(CancellationToken.None);

        Assert.Single(vm.CheckedOutItems);
        Assert.Equal("X1", vm.CheckedOutItems[0].ItemNumber);
        Assert.Equal("Alice", vm.CheckedOutItems[0].CheckedOutBy);
        Assert.Equal(1, itemService.CheckedOutCalls);
    }

    [Fact]
    public async Task CheckInItemCommand_RemovesItemOnSuccess()
    {
        using var db = new DatabaseService(":memory:");
        var itemService = new StubItemService();
        var rentalService = new StubRentalService();
        var customerService = new StubCustomerService();
        var userService = new StubUserService();
        var activityLogService = new StubActivityLogService(db);

        var vm = new DashboardViewModel(
            itemService,
            rentalService,
            customerService,
            userService,
            activityLogService,
            new RelayCommand(() => { }),
            new RelayCommand(() => { }),
            new RelayCommand(() => { }));

        var item = new ItemModel { ItemID = 1, ItemNumber = "X1", CheckedOutBy = "Alice" };
        vm.CheckedOutItems.Add(item);

        await vm.CheckInItemCommand.ExecuteAsync(item);

        Assert.Empty(vm.CheckedOutItems);
        Assert.Equal(1, itemService.ToggleCalls);
        Assert.Equal(1, itemService.LastToggledItemID);
    }

    [Fact]
    public async Task CheckInItemCommand_DoesNotRemoveWhenServiceFails()
    {
        using var db = new DatabaseService(":memory:");
        var itemService = new StubItemService { ToggleResult = false };
        var rentalService = new StubRentalService();
        var customerService = new StubCustomerService();
        var userService = new StubUserService();
        var activityLogService = new StubActivityLogService(db);

        var vm = new DashboardViewModel(
            itemService,
            rentalService,
            customerService,
            userService,
            activityLogService,
            new RelayCommand(() => { }),
            new RelayCommand(() => { }),
            new RelayCommand(() => { }));

        var item = new ItemModel { ItemID = 2, ItemNumber = "Y1", CheckedOutBy = "Bob" };
        vm.CheckedOutItems.Add(item);

        await vm.CheckInItemCommand.ExecuteAsync(item);

        Assert.Single(vm.CheckedOutItems);
        Assert.Equal(1, itemService.ToggleCalls);
        Assert.Equal(2, itemService.LastToggledItemID);
    }

    [Fact]
    public async Task LoadRentedItemsAsync_PopulatesCollection()
    {
        using var db = new DatabaseService(":memory:");
        var itemService = new StubItemService();
        var rentalService = new StubRentalService();
        rentalService.Rentals.Add(new Rental { RentalID = 1, ItemNumber = "R1", CustomerName = "Carl" });
        var customerService = new StubCustomerService();
        var userService = new StubUserService();
        var activityLogService = new StubActivityLogService(db);

        var vm = new DashboardViewModel(
            itemService,
            rentalService,
            customerService,
            userService,
            activityLogService,
            new RelayCommand(() => { }),
            new RelayCommand(() => { }),
            new RelayCommand(() => { }));

        await vm.LoadRentedItemsAsync(CancellationToken.None);

        Assert.Single(vm.RentedItems);
        Assert.Equal("R1", vm.RentedItems[0].ItemNumber);
        Assert.Equal("Carl", vm.RentedItems[0].CustomerName);
        Assert.Equal(1, rentalService.GetCalls);
    }

    [Fact]
    public async Task LoadAsync_PopulatesRentedItems()
    {
        using var db = new DatabaseService(":memory:");
        var itemService = new StubItemService();
        var rentalService = new StubRentalService();
        rentalService.Rentals.Add(new Rental { RentalID = 2, ItemNumber = "R2", CustomerName = "Dana" });
        var customerService = new StubCustomerService();
        var userService = new StubUserService();
        var activityLogService = new StubActivityLogService(db);

        var vm = new DashboardViewModel(
            itemService,
            rentalService,
            customerService,
            userService,
            activityLogService,
            new RelayCommand(() => { }),
            new RelayCommand(() => { }),
            new RelayCommand(() => { }));

        await vm.LoadAsync(CancellationToken.None);

        Assert.Single(vm.RentedItems);
        Assert.Equal("R2", vm.RentedItems[0].ItemNumber);
        Assert.Equal("Dana", vm.RentedItems[0].CustomerName);
    }

    [Fact]
    public async Task ReturnRentalCommand_RemovesRentalOnSuccess()
    {
        using var db = new DatabaseService(":memory:");
        var itemService = new StubItemService();
        var rentalService = new StubRentalService();
        var customerService = new StubCustomerService();
        var userService = new StubUserService();
        var activityLogService = new StubActivityLogService(db);

        var vm = new DashboardViewModel(
            itemService,
            rentalService,
            customerService,
            userService,
            activityLogService,
            new RelayCommand(() => { }),
            new RelayCommand(() => { }),
            new RelayCommand(() => { }));

        var rental = new Rental { RentalID = 3, ItemNumber = "R3", CustomerName = "Eve" };
        vm.RentedItems.Add(rental);

        await vm.ReturnRentalCommand.ExecuteAsync(rental);

        Assert.Empty(vm.RentedItems);
        Assert.Equal(1, rentalService.ReturnCalls);
    }

    [Fact]
    public async Task ReturnRentalCommand_DoesNotRemoveWhenServiceFails()
    {
        using var db = new DatabaseService(":memory:");
        var itemService = new StubItemService();
        var rentalService = new StubRentalService { ReturnShouldThrow = true };
        var customerService = new StubCustomerService();
        var userService = new StubUserService();
        var activityLogService = new StubActivityLogService(db);

        var vm = new DashboardViewModel(
            itemService,
            rentalService,
            customerService,
            userService,
            activityLogService,
            new RelayCommand(() => { }),
            new RelayCommand(() => { }),
            new RelayCommand(() => { }));

        var rental = new Rental { RentalID = 4, ItemNumber = "R4", CustomerName = "Frank" };
        vm.RentedItems.Add(rental);

        await vm.ReturnRentalCommand.ExecuteAsync(rental);

        Assert.Single(vm.RentedItems);
        Assert.Equal(rental, vm.RentedItems[0]);
        Assert.Equal(1, rentalService.ReturnCalls);
    }
}

