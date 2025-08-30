using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using InventoryManagementApp.Interfaces;
using InventoryManagementApp.Models;
using InventoryManagementApp.Models.Domain;
using InventoryManagementApp.ViewModels;
using InventoryManagementApp.Views.Windows;
using InventoryManagementApp.Services.Users;
using InventoryManagementApp.Utilities.Helpers;
using Xunit;

namespace InventoryManagementApp.Tests
{
    public class UserInitialsBrushTests
    {
        [Fact]
        public void UsersWithSameInitialsGetDifferentBrushes()
        {
            Exception? threadEx = null;
            var thread = new Thread(() =>
            {
                try
                {
                    var app = new Application();
                    app.Resources.MergedDictionaries.Add(new ResourceDictionary { Source = new Uri("pack://application:,,,/InventoryManagementApp;component/Resources/Colors.Dark.xaml", UriKind.Absolute) });
                    var users = new List<User>
                    {
                        new User { UserID = 1, UserName = "John Doe" },
                        new User { UserID = 2, UserName = "Jane Doe" },
                        new User { UserID = 3, UserName = "Alice Smith" }
                    };
                    var svc = new StubUserService(users);
                    var vm = new UserManagementViewModel(svc, new DummyFileDialogService(), new DummyDialogService());
                    vm.LoadUsersAsync().GetAwaiter().GetResult();
                    Assert.NotEqual(vm.Users[0].InitialsBrush, vm.Users[1].InitialsBrush);
                    var defaultBrush = Application.Current.TryFindResource("ForegroundBrush") as Brush;
                    Assert.Equal(defaultBrush, vm.Users[2].InitialsBrush);
                }
                catch (Exception ex)
                {
                    threadEx = ex;
                }
                finally
                {
                    Application.Current?.Shutdown();
                }
            });
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            thread.Join();
            if (threadEx != null) throw threadEx;
        }

        [Fact]
        public void EditUserRetainsInitialsBrush()
        {
            Exception? threadEx = null;
            var thread = new Thread(() =>
            {
                try
                {
                    var app = new Application();
                    app.Resources.MergedDictionaries.Add(new ResourceDictionary { Source = new Uri("pack://application:,,,/InventoryManagementApp;component/Resources/Colors.Dark.xaml", UriKind.Absolute) });
                    var users = new List<User>
                    {
                        new User { UserID = 1, UserName = "John Doe" }
                    };
                    var svc = new StubUserService(users);
                    var vm = new UserManagementViewModel(svc, new DummyFileDialogService(), new DummyDialogService());
                    vm.LoadUsersAsync().GetAwaiter().GetResult();
                    vm.SelectedUser = vm.Users[0];
                    var originalBrush = vm.SelectedUser.InitialsBrush;

                    app.Dispatcher.InvokeAsync(async () =>
                    {
                        await Task.Delay(100);
                        var win = app.Windows.OfType<UsersEditWindow>().First();
                        var editVm = (UsersEditViewModel)win.DataContext;
                        await editVm.SaveCommand.ExecuteAsync(null);
                    });

                    vm.EditUserCommand.Execute(null);

                    Assert.Equal(originalBrush, vm.Users[0].InitialsBrush);
                }
                catch (Exception ex)
                {
                    threadEx = ex;
                }
                finally
                {
                    Application.Current?.Shutdown();
                }
            });
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            thread.Join();
            if (threadEx != null) throw threadEx;
        }

