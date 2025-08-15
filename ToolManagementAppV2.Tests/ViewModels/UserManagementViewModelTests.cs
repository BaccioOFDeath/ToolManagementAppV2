using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ToolManagementAppV2.Models.Domain;
using ToolManagementAppV2.Services.Core;
using ToolManagementAppV2.Services.Users;
using ToolManagementAppV2.Interfaces;
using ToolManagementAppV2.ViewModels;
using ToolManagementAppV2.ViewModels.Rental;
using ToolManagementAppV2.Views;
using ToolManagementAppV2.Utilities.Helpers;
using Xunit;

namespace ToolManagementAppV2.Tests.ViewModels
{
    public class UserManagementViewModelTests
    {
        [Fact]
        public async Task UpdateUserCommand_PersistsChanges()
        {
            var dbPath = Path.GetTempFileName();
            try
            {
                var db = new DatabaseService(dbPath);
                IUserService userService = new UserService(db, new ApplicationUserContext());
                var vm = new UserManagementViewModel(userService, new StubFileDialogService(), new StubDialogService());
                userService.AddUser(new User { UserName = "user1", Password = "pw" });
                await vm.LoadUsersAsync();
                vm.SelectedUser = vm.Users.First();
                vm.SelectedUser.Email = "test@example.com";
                await vm.UpdateUserCommand.ExecuteAsync(null);
                var updated = userService.GetAllUsers().First();
                Assert.Equal("test@example.com", updated.Email);
                var field = typeof(UserManagementViewModel).GetField("_allUsers", BindingFlags.NonPublic | BindingFlags.Instance);
                var allUsers = (System.Collections.Generic.List<User>)field!.GetValue(vm);
                Assert.Equal("test@example.com", vm.Users.First().Email);
                Assert.Equal("test@example.com", allUsers.First().Email);
            }
            finally
            {
                if (File.Exists(dbPath))
                    File.Delete(dbPath);
            }
        }

        [Fact]
        public async Task UpdateUserCommand_ShowsErrorOnFailure()
        {
            var svc = new FailingUserService();
            svc.AddUser(new User { UserID = 1, UserName = "user1", Password = "pw" });
            var dialog = new StubDialogService();
            var vm = new UserManagementViewModel(svc, new StubFileDialogService(), dialog);
            await vm.LoadUsersAsync();
            vm.SelectedUser = vm.Users.First();

            await vm.UpdateUserCommand.ExecuteAsync(null);

            Assert.Equal("Error", dialog.LastInfoTitle);
            Assert.StartsWith("Failed to update user:", dialog.LastInfoMessage);
        }

        [Fact]
        public async Task EditUserAsync_PersistsChanges()
        {
            var svc = new InMemoryUserService();
            svc.AddUser(new User { UserName = "user1", Password = "pw" });
            var dialog = new StubDialogService();
            var vm = new UserManagementViewModel(svc, new StubFileDialogService(), dialog);
            await vm.LoadUsersAsync();
            var user = vm.Users.First();
            vm.SelectedUser = user;

            var clone = new User
            {
                UserID = user.UserID,
                UserName = user.UserName,
                Password = user.Password,
                Salt = user.Salt,
                UserPhotoPath = user.UserPhotoPath,
                IsAdmin = user.IsAdmin,
                Email = "edited@example.com",
                Phone = user.Phone,
                Mobile = user.Mobile,
                Address = user.Address,
                Role = user.Role,
                IsActive = user.IsActive,
                CreatedAt = user.CreatedAt
            };

            Func<Task> onSave = async () =>
            {
                try
                {
                    await svc.UpdateUserAsync(clone);
                    var idx = vm.Users.IndexOf(user);
                    if (idx >= 0) vm.Users[idx] = clone;
                    var field = typeof(UserManagementViewModel).GetField("_allUsers", BindingFlags.NonPublic | BindingFlags.Instance);
                    var allUsers = (List<User>)field!.GetValue(vm);
                    var idxAll = allUsers.IndexOf(user);
                    if (idxAll >= 0) allUsers[idxAll] = clone;
                    if (ReferenceEquals(vm.SelectedUser, user)) vm.SelectedUser = clone;
                }
                catch (Exception ex)
                {
                    dialog.ShowInfo($"Failed to update user: {ex.Message}", "Error");
                }
            };

            var editVm = new UsersEditViewModel(clone, onSave, () => { }, () => { }, () => { });
            await editVm.SaveCommand.ExecuteAsync(null);

            Assert.Equal("edited@example.com", svc.Users.First().Email);
            Assert.Equal("edited@example.com", vm.Users.First().Email);
            var allField = typeof(UserManagementViewModel).GetField("_allUsers", BindingFlags.NonPublic | BindingFlags.Instance);
            var allList = (List<User>)allField!.GetValue(vm);
            Assert.Equal("edited@example.com", allList.First().Email);
        }

