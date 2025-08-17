using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Threading.Tasks;
using System.Threading;
using ToolManagementAppV2.Models;
using ToolManagementAppV2.Models.Domain;
using ToolManagementAppV2.Services.Core;
using ToolManagementAppV2.Services.Users;
using ToolManagementAppV2.Services.Settings;
using ToolManagementAppV2.ViewModels;
using ToolManagementAppV2.Interfaces;
using ToolManagementAppV2.Utilities.Helpers;
using ToolManagementAppV2.Views;
using Xunit;
using Microsoft.Extensions.Logging;
using ToolManagementAppV2.Tests;

namespace ToolManagementAppV2.Tests.ViewModels
{
    public class LoginViewModelTests
    {
        [Fact]
        public async Task InitializeAsync_LoadsLogoAndTitle()
        {
            if (System.Windows.Application.Current == null)
                new System.Windows.Application();

            var dbPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + ".db");
            try
            {
                using var dbService = new DatabaseService(dbPath);
                var userContext = new ApplicationUserContext();
                var auth = new AuthorizationService(userContext);
                var userService = new UserService(dbService, userContext, auth);
                var settingsService = new SettingsService(dbService);
                await settingsService.SaveSettingAsync("ApplicationName", "TestApp");

                var vm = new LoginViewModel(userService, settingsService, new StubDialogService(), userContext);
                await vm.InitializeAsync();

                Assert.Equal("TestApp – Login", vm.WindowTitle);
                Assert.NotNull(vm.CompanyLogo);
                Assert.NotEmpty(vm.Users);
            }
            finally
            {
                if (File.Exists(dbPath)) File.Delete(dbPath);
            }
        }

