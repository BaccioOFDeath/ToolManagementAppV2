using ToolManagementAppV2.Interfaces;

namespace ToolManagementAppV2.Services.Users
{
    public class NoOpAuthorizationService : IAuthorizationService
    {
        public void EnsureAdmin() { }
    }
}