        [Fact]
        public async Task EditUserAsync_ShowsErrorOnFailure()
        {
            var svc = new FailingUserService();
            svc.AddUser(new User { UserID = 1, UserName = "user1", Password = "pw" });
            var dialog = new StubDialogService();
            var vm = new UserManagementViewModel(svc, new StubFileDialogService(), dialog);
            await vm.LoadUsersAsync();
            var user = vm.Users.First();
            vm.SelectedUser = user;

            var clone = new User
            {
                UserID = user.UserID,
                UserName = user.UserName,
                Password = user.Password,
                Salt = user.Salt,
                UserPhotoPath = user.UserPhotoPath,
                IsAdmin = user.IsAdmin,
                Email = "edited@example.com",
                Phone = user.Phone,
                Mobile = user.Mobile,
                Address = user.Address,
                Role = user.Role,
                IsActive = user.IsActive,
                CreatedAt = user.CreatedAt
            };

            Func<Task> onSave = async () =>
            {
                try
                {
                    await svc.UpdateUserAsync(clone);
                    var idx = vm.Users.IndexOf(user);
                    if (idx >= 0) vm.Users[idx] = clone;
                    var field = typeof(UserManagementViewModel).GetField("_allUsers", BindingFlags.NonPublic | BindingFlags.Instance);
                    var allUsers = (List<User>)field!.GetValue(vm);
                    var idxAll = allUsers.IndexOf(user);
                    if (idxAll >= 0) allUsers[idxAll] = clone;
                    if (ReferenceEquals(vm.SelectedUser, user)) vm.SelectedUser = clone;
                }
                catch (Exception ex)
                {
                    dialog.ShowInfo($"Failed to update user: {ex.Message}", "Error");
                }
            };

            var editVm = new UsersEditViewModel(clone, onSave, () => { }, () => { }, () => { });
            await editVm.SaveCommand.ExecuteAsync(null);

            Assert.Equal("Error", dialog.LastInfoTitle);
            Assert.StartsWith("Failed to update user:", dialog.LastInfoMessage);
        }

        [Fact]
        public async Task UploadUserPhotoCommand_SetsPhotoPathAndPersists()
        {
            var dbPath = Path.GetTempFileName();
            try
            {
                var db = new DatabaseService(dbPath);
                IUserService userService = new UserService(db, new ApplicationUserContext());
                var fileSvc = new StubFileDialogService { FileToReturn = "path/to/image.png" };
                var vm = new UserManagementViewModel(userService, fileSvc, new StubDialogService());
                userService.AddUser(new User { UserName = "user1", Password = "pw" });
                await vm.LoadUsersAsync();
                vm.SelectedUser = vm.Users.First();
                await vm.UploadUserPhotoCommand.ExecuteAsync(null);
                var updated = userService.GetAllUsers().First();
                var expected = PathHelper.GetAbsolutePath("path/to/image.png");
                Assert.Equal(expected, updated.UserPhotoPath);
                var field = typeof(UserManagementViewModel).GetField("_allUsers", BindingFlags.NonPublic | BindingFlags.Instance);
                var allUsers = (System.Collections.Generic.List<User>)field!.GetValue(vm);
                Assert.Equal(expected, vm.Users.First().UserPhotoPath);
                Assert.Equal(expected, allUsers.First().UserPhotoPath);
            }
            finally
            {
                if (File.Exists(dbPath))
                    File.Delete(dbPath);
            }
        }

