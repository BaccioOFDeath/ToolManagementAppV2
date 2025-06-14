using System.Collections.Generic;

namespace ToolManagementAppV2.Interfaces
{
    public interface ISettingsService
    {
        void SaveSetting(string key, string value);
        string GetSetting(string key);
        Dictionary<string, string> GetAllSettings();
        void UpdateSettings(Dictionary<string, string> settings);
        void DeleteSetting(string key);
    }
}
