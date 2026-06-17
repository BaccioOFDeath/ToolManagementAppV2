using InventoryManagementApp.Interfaces;

namespace InventoryManagementApp.Services.Users
{
    public class NoOpAuthorizationService : IAuthorizationService
    {
        public bool IsAdmin => true;
        public bool HasPermission(string permissionKey) => true;
        public bool HasAnyPermission(params string[] permissionKeys) => true;
        public void EnsureAdmin() { }
        public void EnsurePermission(string permissionKey) { }
        public void EnsureAnyPermission(params string[] permissionKeys) { }
    }
}
