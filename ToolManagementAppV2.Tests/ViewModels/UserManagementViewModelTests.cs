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
                Assert.Equal("path/to/image.png", updated.UserPhotoPath);
                var field = typeof(UserManagementViewModel).GetField("_allUsers", BindingFlags.NonPublic | BindingFlags.Instance);
                var allUsers = (System.Collections.Generic.List<User>)field!.GetValue(vm);
                Assert.Equal("path/to/image.png", vm.Users.First().UserPhotoPath);
                Assert.Equal("path/to/image.png", allUsers.First().UserPhotoPath);
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

                Assert.Equal("test@example.com", vm.Users.First().Email);
                Assert.Equal("test@example.com", allUsers.First().Email);
                Assert.Equal("img.png", vm.Users.First().UserPhotoPath);
                Assert.Equal("img.png", allUsers.First().UserPhotoPath);
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
            var dbPath = Path.GetTempFileName();
            try
            {
                var db = new DatabaseService(dbPath);
                IUserService userService = new UserService(db, new ApplicationUserContext());
                var vm = new UserManagementViewModel(userService, new StubFileDialogService(), new StubDialogService());
                userService.AddUser(new User { UserName = "user1", Password = "pw" });
                await vm.LoadUsersAsync();
                Assert.False(vm.UpdateUserCommand.CanExecute(null));
                Assert.False(vm.EditUserCommand.CanExecute(null));
                vm.SelectedUser = vm.Users.First();
                Assert.True(vm.UpdateUserCommand.CanExecute(null));
                Assert.True(vm.EditUserCommand.CanExecute(null));
            }
            finally
            {
                if (File.Exists(dbPath))
                    File.Delete(dbPath);
            }
        }

        [Fact]
        public async Task AddUserCommand_AddsUser()
        {
            var dbPath = Path.GetTempFileName();
            try
            {
                var db = new DatabaseService(dbPath);
                IUserService userService = new UserService(db, new ApplicationUserContext());
                var vm = new UserManagementViewModel(userService, new StubFileDialogService(), new StubDialogService());
                await vm.AddUserCommand.ExecuteAsync(null);
                Assert.Single(vm.Users);
                Assert.Single(userService.GetAllUsers());
            }
            finally
            {
                if (File.Exists(dbPath))
                    File.Delete(dbPath);
            }
        }

        [Fact]
        public async Task AddUserCommand_SkipsExistingNameFromService()
        {
            var dbPath = Path.GetTempFileName();
            try
            {
                var db = new DatabaseService(dbPath);
                IUserService userService = new UserService(db, new ApplicationUserContext());
                // Pre-populate the database with a user named "user1"
                userService.AddUser(new User { UserName = "user1", Password = "pw" });

                // The view model has not loaded users, so its local collection is empty
                var vm = new UserManagementViewModel(userService, new StubFileDialogService(), new StubDialogService());
                await vm.AddUserCommand.ExecuteAsync(null);

                // Ensure a second user was added with an incremented name
                var all = userService.GetAllUsers();
                Assert.Equal(2, all.Count);
                Assert.Contains(all, u => u.UserName == "user2");
                Assert.Single(vm.Users);
                Assert.Equal("user2", vm.Users.First().UserName);
            }
            finally
            {
                if (File.Exists(dbPath))
                    File.Delete(dbPath);
            }
        }

        [Fact]
        public async Task AddUserCommand_FindsFirstAvailableNumber()
        {
            var dbPath = Path.GetTempFileName();
            try
            {
                var db = new DatabaseService(dbPath);
                IUserService userService = new UserService(db, new ApplicationUserContext());
                // Create a gap: user1 and user3 exist
                userService.AddUser(new User { UserName = "user1", Password = "pw" });
                userService.AddUser(new User { UserName = "user3", Password = "pw" });

                var vm = new UserManagementViewModel(userService, new StubFileDialogService(), new StubDialogService());
                await vm.AddUserCommand.ExecuteAsync(null);

                var all = userService.GetAllUsers();
                Assert.Equal(3, all.Count);
                Assert.Contains(all, u => u.UserName == "user2");
                Assert.Single(vm.Users);
                Assert.Equal("user2", vm.Users.First().UserName);
            }
            finally
            {
                if (File.Exists(dbPath))
                    File.Delete(dbPath);
            }
        }

        [Fact]
        public async Task ResetPasswordFromRowCommand_ChangesPassword()
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
                vm.ResetPasswordFromRowCommand.Execute(user);
                var updated = userService.GetUserByID(user.UserID)!;
                Assert.NotEqual(oldPwd, updated.Password);
                Assert.Equal("Password Reset", dialog.LastInfoTitle);
                Assert.StartsWith("Password reset to: ", dialog.LastInfoMessage);
                var prefix = "Password reset to: ";
                var newPwd = dialog.LastInfoMessage.Substring(prefix.Length);
                Assert.True(SecurityHelper.VerifyPassword(newPwd, updated.Salt, updated.Password));
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
            var dbPath = Path.GetTempFileName();
            try
            {
                var db = new DatabaseService(dbPath);
                IUserService userService = new UserService(db, new ApplicationUserContext());
                var vm = new UserManagementViewModel(userService, new StubFileDialogService(), new StubDialogService());
                userService.AddUser(new User { UserName = "alice", Password = "pw" });
                userService.AddUser(new User { UserName = "bob", Password = "pw" });
                await vm.LoadUsersAsync();

                vm.UserSearchText = "alice";
                vm.SearchUsersCommand.Execute(null);
                Assert.Single(vm.Users);
                Assert.Equal("alice", vm.Users.First().UserName);

                vm.ClearUserSearchCommand.Execute(null);
                Assert.Equal(2, vm.Users.Count);
            }
            finally
            {
                if (File.Exists(dbPath))
                    File.Delete(dbPath);
            }
        }

        [Fact]
        public async Task DeleteUserFromRowCommand_RemovesUser()
        {
            var dbPath = Path.GetTempFileName();
            try
            {
                var db = new DatabaseService(dbPath);
                IUserService userService = new UserService(db, new ApplicationUserContext());
                var vm = new UserManagementViewModel(userService, new StubFileDialogService(), new StubDialogService());
                userService.AddUser(new User { UserName = "user1", Password = "pw" });
                userService.AddUser(new User { UserName = "user2", Password = "pw" });
                await vm.LoadUsersAsync();
                var toDelete = vm.Users.First();
                vm.DeleteUserFromRowCommand.Execute(toDelete);
                Assert.Single(vm.Users);
                Assert.Equal("user2", vm.Users.First().UserName);
            }
            finally
            {
                if (File.Exists(dbPath))
                    File.Delete(dbPath);
            }
        }

        [Fact]
        public async Task AddUserCommand_CancelledPrompt_DoesNotAddUser()
        {
            var dbPath = Path.GetTempFileName();
            try
            {
                var db = new DatabaseService(dbPath);
                IUserService userService = new UserService(db, new ApplicationUserContext());
                var vm = new CancelPromptUserManagementViewModel(userService, new StubFileDialogService());
                await vm.AddUserCommand.ExecuteAsync(null);
                Assert.Empty(vm.Users);
                Assert.Empty(userService.GetAllUsers());
            }
            finally
            {
                if (File.Exists(dbPath))
                    File.Delete(dbPath);
            }
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