        [Fact]
        public void EditUserNameReassignsBrushes()
        {
            Exception? threadEx = null;
            var thread = new Thread(() =>
            {
                try
                {
                    var app = new Application();
                    app.Resources.MergedDictionaries.Add(new ResourceDictionary { Source = new Uri("pack://application:,,,/InventoryManagementApp;component/Resources/Colors.Dark.xaml", UriKind.Absolute) });
                    var users = new List<User>
                    {
                        new User { UserID = 1, UserName = "John Doe" },
                        new User { UserID = 2, UserName = "Jane Doe" }
                    };
                    var svc = new StubUserService(users);
                    var vm = new UserManagementViewModel(svc, new DummyFileDialogService(), new DummyDialogService());
                    vm.LoadUsersAsync().GetAwaiter().GetResult();
                    vm.SelectedUser = vm.Users[0];
                    var defaultBrush = Application.Current.TryFindResource("ForegroundBrush") as Brush;
                    Assert.NotEqual(defaultBrush, vm.Users[1].InitialsBrush);

                    app.Dispatcher.InvokeAsync(async () =>
                    {
                        await Task.Delay(100);
                        var win = app.Windows.OfType<UsersEditWindow>().First();
                        var editVm = (UsersEditViewModel)win.DataContext;
                        editVm.EditingUser.UserName = "Alice Smith";
                        await editVm.SaveCommand.ExecuteAsync(null);
                    });

                    vm.EditUserCommand.Execute(null);

                    Assert.Equal(defaultBrush, vm.Users[0].InitialsBrush);
                    Assert.Equal(defaultBrush, vm.Users[1].InitialsBrush);
                }
                catch (Exception ex)
                {
                    threadEx = ex;
                }
                finally
                {
                    Application.Current?.Shutdown();
                }
            });
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            thread.Join();
            if (threadEx != null) throw threadEx;
        }

        [Fact]
        public void LoginRetainsInitialsBrush()
        {
            Exception? threadEx = null;
            var thread = new Thread(() =>
            {
                try
                {
                    var app = new Application();
                    app.Resources.MergedDictionaries.Add(new ResourceDictionary { Source = new Uri("pack://application:,,,/InventoryManagementApp;component/Resources/Colors.Dark.xaml", UriKind.Absolute) });

                    var hash = SecurityHelper.HashPassword("pass", out var salt);
                    var users = new List<User>
                    {
                        new User { UserID = 1, UserName = "John Doe", PasswordHash = hash, PasswordSalt = salt, IsActive = true }
                    };
                    var svc = new StubUserService(users);
                    var settings = new DummySettingsService();
                    var dialog = new DummyDialogService();
                    var context = new ApplicationUserContext();

                    var vm = new LoginViewModel(svc, settings, dialog, context);
                    vm.LoadUsersCommand.ExecuteAsync(null).GetAwaiter().GetResult();
                    var loginUser = vm.Users[0];
                    var originalBrush = loginUser.InitialsBrush;

                    bool eventFired = false;
                    context.UserChanged += (_, __) => eventFired = true;

                    vm.PromptForPasswordAsync = (_, __) => Task.FromResult<PasswordPromptResult?>(new PasswordPromptResult("pass", false));
                    vm.SelectUserCommand.ExecuteAsync(loginUser).GetAwaiter().GetResult();

                    Assert.True(eventFired);
                    Assert.NotNull(context.CurrentUser);
                    Assert.Equal(originalBrush, context.CurrentUser!.InitialsBrush);
                }
                catch (Exception ex)
                {
                    threadEx = ex;
                }
                finally
                {
                    Application.Current?.Shutdown();
                }
            });
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            thread.Join();
            if (threadEx != null) throw threadEx;
        }

        [Fact]
        public void LoginAssignsDefaultBrushWhenInitialsBrushNull()
        {
            Exception? threadEx = null;
            var thread = new Thread(() =>
            {
                try
                {
                    var app = new Application();
                    app.Resources.MergedDictionaries.Add(new ResourceDictionary { Source = new Uri("pack://application:,,,/InventoryManagementApp;component/Resources/Colors.Dark.xaml", UriKind.Absolute) });

                    var hash = SecurityHelper.HashPassword("pass", out var salt);
                    var users = new List<User>
                    {
                        new User { UserID = 1, UserName = "John Doe", PasswordHash = hash, PasswordSalt = salt, IsActive = true }
                    };
                    var svc = new StubUserService(users);
                    var settings = new DummySettingsService();
                    var dialog = new DummyDialogService();
                    var context = new ApplicationUserContext();

                    var vm = new LoginViewModel(svc, settings, dialog, context);
                    vm.LoadUsersCommand.ExecuteAsync(null).GetAwaiter().GetResult();
                    var loginUser = vm.Users[0];
                    loginUser.InitialsBrush = null!;

                    bool eventFired = false;
                    context.UserChanged += (_, __) => eventFired = true;

                    vm.PromptForPasswordAsync = (_, __) => Task.FromResult<PasswordPromptResult?>(new PasswordPromptResult("pass", false));
                    vm.SelectUserCommand.ExecuteAsync(loginUser).GetAwaiter().GetResult();

                    var defaultBrush = Application.Current.TryFindResource("ForegroundBrush") as Brush;
                    Assert.True(eventFired);
                    Assert.NotNull(context.CurrentUser);
                    Assert.Equal(defaultBrush, context.CurrentUser!.InitialsBrush);
                }
                catch (Exception ex)
                {
                    threadEx = ex;
                }
                finally
                {
                    Application.Current?.Shutdown();
                }
            });
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            thread.Join();
            if (threadEx != null) throw threadEx;
        }

