using System;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using InventoryManagementApp.Interfaces;

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

        public void EnsureAdmin()
        {
            if (!_userContext.IsAdmin)
            {
                _logger.LogWarning("Unauthorized access attempt by {User}", _userContext.UserName);
                throw new UnauthorizedAccessException("Admin privileges required.");
            }
        }
    }
}