        [Fact]
        public async Task UploadUserPhotoCommand_RejectsPathsOutsideAppDirectory()
        {
            var dbPath = Path.GetTempFileName();
            try
            {
                var db = new DatabaseService(dbPath);
                IUserService userService = new UserService(db, new ApplicationUserContext());
                var fileSvc = new StubFileDialogService { FileToReturn = Path.Combine("..", "outside.png") };
                var dialog = new StubDialogService();
                var vm = new UserManagementViewModel(userService, fileSvc, dialog);
                userService.AddUser(new User { UserName = "user1", Password = "pw" });
                await vm.LoadUsersAsync();
                vm.SelectedUser = vm.Users.First();
                await vm.UploadUserPhotoCommand.ExecuteAsync(null);
                var updated = userService.GetAllUsers().First();
                Assert.Null(updated.UserPhotoPath);
                var field = typeof(UserManagementViewModel).GetField("_allUsers", BindingFlags.NonPublic | BindingFlags.Instance);
                var allUsers = (System.Collections.Generic.List<User>)field!.GetValue(vm);
                Assert.Null(vm.Users.First().UserPhotoPath);
                Assert.Null(allUsers.First().UserPhotoPath);
                Assert.Equal("Selected file path is invalid.", dialog.LastInfoMessage);
            }
            finally
            {
                if (File.Exists(dbPath))
                    File.Delete(dbPath);
            }
        }

        [Fact]
        public async Task UpdateUserAndUploadPhoto_UpdateCollections()
        {
            var dbPath = Path.GetTempFileName();
            try
            {
                var db = new DatabaseService(dbPath);
                IUserService userService = new UserService(db, new ApplicationUserContext());
                var fileSvc = new StubFileDialogService { FileToReturn = "img.png" };
                var vm = new UserManagementViewModel(userService, fileSvc, new StubDialogService());
                userService.AddUser(new User { UserName = "user1", Password = "pw" });
                await vm.LoadUsersAsync();
                vm.SelectedUser = vm.Users.First();

                vm.SelectedUser.Email = "test@example.com";
                await vm.UpdateUserCommand.ExecuteAsync(null);

                await vm.UploadUserPhotoCommand.ExecuteAsync(null);

                var field = typeof(UserManagementViewModel).GetField("_allUsers", BindingFlags.NonPublic | BindingFlags.Instance);
                var allUsers = (System.Collections.Generic.List<User>)field!.GetValue(vm);

                var expected = PathHelper.GetAbsolutePath("img.png");
                Assert.Equal("test@example.com", vm.Users.First().Email);
                Assert.Equal("test@example.com", allUsers.First().Email);
                Assert.Equal(expected, vm.Users.First().UserPhotoPath);
                Assert.Equal(expected, allUsers.First().UserPhotoPath);
            }
            finally
            {
                if (File.Exists(dbPath))
                    File.Delete(dbPath);
            }
        }

        [Fact]
        public async Task CommandsDisabledWhenNoUserSelected()
        {
            var svc = new InMemoryUserService();
            await svc.AddUserAsync(new User { UserName = "user1", Password = "pw" });
            var vm = new UserManagementViewModel(svc, new StubFileDialogService(), new StubDialogService());
            await vm.LoadUsersAsync();
            Assert.False(vm.UpdateUserCommand.CanExecute(null));
            Assert.False(vm.EditUserCommand.CanExecute(null));
            vm.SelectedUser = vm.Users.First();
            Assert.True(vm.UpdateUserCommand.CanExecute(null));
            Assert.True(vm.EditUserCommand.CanExecute(null));
        }

        [Fact]
        public async Task LoadUsersAsync_LoadsUsers()
        {
            var svc = new InMemoryUserService();
            await svc.AddUserAsync(new User { UserName = "user1", Password = "pw" });
            await svc.AddUserAsync(new User { UserName = "user2", Password = "pw" });
            var vm = new UserManagementViewModel(svc, new StubFileDialogService(), new StubDialogService());
            await vm.LoadUsersAsync();
            Assert.Equal(2, vm.Users.Count);
            Assert.Contains(vm.Users, u => u.UserName == "user1");
            Assert.Contains(vm.Users, u => u.UserName == "user2");
        }

        [Fact]
        public async Task LoadUsersAsync_ShowsErrorOnFailure()
        {
            var svc = new GetAllUsersFailingUserService();
            var dialog = new StubDialogService();
            var vm = new UserManagementViewModel(svc, new StubFileDialogService(), dialog);

            await vm.LoadUsersAsync();

            Assert.Equal("Error", dialog.LastInfoTitle);
            Assert.StartsWith("Failed to load users:", dialog.LastInfoMessage);
        }

