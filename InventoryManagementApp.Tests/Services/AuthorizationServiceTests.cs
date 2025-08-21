using System;
using Microsoft.Extensions.Logging.Abstractions;
using InventoryManagementApp.Interfaces;
using InventoryManagementApp.Models.Domain;
using InventoryManagementApp.Services.Users;
using Xunit;

namespace InventoryManagementApp.Tests.Services
{
    public class AuthorizationServiceTests
    {
        class StubUserContext : IUserContext
        {
            public User? CurrentUser { get; set; }
            public event EventHandler<User?>? UserChanged;
            public bool IsAdmin => CurrentUser?.IsAdmin ?? false;
            public string UserName => CurrentUser?.UserName ?? string.Empty;
            public string Role => CurrentUser?.Role ?? string.Empty;
        }

        [Fact]
        public void EnsureAdmin_NonAdmin_Throws()
        {
            var ctx = new StubUserContext { CurrentUser = new User { UserName = "user", IsAdmin = false } };
            var svc = new AuthorizationService(ctx, NullLogger<AuthorizationService>.Instance);
            Assert.Throws<UnauthorizedAccessException>(() => svc.EnsureAdmin());
        }

        [Fact]
        public void EnsureAdmin_Admin_DoesNotThrow()
        {
            var ctx = new StubUserContext { CurrentUser = new User { UserName = "admin", IsAdmin = true } };
            var svc = new AuthorizationService(ctx, NullLogger<AuthorizationService>.Instance);
            svc.EnsureAdmin();
        }
    }
}
