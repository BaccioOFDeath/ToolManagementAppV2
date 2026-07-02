using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using InventoryManagementApp;
using InventoryManagementApp.Interfaces;
using InventoryManagementApp.Models;
using InventoryManagementApp.Models.Domain;
using InventoryManagementApp.Models.ImportExport;
using InventoryManagementApp.Services.Core;
using InventoryManagementApp.Services.Users;
using InventoryManagementApp.Services.Categories;
using InventoryManagementApp.Services.Maintenance;
using InventoryManagementApp.Services.Calibration;
using InventoryManagementApp.Services.Reservations;
using InventoryManagementApp.Services.Kits;
using InventoryManagementApp.ViewModels;
using InventoryManagementApp.Views.Pages;
using Xunit;

namespace InventoryManagementApp.Tests
{
    public class MainViewModelCategoriesCommandTests
    {
        [Fact]
        public void Constructor_OpensDashboardByDefault()
        {
            Exception? threadEx = null;
            var thread = new Thread(() =>
            {
                try
                {
                    using var db = new DatabaseService(":memory:");
                    new MigrationRunner(db).Migrate();
                    var host = Host.CreateDefaultBuilder()
                        .ConfigureServices(s =>
                        {
                            s.AddSingleton<IDatabaseService>(db);
                            s.AddSingleton<IDialogService, StubDialogService>();
                            s.AddSingleton<IFileDialogService, StubFileDialogService>();
                            s.AddSingleton<ISettingsService, StubSettingsService>();
                            s.AddSingleton<IThemeService, StubThemeService>();
                            s.AddSingleton<CategoriesService>();
                            s.AddTransient<CategoryManagementViewModel>();
                        })
                        .Build();

                    WpfTestHelper.ShutdownApplication();
                    var app = new App(host);

                    var userContext = new StubUserContext();
                    var maintenanceService = new MaintenanceService(db, userContext);
                    var calibrationService = new CalibrationService(db, userContext);
                    var reservationService = new ReservationService(db, userContext);
                    var kitService = new KitService(db, userContext);

                    var vm = new MainViewModel(
                        new StubItemService(),
                        new StubUserService(),
                        userContext,
                        new StubCustomerService(),
                        new StubRentalService(),
                        new StubFileDialogService(),
                        new ActivityLogService(db),
                        new StubSettingsService(),
                        new StubThemeService(),
                        new StubDatabaseBackupService(),
                        new StubDialogService(),
                        maintenanceService,
                        calibrationService,
                        reservationService,
                        kitService,
                        null,
                        NullLogger<MainViewModel>.Instance,
                        () => Task.FromResult(true),
                        new StubDispatcherTimer(),
                        new StubDispatcherTimer());

                    Assert.IsType<DashboardPage>(vm.CurrentPage);

                    WpfTestHelper.ShutdownApplication();
                }
                catch (Exception ex)
                {
                    threadEx = ex;
                }
            });
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            thread.Join();
            if (threadEx != null) throw threadEx;
        }