        private sealed class StubUserService : IUserService
        {
            private readonly List<User> _users;
            public StubUserService(List<User> users) => _users = users;
            public Task<List<User>> GetAllUsersAsync(CancellationToken cancellationToken = default) => Task.FromResult(_users);
            public Task<int> CountUsersAsync(CancellationToken cancellationToken = default) => Task.FromResult(_users.Count);
            public Task<User?> GetUserByIDAsync(int userID, CancellationToken cancellationToken = default)
            {
                var u = _users.FirstOrDefault(u => u.UserID == userID);
                if (u == null) return Task.FromResult<User?>(null);
                return Task.FromResult<User?>(new User
                {
                    UserID = u.UserID,
                    UserName = u.UserName,
                    PasswordHash = u.PasswordHash,
                    PasswordSalt = u.PasswordSalt,
                    IsAdmin = u.IsAdmin,
                    IsActive = u.IsActive,
                    PasswordExpired = u.PasswordExpired
                });
            }
            public Task<(AuthenticationResult Result, User? User)> AuthenticateUserAsync(string userName, string password)
            {
                var u = _users.FirstOrDefault(u => u.UserName == userName);
                User? copy = null;
                if (u != null)
                {
                    copy = new User
                    {
                        UserID = u.UserID,
                        UserName = u.UserName,
                        PasswordHash = u.PasswordHash,
                        PasswordSalt = u.PasswordSalt,
                        IsAdmin = u.IsAdmin,
                        IsActive = u.IsActive,
                        PasswordExpired = u.PasswordExpired
                    };
                }
                return Task.FromResult<(AuthenticationResult, User?)>((AuthenticationResult.Success, copy));
            }
            public Task<User?> GetCurrentUserAsync() => Task.FromResult<User?>(null);
            public Task AddUserAsync(User user)
            {
                _users.Add(user);
                return Task.CompletedTask;
            }
            public Task UpdateUserAsync(User user)
            {
                var idx = _users.FindIndex(u => u.UserID == user.UserID);
                if (idx >= 0) _users[idx] = user;
                return Task.CompletedTask;
            }
            public Task<bool> TryDeleteUserAsync(int userID)
            {
                var removed = _users.RemoveAll(u => u.UserID == userID) > 0;
                return Task.FromResult(removed);
            }
            public Task<bool> ChangeUserPasswordAsync(int userID, string newPassword)
            {
                var u = _users.FirstOrDefault(u => u.UserID == userID);
                if (u == null) return Task.FromResult(false);
                u.PasswordHash = newPassword;
                return Task.FromResult(true);
            }
        }

        private sealed class DummyFileDialogService : IFileDialogService
        {
            public string? OpenFile(string filter, string? initialDirectory = null) => null;
            public string? SaveFile(string filter) => null;
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
            public void ShowPrintPreview(FlowDocument document, string title, string description) { }
            public void ShowPrintLabelDialog() { }
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
            public Task<IDictionary<ItemDetailField, bool>> GetItemDetailVisibilityAsync(CancellationToken cancellationToken = default) => Task.FromResult<IDictionary<ItemDetailField, bool>>(new Dictionary<ItemDetailField, bool>());
            public Task SaveItemDetailVisibilityAsync(IDictionary<ItemDetailField, bool> visibility, CancellationToken cancellationToken = default)
            {
                ItemDetailVisibilityChanged?.Invoke(this, visibility);
                return Task.CompletedTask;
            }
        }
    }
}
