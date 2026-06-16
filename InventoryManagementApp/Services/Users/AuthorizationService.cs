using System;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using InventoryManagementApp.Interfaces;
using InventoryManagementApp.Models.Domain;

namespace InventoryManagementApp.Services.Users
{
    public class AuthorizationService : IAuthorizationService
    {
        private readonly IUserContext _userContext;
        private readonly ILogger<AuthorizationService> _logger;

        public AuthorizationService(IUserContext userContext, ILogger<AuthorizationService>? logger = null)
        {
            _userContext = userContext;
            _logger = logger ?? NullLogger<AuthorizationService>.Instance;
        }

        public bool IsAdmin => HasElevatedAccess();

        public void EnsureAdmin()
        {
            if (!HasElevatedAccess())
            {
                _logger.LogWarning("Unauthorized access attempt by {User}", _userContext.UserName);
                throw new UnauthorizedAccessException("Admin privileges required.");
            }
        }

        bool HasElevatedAccess()
        {
            var user = _userContext.CurrentUser;
            return user?.IsAdmin == true || user?.HasAnyPermission(
                User.PermissionManageItems,
                User.PermissionImportExport,
                User.PermissionManageUsers,
                User.PermissionSettings) == true;
        }
    }
}