        [Fact]
        public async Task AddUserAsync_AddsUser()
        {
            var svc = new InMemoryUserService();
            var vm = new PromptUserManagementViewModel(svc, "pw");
            await vm.AddUserAsync();
            Assert.Single(vm.Users);
            Assert.Single(svc.Users);
            Assert.Equal("pw", svc.Users[0].Password);
        }

        [Fact]
        public async Task AddUserAsync_SkipsExistingNameFromService()
        {
            var svc = new InMemoryUserService();
            await svc.AddUserAsync(new User { UserName = "user1", Password = "pw" });
            var vm = new PromptUserManagementViewModel(svc, "pw");
            await vm.AddUserAsync();

            Assert.Equal(2, svc.Users.Count);
            Assert.Contains(svc.Users, u => u.UserName == "user2");
            Assert.Single(vm.Users);
            Assert.Equal("user2", vm.Users.First().UserName);
        }

        [Fact]
        public async Task AddUserAsync_FindsFirstAvailableNumber()
        {
            var svc = new InMemoryUserService();
            await svc.AddUserAsync(new User { UserName = "user1", Password = "pw" });
            await svc.AddUserAsync(new User { UserName = "user3", Password = "pw" });
            var vm = new PromptUserManagementViewModel(svc, "pw");
            await vm.AddUserAsync();

            Assert.Equal(3, svc.Users.Count);
            Assert.Contains(svc.Users, u => u.UserName == "user2");
            Assert.Single(vm.Users);
            Assert.Equal("user2", vm.Users.First().UserName);
        }

        [Fact]
        public async Task AddUserAsync_BlankPassword_SetsPasswordExpired()
        {
            var svc = new InMemoryUserService();
            var vm = new PromptUserManagementViewModel(svc, "");
            await vm.AddUserAsync();
            var user = svc.Users.Single();
            Assert.True(user.PasswordExpired);
            Assert.False(string.IsNullOrEmpty(user.Password));
            Assert.NotEqual("changeme", user.Password);
        }

        [Fact]
        public async Task AddUserAsync_BlankPassword_SetsSalt()
        {
            var svc = new InMemoryUserService();
            var vm = new PromptUserManagementViewModel(svc, "");
            await vm.AddUserAsync();
            var user = svc.Users.Single();
            Assert.False(string.IsNullOrEmpty(user.Salt));
        }

        [Fact]
        public async Task AddUserAsync_WithEnteredPassword_DoesNotExpire()
        {
            var svc = new InMemoryUserService();
            var vm = new PromptUserManagementViewModel(svc, "secret");
            await vm.AddUserAsync();
            var user = svc.Users.Single();
            Assert.False(user.PasswordExpired);
            Assert.Equal("secret", user.Password);
        }

        [Fact]
        public async Task ResetPasswordFromRowCommand_DoesNotExposePasswordAndSetsFlag()
        {
            var dbPath = Path.GetTempFileName();
            try
            {
                var db = new DatabaseService(dbPath);
                IUserService userService = new UserService(db, new ApplicationUserContext());
                var dialog = new StubDialogService();
                var vm = new UserManagementViewModel(userService, new StubFileDialogService(), dialog);
                userService.AddUser(new User { UserName = "user1", Password = "pw" });
                await vm.LoadUsersAsync();
                var user = vm.Users.First();
                var original = userService.GetUserByID(user.UserID)!;
                var oldPwd = original.Password;
                await vm.ResetPasswordFromRowCommand.ExecuteAsync(user);
                var updated = userService.GetUserByID(user.UserID)!;
                Assert.NotEqual(oldPwd, updated.Password);
                Assert.True(updated.PasswordExpired);
                Assert.Equal("Password Reset", dialog.LastInfoTitle);
                Assert.Equal("Password has been reset. The user must change it at next login.", dialog.LastInfoMessage);
            }
            finally
            {
                if (File.Exists(dbPath))
                    File.Delete(dbPath);
            }
        }

        [Fact]
        public async Task SearchAndClearUsers_WorkAsExpected()
        {
            var svc = new InMemoryUserService();
            await svc.AddUserAsync(new User { UserName = "alice", Password = "pw" });
            await svc.AddUserAsync(new User { UserName = "bob", Password = "pw" });
            var vm = new UserManagementViewModel(svc, new StubFileDialogService(), new StubDialogService());
            await vm.LoadUsersAsync();

            vm.UserSearchText = "alice";
            vm.SearchUsersCommand.Execute(null);
            Assert.Single(vm.Users);
            Assert.Equal("alice", vm.Users.First().UserName);

            vm.ClearUserSearchCommand.Execute(null);
            Assert.Equal(2, vm.Users.Count);
        }

