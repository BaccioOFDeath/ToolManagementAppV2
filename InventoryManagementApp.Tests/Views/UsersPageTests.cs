using System;
using System.Linq;
using System.Threading;
using System.Windows.Controls;
using System.Reflection;
using System.Collections.Generic;
using System.IO;
using InventoryManagementApp.Interfaces;
using InventoryManagementApp.Models.Domain;
using InventoryManagementApp.Services.Core;
using InventoryManagementApp.Services.Users;
using InventoryManagementApp.ViewModels;
using InventoryManagementApp.Views.Pages;
using InventoryManagementApp.Views.Windows;
using Xunit;

namespace InventoryManagementApp.Tests.Views
{
    public class UsersPageTests
    {
        [Fact]
        public void Constructor_AllowsNullDataContext()
        {
            Exception? threadException = null;

            var thread = new Thread(() =>
            {
                try
                {
                    var page = new UsersPage(null);
                    Assert.Null(page.ViewModel);
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
                        IUserService userService = new UserService(db, new ApplicationUserContext());
                        var fileSvc = new StubFileDialogService { FileToReturn = "img.png" };
                        var vm = new UserManagementViewModel(userService, fileSvc, new StubDialogService());
                        userService.AddUser(new User { UserName = "user1", PasswordHash = "Strong1!" });
                        vm.LoadUsers();
                        vm.SelectedUser = vm.Users.First();
                        vm.SelectedUser.Email = "user@example.com";

                        var page = new UsersPage(vm);
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

        [Fact]
        public void ContextMenuDeleteCommand_RemovesUser()
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
                        IUserService userService = new UserService(db, new ApplicationUserContext());
                        var vm = new UserManagementViewModel(userService, new StubFileDialogService(), new StubDialogService());
                        userService.AddUser(new User { UserName = "user1", PasswordHash = "Strong1!" });
                        userService.AddUser(new User { UserName = "user2", PasswordHash = "Strong1!" });
                        vm.LoadUsers();

                        var page = new UsersPage(vm);
                        var grid = (Grid)page.Content;
                        var dataGrid = (DataGrid)((Border)grid.Children[1]).Child;

                        dataGrid.SelectedItem = vm.Users.First();
                        var deleteItem = (MenuItem)dataGrid.ContextMenu.Items[4];

                        Assert.Equal(vm.DeleteUserFromRowCommand, deleteItem.Command);
                        deleteItem.Command.Execute(dataGrid.SelectedItem);

                        Assert.Single(vm.Users);
                        Assert.Equal("user2", vm.Users.First().UserName);
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

        [Fact]
        public void ContextMenuEditCommand_BindsSelectedItem()
        {
            var dbPath = Path.GetTempFileName();
            Exception? threadException = null;

            try
            {
                var thread = new Thread(() =>
                {
                    try
                    {
                        var db = new DatabaseService(dbPath);
                        IUserService userService = new UserService(db, new ApplicationUserContext());
                        var vm = new UserManagementViewModel(userService, new StubFileDialogService(), new StubDialogService());
                        userService.AddUser(new User { UserName = "user1", PasswordHash = "Strong1!" });
                        vm.LoadUsers();

                        var page = new UsersPage(vm);
                        var grid = (Grid)page.Content;
                        var dataGrid = (DataGrid)((Border)grid.Children[1]).Child;

                        dataGrid.SelectedItem = vm.Users.First();
                        dataGrid.ContextMenu.PlacementTarget = dataGrid;
                        dataGrid.ContextMenu.IsOpen = true;

                        var editItem = (MenuItem)dataGrid.ContextMenu.Items[0];

                        Assert.Equal(vm.Users.First(), editItem.CommandParameter);

                        dataGrid.ContextMenu.IsOpen = false;
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
                if (File.Exists(dbPath))
                    File.Delete(dbPath);
            }
        }

        [Fact]
        public void CreatedAt_Null_ShowsNA()
        {
            var dbPath = Path.GetTempFileName();
            Exception? threadException = null;

            try
            {
                var thread = new Thread(() =>
                {
                    try
                    {
                        var db = new DatabaseService(dbPath);
                        IUserService userService = new UserService(db, new ApplicationUserContext());
                        var vm = new UserManagementViewModel(userService, new StubFileDialogService(), new StubDialogService());
                        userService.AddUser(new User { UserName = "user1", PasswordHash = "Strong1!" });
                        vm.LoadUsers();
                        vm.Users.First().CreatedAt = null;

                        var page = new UsersPage(vm);
                        var grid = (Grid)page.Content;
                        var dataGrid = (DataGrid)((Border)grid.Children[1]).Child;
                        dataGrid.UpdateLayout();
                        var text = ((TextBlock)dataGrid.Columns[7].GetCellContent(dataGrid.Items[0])).Text;
                        Assert.Equal("N/A", text);
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
                if (File.Exists(dbPath))
                    File.Delete(dbPath);
            }
        }
    }

    class StubFileDialogService : IFileDialogService
    {
        public string FileToReturn { get; set; }
        public string OpenFile(string filter, string? initialDirectory = null) => FileToReturn;
        public string SaveFile(string filter) => FileToReturn;
    }

    class StubDialogService : IDialogService
    {
        public void ShowInfo(string message, string title) { }
        public bool ShowConfirmation(string message, string title) => false;
        public ItemModel? ShowEditItemDialog(ItemModel item) => null;
        public void ShowItemDetails(ItemModel item) { }
        public (CustomerModel customer, DateTime dueDate)? ShowRentItemDialog(ItemModel item, IEnumerable<CustomerModel> customers) => null;
        public CustomerModel? ShowAddCustomerDialog() => null;
        public void ShowRentalsFilter(InventoryManagementApp.ViewModels.ManageRentalsViewModel viewModel) { }
        public void ShowRentalHistory(ItemModel item, System.Collections.Generic.IEnumerable<RentalModel> history) { }
        public System.Collections.Generic.Dictionary<string, string>? ShowImportMapping(System.Collections.Generic.IEnumerable<string> headers, System.Collections.Generic.IEnumerable<string> properties, System.Collections.Generic.IEnumerable<string>? requiredPropertyNames = null) => null;
        public System.Func<ItemModel, System.Collections.Generic.IEnumerable<string>>? ShowImageImportMapping() => null;
        public void ShowPrintPreview(System.Windows.Documents.FlowDocument document, string title, string description) { }
        public void ShowPrintLabelDialog() { }
    }
}