        [Fact]
        public void GuestUser_CanOnlySeeDashboardNavigation()
        {
            Exception? threadEx = null;
            var thread = new Thread(() =>
            {
                try
                {
                    using var db = new DatabaseService(":memory:");
                    new MigrationRunner(db).Migrate();
                    var host = Host.CreateDefaultBuilder()
                        .ConfigureServices(s =>
                        {
                            s.AddSingleton<IDatabaseService>(db);
                            s.AddSingleton<IDialogService, StubDialogService>();
                            s.AddSingleton<IFileDialogService, StubFileDialogService>();
                            s.AddSingleton<ISettingsService, StubSettingsService>();
                            s.AddSingleton<IThemeService, StubThemeService>();
                            s.AddSingleton<CategoriesService>();
                            s.AddTransient<CategoryManagementViewModel>();
                        })
                        .Build();

                    WpfTestHelper.ShutdownApplication();
                    var app = new App(host);

                    var userContext = new StubUserContext
                    {
                        CurrentUser = new User
                        {
                            UserID = 1,
                            UserName = "Guest",
                            Role = "Guest",
                            Permissions = User.BuildPermissions(User.PermissionLabels.Keys)
                        }
                    };
                    var maintenanceService = new MaintenanceService(db, userContext);
                    var calibrationService = new CalibrationService(db, userContext);
                    var reservationService = new ReservationService(db, userContext);
                    var kitService = new KitService(db, userContext);

                    var vm = new MainViewModel(
                        new StubItemService(),
                        new StubUserService(),
                        userContext,
                        new StubCustomerService(),
                        new StubRentalService(),
                        new StubFileDialogService(),
                        new ActivityLogService(db),
                        new StubSettingsService(),
                        new StubThemeService(),
                        new StubDatabaseBackupService(),
                        new StubDialogService(),
                        maintenanceService,
                        calibrationService,
                        reservationService,
                        kitService,
                        null,
                        NullLogger<MainViewModel>.Instance,
                        () => Task.FromResult(true),
                        new StubDispatcherTimer(),
                        new StubDispatcherTimer());

                    Assert.True(vm.IsGuestUser);
                    Assert.False(vm.CanUseSearchTools);
                    Assert.False(vm.IsOperationsSectionVisible);
                    Assert.False(vm.IsInsightsSectionVisible);
                    Assert.False(vm.IsDataSectionVisible);
                    Assert.False(vm.IsAdminSectionVisible);
                    Assert.Single(vm.CurrentNavItems);
                    Assert.Equal("Dashboard", vm.CurrentNavItems[0].Title);

                    WpfTestHelper.ShutdownApplication();
                }
                catch (Exception ex)
                {
                    threadEx = ex;
                }
            });
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            thread.Join();
            if (threadEx != null) throw threadEx;
        }

        [Fact]
        public void SwitchingToGuestUser_ReturnsToDashboard()
        {
            Exception? threadEx = null;
            var thread = new Thread(() =>
            {
                try
                {
                    using var db = new DatabaseService(":memory:");
                    new MigrationRunner(db).Migrate();
                    var host = Host.CreateDefaultBuilder()
                        .ConfigureServices(s =>
                        {
                            s.AddSingleton<IDatabaseService>(db);
                            s.AddSingleton<IDialogService, StubDialogService>();
                            s.AddSingleton<IFileDialogService, StubFileDialogService>();
                            s.AddSingleton<ISettingsService, StubSettingsService>();
                            s.AddSingleton<IThemeService, StubThemeService>();
                            s.AddSingleton<CategoriesService>();
                            s.AddTransient<CategoryManagementViewModel>();
                        })
                        .Build();

                    WpfTestHelper.ShutdownApplication();
                    var app = new App(host);

                    var userContext = new StubUserContext
                    {
                        CurrentUser = new User { UserID = 1, UserName = "Admin", IsAdmin = true, Role = "Admin" }
                    };
                    var maintenanceService = new MaintenanceService(db, userContext);
                    var calibrationService = new CalibrationService(db, userContext);
                    var reservationService = new ReservationService(db, userContext);
                    var kitService = new KitService(db, userContext);

                    var vm = new MainViewModel(
                        new StubItemService(),
                        new StubUserService(),
                        userContext,
                        new StubCustomerService(),
                        new StubRentalService(),
                        new StubFileDialogService(),
                        new ActivityLogService(db),
                        new StubSettingsService(),
                        new StubThemeService(),
                        new StubDatabaseBackupService(),
                        new StubDialogService(),
                        maintenanceService,
                        calibrationService,
                        reservationService,
                        kitService,
                        null,
                        NullLogger<MainViewModel>.Instance,
                        () => Task.FromResult(true),
                        new StubDispatcherTimer(),
                        new StubDispatcherTimer());

                    vm.OpenCategoriesCommand.ExecuteAsync(null).GetAwaiter().GetResult();
                    Assert.IsType<CategoriesPage>(vm.CurrentPage);

                    userContext.CurrentUser = new User { UserID = 2, UserName = "Guest", Role = "Guest" };

                    Assert.IsType<DashboardPage>(vm.CurrentPage);

                    WpfTestHelper.ShutdownApplication();
                }
                catch (Exception ex)
                {
                    threadEx = ex;
                }
            });
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            thread.Join();
            if (threadEx != null) throw threadEx;
        }

