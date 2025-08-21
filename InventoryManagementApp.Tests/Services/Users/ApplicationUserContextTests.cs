using System;
using System.Windows;
using InventoryManagementApp.Models.Domain;
using InventoryManagementApp.Services.Users;
using Xunit;

namespace InventoryManagementApp.Tests.Services.Users
{
    public class ApplicationUserContextTests
    {
        [Fact]
        public void SettingCurrentUser_RaisesUserChanged()
        {
            if (Application.Current == null)
                new Application();

            var context = new ApplicationUserContext();
            User? raisedUser = null;
            context.UserChanged += (_, u) => raisedUser = u;

            var user = new User { UserName = "test" };
            context.CurrentUser = user;

            Assert.Equal(user, raisedUser);
        }

        [Fact]
        public void SettingCurrentUserToNull_RaisesUserChanged()
        {
            if (Application.Current == null)
                new Application();

            var context = new ApplicationUserContext();
            context.CurrentUser = new User { UserName = "x" };
            User? raisedUser = null;
            context.UserChanged += (_, u) => raisedUser = u;

            context.CurrentUser = null;

            Assert.Null(raisedUser);
        }
    }
}
