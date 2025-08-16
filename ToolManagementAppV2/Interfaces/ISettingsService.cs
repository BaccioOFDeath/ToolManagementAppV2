using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ToolManagementAppV2.Interfaces
{
    public interface ISettingsService
    {
        void SaveSetting(string key, string value, CancellationToken cancellationToken = default);
        string? GetSetting(string key, CancellationToken cancellationToken = default);
        Dictionary<string, string> GetAllSettings(CancellationToken cancellationToken = default);
        void UpdateSettings(Dictionary<string, string> settings, CancellationToken cancellationToken = default);
        void DeleteSetting(string key, CancellationToken cancellationToken = default);
        Task SaveSettingAsync(string key, string value, CancellationToken cancellationToken = default);
        Task<string?> GetSettingAsync(string key, CancellationToken cancellationToken = default);
        Task<Dictionary<string, string>> GetAllSettingsAsync(CancellationToken cancellationToken = default);
        Task UpdateSettingsAsync(Dictionary<string, string> settings, CancellationToken cancellationToken = default);
        Task DeleteSettingAsync(string key, CancellationToken cancellationToken = default);
        IEnumerable<string> GetScannerIpAddresses(CancellationToken cancellationToken = default);
        IEnumerable<string> SaveScannerIpAddresses(IEnumerable<string>? ipAddresses, CancellationToken cancellationToken = default);
        Task<IEnumerable<string>> GetScannerIpAddressesAsync(CancellationToken cancellationToken = default);
        Task<IEnumerable<string>> SaveScannerIpAddressesAsync(IEnumerable<string>? ipAddresses, CancellationToken cancellationToken = default);

        // Password hashing configuration
        int GetPasswordIterations(CancellationToken cancellationToken = default);
        void SavePasswordIterations(int iterations, CancellationToken cancellationToken = default);
        Task<int> GetPasswordIterationsAsync(CancellationToken cancellationToken = default);
        Task SavePasswordIterationsAsync(int iterations, CancellationToken cancellationToken = default);
    }
}
