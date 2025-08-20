using ToolManagementAppV2.Interfaces;
using ToolManagementAppV2.Services;
using ToolManagementAppV2.Services.Customers;
using ToolManagementAppV2.Services.Rentals;
using ToolManagementAppV2.Services.Tools;
using ToolManagementAppV2.Services.Users;
using ToolManagementAppV2.Services.Settings;
using ToolManagementAppV2.Services.Core;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace ToolManagementAppV2.Tests.Tests
{
    public class DependencyInjectionTests
    {
        [Fact]
        public void Services_Are_Resolvable()
        {
            var app = new ToolManagementAppV2.App();
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
            var app = new ToolManagementAppV2.App();
            var provider = app.Host.Services;

            Assert.NotNull(provider.GetService<IMainWindow>());
            Assert.NotNull(provider.GetService<ILoginWindow>());
        }

        [Fact]
        public void LoginWindow_Is_Transient()
        {
            var app = new ToolManagementAppV2.App();
            var provider = app.Host.Services;

            var first = provider.GetRequiredService<ILoginWindow>();
            var second = provider.GetRequiredService<ILoginWindow>();

            Assert.NotSame(first, second);
        }
    }
}
