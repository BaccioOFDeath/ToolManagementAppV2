using System;
using System.Linq;
using System.Threading;
using System.Windows.Controls;
using System.Reflection;
using ToolManagementAppV2.Interfaces;
using ToolManagementAppV2.Models.Domain;
using ToolManagementAppV2.Services.Core;
using ToolManagementAppV2.Services.Users;
using ToolManagementAppV2.ViewModels;
using ToolManagementAppV2.Views;
using Xunit;

namespace ToolManagementAppV2.Tests.Views
{
    public class UsersPageTests
    {
        [Fact]
        public void ButtonsExecuteCommandsAndUpdateCollections()
        {
            var dbPath = System.IO.Path.GetTempFileName();
            Exception? threadException = null;

            try
            {
                var thread = new Thread(() =>
                {
                    try
                    {
                        var db = new DatabaseService(dbPath);
                        IUserService userService = new UserService(db);
                        var fileSvc = new StubFileDialogService { FileToReturn = "img.png" };
                        var vm = new UserManagementViewModel(userService, fileSvc);
                        userService.AddUser(new User { UserName = "user1", Password = "pw" });
                        vm.LoadUsers();
                        vm.SelectedUser = vm.Users.First();
                        vm.SelectedUser.Email = "user@example.com";

                        var page = new UsersPage { DataContext = vm };
                        var updateBtn = (Button)page.FindName("UpdateUserButton");
                        var uploadBtn = (Button)page.FindName("UploadPhotoButton");

                        Assert.Equal(vm.UpdateUserCommand, updateBtn.Command);
                        Assert.Equal(vm.UploadUserPhotoCommand, uploadBtn.Command);

                        updateBtn.Command.Execute(null);
                        uploadBtn.Command.Execute(null);

                        var field = typeof(UserManagementViewModel).GetField("_allUsers", BindingFlags.NonPublic | BindingFlags.Instance);
                        var allUsers = (System.Collections.Generic.List<User>)field!.GetValue(vm);

                        Assert.Equal("user@example.com", vm.Users.First().Email);
                        Assert.Equal("user@example.com", allUsers.First().Email);
                        Assert.Equal("img.png", vm.Users.First().UserPhotoPath);
                        Assert.Equal("img.png", allUsers.First().UserPhotoPath);
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
            }
            finally
            {
                if (System.IO.File.Exists(dbPath))
                    System.IO.File.Delete(dbPath);
            }
        }
    }

    class StubFileDialogService : IFileDialogService
    {
        public string FileToReturn { get; set; }
        public string OpenFile(string filter) => FileToReturn;
        public string SaveFile(string filter) => FileToReturn;
    }
}