        [Fact]
        public async Task DeleteUserFromRowCommand_RemovesUser()
        {
            var svc = new InMemoryUserService();
            await svc.AddUserAsync(new User { UserName = "user1", Password = "pw" });
            await svc.AddUserAsync(new User { UserName = "user2", Password = "pw" });
            var vm = new UserManagementViewModel(svc, new StubFileDialogService(), new StubDialogService());
            await vm.LoadUsersAsync();
            var toDelete = vm.Users.First();
            await vm.DeleteUserFromRowCommand.ExecuteAsync(toDelete);
            Assert.Single(vm.Users);
            Assert.Equal("user2", vm.Users.First().UserName);
        }

        [Fact]
        public async Task DeleteUserFromRowCommand_FailureDoesNotRemoveUser()
        {
            var svc = new FailingUserService();
            svc.AddUser(new User { UserName = "user1", Password = "pw" });
            var vm = new UserManagementViewModel(svc, new StubFileDialogService(), new StubDialogService());
            await vm.LoadUsersAsync();
            var toDelete = vm.Users.First();
            await vm.DeleteUserFromRowCommand.ExecuteAsync(toDelete);
            Assert.Single(vm.Users);
        }

        [Fact]
        public async Task AddUserAsync_CancelledPrompt_DoesNotAddUser()
        {
            var svc = new InMemoryUserService();
            var vm = new CancelPromptUserManagementViewModel(svc, new StubFileDialogService());
            await vm.AddUserAsync();
            Assert.Empty(vm.Users);
            Assert.Empty(svc.Users);
        }

        [Fact]
        public void RentToolPopupWindow_RequestCloseHandler_DoesNotLeak()
        {
            WeakReference? winRef = null;
            bool? aliveBeforeUnsubscribe = null;
            Exception? threadException = null;

            var thread = new Thread(() =>
            {
                try
                {
                    var vm = new RentToolPopupViewModel(new ToolModel(), new List<CustomerModel>());
                    var win = new RentToolPopupWindow { DataContext = vm };
                    var captured = win;
                    EventHandler handler = (_, _) => captured.Close();
                    vm.RequestClose += handler;
                    winRef = new WeakReference(captured);
                    win = null;

                    GC.Collect();
                    GC.WaitForPendingFinalizers();
                    GC.Collect();
                    aliveBeforeUnsubscribe = winRef.IsAlive;

                    vm.RequestClose -= handler;
                    captured = null;

                    GC.Collect();
                    GC.WaitForPendingFinalizers();
                    GC.Collect();
                }
                catch (Exception ex)
                {
                    threadException = ex;
                }
            });

            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            thread.Join();

            if (threadException != null)
            {
                throw threadException;
            }

            Assert.True(aliveBeforeUnsubscribe);

            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();

            Assert.False(winRef!.IsAlive);
        }
    }
}

class StubFileDialogService : IFileDialogService
{
    public string FileToReturn { get; set; }
    public string OpenFile(string filter) => FileToReturn;
    public string SaveFile(string filter) => FileToReturn;
}

