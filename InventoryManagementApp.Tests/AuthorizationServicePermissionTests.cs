using System;
using InventoryManagementApp.Interfaces;
using InventoryManagementApp.Models.Domain;
using InventoryManagementApp.Services.Users;
using Xunit;

public class AuthorizationServicePermissionTests
{
    private sealed class TestUserContext : IUserContext
    {
        private User? _currentUser;

        public User? CurrentUser
        {
            get => _currentUser;
            set
            {
                _currentUser = value;
                UserChanged?.Invoke(this, value);
            }
        }

        public event EventHandler<User?>? UserChanged;

        public bool IsAdmin => CurrentUser?.IsAdmin == true;
        public string UserName => CurrentUser?.UserName ?? string.Empty;
        public string Role => CurrentUser?.Role ?? string.Empty;
    }

    [Fact]
    public void EnsurePermission_AllowsExplicitCheckboxPermission()
    {
        var context = new TestUserContext
        {
            CurrentUser = new User
            {
                UserName = "advisor",
                Permissions = User.BuildPermissions(new[] { User.PermissionRentals })
            }
        };
        var auth = new AuthorizationService(context);

        auth.EnsurePermission(User.PermissionRentals);

        Assert.True(auth.HasPermission(User.PermissionRentals));
    }

    [Fact]
    public void EnsurePermission_BlocksUncheckedPermission()
    {
        var context = new TestUserContext
        {
            CurrentUser = new User
            {
                UserName = "advisor",
                Permissions = User.BuildPermissions(new[] { User.PermissionRentals })
            }
        };
        var auth = new AuthorizationService(context);

        var ex = Assert.Throws<UnauthorizedAccessException>(() => auth.EnsurePermission(User.PermissionImportExport));

        Assert.Contains("Import / export", ex.Message);
    }

    [Fact]
    public void EnsureAnyPermission_AllowsAnyCheckedPermission()
    {
        var context = new TestUserContext
        {
            CurrentUser = new User
            {
                UserName = "technician",
                Permissions = User.BuildPermissions(new[] { User.PermissionMaintenance })
            }
        };
        var auth = new AuthorizationService(context);

        auth.EnsureAnyPermission(User.PermissionRentals, User.PermissionMaintenance);

        Assert.True(auth.HasAnyPermission(User.PermissionRentals, User.PermissionMaintenance));
    }

    [Fact]
    public void FullAdmin_PassesEveryPermissionCheck()
    {
        var context = new TestUserContext
        {
            CurrentUser = new User
            {
                UserName = "admin",
                IsAdmin = true,
                Permissions = User.BuildPermissions(Array.Empty<string>())
            }
        };
        var auth = new AuthorizationService(context);

        auth.EnsurePermission(User.PermissionSettings);
        auth.EnsureAnyPermission(User.PermissionManageUsers, User.PermissionImportExport);

        Assert.True(auth.HasPermission(User.PermissionSettings));
    }
}