        [Fact]
        public async Task LoadUsersAsync_SeedsAdmin_LogsInformation()
        {
            if (System.Windows.Application.Current == null)
                new System.Windows.Application();

            var dbPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + ".db");
            try
            {
                using var dbService = new DatabaseService(dbPath);
                var userContext = new ApplicationUserContext();
                var auth = new AuthorizationService(userContext);
                var userService = new UserService(dbService, userContext, auth);
                var settingsService = new SettingsService(dbService);

                var logs = new List<LogEntry>();
                using var provider = new ListLoggerProvider(logs);
                using var loggerFactory = LoggerFactory.Create(builder => builder.AddProvider(provider));
                var logger = loggerFactory.CreateLogger<LoginViewModel>();

                var vm = new LoginViewModel(userService, settingsService, new StubDialogService(), userContext, logger);
                await vm.LoadUsersCommand.ExecuteAsync(null);

                Assert.Contains(logs, l => l.Level == LogLevel.Information && l.Message.Contains("admin"));
            }
            finally
            {
                if (File.Exists(dbPath)) File.Delete(dbPath);
            }
        }

        [Fact]
        public async Task SelectUserCommand_SetsCurrentUser()
        {
            if (System.Windows.Application.Current == null)
                new System.Windows.Application();

            var dbPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + ".db");
            try
            {
                using var dbService = new DatabaseService(dbPath);
                var userContext = new ApplicationUserContext();
                var auth = new AuthorizationService(userContext);
                var userService = new UserService(dbService, userContext, auth);
                var settingsService = new SettingsService(dbService);
                userService.AddUser(new User { UserName = "user", Password = "newpassword", IsAdmin = false });

                var vm = new LoginViewModel(userService, settingsService, new StubDialogService(), userContext);
                await vm.InitializeAsync();
                bool success = false;
                vm.LoginSucceeded += (_, __) => success = true;

                await vm.SelectUserCommand.ExecuteAsync(vm.Users.First());

                Assert.True(success);
                Assert.NotNull(userContext.CurrentUser);
                Assert.Equal("user", userContext.UserName);
            }
            finally
            {
                if (File.Exists(dbPath)) File.Delete(dbPath);
            }
        }

        [Fact]
        public async Task SelectUserCommand_PromptsForPasswordChange_WhenExpired()
        {
            if (System.Windows.Application.Current == null)
                new System.Windows.Application();

            var dbPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + ".db");
            try
            {
                using var dbService = new DatabaseService(dbPath);
                var userContext = new ApplicationUserContext();
                var auth = new AuthorizationService(userContext);
                var userService = new UserService(dbService, userContext, auth);
                var settingsService = new SettingsService(dbService);
                var user = new User { UserName = "user", Password = "newpassword", IsAdmin = false, PasswordExpired = true };
                userService.AddUser(user);

                var vm = new LoginViewModel(userService, settingsService, new StubDialogService(), userContext)
                {
                    PromptForNewPassword = () => "changed"
                };
                await vm.InitializeAsync();
                bool success = false;
                vm.LoginSucceeded += (_, __) => success = true;

                await vm.SelectUserCommand.ExecuteAsync(vm.Users.First());

                Assert.True(success);
                var updated = userService.GetUserByID(user.UserID)!;
                Assert.False(updated.PasswordExpired);
                Assert.True(SecurityHelper.VerifyPassword("changed", updated.Salt, updated.Password));
            }
            finally
            {
                if (File.Exists(dbPath)) File.Delete(dbPath);
            }
        }

        [Fact]
        public async Task SelectUserCommand_PromptsForPasswordChange_WhenAdminExpired()
        {
            if (System.Windows.Application.Current == null)
                new System.Windows.Application();

            var dbPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + ".db");
            try
            {
                using var dbService = new DatabaseService(dbPath);
                var userContext = new ApplicationUserContext();
                var auth = new AuthorizationService(userContext);
                var userService = new UserService(dbService, userContext, auth);
                var settingsService = new SettingsService(dbService);
                var admin = new User { UserName = "admin", Password = "secret", IsAdmin = true, PasswordExpired = true };
                userService.AddUser(admin);

                var vm = new LoginViewModel(userService, settingsService, new StubDialogService(), userContext)
                {
                    PromptForPasswordAsync = (u, ct) =>
                        Task.FromResult<PasswordPromptResult?>(new PasswordPromptResult("secret", false)),
                    PromptForNewPassword = () => "changed"
                };
                await vm.InitializeAsync();
                bool success = false;
                vm.LoginSucceeded += (_, __) => success = true;

                await vm.SelectUserCommand.ExecuteAsync(vm.Users.First());

                Assert.True(success);
                var updated = userService.GetUserByID(admin.UserID)!;
                Assert.False(updated.PasswordExpired);
                Assert.True(SecurityHelper.VerifyPassword("changed", updated.Salt, updated.Password));
            }
            finally
            {
                if (File.Exists(dbPath)) File.Delete(dbPath);
            }
        }

        [Fact]
        public async Task SelectUserCommand_PromptsForPassword_AuthenticatesAdmin()
        {
            if (System.Windows.Application.Current == null)
                new System.Windows.Application();

            var dbPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + ".db");
            try
            {
                using var dbService = new DatabaseService(dbPath);
                var userContext = new ApplicationUserContext();
                var userService = new UserService(dbService, userContext);
                var settingsService = new SettingsService(dbService);
                userService.AddUser(new User { UserName = "admin", Password = "secret", IsAdmin = true });

                var vm = new LoginViewModel(userService, settingsService, new StubDialogService(), userContext)
                {
                    PromptForPasswordAsync = (u, ct) =>
                        Task.FromResult<PasswordPromptResult?>(new PasswordPromptResult("secret", false))
                };
                await vm.InitializeAsync();
                bool success = false;
                vm.LoginSucceeded += (_, __) => success = true;

                await vm.SelectUserCommand.ExecuteAsync(vm.Users.First());

                Assert.True(success);
                Assert.Equal("admin", userContext.UserName);
            }
            finally
            {
                if (File.Exists(dbPath)) File.Delete(dbPath);
            }
        }

        [Fact]
        public async Task SelectUserCommand_CanBeCancelled()
        {
            if (System.Windows.Application.Current == null)
                new System.Windows.Application();

            var dbPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + ".db");
            try
            {
                using var dbService = new DatabaseService(dbPath);
                var userContext = new ApplicationUserContext();
                var userService = new UserService(dbService, userContext);
                var settingsService = new SettingsService(dbService);
                userService.AddUser(new User { UserName = "admin", Password = "secret", IsAdmin = true });

                var vm = new LoginViewModel(userService, settingsService, new StubDialogService(), userContext)
                {
                    PromptForPasswordAsync = async (u, ct) =>
                    {
                        await Task.Delay(TimeSpan.FromSeconds(5), ct);
                        return new PasswordPromptResult("secret", false);
                    }
                };
                await vm.InitializeAsync();

                var execTask = vm.SelectUserCommand.ExecuteAsync(vm.Users.First());
                vm.SelectUserCommand.Cancel();

                await Assert.ThrowsAsync<OperationCanceledException>(async () => await execTask);
                Assert.Null(userContext.CurrentUser);
            }
            finally
            {
                if (File.Exists(dbPath)) File.Delete(dbPath);
            }
        }

        [Fact]
        public void ChangePasswordWindow_Dispose_ClosesWindow()
        {
            if (System.Windows.Application.Current == null)
                new System.Windows.Application();

            var win = new ChangePasswordWindow();
            var closed = false;
            win.Closed += (_, __) => closed = true;

            win.Dispose();

            Assert.True(closed);
        }

        [Fact]
        public async Task LoadUsersAsync_AwaitsAdminCreation()
        {
            if (System.Windows.Application.Current == null)
                new System.Windows.Application();

            var tcs = new TaskCompletionSource<bool>();
            var userService = new AwaitableUserService(tcs);
            var settingsService = new StubSettingsService();
            var userContext = new ApplicationUserContext();
            var vm = new LoginViewModel(userService, settingsService, new StubDialogService(), userContext);

            var loadTask = vm.LoadUsersCommand.ExecuteAsync(null);

            await Task.Delay(50);
            Assert.False(loadTask.IsCompleted);

            tcs.SetResult(true);
            await loadTask;

            Assert.Single(vm.Users);
            Assert.Equal("admin", vm.Users[0].UserName);
        }

        [Fact]
        public async Task PromptChangePassword_ShowsError_WhenServiceFails()
        {
            if (System.Windows.Application.Current == null)
                new System.Windows.Application();

            var hashed = SecurityHelper.HashPassword("newpassword", out var salt);
            var user = new User { UserID = 1, UserName = "user", Password = hashed, Salt = salt, IsAdmin = false, PasswordExpired = true };
            var userService = new ThrowingUserService(user);
            var settingsService = new StubSettingsService();
            var dialog = new CapturingDialogService();
            var userContext = new ApplicationUserContext();
            var logger = new CapturingLogger<LoginViewModel>();

            var vm = new LoginViewModel(userService, settingsService, dialog, userContext, logger)
            {
                PromptForNewPassword = () => "changed"
            };
            await vm.LoadUsersCommand.ExecuteAsync(null);

            await vm.SelectUserCommand.ExecuteAsync(vm.Users[0]);

            Assert.True(dialog.InfoShown);
            Assert.Null(userContext.CurrentUser);
            Assert.NotNull(logger.LastException);
        }

        [Fact]
        public async Task SelectUserCommand_AdminWithoutPassword_IgnoresUnauthorized()
        {
            if (System.Windows.Application.Current == null)
                new System.Windows.Application();

            var dbPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + ".db");
            try
            {
                using var dbService = new DatabaseService(dbPath);
                var userContext = new ApplicationUserContext();
                var auth = new AuthorizationService(userContext);
                var userService = new UserService(dbService, userContext, auth);
                var settingsService = new SettingsService(dbService);
                await userService.AddUserAsync(new User { UserName = "admin", IsAdmin = true });

                var vm = new LoginViewModel(userService, settingsService, new StubDialogService(), userContext)
                {
                    PromptForPasswordAsync = (u, ct) => Task.FromResult<PasswordPromptResult?>(null)
                };
                await vm.InitializeAsync();

                await vm.SelectUserCommand.ExecuteAsync(vm.Users[0]);

                Assert.Null(userContext.CurrentUser);
            }
            finally
            {
                if (File.Exists(dbPath)) File.Delete(dbPath);
            }
        }

        [Fact]
        public async Task SelectUserCommand_DoesNotResetAdminPassword_WhenPasswordPresentInDatabase()
        {
            if (System.Windows.Application.Current == null)
                new System.Windows.Application();

            var hashed = SecurityHelper.HashPassword("secret", out var salt);
            var dbUser = new User { UserID = 1, UserName = "admin", Password = hashed, Salt = salt, IsAdmin = true };
            var userService = new OmittingPasswordUserService(dbUser);
            var settingsService = new StubSettingsService();
            var userContext = new ApplicationUserContext();

            var vm = new LoginViewModel(userService, settingsService, new StubDialogService(), userContext)
            {
                PromptForPasswordAsync = (u, ct) => Task.FromResult<PasswordPromptResult?>(new PasswordPromptResult("secret", false))
            };
            await vm.LoadUsersCommand.ExecuteAsync(null);
            bool success = false;
            vm.LoginSucceeded += (_, __) => success = true;

            await vm.SelectUserCommand.ExecuteAsync(vm.Users[0]);

            Assert.True(success);
            Assert.False(userService.ChangePasswordCalled);
        }

        [Fact]
        public async Task SelectUserCommand_ShowsLockoutMessage_WhenAccountLocked()
        {
            if (System.Windows.Application.Current == null)
                new System.Windows.Application();

            var dbPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + ".db");
            try
            {
                using var dbService = new DatabaseService(dbPath);
                var userContext = new ApplicationUserContext();
                var auth = new AuthorizationService(userContext);
                var userService = new UserService(dbService, userContext, auth);
                var settingsService = new SettingsService(dbService);
                var lockout = DateTime.UtcNow.AddMinutes(15);
                await userService.AddUserAsync(new User { UserName = "locked", Password = "newpassword", LockoutUntil = lockout });

                var dialog = new CapturingDialogService();
                var vm = new LoginViewModel(userService, settingsService, dialog, userContext)
                {
                    PromptForPasswordAsync = (u, ct) => Task.FromResult<PasswordPromptResult?>(new PasswordPromptResult("secret", false))
                };
                await vm.InitializeAsync();

                await vm.SelectUserCommand.ExecuteAsync(vm.Users.First());

                Assert.True(dialog.InfoShown);
                Assert.Contains(lockout.ToString(), dialog.LastMessage);
            }
            finally
            {
                if (File.Exists(dbPath)) File.Delete(dbPath);
            }
        }
    }

    class StubDialogService : IDialogService
    {
        public void ShowInfo(string message, string title) { }
        public bool ShowConfirmation(string message, string title) => true;
       public ToolModel? ShowEditToolDialog(ToolModel tool) => null;
        public void ShowToolDetails(ToolModel tool) { }
        public (CustomerModel customer, DateTime dueDate)? ShowRentToolDialog(ToolModel tool, IEnumerable<CustomerModel> customers) => null;
        public CustomerModel? ShowAddCustomerDialog() => null;
        public void ShowRentalsFilter(ToolManagementAppV2.ViewModels.ManageRentalsViewModel viewModel) { }
        public void ShowRentalHistory(ToolModel tool, System.Collections.Generic.IEnumerable<RentalModel> history) { }
        public System.Collections.Generic.Dictionary<string, string>? ShowImportMapping(System.Collections.Generic.IEnumerable<string> headers, System.Collections.Generic.IEnumerable<string> properties) => null;
        public System.Func<ToolModel, System.Collections.Generic.IEnumerable<string>>? ShowImageImportMapping() => null;
        public void ShowPrintPreview(System.Windows.Documents.FlowDocument document, string title, string description) { }
        public void ShowPrintLabelDialog() { }
        public void ShowScannerStatus() { }
    }

    class AwaitableUserService : IUserService
    {
        readonly TaskCompletionSource<bool> _tcs;
        readonly List<User> _users = new();

        public AwaitableUserService(TaskCompletionSource<bool> tcs)
        {
            _tcs = tcs;
        }

        public List<User> GetAllUsers() => _users.ToList();
        public Task<List<User>> GetAllUsersAsync() => Task.FromResult(_users.ToList());
        public User? GetUserByID(int userID) => null;
        public Task<User?> GetUserByIDAsync(int userID) => Task.FromResult<User?>(null);
        public User? AuthenticateUser(string userName, string password) => null;
        public Task<(AuthenticationResult Result, User? User)> AuthenticateUserAsync(string userName, string password) => Task.FromResult<(AuthenticationResult, User?)>((AuthenticationResult.Failed, null));
        public User? GetCurrentUser() => null;
        public Task<User?> GetCurrentUserAsync() => Task.FromResult<User?>(null);
        public void AddUser(User user) => _users.Add(user);
        public async Task AddUserAsync(User user)
        {
            await _tcs.Task;
            _users.Add(user);
        }
        public void UpdateUser(User user) { }
        public Task UpdateUserAsync(User user) => Task.CompletedTask;
        public Task<bool> TryDeleteUserAsync(int userID) => Task.FromResult(false);
        public bool ChangeUserPassword(int userID, string newPassword) => false;
        public Task<bool> ChangeUserPasswordAsync(int userID, string newPassword) => Task.FromResult(false);
    }

    class ThrowingUserService : IUserService
    {
        readonly List<User> _users;

        public ThrowingUserService(User user)
        {
            _users = new List<User> { user };
        }

        public List<User> GetAllUsers() => _users.ToList();
        public Task<List<User>> GetAllUsersAsync() => Task.FromResult(GetAllUsers());
        public User? GetUserByID(int userID) => _users.FirstOrDefault(u => u.UserID == userID);
        public Task<User?> GetUserByIDAsync(int userID) => Task.FromResult(GetUserByID(userID));
        public User? AuthenticateUser(string userName, string password) => null;
        public Task<(AuthenticationResult Result, User? User)> AuthenticateUserAsync(string userName, string password) => Task.FromResult<(AuthenticationResult, User?)>((AuthenticationResult.Failed, null));
        public User? GetCurrentUser() => null;
        public Task<User?> GetCurrentUserAsync() => Task.FromResult<User?>(null);
        public void AddUser(User user) => _users.Add(user);
        public Task AddUserAsync(User user)
        {
            _users.Add(user);
            return Task.CompletedTask;
        }
        public void UpdateUser(User user) { }
        public Task UpdateUserAsync(User user) => Task.CompletedTask;
        public Task<bool> TryDeleteUserAsync(int userID) => Task.FromResult(false);
        public bool ChangeUserPassword(int userID, string newPassword) => throw new InvalidOperationException();
        public Task<bool> ChangeUserPasswordAsync(int userID, string newPassword) => throw new InvalidOperationException();
    }

    class OmittingPasswordUserService : IUserService
    {
        readonly User _dbUser;
        public bool ChangePasswordCalled { get; private set; }

        public OmittingPasswordUserService(User dbUser)
        {
            _dbUser = dbUser;
        }

        public Task<List<User>> GetAllUsersAsync()
            => Task.FromResult(new List<User>
            {
                new User { UserID = _dbUser.UserID, UserName = _dbUser.UserName, IsAdmin = _dbUser.IsAdmin }
            });

        public Task<User?> GetUserByIDAsync(int userID)
            => Task.FromResult(userID == _dbUser.UserID ? _dbUser : null);

        public Task<(AuthenticationResult Result, User? User)> AuthenticateUserAsync(string userName, string password)
            => Task.FromResult(
                userName == _dbUser.UserName && SecurityHelper.VerifyPassword(password, _dbUser.Salt, _dbUser.Password)
                    ? (AuthenticationResult.Success, (User?)_dbUser)
                    : (AuthenticationResult.Failed, (User?)null));

        public Task<User?> GetCurrentUserAsync() => Task.FromResult<User?>(null);
        public Task AddUserAsync(User user) => Task.CompletedTask;
        public Task UpdateUserAsync(User user) => Task.CompletedTask;
        public Task<bool> TryDeleteUserAsync(int userID) => Task.FromResult(false);
        public Task<bool> ChangeUserPasswordAsync(int userID, string newPassword)
        {
            ChangePasswordCalled = true;
            return Task.FromResult(true);
        }
    }

    class CapturingDialogService : IDialogService
    {
        public bool InfoShown { get; private set; }
        public string? LastMessage { get; private set; }
        public void ShowInfo(string message, string title)
        {
            InfoShown = true;
            LastMessage = message;
        }
        public bool ShowConfirmation(string message, string title) => true;
        public ToolModel? ShowEditToolDialog(ToolModel tool) => null;
        public void ShowToolDetails(ToolModel tool) { }
        public (CustomerModel customer, DateTime dueDate)? ShowRentToolDialog(ToolModel tool, IEnumerable<CustomerModel> customers) => null;
        public CustomerModel? ShowAddCustomerDialog() => null;
        public void ShowRentalsFilter(ToolManagementAppV2.ViewModels.ManageRentalsViewModel viewModel) { }
        public void ShowRentalHistory(ToolModel tool, IEnumerable<RentalModel> history) { }
        public Dictionary<string, string>? ShowImportMapping(IEnumerable<string> headers, IEnumerable<string> properties) => null;
        public Func<ToolModel, IEnumerable<string>>? ShowImageImportMapping() => null;
        public void ShowPrintPreview(System.Windows.Documents.FlowDocument document, string title, string description) { }
        public void ShowPrintLabelDialog() { }
        public void ShowScannerStatus() { }
    }

    class CapturingLogger<T> : ILogger<T>
    {
        public Exception? LastException { get; private set; }
        public IDisposable BeginScope<TState>(TState state) => NullDisposable.Instance;
        public bool IsEnabled(LogLevel logLevel) => true;
        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            if (logLevel == LogLevel.Error)
                LastException = exception;
        }

        sealed class NullDisposable : IDisposable
        {
            public static readonly NullDisposable Instance = new();
            public void Dispose() { }
        }
    }
}
