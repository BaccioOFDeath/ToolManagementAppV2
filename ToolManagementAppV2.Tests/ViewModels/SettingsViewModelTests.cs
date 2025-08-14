using System;
using System.IO;
using System.Collections.Generic;
using ToolManagementAppV2.ViewModels;
using ToolManagementAppV2.Interfaces;
using Xunit;

namespace ToolManagementAppV2.Tests.ViewModels
{
    public class SettingsViewModelTests
    {
        [Fact]
        public void Constructor_InitializesThemeDefaults()
        {
            var vm = new SettingsViewModel(new StubFileDialogService(), new StubSettingsService(), new StubDialogService());
            Assert.Contains("Light", vm.ThemeOptions);
            Assert.Equal("Light", vm.Theme);
        }

        [Fact]
        public void Constructor_PopulatesCompanyLogoPath_WhenSettingExists()
        {
            var settings = new StubSettingsService { GetSettingValue = "logo.png" };
            var vm = new SettingsViewModel(new StubFileDialogService(), settings, new StubDialogService());
            Assert.Equal("logo.png", vm.CompanyLogoPath);
        }

        [Fact]
        public void TestDbConnection_CreatesDatabaseFile()
        {
            var path = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + ".db");
            try
            {
                var vm = new SettingsViewModel(new StubFileDialogService(), new StubSettingsService(), new StubDialogService()) { ConnectionString = path };
                var success = vm.TestDbConnection(out var message);
                Assert.True(success);
                Assert.Equal("Connection successful.", message);
                Assert.True(File.Exists(path));
            }
            finally
            {
                if (File.Exists(path)) File.Delete(path);
            }
        }

        [Fact]
        public void TestDbConnection_InvalidPath_ReturnsErrorMessage()
        {
            var vm = new SettingsViewModel(new StubFileDialogService(), new StubSettingsService(), new StubDialogService())
            {
                ConnectionString = "/nonexistent/path/db.sqlite"
            };
            var success = vm.TestDbConnection(out var message);
            Assert.False(success);
            Assert.Contains("Connection failed", message);
        }
        [Fact]
        public void BrowseCompanyLogoCommand_SetsPath()
        {
            var fileDialog = new StubFileDialogService { OpenPath = "logo.png" };
            var vm = new SettingsViewModel(fileDialog, new StubSettingsService(), new StubDialogService());
            vm.BrowseCompanyLogoCommand.Execute(null);
            Assert.Equal("logo.png", vm.CompanyLogoPath);
        }

        [Fact]
        public void SaveCompanyLogoCommand_PersistsPath()
        {
            var settings = new StubSettingsService();
            var vm = new SettingsViewModel(new StubFileDialogService(), settings, new StubDialogService())
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
        public string? GetSettingValue { get; set; } = string.Empty;

        public void SaveSetting(string key, string value)
        {
            SavedKey = key;
            SavedValue = value;
        }

        public string? GetSetting(string key) => GetSettingValue;
        public Dictionary<string, string> GetAllSettings() => new();
        public void UpdateSettings(Dictionary<string, string> settings) { }
        public void DeleteSetting(string key) { }
    }

    class StubDialogService : IDialogService
    {
        public void ShowInfo(string message, string title) { }
        public bool ShowConfirmation(string message, string title) => true;
        public ToolModel? ShowEditToolDialog(ToolModel tool) => null;
        public void ShowToolDetails(ToolModel tool) { }
        public (CustomerModel customer, DateTime dueDate)? ShowRentToolDialog(ToolModel tool, IEnumerable<CustomerModel> customers) => null;
        public CustomerModel? ShowAddCustomerDialog() => null;
    }
}

