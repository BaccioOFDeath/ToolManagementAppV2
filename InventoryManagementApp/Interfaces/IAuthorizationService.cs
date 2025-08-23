namespace InventoryManagementApp.Interfaces
{
    public interface IAuthorizationService
    {
        bool IsAdmin { get; }
        void EnsureAdmin();
    }
}
