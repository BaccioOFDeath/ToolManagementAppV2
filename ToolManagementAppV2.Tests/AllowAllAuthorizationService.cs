using ToolManagementAppV2.Interfaces;

namespace ToolManagementAppV2.Tests
{
    public class AllowAllAuthorizationService : IAuthorizationService
    {
        public void EnsureAdmin() { }
    }
}
