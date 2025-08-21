using InventoryManagementApp.Interfaces;
using InventoryManagementApp.Services;
using InventoryManagementApp.Services.Customers;
using InventoryManagementApp.Services.Rentals;
using InventoryManagementApp.Services.Items;
using InventoryManagementApp.Services.Users;
using InventoryManagementApp.Services.Settings;
using InventoryManagementApp.Services.Core;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace InventoryManagementApp.Tests.Tests
{
    public class DependencyInjectionTests
    {
        [Fact]
        public void Services_Are_Resolvable()
        {
            var app = new InventoryManagementApp.App();
            var provider = app.Host.Services;

            Assert.NotNull(provider.GetService<IDatabaseService>());
            Assert.NotNull(provider.GetService<IItemService>());
            Assert.NotNull(provider.GetService<ICustomerService>());
            Assert.NotNull(provider.GetService<IUserService>());
            Assert.NotNull(provider.GetService<IRentalService>());
            Assert.NotNull(provider.GetService<ActivityLogService>());
            Assert.NotNull(provider.GetService<ISettingsService>());
            Assert.NotNull(provider.GetService<IDialogService>());
        }

        [Fact]
        public void Windows_Are_Resolvable()
        {
            var app = new InventoryManagementApp.App();
            var provider = app.Host.Services;

            Assert.NotNull(provider.GetService<IMainWindow>());
            Assert.NotNull(provider.GetService<ILoginWindow>());
        }

        [Fact]
        public void LoginWindow_Is_Transient()
        {
            var app = new InventoryManagementApp.App();
            var provider = app.Host.Services;

            var first = provider.GetRequiredService<ILoginWindow>();
            var second = provider.GetRequiredService<ILoginWindow>();

            Assert.NotSame(first, second);
        }
    }
}
