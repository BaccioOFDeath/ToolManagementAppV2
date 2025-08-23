using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace InventoryManagementApp.Interfaces
{
    public interface ISettingsService
    {
        Task SaveSettingAsync(string key, string value, CancellationToken cancellationToken = default);
        Task<string?> GetSettingAsync(string key, CancellationToken cancellationToken = default);
        Task<Dictionary<string, string>> GetAllSettingsAsync(CancellationToken cancellationToken = default);
        Task UpdateSettingsAsync(Dictionary<string, string> settings, CancellationToken cancellationToken = default);
        Task DeleteSettingAsync(string key, CancellationToken cancellationToken = default);
        Task<IEnumerable<string>> GetScannerIpAddressesAsync(CancellationToken cancellationToken = default);
        Task<IEnumerable<string>> SaveScannerIpAddressesAsync(IEnumerable<string>? ipAddresses, CancellationToken cancellationToken = default);

        // Password hashing configuration
        Task<int> GetPasswordIterationsAsync(CancellationToken cancellationToken = default);
        Task SavePasswordIterationsAsync(int iterations, CancellationToken cancellationToken = default);

        // Auto logout configuration
        Task<int> GetAutoLogoutMinutesAsync(CancellationToken cancellationToken = default);
        Task SaveAutoLogoutMinutesAsync(int minutes, CancellationToken cancellationToken = default);

        // Item label configuration
        Task<string> GetItemLabelSingularAsync(CancellationToken cancellationToken = default);
        Task SaveItemLabelSingularAsync(string label, CancellationToken cancellationToken = default);
        Task<string> GetItemLabelPluralAsync(CancellationToken cancellationToken = default);
        Task SaveItemLabelPluralAsync(string label, CancellationToken cancellationToken = default);

        // Item display configuration
        Task<bool> GetShowItemImageAsync(CancellationToken cancellationToken = default);
        Task SaveShowItemImageAsync(bool value, CancellationToken cancellationToken = default);
        Task<bool> GetShowItemNameAsync(CancellationToken cancellationToken = default);
        Task SaveShowItemNameAsync(bool value, CancellationToken cancellationToken = default);
        Task<bool> GetShowItemNumberAsync(CancellationToken cancellationToken = default);
        Task SaveShowItemNumberAsync(bool value, CancellationToken cancellationToken = default);
        Task<bool> GetShowItemLocationAsync(CancellationToken cancellationToken = default);
        Task SaveShowItemLocationAsync(bool value, CancellationToken cancellationToken = default);
        Task<bool> GetShowItemNotesAsync(CancellationToken cancellationToken = default);
        Task SaveShowItemNotesAsync(bool value, CancellationToken cancellationToken = default);
    }
}
