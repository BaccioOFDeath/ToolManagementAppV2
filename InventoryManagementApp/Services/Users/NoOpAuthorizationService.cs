using InventoryManagementApp.Interfaces;

namespace InventoryManagementApp.Services.Users
{
    public class NoOpAuthorizationService : IAuthorizationService
    {
        public void EnsureAdmin() { }
    }
}
