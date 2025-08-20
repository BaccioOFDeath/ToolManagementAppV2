using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Documents;
using System.Data.SQLite;
using ToolManagementAppV2.Interfaces;
using ToolManagementAppV2.Models;
using ToolManagementAppV2.Models.Domain;
using ToolManagementAppV2.Models.ImportExport;
using ToolManagementAppV2.Services.Core;
using ToolManagementAppV2.Services.Tools;
using ToolManagementAppV2.Services.Rentals;
using ToolManagementAppV2.Services.Users;
using ToolManagementAppV2.Services.Customers;
using Xunit;

namespace ToolManagementAppV2.Tests.Services;

public class ReportServiceAsyncTests
{
    [Fact]
    public void GenerateSummaryReport_UsesConcurrentAsyncCalls()
    {
        const int delay = 200;
        var toolService = new DelayItemService(delay, 3);
        var rentalService = new DelayRentalService(delay, 5, 2);
        var customerService = new DelayCustomerService(delay, 4);
        var userService = new DelayUserService(delay, 1);
        using var db = new DatabaseService(Path.GetTempFileName());
        var activity = new ActivityLogService(db);
        var svc = new ReportService(toolService, rentalService, activity, customerService, userService);

        var sw = Stopwatch.StartNew();
        var doc = svc.GenerateSummaryReport().GetAwaiter().GetResult();
        sw.Stop();

        var text = new TextRange(doc.ContentStart, doc.ContentEnd).Text;
        Assert.Contains("Total Tools: 3", text);
        Assert.Contains("Total Rentals (History): 5", text);
        Assert.Contains("Active Rentals: 2", text);
        Assert.Contains("Total Customers: 4", text);
        Assert.Contains("Total Users: 1", text);

        Assert.True(sw.ElapsedMilliseconds < delay * 3, $"Expected < {delay * 3}ms but was {sw.ElapsedMilliseconds}ms");
    }

    class DelayItemService : IItemService
    {
        readonly int _delay; readonly List<ItemModel> _tools;
        public DelayItemService(int delay, int count)
        { _delay = delay; _tools = new List<ItemModel>(new ItemModel[count]); }

        public Task<List<ItemModel>> GetAllToolsAsync(CancellationToken cancellationToken = default) =>
            Task.Delay(_delay, cancellationToken).ContinueWith(_ => _tools, cancellationToken);

