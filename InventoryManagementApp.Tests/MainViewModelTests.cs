using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Media;
using CommunityToolkit.Mvvm.Input;
using InventoryManagementApp.Data;
using InventoryManagementApp.Interfaces;
using InventoryManagementApp.Models;
using InventoryManagementApp.Models.Domain;
using CustomerModel = InventoryManagementApp.Models.Domain.Customer;
using RentalModel = InventoryManagementApp.Models.Domain.Rental;
using InventoryManagementApp.Utilities;
using InventoryManagementApp.Services.Core;
using InventoryManagementApp.Services.Users;
using InventoryManagementApp.ViewModels;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace InventoryManagementApp.Tests
{
    public class MainViewModelTests
    {
        [Fact]
        public async Task GlobalSearchText_TriggersCommand_AfterDebounce()
        {
            await RunOnStaThread(async () =>
            {
                using var db = new DatabaseService(":memory:");
                var debounceTimer = new MockDispatcherTimer();
                bool executed = false;
                using var vm = new MainViewModel(
                    new DummyItemService(),
                    new DummyUserService(),
                    new DummyUserContext(),
                    new DummyCustomerService(),
                    new DummyRentalService(),
                    new DummyFileDialogService(),
                    new ActivityLogService(db),
                    new DummySettingsService(),
                    new DummyThemeService(),
                    db,
                    new DummyDialogService(),
                    NullLogger<MainViewModel>.Instance,
                    () => Task.FromResult(true),
                    new DummyDispatcherTimer(),
                    new DummyScannerService(),
                    new DummyScannerGroupService(),
                    debounceTimer);

                var field = typeof(MainViewModel).GetField("<GlobalSearchCommand>k__BackingField", BindingFlags.Instance | BindingFlags.NonPublic);
                field!.SetValue(vm, new AsyncRelayCommand(ct =>
                {
                    executed = true;
                    return Task.CompletedTask;
                }));

                vm.GlobalSearchText = "test";
                Assert.False(executed);

                debounceTimer.Fire();

                Assert.True(executed);
            });
        }

        [Fact]
        public async Task GlobalSearchAsync_PreservesTextWithoutTriggeringNewSearch()
        {
            await RunOnStaThread(async () =>
            {
                using var db = new DatabaseService(":memory:");
                var globalDebounceTimer = new MockDispatcherTimer();
                var searchDebounceTimer = new MockDispatcherTimer();
                using var vm = new MainViewModel(
                    new DummyItemService(),
                    new DummyUserService(),
                    new DummyUserContext(),
                    new DummyCustomerService(),
                    new DummyRentalService(),
                    new DummyFileDialogService(),
                    new ActivityLogService(db),
                    new DummySettingsService(),
                    new DummyThemeService(),
                    db,
                    new DummyDialogService(),
                    NullLogger<MainViewModel>.Instance,
                    () => Task.FromResult(true),
                    new DummyDispatcherTimer(),
                    new DummyScannerService(),
                    new DummyScannerGroupService(),
                    globalDebounceTimer);

                var itemManagement = new ItemManagementViewModel(
                    new DummyItemService(),
                    new DummyCustomerService(),
                    new DummyRentalService(),
                    new DummyDialogService(),
                    new DummySettingsService(),
                    NullLogger<ItemManagementViewModel>.Instance,
                    searchDebounceTimer);

                int searchExecuted = 0;
                var searchField = typeof(ItemManagementViewModel).GetField("<SearchCommand>k__BackingField", BindingFlags.Instance | BindingFlags.NonPublic);
                searchField!.SetValue(itemManagement, new AsyncRelayCommand(ct =>
                {
                    searchExecuted++;
                    return Task.CompletedTask;
                }));

                var itemManagementField = typeof(MainViewModel).GetField("<ItemManagement>k__BackingField", BindingFlags.Instance | BindingFlags.NonPublic);
                itemManagementField!.SetValue(vm, itemManagement);

                int openSearchCalls = 0;
                var openSearchField = typeof(MainViewModel).GetField("<OpenSearchItemsCommand>k__BackingField", BindingFlags.Instance | BindingFlags.NonPublic);
                openSearchField!.SetValue(vm, new AsyncRelayCommand(ct =>
                {
                    openSearchCalls++;
                    return Task.CompletedTask;
                }));

                vm.GlobalSearchText = "hammer";
                globalDebounceTimer.Fire();

                Assert.Equal("hammer", itemManagement.SearchText);
                Assert.Equal("hammer", vm.GlobalSearchText);
                Assert.Equal(1, searchExecuted);
                Assert.Equal(1, openSearchCalls);
            });
        }

        [Fact]
        public async Task Dispose_NullifiesGlobalSearchToken()
        {
            await RunOnStaThread(async () =>
            {
                using var db = new DatabaseService(":memory:");
                var debounceTimer = new MockDispatcherTimer();
                var vm = new MainViewModel(
                    new DummyItemService(),
                    new DummyUserService(),
                    new DummyUserContext(),
                    new DummyCustomerService(),
                    new DummyRentalService(),
                    new DummyFileDialogService(),
                    new ActivityLogService(db),
                    new DummySettingsService(),
                    new DummyThemeService(),
                    db,
                    new DummyDialogService(),
                    NullLogger<MainViewModel>.Instance,
                    () => Task.FromResult(true),
                    new DummyDispatcherTimer(),
                    new DummyScannerService(),
                    new DummyScannerGroupService(),
                    debounceTimer);

                vm.Dispose();

                var ex = Record.Exception(() => vm.GlobalSearchText = "test");
                Assert.Null(ex);

                var ex2 = Record.Exception(() => vm.Dispose());
                Assert.Null(ex2);
            });
        }

        [Fact]
        public async Task SwitchUserCommand_ClearsGlobalSearchText()
        {
            await RunOnStaThread(async () =>
            {
                using var db = new DatabaseService(":memory:");
                var debounceTimer = new MockDispatcherTimer();
                using var vm = new MainViewModel(
                    new DummyItemService(),
                    new DummyUserService(),
                    new DummyUserContext(),
                    new DummyCustomerService(),
                    new DummyRentalService(),
                    new DummyFileDialogService(),
                    new ActivityLogService(db),
                    new DummySettingsService(),
                    new DummyThemeService(),
                    db,
                    new DummyDialogService(),
                    NullLogger<MainViewModel>.Instance,
                    () => Task.FromResult(true),
                    new DummyDispatcherTimer(),
                    new DummyScannerService(),
                    new DummyScannerGroupService(),
                    debounceTimer);

                var openDashboardField = typeof(MainViewModel).GetField("<OpenDashboardCommand>k__BackingField", BindingFlags.Instance | BindingFlags.NonPublic);
                openDashboardField!.SetValue(vm, new AsyncRelayCommand(() => Task.CompletedTask));

                vm.GlobalSearchText = "test";

                await vm.SwitchUserCommand.ExecuteAsync(null);

                Assert.Equal(string.Empty, vm.GlobalSearchText);
                Assert.False(debounceTimer.IsEnabled);
            });
        }

        [Fact]
        public async Task SwitchUserCommand_ClearsItemsViewModelFilter()
        {
            await RunOnStaThread(async () =>
            {
                using var db = new DatabaseService(":memory:");
                var debounceTimer = new MockDispatcherTimer();
                using var vm = new MainViewModel(
                    new DummyItemService(),
                    new DummyUserService(),
                    new DummyUserContext(),
                    new DummyCustomerService(),
                    new DummyRentalService(),
                    new DummyFileDialogService(),
                    new ActivityLogService(db),
                    new DummySettingsService(),
                    new DummyThemeService(),
                    db,
                    new DummyDialogService(),
                    NullLogger<MainViewModel>.Instance,
                    () => Task.FromResult(true),
                    new DummyDispatcherTimer(),
                    new DummyScannerService(),
                    new DummyScannerGroupService(),
                    debounceTimer);

                var openDashboardField = typeof(MainViewModel).GetField("<OpenDashboardCommand>k__BackingField", BindingFlags.Instance | BindingFlags.NonPublic);
                openDashboardField!.SetValue(vm, new AsyncRelayCommand(() => Task.CompletedTask));

                using var itemsVm = new ItemsViewModel(new DummyItemService(), new MemoryBudget(), new DummyDialogService(), new DummyRentalService(), new DummySettingsService(), NullLogger<ItemsViewModel>.Instance);
                itemsVm.Filter = "test";
                vm.CurrentPage = new Page { DataContext = itemsVm };

                await vm.SwitchUserCommand.ExecuteAsync(null);

                Assert.Equal(string.Empty, itemsVm.Filter);
            });
        }

        [Fact]
        public async Task CurrentUserInitialsBrush_IsNotTransparent_WhenNoPhoto()
        {
            await RunOnStaThread(async () =>
            {
                var app = new Application();
                app.Resources.MergedDictionaries.Add(new ResourceDictionary
                {
                    Source = new Uri("pack://application:,,,/InventoryManagementApp;component/Resources/Colors.Dark.xaml", UriKind.Absolute)
                });

                try
                {
                    var user = new User
                    {
                        UserID = 1,
                        UserName = "John Doe",
                        PasswordHash = "hash",
                        PasswordSalt = "salt",
                        UserPhotoPath = string.Empty
                    };

                    var userContext = new DummyUserContext();
                    var userService = new AuthStubUserService(user);
                    var settingsService = new DummySettingsService();
                    var dialogService = new DummyDialogService();

                    var loginVm = new LoginViewModel(userService, settingsService, dialogService, userContext);
                    loginVm.PromptForPasswordAsync = (u, ct) => Task.FromResult<PasswordPromptResult?>(new PasswordPromptResult("pwd", false));

                    await loginVm.LoadUsersCommand.ExecuteAsync(null);
                    await loginVm.SelectUserCommand.ExecuteAsync(loginVm.Users[0]);

                    using var db = new DatabaseService(":memory:");
                    using var vm = new MainViewModel(
                        new DummyItemService(),
                        userService,
                        userContext,
                        new DummyCustomerService(),
                        new DummyRentalService(),
                        new DummyFileDialogService(),
                        new ActivityLogService(db),
                        new DummySettingsService(),
                        new DummyThemeService(),
                        db,
                        dialogService,
                        NullLogger<MainViewModel>.Instance,
                        () => Task.FromResult(true),
                        new DummyDispatcherTimer(),
                        new DummyScannerService(),
                        new DummyScannerGroupService(),
                        new DummyDispatcherTimer());

                    Assert.NotNull(vm.CurrentUserInitialsBrush);
                    Assert.NotEqual(Brushes.Transparent, vm.CurrentUserInitialsBrush);
                }
                finally
                {
                    app.Shutdown();
                }
            });
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
            public Task AddItemAsync(ItemModel item, CancellationToken cancellationToken = default) => Task.CompletedTask;
            public Task UpdateItemAsync(ItemModel item, CancellationToken cancellationToken = default) => Task.CompletedTask;
            public Task DeleteItemAsync(int itemID, CancellationToken cancellationToken = default) => Task.CompletedTask;
            public Task<ItemModel?> GetItemByIDAsync(int itemID, CancellationToken cancellationToken = default) => Task.FromResult<ItemModel?>(null);
            public IAsyncEnumerable<ItemModel> GetItemsAsync(ItemPage page, SortField sortField = SortField.Name, SortDirection sortDirection = SortDirection.Ascending, bool? isRentalItem = null, CancellationToken cancellationToken = default) => AsyncEnumerable.Empty<ItemModel>();
            public IAsyncEnumerable<ItemModel> SearchItemsAsync(string? searchText, ItemPage page, SortField sortField = SortField.Name, SortDirection sortDirection = SortDirection.Ascending, bool? isRentalItem = null, CancellationToken cancellationToken = default) => AsyncEnumerable.Empty<ItemModel>();
            public Task<int> CountItemsAsync(ItemFilter filter, CancellationToken ct) => Task.FromResult(0);
            public Task<bool> ToggleItemCheckOutStatusAsync(int itemID, CancellationToken cancellationToken = default) => Task.FromResult(false);
            public Task<List<ItemModel>> GetItemsCheckedOutByAsync(string userName, CancellationToken cancellationToken = default) => Task.FromResult(new List<ItemModel>());
            public Task UpdateItemImageAsync(int itemID, string imagePath, CancellationToken cancellationToken = default) => Task.CompletedTask;
            public Task<List<int>> ImportItemsFromCsvAsync(string filePath, IDictionary<string, string> map, CancellationToken cancellationToken) => Task.FromResult(new List<int>());
            public Task ExportItemsToCsvAsync(string filePath, CancellationToken cancellationToken = default) => Task.CompletedTask;
            public Task<ImageImportResult> ImportItemImagesAsync(string folderPath, Func<ItemModel, IEnumerable<string>> keySelector, IProgress<ImageImportProgress>? progress = null, CancellationToken cancellationToken = default) => Task.FromResult(new ImageImportResult());
            public Task<string> GenerateNextItemNumberAsync(CancellationToken cancellationToken = default) => Task.FromResult(string.Empty);
            public Task UpdateItemQuantitiesAsync(int itemID, int qtyChange, bool isRental, System.Data.Sqlite.SqliteConnection? conn = null, System.Data.Sqlite.SqliteTransaction? tx = null, CancellationToken cancellationToken = default) => Task.CompletedTask;
            public Task SaveChangesAsync(IEnumerable<ItemModel> changes, CancellationToken ct) => Task.CompletedTask;
        }

        private sealed class DummyUserService : IUserService
        {
            public Task<List<User>> GetAllUsersAsync(CancellationToken cancellationToken = default) => Task.FromResult(new List<User>());
            public Task<int> CountUsersAsync(CancellationToken cancellationToken = default) => Task.FromResult(0);
            public Task<User?> GetUserByIDAsync(int userID, CancellationToken cancellationToken = default) => Task.FromResult<User?>(null);
            public Task<(AuthenticationResult Result, User? User)> AuthenticateUserAsync(string userName, string password) => Task.FromResult((AuthenticationResult.Failure, (User?)null));
            public Task<User?> GetCurrentUserAsync() => Task.FromResult<User?>(null);
            public Task AddUserAsync(User user) => Task.CompletedTask;
            public Task UpdateUserAsync(User user) => Task.CompletedTask;
            public Task<bool> TryDeleteUserAsync(int userID) => Task.FromResult(false);
            public Task<bool> ChangeUserPasswordAsync(int userID, string newPassword) => Task.FromResult(false);
        }

        private sealed class AuthStubUserService : IUserService
        {
            private readonly User _user;
            public AuthStubUserService(User user) => _user = user;

            public Task<List<User>> GetAllUsersAsync(CancellationToken cancellationToken = default) => Task.FromResult(new List<User> { _user });
            public Task<int> CountUsersAsync(CancellationToken cancellationToken = default) => Task.FromResult(1);
            public Task<User?> GetUserByIDAsync(int userID, CancellationToken cancellationToken = default) => Task.FromResult(userID == _user.UserID ? _user : null);
            public Task<(AuthenticationResult Result, User? User)> AuthenticateUserAsync(string userName, string password)
                => Task.FromResult<(AuthenticationResult, User?)>((AuthenticationResult.Success, new User
                {
                    UserID = _user.UserID,
                    UserName = _user.UserName,
                    PasswordHash = _user.PasswordHash,
                    PasswordSalt = _user.PasswordSalt,
                    UserPhotoPath = _user.UserPhotoPath,
                    IsAdmin = _user.IsAdmin,
                    Email = _user.Email,
                    Phone = _user.Phone,
                    Mobile = _user.Mobile,
                    Address = _user.Address,
                    Role = _user.Role,
                    IsActive = _user.IsActive,
                    CreatedAt = _user.CreatedAt,
                    PasswordExpired = _user.PasswordExpired,
                    InitialsBrush = Brushes.Transparent
                }));
            public Task<User?> GetCurrentUserAsync() => Task.FromResult<User?>(null);
            public Task AddUserAsync(User user) => Task.CompletedTask;
            public Task UpdateUserAsync(User user) => Task.CompletedTask;
            public Task<bool> TryDeleteUserAsync(int userID) => Task.FromResult(true);
            public Task<bool> ChangeUserPasswordAsync(int userID, string newPassword) => Task.FromResult(true);
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

        private sealed class DummyCustomerService : ICustomerService
        {
            public Task AddCustomerAsync(Customer customer, CancellationToken cancellationToken = default) => Task.CompletedTask;
            public Task UpdateCustomerAsync(Customer customer, CancellationToken cancellationToken = default) => Task.CompletedTask;
            public Task DeleteCustomerAsync(int customerID, CancellationToken cancellationToken = default) => Task.CompletedTask;
            public Task<Customer?> GetCustomerByIDAsync(int customerID, CancellationToken cancellationToken = default) => Task.FromResult<Customer?>(new Customer());
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
            public event EventHandler<IDictionary<ItemDetailField, bool>>? ItemDetailVisibilityChanged;
            public Task SaveSettingAsync(string key, string value, CancellationToken cancellationToken = default) => Task.CompletedTask;
            public Task<string?> GetSettingAsync(string? key, CancellationToken cancellationToken = default) => Task.FromResult<string?>(null);
            public Task<Dictionary<string, string>> GetAllSettingsAsync(CancellationToken cancellationToken = default) => Task.FromResult(new Dictionary<string, string>());
            public Task UpdateSettingsAsync(Dictionary<string, string> settings, CancellationToken cancellationToken = default) => Task.CompletedTask;
            public Task DeleteSettingAsync(string key, CancellationToken cancellationToken = default) => Task.CompletedTask;
            public Task<IEnumerable<string>> GetScannerIpAddressesAsync(CancellationToken cancellationToken = default) => Task.FromResult<IEnumerable<string>>(Array.Empty<string>());
            public Task<IEnumerable<string>> SaveScannerIpAddressesAsync(IEnumerable<string>? ipAddresses, CancellationToken cancellationToken = default) => Task.FromResult<IEnumerable<string>>(Array.Empty<string>());
            public Task<string?> GetThemeAsync(CancellationToken cancellationToken = default) => Task.FromResult<string?>(null);
            public Task SaveThemeAsync(string theme, CancellationToken cancellationToken = default) => Task.CompletedTask;
            public Task<int> GetPasswordIterationsAsync(CancellationToken cancellationToken = default) => Task.FromResult(0);
            public Task SavePasswordIterationsAsync(int iterations, CancellationToken cancellationToken = default) => Task.CompletedTask;
            public Task<int> GetAutoLogoutMinutesAsync(CancellationToken cancellationToken = default) => Task.FromResult(0);
            public Task SaveAutoLogoutMinutesAsync(int minutes, CancellationToken cancellationToken = default) => Task.CompletedTask;
            public Task<string> GetItemLabelSingularAsync(CancellationToken cancellationToken = default) => Task.FromResult(string.Empty);
            public Task SaveItemLabelSingularAsync(string label, CancellationToken cancellationToken = default) => Task.CompletedTask;
            public Task<string> GetItemLabelPluralAsync(CancellationToken cancellationToken = default) => Task.FromResult(string.Empty);
            public Task SaveItemLabelPluralAsync(string label, CancellationToken cancellationToken = default) => Task.CompletedTask;
            public Task<IDictionary<ItemDetailField, bool>> GetItemDetailVisibilityAsync(CancellationToken cancellationToken = default)
                => Task.FromResult<IDictionary<ItemDetailField, bool>>(Enum.GetValues<ItemDetailField>().ToDictionary(f => f, _ => true));
            public Task SaveItemDetailVisibilityAsync(IDictionary<ItemDetailField, bool> visibility, CancellationToken cancellationToken = default)
            {
                ItemDetailVisibilityChanged?.Invoke(this, visibility);
                return Task.CompletedTask;
            }
        }

        private sealed class DummyThemeService : IThemeService
        {
            public void ApplyTheme(string? theme) { }
        }

        private sealed class DummyDialogService : IDialogService
        {
            public void ShowInfo(string message, string title) { }
            public bool ShowConfirmation(string message, string title) => false;
            public ItemModel? ShowEditItemDialog(ItemModel item) => null;
            public void ShowItemDetails(ItemModel item) { }
            public (CustomerModel customer, DateTime dueDate)? ShowRentItemDialog(ItemModel item, IEnumerable<CustomerModel> customers) => null;
            public CustomerModel? ShowAddCustomerDialog() => null;
            public CustomerModel? ShowEditCustomerDialog(CustomerModel customer) => null;
            public void ShowRentalsFilter(ManageRentalsViewModel viewModel) { }
            public void ShowRentalHistory(ItemModel item, IEnumerable<RentalModel> history) { }
            public Dictionary<string, string>? ShowImportMapping(IEnumerable<string> headers, IEnumerable<string> properties, IEnumerable<string>? requiredPropertyNames = null) => null;
            public Func<ItemModel, IEnumerable<string>>? ShowImageImportMapping() => null;
            public void ShowPrintPreview(System.Windows.Documents.FlowDocument document, string title, string description) { }
            public void ShowPrintLabelDialog() { }
        }

        private sealed class DummyScannerService : IScannerService
        {
            public Task<IEnumerable<ScannerDevice>> GetScannerDevicesAsync(CancellationToken cancellationToken) => Task.FromResult<IEnumerable<ScannerDevice>>(Array.Empty<ScannerDevice>());
        }

        private sealed class DummyScannerGroupService : IScannerGroupService
        {
            public Task<int> CreateGroupAsync(string name, CancellationToken cancellationToken = default) => Task.FromResult(0);
            public Task<IEnumerable<ScannerGroup>> GetGroupsAsync(CancellationToken cancellationToken = default) => Task.FromResult<IEnumerable<ScannerGroup>>(Array.Empty<ScannerGroup>());
            public Task UpdateGroupAsync(ScannerGroup group, CancellationToken cancellationToken = default) => Task.CompletedTask;
            public Task DeleteGroupAsync(int groupId, CancellationToken cancellationToken = default) => Task.CompletedTask;
            public Task AssignDeviceToGroupAsync(string deviceIp, int? groupId, CancellationToken cancellationToken = default) => Task.CompletedTask;
            public Task<int?> GetDeviceGroupIdAsync(string deviceIp, CancellationToken cancellationToken = default) => Task.FromResult<int?>(null);
        }

        private sealed class DummyDispatcherTimer : IDispatcherTimer
        {
            public event EventHandler? Tick;
            public TimeSpan Interval { get; set; }
            public bool IsEnabled { get; private set; }
            public void Start() => IsEnabled = true;
            public void Stop() => IsEnabled = false;
        }

        private sealed class MockDispatcherTimer : IDispatcherTimer
        {
            public event EventHandler? Tick;
            public TimeSpan Interval { get; set; }
            public bool IsEnabled { get; private set; }
            public void Start() => IsEnabled = true;
            public void Stop() => IsEnabled = false;
            public void Fire() => Tick?.Invoke(this, EventArgs.Empty);
        }
    }
}