        [Fact]
        public void OpenCategoriesCommand_NavigatesToCategoriesPage()
        {
            Exception? threadEx = null;
            var thread = new Thread(() =>
            {
                try
                {
                    using var db = new DatabaseService(":memory:");
                    new MigrationRunner(db).Migrate();
                    var host = Host.CreateDefaultBuilder()
                        .ConfigureServices(s =>
                        {
                            s.AddSingleton<IDatabaseService>(db);
                            s.AddSingleton<IDialogService, StubDialogService>();
                            s.AddSingleton<IFileDialogService, StubFileDialogService>();
                            s.AddSingleton<ISettingsService, StubSettingsService>();
                            s.AddSingleton<IThemeService, StubThemeService>();
                            s.AddSingleton<CategoriesService>();
                            s.AddTransient<CategoryManagementViewModel>();
                        })
                        .Build();

                    _ = host.Services.GetRequiredService<ILogger<App>>();
                    _ = host.Services.GetRequiredService<IDialogService>();

                    WpfTestHelper.ShutdownApplication();
                    var app = new App(host);

                    var userContext = new StubUserContext();
                    var maintenanceService = new MaintenanceService(db, userContext);
                    var calibrationService = new CalibrationService(db, userContext);
                    var reservationService = new ReservationService(db, userContext);
                    var kitService = new KitService(db, userContext);

                    var vm = new MainViewModel(
                        new StubItemService(),
                        new StubUserService(),
                        userContext,
                        new StubCustomerService(),
                        new StubRentalService(),
                        new StubFileDialogService(),
                        new ActivityLogService(db),
                        new StubSettingsService(),
                        new StubThemeService(),
                        new StubDatabaseBackupService(),
                        new StubDialogService(),
                        maintenanceService,
                        calibrationService,
                        reservationService,
                        kitService,
                        null,
                        NullLogger<MainViewModel>.Instance,
                        () => Task.FromResult(true),
                        new StubDispatcherTimer(),
                        new StubDispatcherTimer());

                    vm.OpenCategoriesCommand.ExecuteAsync(null).GetAwaiter().GetResult();

                    Assert.IsType<CategoriesPage>(vm.CurrentPage);

                    WpfTestHelper.ShutdownApplication();
                }
                catch (Exception ex)
                {
                    threadEx = ex;
                }
            });
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            thread.Join();
            if (threadEx != null) throw threadEx;
        }

        [Fact]
        public void SwitchUserCommand_CancelledLogin_ShutsDownApplication()
        {
            Exception? threadEx = null;
            var thread = new Thread(() =>
            {
                try
                {
                    using var db = new DatabaseService(":memory:");
                    new MigrationRunner(db).Migrate();
                    var host = Host.CreateDefaultBuilder()
                        .ConfigureServices(s =>
                        {
                            s.AddSingleton<IDatabaseService>(db);
                            s.AddSingleton<IDialogService, StubDialogService>();
                            s.AddSingleton<IFileDialogService, StubFileDialogService>();
                            s.AddSingleton<ISettingsService, StubSettingsService>();
                            s.AddSingleton<IThemeService, StubThemeService>();
                            s.AddSingleton<CategoriesService>();
                            s.AddTransient<CategoryManagementViewModel>();
                        })
                        .Build();

                    WpfTestHelper.ShutdownApplication();
                    var app = new App(host);

                    var userContext = new StubUserContext
                    {
                        CurrentUser = new User { UserID = 1, UserName = "Existing User", IsAdmin = true }
                    };
                    var maintenanceService = new MaintenanceService(db, userContext);
                    var calibrationService = new CalibrationService(db, userContext);
                    var reservationService = new ReservationService(db, userContext);
                    var kitService = new KitService(db, userContext);
                    var loginShown = false;
                    var shutdownRequested = false;
                    MainViewModel? vm = null;

                    vm = new MainViewModel(
                        new StubItemService(),
                        new StubUserService(),
                        userContext,
                        new StubCustomerService(),
                        new StubRentalService(),
                        new StubFileDialogService(),
                        new ActivityLogService(db),
                        new StubSettingsService(),
                        new StubThemeService(),
                        new StubDatabaseBackupService(),
                        new StubDialogService(),
                        maintenanceService,
                        calibrationService,
                        reservationService,
                        kitService,
                        null,
                        NullLogger<MainViewModel>.Instance,
                        () =>
                        {
                            loginShown = true;
                            Assert.IsType<DashboardPage>(vm!.CurrentPage);
                            return Task.FromResult(false);
                        },
                        new StubDispatcherTimer(),
                        new StubDispatcherTimer(),
                        null,
                        () => shutdownRequested = true);

                    vm.OpenCategoriesCommand.ExecuteAsync(null).GetAwaiter().GetResult();
                    Assert.IsType<CategoriesPage>(vm.CurrentPage);

                    vm.SwitchUserCommand.ExecuteAsync(null).GetAwaiter().GetResult();

                    Assert.True(loginShown);
                    Assert.True(shutdownRequested);
                    Assert.Null(userContext.CurrentUser);
                    Assert.IsType<DashboardPage>(vm.CurrentPage);

                    WpfTestHelper.ShutdownApplication();
                }
                catch (Exception ex)
                {
                    threadEx = ex;
                }
            });
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            thread.Join();
            if (threadEx != null) throw threadEx;
        }

