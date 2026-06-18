using InventoryManagementApp.Models;

namespace InventoryManagementApp.Interfaces
{
    public interface IThemeService
    {
        void ApplyTheme(string? theme);
        void ApplyCustomTheme(AppThemeSettings? settings);
    }
}
