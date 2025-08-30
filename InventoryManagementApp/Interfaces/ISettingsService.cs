using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using InventoryManagementApp.Models;

namespace InventoryManagementApp.Interfaces
{
    public interface ISettingsService
    {
        event EventHandler<IDictionary<ItemDetailField, bool>>? ItemDetailVisibilityChanged;
        Task SaveSettingAsync(string key, string value, CancellationToken cancellationToken = default);
        Task<string?> GetSettingAsync(string? key, CancellationToken cancellationToken = default);
        Task<Dictionary<string, string>> GetAllSettingsAsync(CancellationToken cancellationToken = default);
        Task UpdateSettingsAsync(Dictionary<string, string> settings, CancellationToken cancellationToken = default);
        Task DeleteSettingAsync(string key, CancellationToken cancellationToken = default);
        Task<IEnumerable<string>> GetScannerIpAddressesAsync(CancellationToken cancellationToken = default);
        Task<IEnumerable<string>> SaveScannerIpAddressesAsync(IEnumerable<string>? ipAddresses, CancellationToken cancellationToken = default);

        // Theme configuration
        Task<string?> GetThemeAsync(CancellationToken cancellationToken = default);
        Task SaveThemeAsync(string theme, CancellationToken cancellationToken = default);

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
        Task<IDictionary<ItemDetailField, bool>> GetItemDetailVisibilityAsync(CancellationToken cancellationToken = default);
        Task SaveItemDetailVisibilityAsync(IDictionary<ItemDetailField, bool> visibility, CancellationToken cancellationToken = default);
    }
}
