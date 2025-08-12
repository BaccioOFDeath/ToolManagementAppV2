using System.IO;
using System.Linq;
using ToolManagementAppV2.Models.Domain;
using ToolManagementAppV2.Services.Core;
using ToolManagementAppV2.Services.Users;
using ToolManagementAppV2.Interfaces;
using ToolManagementAppV2.ViewModels;
using Xunit;

namespace ToolManagementAppV2.Tests.ViewModels
{
    public class UserManagementViewModelTests
    {
        [Fact]
        public void UpdateUserCommand_PersistsChanges()
        {
            var dbPath = Path.GetTempFileName();
            try
            {
                var db = new DatabaseService(dbPath);
                IUserService userService = new UserService(db);
                var vm = new UserManagementViewModel(userService, new StubFileDialogService());
                userService.AddUser(new User { UserName = "user1", Password = "pw" });
                vm.LoadUsers();
                vm.SelectedUser = vm.Users.First();
                vm.SelectedUser.Email = "test@example.com";
                vm.UpdateUserCommand.Execute(null);
                var updated = userService.GetAllUsers().First();
                Assert.Equal("test@example.com", updated.Email);
            }
            finally
            {
                if (File.Exists(dbPath))
                    File.Delete(dbPath);
            }
        }

        [Fact]
        public void UploadUserPhotoCommand_SetsPhotoPathAndPersists()
        {
            var dbPath = Path.GetTempFileName();
            try
            {
                var db = new DatabaseService(dbPath);
                IUserService userService = new UserService(db);
                var fileSvc = new StubFileDialogService { FileToReturn = "path/to/image.png" };
                var vm = new UserManagementViewModel(userService, fileSvc);
                userService.AddUser(new User { UserName = "user1", Password = "pw" });
                vm.LoadUsers();
                vm.SelectedUser = vm.Users.First();
                vm.UploadUserPhotoCommand.Execute(null);
                var updated = userService.GetAllUsers().First();
                Assert.Equal("path/to/image.png", updated.UserPhotoPath);
            }
            finally
            {
                if (File.Exists(dbPath))
                    File.Delete(dbPath);
            }
        }

        [Fact]
        public void CommandsDisabledWhenNoUserSelected()
        {
            var dbPath = Path.GetTempFileName();
            try
            {
                var db = new DatabaseService(dbPath);
                IUserService userService = new UserService(db);
                var vm = new UserManagementViewModel(userService, new StubFileDialogService());
                userService.AddUser(new User { UserName = "user1", Password = "pw" });
                vm.LoadUsers();
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
        public void AddUserCommand_AddsUser()
        {
            var dbPath = Path.GetTempFileName();
            try
            {
                var db = new DatabaseService(dbPath);
                IUserService userService = new UserService(db);
                var vm = new UserManagementViewModel(userService, new StubFileDialogService());
                vm.AddUserCommand.Execute(null);
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
        public void ResetPasswordFromRowCommand_ChangesPassword()
        {
            var dbPath = Path.GetTempFileName();
            try
            {
                var db = new DatabaseService(dbPath);
                IUserService userService = new UserService(db);
                var vm = new UserManagementViewModel(userService, new StubFileDialogService());
                userService.AddUser(new User { UserName = "user1", Password = "pw" });
                vm.LoadUsers();
                var user = vm.Users.First();
                var oldPwd = user.Password;
                vm.ResetPasswordFromRowCommand.Execute(user);
                var updated = userService.GetAllUsers().First();
                Assert.NotEqual(oldPwd, updated.Password);
            }
            finally
            {
                if (File.Exists(dbPath))
                    File.Delete(dbPath);
            }
        }

        [Fact]
        public void SearchAndClearUsers_WorkAsExpected()
        {
            var dbPath = Path.GetTempFileName();
            try
            {
                var db = new DatabaseService(dbPath);
                IUserService userService = new UserService(db);
                var vm = new UserManagementViewModel(userService, new StubFileDialogService());
                userService.AddUser(new User { UserName = "alice", Password = "pw" });
                userService.AddUser(new User { UserName = "bob", Password = "pw" });
                vm.LoadUsers();

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
        public void DeleteUserFromRowCommand_RemovesUser()
        {
            var dbPath = Path.GetTempFileName();
            try
            {
                var db = new DatabaseService(dbPath);
                IUserService userService = new UserService(db);
                var vm = new UserManagementViewModel(userService, new StubFileDialogService());
                userService.AddUser(new User { UserName = "user1", Password = "pw" });
                userService.AddUser(new User { UserName = "user2", Password = "pw" });
                vm.LoadUsers();
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
    }
}

class StubFileDialogService : IFileDialogService
{
    public string FileToReturn { get; set; }
    public string OpenFile(string filter) => FileToReturn;
    public string SaveFile(string filter) => FileToReturn;
}
