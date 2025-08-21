using InventoryManagementApp.Interfaces;

namespace InventoryManagementApp.Tests
{
    public class AllowAllAuthorizationService : IAuthorizationService
    {
        public void EnsureAdmin() { }
    }
}
