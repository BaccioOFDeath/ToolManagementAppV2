using InventoryManagementApp.Interfaces;

namespace InventoryManagementApp.Services.Users
{
    public class NoOpAuthorizationService : IAuthorizationService
    {
        public bool IsAdmin => true;
        public void EnsureAdmin() { }
    }
}
