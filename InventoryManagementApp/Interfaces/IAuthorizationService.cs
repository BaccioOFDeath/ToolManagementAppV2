namespace InventoryManagementApp.Interfaces
{
    public interface IAuthorizationService
    {
        bool IsAdmin { get; }
        bool HasPermission(string permissionKey);
        bool HasAnyPermission(params string[] permissionKeys);
        void EnsureAdmin();
        void EnsurePermission(string permissionKey);
        void EnsureAnyPermission(params string[] permissionKeys);
    }
}
