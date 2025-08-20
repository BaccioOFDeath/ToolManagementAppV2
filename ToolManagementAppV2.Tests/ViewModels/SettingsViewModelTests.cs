using System;
using System.IO;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ToolManagementAppV2.ViewModels;
using ToolManagementAppV2.Interfaces;
using Microsoft.Extensions.Logging;
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
        public void Constructor_PopulatesApplicationName_WhenSettingExists()
        {
            var settings = new StubSettingsService { GetSettingValue = "MyApp" };
            var vm = new SettingsViewModel(new StubFileDialogService(), settings, new StubDialogService());
            Assert.Equal("MyApp", vm.ApplicationName);
        }

        [Fact]
        public void ApplicationName_Setter_SavesValue()
        {
            var settings = new StubSettingsService();
            var vm = new SettingsViewModel(new StubFileDialogService(), settings, new StubDialogService());
            vm.ApplicationName = "NewName";
            Assert.Equal("ApplicationName", settings.SavedKey);
            Assert.Equal("NewName", settings.SavedValue);
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
        public void SaveCompanyLogoCommand_PersistsRelativePath()
        {
            var settings = new StubSettingsService();
            var baseDir = AppDomain.CurrentDomain.BaseDirectory;
            var file = Path.Combine(baseDir, "logo.png");
            File.WriteAllText(file, "data");
            try
            {
                var vm = new SettingsViewModel(new StubFileDialogService(), settings, new StubDialogService())
                {
                    CompanyLogoPath = file
                };
                var expected = Path.GetRelativePath(baseDir, file);
                vm.SaveCompanyLogoCommand.ExecuteAsync(null).GetAwaiter().GetResult();
                Assert.Equal("CompanyLogoPath", settings.SavedKey);
                Assert.Equal(expected, settings.SavedValue);
                Assert.Equal(expected, vm.CompanyLogoPath);
            }
            finally
            {
                if (File.Exists(file)) File.Delete(file);
            }
        }

        [Fact]
        public void SaveCompanyLogoCommand_CopiesExternalFile()
        {
            var settings = new StubSettingsService();
            var external = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + ".png");
            File.WriteAllText(external, "data");
            try
            {
                var vm = new SettingsViewModel(new StubFileDialogService(), settings, new StubDialogService())
                {
                    CompanyLogoPath = external
                };
                vm.SaveCompanyLogoCommand.ExecuteAsync(null).GetAwaiter().GetResult();
                var expected = Path.Combine("Assets", "CompanyLogo", Path.GetFileName(external));
                Assert.Equal(expected, settings.SavedValue);
                Assert.Equal(expected, vm.CompanyLogoPath);
                Assert.True(File.Exists(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, expected)));
            }
            finally
            {
                if (File.Exists(external)) File.Delete(external);
                var copied = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "CompanyLogo", Path.GetFileName(external));
                if (File.Exists(copied)) File.Delete(copied);
            }
        }

        [Fact]
        public void SaveCompanyLogoCommand_InvalidPath_ShowsError()
        {
            var settings = new StubSettingsService();
            var dialog = new StubDialogService();
            var vm = new SettingsViewModel(new StubFileDialogService(), settings, dialog)
            {
                CompanyLogoPath = Path.Combine("..", "logo.png")
            };
            vm.SaveCompanyLogoCommand.ExecuteAsync(null).GetAwaiter().GetResult();
            Assert.Null(settings.SavedKey);
            Assert.Equal("Selected logo path is invalid.", dialog.LastMessage);
            Assert.Equal("Invalid Path", dialog.LastTitle);
        }

        [Fact]
        public void TestDbCommand_LogsError_WhenDialogServiceFails()
        {
            var logger = new CapturingLogger<SettingsViewModel>();
            var vm = new SettingsViewModel(new StubFileDialogService(), new StubSettingsService(), new FailingDialogService(), logger)
            {
                ConnectionString = "invalid"
            };
            var ex = Record.Exception(() => vm.TestDbCommand.Execute(null));
            Assert.Null(ex);
            Assert.NotNull(logger.LastError);
            Assert.Contains("Failed to display info dialog", logger.LastError);
        }

        [Fact]
        public void PasswordIterations_AboveLimit_IsClampedAndNotifies()
        {
            var settings = new StubSettingsService();
            var dialog = new StubDialogService();
            var vm = new SettingsViewModel(new StubFileDialogService(), settings, dialog);

            vm.PasswordIterations = 2_000_000;

            Assert.Equal(1_000_000, vm.PasswordIterations);
            Assert.Equal(1_000_000, settings.PasswordIterations);
            Assert.NotNull(dialog.LastInfoMessage);
        }

        [Fact]
        public void AutoLogoutMinutes_LoadsAndSaves()
        {
            var settings = new StubSettingsService { AutoLogoutMinutes = 5 };
            var vm = new SettingsViewModel(new StubFileDialogService(), settings, new StubDialogService());
            Assert.Equal(5, vm.AutoLogoutMinutes);

            vm.AutoLogoutMinutes = 10;
            Assert.Equal(10, settings.AutoLogoutMinutes);
        }

        [Fact]
        public void ItemLabels_UpdateServiceAndProvider()
        {
            var settings = new StubSettingsService();
            LabelProvider.Instance.UpdateLabels("ItemModel", "Tools");
            var vm = new SettingsViewModel(new StubFileDialogService(), settings, new StubDialogService());
            vm.ItemLabelSingular = "Widget";
            vm.ItemLabelPlural = "Widgets";
            Assert.Equal("Widget", settings.ItemLabelSingular);
            Assert.Equal("Widgets", settings.ItemLabelPlural);
            Assert.Equal("Widget", LabelProvider.Instance.ItemLabelSingular);
            Assert.Equal("Widgets", LabelProvider.Instance.ItemLabelPlural);
            LabelProvider.Instance.UpdateLabels("ItemModel", "Tools");
        }

        [Fact]
        public void PasswordIterations_AboveLimit_PersistsClampedValue()
        {
            var path = System.IO.Path.GetTempFileName();
            try
            {
                var db = new ToolManagementAppV2.Services.Core.DatabaseService(path);
                var settings = new ToolManagementAppV2.Services.Settings.SettingsService(db);
                var dialog = new StubDialogService();
                var vm = new SettingsViewModel(new StubFileDialogService(), settings, dialog);

                vm.PasswordIterations = 2_000_000;

                Assert.Equal(1_000_000, settings.GetPasswordIterationsAsync().GetAwaiter().GetResult());
            }
            finally
            {
                if (System.IO.File.Exists(path)) System.IO.File.Delete(path);
            }
        }

    }

    class StubFileDialogService : ToolManagementAppV2.Interfaces.IFileDialogService
    {
        public string OpenPath { get; set; } = string.Empty;
        public string? OpenFile(string filter, string? initialDirectory = null) => OpenPath;
        public string? SaveFile(string filter) => null;
    }

    class StubSettingsService : ToolManagementAppV2.Interfaces.ISettingsService
    {
        public string SavedKey { get; private set; }
        public string SavedValue { get; private set; }
        public string? GetSettingValue { get; set; } = string.Empty;
        public IEnumerable<string> ScannerIps { get; set; } = Array.Empty<string>();
        public int PasswordIterations { get; set; } = 100_000;
        public int AutoLogoutMinutes { get; set; }
        public string ItemLabelSingular { get; set; } = "ItemModel";
        public string ItemLabelPlural { get; set; } = "Tools";

        public Task SaveSettingAsync(string key, string value, CancellationToken cancellationToken = default)
        {
            SavedKey = key;
            SavedValue = value;
            return Task.CompletedTask;
        }
        public Task<string?> GetSettingAsync(string key, CancellationToken cancellationToken = default)
            => Task.FromResult(GetSettingValue);
        public Task<Dictionary<string, string>> GetAllSettingsAsync(CancellationToken cancellationToken = default)
            => Task.FromResult(new Dictionary<string, string>());
        public Task UpdateSettingsAsync(Dictionary<string, string> settings, CancellationToken cancellationToken = default)
            => Task.CompletedTask;
        public Task DeleteSettingAsync(string key, CancellationToken cancellationToken = default)
            => Task.CompletedTask;
        public Task<IEnumerable<string>> GetScannerIpAddressesAsync(CancellationToken cancellationToken = default)
            => Task.FromResult(ScannerIps);
        public Task<IEnumerable<string>> SaveScannerIpAddressesAsync(IEnumerable<string>? ipAddresses, CancellationToken cancellationToken = default)
        {
            ScannerIps = ipAddresses ?? Array.Empty<string>();
            return Task.FromResult<IEnumerable<string>>(Array.Empty<string>());
        }
        public Task<int> GetPasswordIterationsAsync(CancellationToken cancellationToken = default)
            => Task.FromResult(PasswordIterations);
        public Task SavePasswordIterationsAsync(int iterations, CancellationToken cancellationToken = default)
        {
            PasswordIterations = iterations;
            return Task.CompletedTask;
        }
        public Task<int> GetAutoLogoutMinutesAsync(CancellationToken cancellationToken = default)
            => Task.FromResult(AutoLogoutMinutes);
        public Task SaveAutoLogoutMinutesAsync(int minutes, CancellationToken cancellationToken = default)
        {
            AutoLogoutMinutes = minutes;
            return Task.CompletedTask;
        }

        public Task<string> GetItemLabelSingularAsync(CancellationToken cancellationToken = default)
            => Task.FromResult(ItemLabelSingular);
        public Task SaveItemLabelSingularAsync(string label, CancellationToken cancellationToken = default)
        {
            ItemLabelSingular = label;
            return Task.CompletedTask;
        }
        public Task<string> GetItemLabelPluralAsync(CancellationToken cancellationToken = default)
            => Task.FromResult(ItemLabelPlural);
        public Task SaveItemLabelPluralAsync(string label, CancellationToken cancellationToken = default)
        {
            ItemLabelPlural = label;
            return Task.CompletedTask;
        }
    }

    class StubDialogService : IDialogService
    {
        public string? LastInfoMessage { get; private set; }
        public string? LastInfoTitle { get; private set; }
        public void ShowInfo(string message, string title)
        {
            LastInfoMessage = message;
            LastInfoTitle = title;
        }
        
        public bool ShowConfirmation(string message, string title) => true;
        public ItemModel? ShowEditToolDialog(ItemModel tool) => null;
        public void ShowToolDetails(ItemModel tool) { }
        public (CustomerModel customer, DateTime dueDate)? ShowRentToolDialog(ItemModel tool, IEnumerable<CustomerModel> customers) => null;
        public CustomerModel? ShowAddCustomerDialog() => null;
        public void ShowRentalsFilter(ToolManagementAppV2.ViewModels.ManageRentalsViewModel viewModel) { }
        public void ShowRentalHistory(ItemModel tool, System.Collections.Generic.IEnumerable<RentalModel> history) { }
        public System.Collections.Generic.Dictionary<string, string>? ShowImportMapping(System.Collections.Generic.IEnumerable<string> headers, System.Collections.Generic.IEnumerable<string> properties) => null;
        public System.Func<ItemModel, System.Collections.Generic.IEnumerable<string>>? ShowImageImportMapping() => null;
        public void ShowPrintPreview(System.Windows.Documents.FlowDocument document, string title, string description) { }
        public void ShowPrintLabelDialog() { }
    }

    class FailingDialogService : IDialogService
    {
        public void ShowInfo(string message, string title) => throw new InvalidOperationException("dialog failure");
        public bool ShowConfirmation(string message, string title) => false;
        public ItemModel? ShowEditToolDialog(ItemModel tool) => null;
        public void ShowToolDetails(ItemModel tool) { }
        public (CustomerModel customer, DateTime dueDate)? ShowRentToolDialog(ItemModel tool, IEnumerable<CustomerModel> customers) => null;
        public CustomerModel? ShowAddCustomerDialog() => null;
        public void ShowRentalsFilter(ToolManagementAppV2.ViewModels.ManageRentalsViewModel viewModel) { }
        public void ShowRentalHistory(ItemModel tool, IEnumerable<RentalModel> history) { }
        public Dictionary<string, string>? ShowImportMapping(IEnumerable<string> headers, IEnumerable<string> properties) => null;
        public Func<ItemModel, IEnumerable<string>>? ShowImageImportMapping() => null;
        public void ShowPrintPreview(System.Windows.Documents.FlowDocument document, string title, string description) { }
        public void ShowPrintLabelDialog() { }
    }

    class CapturingLogger<T> : ILogger<T>
    {
        public string? LastError { get; private set; }
        public IDisposable BeginScope<TState>(TState state) => NullDisposable.Instance;
        public bool IsEnabled(LogLevel logLevel) => true;
        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            if (logLevel == LogLevel.Error)
                LastError = formatter(state, exception);
        }
        private sealed class NullDisposable : IDisposable
        {
            public static readonly NullDisposable Instance = new();
            public void Dispose() { }
        }
    }
}