        private sealed class StubItemService : IItemService
        {
            public Task AddItemAsync(ItemModel item, CancellationToken cancellationToken = default) => Task.CompletedTask;
            public Task UpdateItemAsync(ItemModel item, CancellationToken cancellationToken = default) => Task.CompletedTask;
            public Task DeleteItemAsync(int itemID, CancellationToken cancellationToken = default) => Task.CompletedTask;
            public Task<ItemModel?> GetItemByIDAsync(int itemID, CancellationToken cancellationToken = default) => Task.FromResult<ItemModel?>(null);
            public IAsyncEnumerable<ItemModel> GetItemsAsync(ItemPage page, SortField sortField = SortField.Name, SortDirection sortDirection = SortDirection.Ascending, bool? isRentalItem = null, CancellationToken cancellationToken = default) => AsyncEnumerable.Empty<ItemModel>();
            public IAsyncEnumerable<ItemModel> SearchItemsAsync(string? searchText, ItemPage page, SortField sortField = SortField.Name, SortDirection sortDirection = SortDirection.Ascending, bool? isRentalItem = null, CancellationToken cancellationToken = default) => AsyncEnumerable.Empty<ItemModel>();
            public Task<int> CountItemsAsync(ItemFilter filter, CancellationToken ct) => Task.FromResult(0);
            public Task SaveChangesAsync(IEnumerable<ItemModel> changes, CancellationToken ct) => Task.CompletedTask;
            public Task<bool> ToggleItemCheckOutStatusAsync(int itemID, CancellationToken cancellationToken = default) => Task.FromResult(false);
            public Task<List<ItemModel>> GetItemsCheckedOutByAsync(string userName, CancellationToken cancellationToken = default) => Task.FromResult(new List<ItemModel>());
            public Task<List<ItemModel>> GetCheckedOutItemsAsync(CancellationToken cancellationToken = default) => Task.FromResult(new List<ItemModel>());
            public Task UpdateItemImageAsync(int itemID, string imagePath, CancellationToken cancellationToken = default) => Task.CompletedTask;
            public Task<List<int>> ImportItemsFromCsvAsync(string filePath, IDictionary<string, string> map, CancellationToken cancellationToken) => Task.FromResult(new List<int>());
            public Task ExportItemsToCsvAsync(string filePath, CancellationToken cancellationToken = default) => Task.CompletedTask;
            public Task<List<int>> ImportItemsAsync(string filePath, IDataImporter<ItemModel> importer, CancellationToken cancellationToken = default) => Task.FromResult(new List<int>());
            public Task ExportItemsAsync(string filePath, IDataExporter<ItemModel> exporter, CancellationToken cancellationToken = default) => Task.CompletedTask;
            public Task<ImageImportResult> ImportItemImagesAsync(string folderPath, Func<ItemModel, IEnumerable<string>> keySelector, IProgress<ImageImportProgress>? progress = null, CancellationToken cancellationToken = default) => Task.FromResult(new ImageImportResult());
            public Task<string> GenerateNextItemNumberAsync(CancellationToken cancellationToken = default) => Task.FromResult(string.Empty);
            public Task UpdateItemQuantitiesAsync(int itemID, int qtyChange, bool isRental, Microsoft.Data.Sqlite.SqliteConnection? conn = null, Microsoft.Data.Sqlite.SqliteTransaction? tx = null, CancellationToken cancellationToken = default) => Task.CompletedTask;
            public Task<List<ItemModel>> GetMostCommonlyUsedItemsAsync(int limit, CancellationToken cancellationToken = default) => Task.FromResult(new List<ItemModel>());
            public Task<List<ItemModel>> GetIncompleteItemsAsync(CancellationToken cancellationToken = default) => Task.FromResult(new List<ItemModel>());
        }

