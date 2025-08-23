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

        public Task SaveChangesAsync(IEnumerable<ItemModel> changes, CancellationToken ct)
            => Task.CompletedTask;
    }

    private sealed class StubRentalService : IRentalService
    {
        public int CountCalls { get; private set; }
        public int GetCalls { get; private set; }

        public Task<int> CountActiveRentalsAsync()
        {
            CountCalls++;
            return Task.FromResult(3);
        }

        public Task<List<Rental>> GetActiveRentalsAsync()
        {
            GetCalls++;
            return Task.FromResult(new List<Rental>());
        }

        public Task DeleteRentalAsync(int rentalID) => throw new NotImplementedException();
        public Task ExtendRentalAsync(int rentalID, DateTime newDueDate) => throw new NotImplementedException();
        public Task<List<Rental>> GetAllRentalsAsync() => throw new NotImplementedException();
        public Task<List<Rental>> GetOverdueRentalsAsync() => throw new NotImplementedException();
        public Task<List<Rental>> GetRentalHistoryForItemAsync(int itemID) => throw new NotImplementedException();
        public Task<List<Rental>> GetRentalHistoryForCustomerAsync(int customerID) => throw new NotImplementedException();
        public Task RentItemAsync(int itemID, int customerID, DateTime rentalDate, DateTime dueDate) => throw new NotImplementedException();
        public Task ReturnItemAsync(int rentalID, DateTime returnDate) => throw new NotImplementedException();
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
}

