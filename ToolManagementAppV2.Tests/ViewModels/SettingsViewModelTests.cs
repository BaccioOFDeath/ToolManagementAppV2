using System;
using System.IO;
using System.Collections.Generic;
using System.Threading.Tasks;
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
        public IEnumerable<string> ScannerIps { get; set; } = Array.Empty<string>();
        public IEnumerable<string> GetScannerIpAddresses() => ScannerIps;
        public IEnumerable<string> SaveScannerIpAddresses(IEnumerable<string>? ipAddresses)
        {
            ScannerIps = ipAddresses ?? Array.Empty<string>();
            return Array.Empty<string>();
        }
        public int PasswordIterations { get; set; } = 100_000;
        public int GetPasswordIterations() => PasswordIterations;
        public void SavePasswordIterations(int iterations) => PasswordIterations = iterations;

        public Task SaveSettingAsync(string key, string value)
        {
            SaveSetting(key, value);
            return Task.CompletedTask;
        }
        public Task<string?> GetSettingAsync(string key) => Task.FromResult(GetSetting(key));
        public Task<Dictionary<string, string>> GetAllSettingsAsync() => Task.FromResult(GetAllSettings());
        public Task UpdateSettingsAsync(Dictionary<string, string> settings)
        {
            UpdateSettings(settings);
            return Task.CompletedTask;
        }
        public Task DeleteSettingAsync(string key)
        {
            DeleteSetting(key);
            return Task.CompletedTask;
        }
        public Task<IEnumerable<string>> GetScannerIpAddressesAsync() => Task.FromResult(GetScannerIpAddresses());
        public Task<IEnumerable<string>> SaveScannerIpAddressesAsync(IEnumerable<string>? ipAddresses) => Task.FromResult(SaveScannerIpAddresses(ipAddresses));
        public Task<int> GetPasswordIterationsAsync() => Task.FromResult(GetPasswordIterations());
        public Task SavePasswordIterationsAsync(int iterations)
        {
            SavePasswordIterations(iterations);
            return Task.CompletedTask;
        }
    }

    class StubDialogService : IDialogService
    {
        public void ShowInfo(string message, string title) { }
        public bool ShowConfirmation(string message, string title) => true;
        public ToolModel? ShowEditToolDialog(ToolModel tool) => null;
        public void ShowToolDetails(ToolModel tool) { }
        public (CustomerModel customer, DateTime dueDate)? ShowRentToolDialog(ToolModel tool, IEnumerable<CustomerModel> customers) => null;
        public CustomerModel? ShowAddCustomerDialog() => null;
        public void ShowRentalsFilter(ToolManagementAppV2.ViewModels.ManageRentalsViewModel viewModel) { }
        public void ShowRentalHistory(ToolModel tool, System.Collections.Generic.IEnumerable<RentalModel> history) { }
        public System.Collections.Generic.Dictionary<string, string>? ShowImportMapping(System.Collections.Generic.IEnumerable<string> headers, System.Collections.Generic.IEnumerable<string> properties) => null;
        public System.Func<ToolModel, System.Collections.Generic.IEnumerable<string>>? ShowImageImportMapping() => null;
        public void ShowPrintPreview(System.Windows.Documents.FlowDocument document, string title, string description) { }
        public void ShowPrintLabelDialog() { }
        public void ShowScannerStatus() { }
    }
}