        private sealed class StubUserService : IUserService
        {
            public Task<List<User>> GetAllUsersAsync(CancellationToken cancellationToken = default) => Task.FromResult(new List<User>());
            public Task<int> CountUsersAsync(CancellationToken cancellationToken = default) => Task.FromResult(0);
            public Task<User?> GetUserByIDAsync(int userID, CancellationToken cancellationToken = default) => Task.FromResult<User?>(null);
            public Task<(AuthenticationResult Result, User? User)> AuthenticateUserAsync(string userName, string password) => Task.FromResult<(AuthenticationResult, User?)>((AuthenticationResult.IncorrectPassword, null));
            public Task<User?> GetCurrentUserAsync() => Task.FromResult<User?>(null);
            public Task AddUserAsync(User user) => Task.CompletedTask;
            public Task UpdateUserAsync(User user) => Task.CompletedTask;
            public Task<bool> TryDeleteUserAsync(int userID) => Task.FromResult(true);
            public Task<bool> ChangeUserPasswordAsync(int userID, string newPassword) => Task.FromResult(true);
        }

        private sealed class StubUserContext : IUserContext
        {
            private User? _currentUser;
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
            public bool IsAdmin => CurrentUser?.IsAdmin == true;
            public string UserName => CurrentUser?.UserName ?? "admin";
            public string Role => CurrentUser?.Role ?? (IsAdmin ? "Admin" : "User");
        }

        private sealed class StubCustomerService : ICustomerService
        {
            public Task AddCustomerAsync(Customer customer, CancellationToken cancellationToken = default) => Task.CompletedTask;
            public Task UpdateCustomerAsync(Customer customer, CancellationToken cancellationToken = default) => Task.CompletedTask;
            public Task DeleteCustomerAsync(int customerID, CancellationToken cancellationToken = default) => Task.CompletedTask;
            public Task<CustomerModel?> GetCustomerByIDAsync(int customerID, CancellationToken cancellationToken = default) => Task.FromResult<CustomerModel?>(null);
            public Task<List<Customer>> GetAllCustomersAsync(CancellationToken cancellationToken = default) => Task.FromResult(new List<Customer>());
            public Task<int> CountCustomersAsync(CancellationToken cancellationToken = default) => Task.FromResult(0);
            public Task<List<Customer>> SearchCustomersAsync(string searchTerm, CancellationToken cancellationToken = default) => Task.FromResult(new List<Customer>());
            public Task<CustomerImportResult> ImportCustomersFromCsvAsync(string filePath, IDictionary<string, string> map, CancellationToken cancellationToken = default) => Task.FromResult(new CustomerImportResult());
            public Task ExportCustomersToCsvAsync(string filePath, CancellationToken cancellationToken = default) => Task.CompletedTask;
            public Task<int> ImportCustomersAsync(string filePath, IDataImporter<Customer> importer, CancellationToken cancellationToken = default) => Task.FromResult(0);
            public Task ExportCustomersAsync(string filePath, IDataExporter<Customer> exporter, CancellationToken cancellationToken = default) => Task.CompletedTask;
        }

        private sealed class StubRentalService : IRentalService
        {
            public Task RentItemAsync(int itemID, int customerID, DateTime rentalDate, DateTime dueDate) => Task.CompletedTask;
            public Task ReturnItemAsync(int rentalID, DateTime returnDate) => Task.CompletedTask;
            public Task ExtendRentalAsync(int rentalID, DateTime newDueDate) => Task.CompletedTask;
            public Task DeleteRentalAsync(int rentalID) => Task.CompletedTask;
            public Task<List<Rental>> GetActiveRentalsAsync() => Task.FromResult(new List<Rental>());
            public Task<int> CountRentalsAsync() => Task.FromResult(0);
            public Task<int> CountActiveRentalsAsync() => Task.FromResult(0);
            public Task<List<Rental>> GetOverdueRentalsAsync() => Task.FromResult(new List<Rental>());
            public Task<List<Rental>> GetAllRentalsAsync() => Task.FromResult(new List<Rental>());
            public Task<List<Rental>> GetRentalHistoryForItemAsync(int itemID) => Task.FromResult(new List<Rental>());
            public Task<List<Rental>> GetRentalHistoryForCustomerAsync(int customerID) => Task.FromResult(new List<Rental>());
            public Task<List<ItemRentalFrequency>> GetRentalFrequencyAsync(int topN = 10) => Task.FromResult(new List<ItemRentalFrequency>());
        }

