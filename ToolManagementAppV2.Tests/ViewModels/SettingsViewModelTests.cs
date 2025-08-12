using System;
using System.IO;
using ToolManagementAppV2.ViewModels;
using Xunit;

namespace ToolManagementAppV2.Tests.ViewModels
{
    public class SettingsViewModelTests
    {
        [Fact]
        public void Constructor_InitializesThemeDefaults()
        {
            var vm = new SettingsViewModel(new StubFileDialogService(), new StubSettingsService());
            Assert.Contains("Light", vm.ThemeOptions);
            Assert.Equal("Light", vm.Theme);
        }

        [Fact]
        public void TestDbCommand_CreatesDatabaseFile()
        {
            var path = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + ".db");
            try
            {
                var vm = new SettingsViewModel(new StubFileDialogService(), new StubSettingsService()) { ConnectionString = path };
                vm.TestDbCommand.Execute(null);
                Assert.True(File.Exists(path));
            }
            finally
            {
                if (File.Exists(path)) File.Delete(path);
            }
        }
        [Fact]
        public void BrowseCompanyLogoCommand_SetsPath()
        {
            var fileDialog = new StubFileDialogService { OpenPath = "logo.png" };
            var vm = new SettingsViewModel(fileDialog, new StubSettingsService());
            vm.BrowseCompanyLogoCommand.Execute(null);
            Assert.Equal("logo.png", vm.CompanyLogoPath);
        }

        [Fact]
        public void SaveCompanyLogoCommand_PersistsPath()
        {
            var settings = new StubSettingsService();
            var vm = new SettingsViewModel(new StubFileDialogService(), settings)
            {
                CompanyLogoPath = "logo.png"
            };
            vm.SaveCompanyLogoCommand.Execute(null);
            Assert.Equal("CompanyLogoPath", settings.SavedKey);
            Assert.Equal("logo.png", settings.SavedValue);
        }
    }

    class StubFileDialogService : ToolManagementAppV2.Interfaces.IFileDialogService
    {
        public string OpenPath { get; set; } = string.Empty;
        public string? OpenFile(string filter) => OpenPath;
        public string? SaveFile(string filter) => null;
    }

    class StubSettingsService : ToolManagementAppV2.Interfaces.ISettingsService
    {
        public string SavedKey { get; private set; }
        public string SavedValue { get; private set; }

        public void SaveSetting(string key, string value)
        {
            SavedKey = key;
            SavedValue = value;
        }

        public string GetSetting(string key) => string.Empty;
        public Dictionary<string, string> GetAllSettings() => new();
        public void UpdateSettings(Dictionary<string, string> settings) { }
        public void DeleteSetting(string key) { }
    }
}

