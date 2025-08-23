using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using InventoryManagementApp.Data;
using InventoryManagementApp.Interfaces;
using InventoryManagementApp.Models.Domain;
using InventoryManagementApp.Models;
using InventoryManagementApp.Services.Core;
using InventoryManagementApp.Services.Users;
using InventoryManagementApp.ViewModels;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace InventoryManagementApp.Tests
{
    public class MainViewModelNavigationTests
    {
        [Fact]
        public async Task OpenSearchItemsCommand_PropagatesException()
        {
            await RunOnStaThread(async () =>
            {
                using var db = new DatabaseService(":memory:");
                using var vm = CreateMainViewModel(db, new FailingItemService(), new DummyDialogService());
                await Assert.ThrowsAsync<InvalidOperationException>(() => vm.OpenSearchItemsCommand.ExecuteAsync(null));
            });
        }

        [Fact]
        public async Task OpenPrintLabelWindowCommand_PropagatesException()
        {
            await RunOnStaThread(async () =>
            {
                using var db = new DatabaseService(":memory:");
                using var vm = CreateMainViewModel(db, new DummyItemService(), new ThrowingDialogService());
                await Assert.ThrowsAsync<InvalidOperationException>(() => vm.OpenPrintLabelWindowCommand.ExecuteAsync(null));
            });
        }

        static MainViewModel CreateMainViewModel(DatabaseService db, IItemService itemService, IDialogService dialogService)
        {
            var activityLog = new ActivityLogService(db);
            return new MainViewModel(
                itemService,
                new DummyUserService(),
                new DummyUserContext(),
                new DummyCustomerService(),
                new DummyRentalService(),
                new DummyFileDialogService(),
                activityLog,
                new DummySettingsService(),
                db,
                dialogService,
                NullLogger<MainViewModel>.Instance,
                () => Task.FromResult(true),
                new DummyDispatcherTimer(),
                new DummyScannerService());
        }

        static Task RunOnStaThread(Func<Task> action)
        {
            var tcs = new TaskCompletionSource<object?>();
            var thread = new Thread(async () =>
            {
                try
                {
                    await action();
                    tcs.SetResult(null);
                }
                catch (Exception ex)
                {
                    tcs.SetException(ex);
                }
            });
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            return tcs.Task;
        }

        private sealed class DummyItemService : IItemService
        {
            public virtual Task AddItemAsync(ItemModel item, CancellationToken cancellationToken = default) => Task.CompletedTask;
            public virtual Task UpdateItemAsync(ItemModel item, CancellationToken cancellationToken = default) => Task.CompletedTask;
            public virtual Task DeleteItemAsync(int itemID, CancellationToken cancellationToken = default) => Task.CompletedTask;
            public virtual Task<ItemModel?> GetItemByIDAsync(int itemID, CancellationToken cancellationToken = default) => Task.FromResult<ItemModel?>(null);
            public virtual IAsyncEnumerable<ItemModel> GetItemsAsync(ItemPage page, CancellationToken cancellationToken = default) => AsyncEnumerable.Empty<ItemModel>();
            public virtual IAsyncEnumerable<ItemModel> SearchItemsAsync(string? searchText, ItemPage page, CancellationToken cancellationToken = default) => AsyncEnumerable.Empty<ItemModel>();
            public virtual Task<int> CountItemsAsync(ItemFilter filter, CancellationToken ct) => Task.FromResult(0);
            public virtual Task<bool> ToggleItemCheckOutStatusAsync(int itemID, string currentUser, CancellationToken cancellationToken = default) => Task.FromResult(false);
            public virtual Task<List<ItemModel>> GetItemsCheckedOutByAsync(string userName, CancellationToken cancellationToken = default) => Task.FromResult(new List<ItemModel>());
            public virtual Task UpdateItemImageAsync(int itemID, string imagePath, CancellationToken cancellationToken = default) => Task.CompletedTask;
            public virtual Task<List<int>> ImportItemsFromCsvAsync(string filePath, IDictionary<string, string> map, CancellationToken cancellationToken) => Task.FromResult(new List<int>());
            public virtual Task ExportItemsToCsvAsync(string filePath, CancellationToken cancellationToken = default) => Task.CompletedTask;
            public virtual Task<ImageImportResult> ImportItemImagesAsync(string folderPath, Func<ItemModel, IEnumerable<string>> keySelector, IProgress<ImageImportProgress>? progress = null, CancellationToken cancellationToken = default) => Task.FromResult(new ImageImportResult());
            public virtual Task<string> GenerateNextItemNumberAsync(CancellationToken cancellationToken = default) => Task.FromResult(string.Empty);
            public virtual Task UpdateItemQuantitiesAsync(int itemID, int qtyChange, bool isRental, System.Data.SQLite.SQLiteConnection? conn = null, System.Data.SQLite.SQLiteTransaction? tx = null, CancellationToken cancellationToken = default) => Task.CompletedTask;
        }

        private sealed class FailingItemService : DummyItemService
        {
            public override IAsyncEnumerable<ItemModel> GetItemsAsync(ItemPage page, CancellationToken cancellationToken = default) => AsyncEnumerable.Throw<ItemModel>(new InvalidOperationException("fail"));
        }

        private sealed class DummyUserService : IUserService
        {
            public Task<List<User>> GetAllUsersAsync() => Task.FromResult(new List<User>());
            public Task<int> CountUsersAsync() => Task.FromResult(0);
            public Task<User?> GetUserByIDAsync(int userID) => Task.FromResult<User?>(null);
            public Task<(AuthenticationResult Result, User? User)> AuthenticateUserAsync(string userName, string password) => Task.FromResult((AuthenticationResult.Failure, (User?)null));
            public Task<User?> GetCurrentUserAsync() => Task.FromResult<User?>(null);
            public Task AddUserAsync(User user) => Task.CompletedTask;
            public Task UpdateUserAsync(User user) => Task.CompletedTask;
            public Task<bool> TryDeleteUserAsync(int userID) => Task.FromResult(false);
            public Task<bool> ChangeUserPasswordAsync(int userID, string newPassword) => Task.FromResult(false);
        }

        private sealed class DummyCustomerService : ICustomerService
        {
            public Task AddCustomerAsync(Customer customer, CancellationToken cancellationToken = default) => Task.CompletedTask;
            public Task UpdateCustomerAsync(Customer customer, CancellationToken cancellationToken = default) => Task.CompletedTask;
            public Task DeleteCustomerAsync(int customerID, CancellationToken cancellationToken = default) => Task.CompletedTask;
            public Task<Customer> GetCustomerByIDAsync(int customerID, CancellationToken cancellationToken = default) => Task.FromResult(new Customer());
            public Task<List<Customer>> GetAllCustomersAsync(CancellationToken cancellationToken = default) => Task.FromResult(new List<Customer>());
            public Task<int> CountCustomersAsync(CancellationToken cancellationToken = default) => Task.FromResult(0);
            public Task<List<Customer>> SearchCustomersAsync(string searchTerm, CancellationToken cancellationToken = default) => Task.FromResult(new List<Customer>());
            public Task<CustomerImportResult> ImportCustomersFromCsvAsync(string filePath, IDictionary<string, string> map, CancellationToken cancellationToken = default) => Task.FromResult(new CustomerImportResult());
            public Task ExportCustomersToCsvAsync(string filePath, CancellationToken cancellationToken = default) => Task.CompletedTask;
        }

        private sealed class DummyRentalService : IRentalService
        {
            public Task RentItemAsync(int itemID, int customerID, DateTime rentalDate, DateTime dueDate) => Task.CompletedTask;
            public Task ReturnItemAsync(int rentalID, DateTime returnDate) => Task.CompletedTask;
            public Task ExtendRentalAsync(int rentalID, DateTime newDueDate) => Task.CompletedTask;
            public Task DeleteRentalAsync(int rentalID) => Task.CompletedTask;
            public Task<List<Rental>> GetActiveRentalsAsync() => Task.FromResult(new List<Rental>());
            public Task<int> CountActiveRentalsAsync() => Task.FromResult(0);
            public Task<List<Rental>> GetOverdueRentalsAsync() => Task.FromResult(new List<Rental>());
            public Task<List<Rental>> GetAllRentalsAsync() => Task.FromResult(new List<Rental>());
            public Task<List<Rental>> GetRentalHistoryForItemAsync(int itemID) => Task.FromResult(new List<Rental>());
            public Task<List<Rental>> GetRentalHistoryForCustomerAsync(int customerID) => Task.FromResult(new List<Rental>());
        }

        private sealed class DummyFileDialogService : IFileDialogService
        {
            public string? OpenFile(string filter, string? initialDirectory = null) => null;
            public string? SaveFile(string filter) => null;
        }

        private sealed class DummySettingsService : ISettingsService
        {
            public Task SaveSettingAsync(string key, string value, CancellationToken cancellationToken = default) => Task.CompletedTask;
            public Task<string?> GetSettingAsync(string key, CancellationToken cancellationToken = default) => Task.FromResult<string?>(null);
            public Task<Dictionary<string, string>> GetAllSettingsAsync(CancellationToken cancellationToken = default) => Task.FromResult(new Dictionary<string, string>());
            public Task UpdateSettingsAsync(Dictionary<string, string> settings, CancellationToken cancellationToken = default) => Task.CompletedTask;
            public Task DeleteSettingAsync(string key, CancellationToken cancellationToken = default) => Task.CompletedTask;
            public Task<IEnumerable<string>> GetScannerIpAddressesAsync(CancellationToken cancellationToken = default) => Task.FromResult<IEnumerable<string>>(Array.Empty<string>());
            public Task<IEnumerable<string>> SaveScannerIpAddressesAsync(IEnumerable<string>? ipAddresses, CancellationToken cancellationToken = default) => Task.FromResult<IEnumerable<string>>(Array.Empty<string>());
            public Task<int> GetPasswordIterationsAsync(CancellationToken cancellationToken = default) => Task.FromResult(0);
            public Task SavePasswordIterationsAsync(int iterations, CancellationToken cancellationToken = default) => Task.CompletedTask;
            public Task<int> GetAutoLogoutMinutesAsync(CancellationToken cancellationToken = default) => Task.FromResult(0);
            public Task SaveAutoLogoutMinutesAsync(int minutes, CancellationToken cancellationToken = default) => Task.CompletedTask;
            public Task<string> GetItemLabelSingularAsync(CancellationToken cancellationToken = default) => Task.FromResult(string.Empty);
            public Task SaveItemLabelSingularAsync(string label, CancellationToken cancellationToken = default) => Task.CompletedTask;
            public Task<string> GetItemLabelPluralAsync(CancellationToken cancellationToken = default) => Task.FromResult(string.Empty);
            public Task SaveItemLabelPluralAsync(string label, CancellationToken cancellationToken = default) => Task.CompletedTask;
        }

        private sealed class DummyDialogService : IDialogService
        {
            public virtual void ShowInfo(string message, string title) { }
            public virtual bool ShowConfirmation(string message, string title) => false;
            public virtual ItemModel? ShowEditItemDialog(ItemModel item) => null;
            public virtual void ShowItemDetails(ItemModel item) { }
            public virtual (CustomerModel customer, DateTime dueDate)? ShowRentItemDialog(ItemModel item, IEnumerable<CustomerModel> customers) => null;
            public virtual CustomerModel? ShowAddCustomerDialog() => null;
            public virtual void ShowRentalsFilter(ManageRentalsViewModel viewModel) { }
            public virtual void ShowRentalHistory(ItemModel item, IEnumerable<RentalModel> history) { }
            public virtual Dictionary<string, string>? ShowImportMapping(IEnumerable<string> headers, IEnumerable<string> properties, IEnumerable<string>? requiredPropertyNames = null) => null;
            public virtual Func<ItemModel, IEnumerable<string>>? ShowImageImportMapping() => null;
            public virtual void ShowPrintPreview(System.Windows.Documents.FlowDocument document, string title, string description) { }
            public virtual void ShowPrintLabelDialog() { }
        }

        private sealed class ThrowingDialogService : DummyDialogService
        {
            public override void ShowPrintLabelDialog() => throw new InvalidOperationException("fail");
        }

        private sealed class DummyUserContext : IUserContext
        {
            User? _currentUser;
            public User? CurrentUser
            {
                get => _currentUser;
                set
                {
                    _currentUser = value;
                    UserChanged?.Invoke(this, value);
                }
            }
            public event EventHandler<User?>? UserChanged;
            public bool IsAdmin => false;
            public string UserName => string.Empty;
            public string Role => string.Empty;
        }

        private sealed class DummyDispatcherTimer : IDispatcherTimer
        {
            public event EventHandler? Tick;
            public TimeSpan Interval { get; set; }
            public bool IsEnabled { get; private set; }
            public void Start() => IsEnabled = true;
            public void Stop() => IsEnabled = false;
        }

        private sealed class DummyScannerService : IScannerService
        {
            public Task<IEnumerable<ScannerDevice>> GetScannerDevicesAsync(CancellationToken cancellationToken) => Task.FromResult<IEnumerable<ScannerDevice>>(Array.Empty<ScannerDevice>());
        }
    }
}
