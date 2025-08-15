using System.Collections.Generic;
using System.Threading.Tasks;

namespace ToolManagementAppV2.Interfaces
{
    public interface ISettingsService
    {
        void SaveSetting(string key, string value);
        string? GetSetting(string key);
        Dictionary<string, string> GetAllSettings();
        void UpdateSettings(Dictionary<string, string> settings);
        void DeleteSetting(string key);
        Task SaveSettingAsync(string key, string value);
        Task<string?> GetSettingAsync(string key);
        Task<Dictionary<string, string>> GetAllSettingsAsync();
        Task UpdateSettingsAsync(Dictionary<string, string> settings);
        Task DeleteSettingAsync(string key);
        IEnumerable<string> GetScannerIpAddresses();
        IEnumerable<string> SaveScannerIpAddresses(IEnumerable<string>? ipAddresses);
        Task<IEnumerable<string>> GetScannerIpAddressesAsync();
        Task<IEnumerable<string>> SaveScannerIpAddressesAsync(IEnumerable<string>? ipAddresses);

        // Password hashing configuration
        int GetPasswordIterations();
        void SavePasswordIterations(int iterations);
        Task<int> GetPasswordIterationsAsync();
        Task SavePasswordIterationsAsync(int iterations);
    }
}
