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
        public void DeleteUserCommand_RemovesUser()
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
                vm.DeleteUserCommand.Execute(null);
                Assert.Empty(userService.GetAllUsers());
                Assert.Empty(vm.Users);
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
                Assert.False(vm.DeleteUserCommand.CanExecute(null));
                Assert.False(vm.ResetPasswordCommand.CanExecute(null));
                vm.SelectedUser = vm.Users.First();
                Assert.True(vm.UpdateUserCommand.CanExecute(null));
                Assert.True(vm.DeleteUserCommand.CanExecute(null));
                Assert.True(vm.ResetPasswordCommand.CanExecute(null));
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
        public void ResetPasswordCommand_ChangesPassword()
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
                var oldPwd = vm.SelectedUser.Password;
                vm.ResetPasswordCommand.Execute(null);
                var updated = userService.GetAllUsers().First();
                Assert.NotEqual(oldPwd, updated.Password);
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
}
