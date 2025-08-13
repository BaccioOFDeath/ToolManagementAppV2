using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using ToolManagementAppV2.Models.Domain;
using ToolManagementAppV2.Services.Core;
using ToolManagementAppV2.Services.Users;
using ToolManagementAppV2.ViewModels;
using ToolManagementAppV2.Interfaces;
using Xunit;

namespace ToolManagementAppV2.Tests.ViewModels
{
    public class LoginViewModelTests
    {
        [Fact]
        public void SelectUserCommand_SetsCurrentUser()
        {
            if (Application.Current == null)
                new Application();

            var dbPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + ".db");
            try
            {
                var dbService = new DatabaseService(dbPath);
                var userService = new UserService(dbService, new ApplicationUserContext());
                userService.AddUser(new User { UserName = "user", Password = "newpassword", IsAdmin = false });

                var vm = new LoginViewModel(new StubDialogService(), dbPath);
                bool success = false;
                vm.LoginSucceeded += (_, __) => success = true;

                vm.SelectUserCommand.Execute(vm.Users.First());

                Assert.True(success);
                var current = Application.Current.Properties["CurrentUser"] as User;
                Assert.NotNull(current);
                Assert.Equal("user", current.UserName);
            }
            finally
            {
                if (File.Exists(dbPath)) File.Delete(dbPath);
                Application.Current.Properties.Remove("CurrentUser");
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
    }
}
