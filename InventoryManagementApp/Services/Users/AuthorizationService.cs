using System;
using System.Linq;
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

        public bool IsAdmin => _userContext.CurrentUser?.IsAdmin == true;

        public bool HasPermission(string permissionKey)
            => _userContext.CurrentUser?.HasPermission(permissionKey) == true;

        public bool HasAnyPermission(params string[] permissionKeys)
            => _userContext.CurrentUser?.HasAnyPermission(permissionKeys) == true;

        public void EnsureAdmin()
        {
            if (!IsAdmin)
            {
                _logger.LogWarning("Unauthorized admin-only access attempt by {User}", _userContext.UserName);
                throw new UnauthorizedAccessException("Full admin access is required.");
            }
        }

        public void EnsurePermission(string permissionKey)
        {
            if (IsAdmin || HasPermission(permissionKey))
                return;

            var label = User.PermissionLabels.TryGetValue(permissionKey, out var display)
                ? display
                : permissionKey;
            _logger.LogWarning("Unauthorized {Permission} access attempt by {User}", label, _userContext.UserName);
            throw new UnauthorizedAccessException($"The '{label}' permission is required.");
        }

        public void EnsureAnyPermission(params string[] permissionKeys)
        {
            if (IsAdmin || HasAnyPermission(permissionKeys))
                return;

            var labels = permissionKeys
                .Select(key => User.PermissionLabels.TryGetValue(key, out var display) ? display : key)
                .ToList();
            var required = labels.Count == 0 ? "required" : string.Join(" or ", labels);
            _logger.LogWarning("Unauthorized access attempt by {User}. Required: {Permissions}", _userContext.UserName, required);
            throw new UnauthorizedAccessException($"The {required} permission is required.");
        }
    }
}