        public Task AddToolAsync(ItemModel tool, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task UpdateToolAsync(ItemModel tool, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task DeleteToolAsync(int toolID, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<ItemModel?> GetToolByIDAsync(int toolID, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<List<ItemModel>> SearchToolsAsync(string? searchText, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<bool> ToggleToolCheckOutStatusAsync(int toolID, string currentUser, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<List<ItemModel>> GetToolsCheckedOutByAsync(string userName, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task UpdateToolImageAsync(int toolID, string imagePath, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<List<int>> ImportToolsFromCsvAsync(string filePath, IDictionary<string, string> map, CancellationToken cancellationToken) => throw new NotImplementedException();
        public Task ExportToolsToCsvAsync(string filePath, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<ImageImportResult> ImportToolImagesAsync(string folderPath, Func<ItemModel, IEnumerable<string>> keySelector, IProgress<ImageImportProgress>? progress = null, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task UpdateToolQuantitiesAsync(int toolID, int qtyChange, bool isRental, SQLiteConnection? conn = null, SQLiteTransaction? tx = null, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<string> GenerateNextItemNumberAsync(CancellationToken cancellationToken = default) => Task.FromResult("T1");
    }

    class DelayRentalService : IRentalService
    {
        readonly int _delay; readonly List<Rental> _all; readonly List<Rental> _active;
        public DelayRentalService(int delay, int allCount, int activeCount)
        { _delay = delay; _all = new List<Rental>(new Rental[allCount]); _active = _all.GetRange(0, activeCount); }
        public List<Rental> GetAllRentals() => _all;
        public Task<List<Rental>> GetAllRentalsAsync() => Task.Delay(_delay).ContinueWith(_ => _all);
        public List<Rental> GetActiveRentals() => _active;
        public Task<List<Rental>> GetActiveRentalsAsync() => Task.Delay(_delay).ContinueWith(_ => _active);
        public void RentTool(int toolID, int customerID, DateTime rentalDate, DateTime dueDate) => throw new NotImplementedException();
        public Task RentToolAsync(int toolID, int customerID, DateTime rentalDate, DateTime dueDate) => throw new NotImplementedException();
        public void ReturnTool(int rentalID, DateTime returnDate) => throw new NotImplementedException();
        public Task ReturnToolAsync(int rentalID, DateTime returnDate) => throw new NotImplementedException();
        public void ExtendRental(int rentalID, DateTime newDueDate) => throw new NotImplementedException();
        public Task ExtendRentalAsync(int rentalID, DateTime newDueDate) => throw new NotImplementedException();
        public void DeleteRental(int rentalID) => throw new NotImplementedException();
        public Task DeleteRentalAsync(int rentalID) => throw new NotImplementedException();
        public List<Rental> GetOverdueRentals() => throw new NotImplementedException();
        public Task<List<Rental>> GetOverdueRentalsAsync() => throw new NotImplementedException();
        public List<Rental> GetRentalHistoryForTool(int toolID) => throw new NotImplementedException();
        public Task<List<Rental>> GetRentalHistoryForToolAsync(int toolID) => throw new NotImplementedException();
        public List<Rental> GetRentalHistoryForCustomer(int customerID) => throw new NotImplementedException();
        public Task<List<Rental>> GetRentalHistoryForCustomerAsync(int customerID) => throw new NotImplementedException();
    }

    class DelayCustomerService : ICustomerService
    {
        readonly int _delay; readonly List<Customer> _customers;
        public DelayCustomerService(int delay, int count)
        { _delay = delay; _customers = new List<Customer>(new Customer[count]); }
        public List<Customer> GetAllCustomers() => _customers;
        public Task<List<Customer>> GetAllCustomersAsync(CancellationToken cancellationToken = default) => Task.Delay(_delay, cancellationToken).ContinueWith(_ => _customers, cancellationToken);
        public void AddCustomer(Customer customer) => throw new NotImplementedException();
        public Task AddCustomerAsync(Customer customer, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public void UpdateCustomer(Customer customer) => throw new NotImplementedException();
        public Task UpdateCustomerAsync(Customer customer, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public void DeleteCustomer(int customerID) => throw new NotImplementedException();
        public Task DeleteCustomerAsync(int customerID, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Customer GetCustomerByID(int customerID) => throw new NotImplementedException();
        public Task<Customer> GetCustomerByIDAsync(int customerID, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public List<Customer> SearchCustomers(string searchTerm) => throw new NotImplementedException();
        public Task<List<Customer>> SearchCustomersAsync(string searchTerm, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public CustomerImportResult ImportCustomersFromCsv(string filePath, IDictionary<string, string> map) => throw new NotImplementedException();
        public Task<CustomerImportResult> ImportCustomersFromCsvAsync(string filePath, IDictionary<string, string> map, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public void ExportCustomersToCsv(string filePath) => throw new NotImplementedException();
        public Task ExportCustomersToCsvAsync(string filePath, CancellationToken cancellationToken = default) => throw new NotImplementedException();
    }

    class DelayUserService : IUserService
    {
        readonly int _delay; readonly List<User> _users;
        public DelayUserService(int delay, int count)
        { _delay = delay; _users = new List<User>(new User[count]); }
        public List<User> GetAllUsers() => _users;
        public Task<List<User>> GetAllUsersAsync() => Task.Delay(_delay).ContinueWith(_ => _users);
        public User? GetUserByID(int userID) => throw new NotImplementedException();
        public Task<User?> GetUserByIDAsync(int userID) => throw new NotImplementedException();
        public User? AuthenticateUser(string userName, string password) => throw new NotImplementedException();
        public Task<(AuthenticationResult Result, User? User)> AuthenticateUserAsync(string userName, string password) => throw new NotImplementedException();
        public User? GetCurrentUser() => null;
        public Task<User?> GetCurrentUserAsync() => Task.FromResult<User?>(null);
        public void AddUser(User user) => throw new NotImplementedException();
        public Task AddUserAsync(User user) => throw new NotImplementedException();
        public void UpdateUser(User user) => throw new NotImplementedException();
        public Task UpdateUserAsync(User user) => throw new NotImplementedException();
        public Task<bool> TryDeleteUserAsync(int userID) => throw new NotImplementedException();
        public bool ChangeUserPassword(int userID, string newPassword) => throw new NotImplementedException();
        public Task<bool> ChangeUserPasswordAsync(int userID, string newPassword) => throw new NotImplementedException();
    }
}