class StubDialogService : IDialogService
{
    public string? LastInfoMessage { get; private set; }
    public string? LastInfoTitle { get; private set; }
    public void ShowInfo(string message, string title)
    {
        LastInfoMessage = message;
        LastInfoTitle = title;
    }
    public bool ShowConfirmation(string message, string title) => false;
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

class InMemoryUserService : IUserService
{
    public List<User> Users { get; } = new();
    public List<User> GetAllUsers() => Users;
    public Task<List<User>> GetAllUsersAsync() => Task.FromResult(Users.ToList());
    public User? GetUserByID(int userID) => Users.FirstOrDefault(u => u.UserID == userID);
    public Task<User?> GetUserByIDAsync(int userID) => Task.FromResult(GetUserByID(userID));
    public User? AuthenticateUser(string userName, string password) => null;
    public Task<User?> AuthenticateUserAsync(string userName, string password) => Task.FromResult<User?>(null);
    public User? GetCurrentUser() => null;
    public Task<User?> GetCurrentUserAsync() => Task.FromResult<User?>(null);
    public void AddUser(User user)
    {
        user.UserID = Users.Count == 0 ? 1 : Users.Max(u => u.UserID) + 1;
        Users.Add(user);
    }
    public Task AddUserAsync(User user) { AddUser(user); return Task.CompletedTask; }
    public void UpdateUser(User user)
    {
        var idx = Users.FindIndex(u => u.UserID == user.UserID);
        if (idx >= 0) Users[idx] = user;
    }
    public Task UpdateUserAsync(User user) { UpdateUser(user); return Task.CompletedTask; }
    public Task<bool> TryDeleteUserAsync(int userID)
    {
        var u = Users.FirstOrDefault(x => x.UserID == userID);
        if (u != null) { Users.Remove(u); return Task.FromResult(true); }
        return Task.FromResult(false);
    }
    public bool ChangeUserPassword(int userID, string newPassword) => false;
    public Task<bool> ChangeUserPasswordAsync(int userID, string newPassword) => Task.FromResult(false);
}

class PromptUserManagementViewModel : UserManagementViewModel
{
    readonly string _password;
    public PromptUserManagementViewModel(IUserService svc, string password)
        : base(svc, new StubFileDialogService(), new StubDialogService())
    {
        _password = password;
    }

    protected override bool TryPromptForPassword(UserModel newUser, out string password)
    {
        password = _password;
        return true;
    }
}

class FailingUserService : IUserService
{
    readonly List<User> _users = new();
    public List<User> GetAllUsers() => _users;
    public Task<List<User>> GetAllUsersAsync() => Task.FromResult(_users);
    public User? GetUserByID(int userID) => _users.FirstOrDefault(u => u.UserID == userID);
    public Task<User?> GetUserByIDAsync(int userID) => Task.FromResult(GetUserByID(userID));
    public User? AuthenticateUser(string userName, string password) => null;
    public Task<User?> AuthenticateUserAsync(string userName, string password) => Task.FromResult<User?>(null);
    public User? GetCurrentUser() => null;
    public Task<User?> GetCurrentUserAsync() => Task.FromResult<User?>(null);
    public void AddUser(User user) => _users.Add(user);
    public Task AddUserAsync(User user) { _users.Add(user); return Task.CompletedTask; }
    public void UpdateUser(User user) => throw new Exception("update failed");
    public Task UpdateUserAsync(User user) => Task.FromException(new Exception("update failed"));
    public Task<bool> TryDeleteUserAsync(int userID) => Task.FromResult(false);
    public bool ChangeUserPassword(int userID, string newPassword) => false;
    public Task<bool> ChangeUserPasswordAsync(int userID, string newPassword) => Task.FromResult(false);
}

class GetAllUsersFailingUserService : IUserService
{
    public List<User> GetAllUsers() => throw new Exception("load failed");
    public Task<List<User>> GetAllUsersAsync() => Task.FromException<List<User>>(new Exception("load failed"));
    public User? GetUserByID(int userID) => null;
    public Task<User?> GetUserByIDAsync(int userID) => Task.FromResult<User?>(null);
    public User? AuthenticateUser(string userName, string password) => null;
    public Task<User?> AuthenticateUserAsync(string userName, string password) => Task.FromResult<User?>(null);
    public User? GetCurrentUser() => null;
    public Task<User?> GetCurrentUserAsync() => Task.FromResult<User?>(null);
    public void AddUser(User user) { }
    public Task AddUserAsync(User user) => Task.CompletedTask;
    public void UpdateUser(User user) { }
    public Task UpdateUserAsync(User user) => Task.CompletedTask;
    public Task<bool> TryDeleteUserAsync(int userID) => Task.FromResult(false);
    public bool ChangeUserPassword(int userID, string newPassword) => false;
    public Task<bool> ChangeUserPasswordAsync(int userID, string newPassword) => Task.FromResult(false);
}

class CancelPromptUserManagementViewModel : UserManagementViewModel
{
    public CancelPromptUserManagementViewModel(IUserService userService, IFileDialogService fileDialogService)
        : base(userService, fileDialogService, new StubDialogService()) { }

    protected override bool TryPromptForPassword(UserModel newUser, out string password)
    {
        password = null;
        return false;
    }
}
