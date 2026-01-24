using System;
using System.Threading;
using System.Windows;
using InventoryManagementApp.Models.Domain;
using InventoryManagementApp.Services.Users;
using Xunit;

namespace InventoryManagementApp.Tests
{
    public class ApplicationUserContextTests
    {
        [Fact]
        public void UserChanged_Fires_WhenCurrentUserPropertyChanges()
        {
            Exception? threadEx = null;
            var thread = new Thread(() =>
            {
                try
                {
                    var app = WpfTestHelper.CreateApplication();
                    var ctx = new ApplicationUserContext();
                    var user = new User();
                    int eventCount = 0;
                    User? eventUser = null;
                    ctx.UserChanged += (_, u) => { eventCount++; eventUser = u; };

                    ctx.CurrentUser = user;
                    eventCount = 0;
                    eventUser = null;

                    user.UserPhotoPath = "new.png";

                    Assert.Equal(1, eventCount);
                    Assert.Same(user, eventUser);
                }
                catch (Exception ex)
                {
                    threadEx = ex;
                }
                finally
                {
                    WpfTestHelper.ShutdownApplication();
                }
            });
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            thread.Join();
            if (threadEx != null) throw threadEx;
        }

        [Fact]
        public void UserChanged_DoesNotFire_ForPreviousUser()
        {
            Exception? threadEx = null;
            var thread = new Thread(() =>
            {
                try
                {
                    var app = WpfTestHelper.CreateApplication();
                    var ctx = new ApplicationUserContext();
                    var user1 = new User();
                    var user2 = new User();
                    bool fired = false;
                    ctx.UserChanged += (_, __) => fired = true;

                    ctx.CurrentUser = user1;
                    ctx.CurrentUser = user2; // switch users
                    fired = false; // reset flag after setting user2

                    user1.UserPhotoPath = "old.png";

                    Assert.False(fired);
                }
                catch (Exception ex)
                {
                    threadEx = ex;
                }
                finally
                {
                    WpfTestHelper.ShutdownApplication();
                }
            });
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            thread.Join();
            if (threadEx != null) throw threadEx;
        }
    }
}