        private sealed class StubFileDialogService : IFileDialogService
        {
            public string? OpenFile(string filter, string? initialDirectory = null) => null;
            public string? SaveFile(string filter, string? initialDirectory = null) => null;
            public string? BrowseFolder(string? initialDirectory = null) => null;
        }

        private sealed class StubSettingsService : ISettingsService
        {
            public event EventHandler<IDictionary<ItemDetailField, bool>>? ItemDetailVisibilityChanged;
            public event EventHandler<double>? ItemCardSizeChanged;
            public Task SaveSettingAsync(string key, string value, CancellationToken cancellationToken = default) => Task.CompletedTask;
            public Task<string?> GetSettingAsync(string? key, CancellationToken cancellationToken = default) => Task.FromResult(key == "DefaultInventoryId" ? "1" : null);
            public Task<Dictionary<string, string>> GetAllSettingsAsync(CancellationToken cancellationToken = default) => Task.FromResult(new Dictionary<string, string>());
            public Task UpdateSettingsAsync(Dictionary<string, string> settings, CancellationToken cancellationToken = default) => Task.CompletedTask;
            public Task DeleteSettingAsync(string key, CancellationToken cancellationToken = default) => Task.CompletedTask;
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
            public Task<IDictionary<ItemDetailField, bool>> GetItemDetailVisibilityAsync(CancellationToken cancellationToken = default) => Task.FromResult<IDictionary<ItemDetailField, bool>>(new Dictionary<ItemDetailField, bool>());
            public Task SaveItemDetailVisibilityAsync(IDictionary<ItemDetailField, bool> visibility, CancellationToken cancellationToken = default) => Task.CompletedTask;
            public Task<double> GetItemCardSizeAsync(CancellationToken cancellationToken = default) => Task.FromResult(1.0);
            public Task SaveItemCardSizeAsync(double size, CancellationToken cancellationToken = default)
            {
                ItemCardSizeChanged?.Invoke(this, size);
                return Task.CompletedTask;
            }
        }

        private sealed class StubThemeService : IThemeService
        {
            public void ApplyTheme(string? theme) { }
        }

        private sealed class StubDatabaseBackupService : IDatabaseBackupService
        {
            public Task BackupDatabaseAsync(string backupFilePath, CancellationToken cancellationToken) => Task.CompletedTask;
        }

        private sealed class StubDialogService : IDialogService
        {
            public void ShowInfo(string message, string title) { }
            public bool ShowConfirmation(string message, string title) => true;
            public ItemModel? ShowEditItemDialog(ItemModel item) => null;
            public void ShowItemDetails(ItemModel item) { }
            public (CustomerModel customer, DateTime dueDate)? ShowRentItemDialog(ItemModel item, IEnumerable<CustomerModel> customers) => null;
            public CustomerModel? ShowAddCustomerDialog() => null;
            public CustomerModel? ShowEditCustomerDialog(CustomerModel customer) => null;
            public void ShowRentalsFilter(ManageRentalsViewModel viewModel) { }
            public void ShowRentalHistory(ItemModel item, IEnumerable<RentalModel> history) { }
            public Dictionary<string, string>? ShowImportMapping(IEnumerable<string> headers, IEnumerable<string> properties, IEnumerable<string>? requiredPropertyNames = null) => null;
            public Func<ItemModel, IEnumerable<string>>? ShowImageImportMapping() => null;
            public void ShowPrintPreview(FlowDocument document, string title, string description) { }
            public void ShowPrintLabelDialog() { }
        }

        private sealed class StubDispatcherTimer : IDispatcherTimer
        {
            public event EventHandler? Tick;
            public TimeSpan Interval { get; set; }
            public bool IsEnabled { get; private set; }
            public void Start() => IsEnabled = true;
            public void Stop() => IsEnabled = false;
        }
    }
}
