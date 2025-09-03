using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using InventoryManagementApp.Interfaces;
using InventoryManagementApp.ViewModels;
using Microsoft.Extensions.Configuration;
using Xunit;
using InventoryManagementApp.Models;

namespace InventoryManagementApp.Tests
{
    public class DeviceSettingsViewModelTests
    {
        private sealed class StubSettingsService : ISettingsService
        {
            private readonly Dictionary<string, string> _settings = new();
            public event EventHandler<IDictionary<ItemDetailField, bool>>? ItemDetailVisibilityChanged;
            public Task SaveSettingAsync(string key, string value, CancellationToken cancellationToken = default)
            { _settings[key] = value; return Task.CompletedTask; }
            public Task<string?> GetSettingAsync(string? key, CancellationToken cancellationToken = default)
            { return Task.FromResult(key != null && _settings.TryGetValue(key, out var v) ? v : null); }
            public Task<Dictionary<string, string>> GetAllSettingsAsync(CancellationToken cancellationToken = default)
            { return Task.FromResult(new Dictionary<string, string>(_settings)); }
            public Task UpdateSettingsAsync(Dictionary<string, string> settings, CancellationToken cancellationToken = default)
            { foreach (var kv in settings) _settings[kv.Key] = kv.Value; return Task.CompletedTask; }
            public Task DeleteSettingAsync(string key, CancellationToken cancellationToken = default)
            { _settings.Remove(key); return Task.CompletedTask; }
            public Task<string?> GetThemeAsync(CancellationToken cancellationToken = default) => Task.FromResult<string?>(null);
            public Task SaveThemeAsync(string theme, CancellationToken cancellationToken = default) => Task.CompletedTask;
            public Task<int> GetPasswordIterationsAsync(CancellationToken cancellationToken = default) => Task.FromResult(0);
            public Task SavePasswordIterationsAsync(int iterations, CancellationToken cancellationToken = default) => Task.CompletedTask;
            public Task<int> GetAutoLogoutMinutesAsync(CancellationToken cancellationToken = default) => Task.FromResult(0);
            public Task SaveAutoLogoutMinutesAsync(int minutes, CancellationToken cancellationToken = default) => Task.CompletedTask;
            public Task<string> GetItemLabelSingularAsync(CancellationToken cancellationToken = default) => Task.FromResult(string.Empty);
            public Task SaveItemLabelSingularAsync(string label, CancellationToken cancellationToken = default) => Task.CompletedTask;
            public Task<string> GetItemLabelPluralAsync(CancellationToken cancellationToken = default) => Task.FromResult(string.Empty);
            public Task SaveItemLabelPluralAsync(string label, CancellationToken cancellationToken = default) => Task.CompletedTask;
            public Task<IDictionary<ItemDetailField, bool>> GetItemDetailVisibilityAsync(CancellationToken cancellationToken = default)
                => Task.FromResult<IDictionary<ItemDetailField, bool>>(new Dictionary<ItemDetailField, bool>());
            public Task SaveItemDetailVisibilityAsync(IDictionary<ItemDetailField, bool> visibility, CancellationToken cancellationToken = default)
            { ItemDetailVisibilityChanged?.Invoke(this, visibility); return Task.CompletedTask; }
        }

        private sealed class DummyDialogService : IDialogService
        {
            public void ShowInfo(string message, string title) { }
            public bool ShowConfirmation(string message, string title) => false;
            public InventoryManagementApp.Models.ItemModel? ShowEditItemDialog(InventoryManagementApp.Models.ItemModel item) => null;
            public void ShowItemDetails(InventoryManagementApp.Models.ItemModel item) { }
            public (InventoryManagementApp.Models.CustomerModel customer, DateTime dueDate)? ShowRentItemDialog(InventoryManagementApp.Models.ItemModel item, IEnumerable<InventoryManagementApp.Models.CustomerModel> customers) => null;
            public InventoryManagementApp.Models.CustomerModel? ShowAddCustomerDialog() => null;
            public InventoryManagementApp.Models.CustomerModel? ShowEditCustomerDialog(InventoryManagementApp.Models.CustomerModel customer) => null;
            public void ShowRentalsFilter(InventoryManagementApp.ViewModels.ManageRentalsViewModel viewModel) { }
            public void ShowRentalHistory(InventoryManagementApp.Models.ItemModel item, IEnumerable<InventoryManagementApp.Models.RentalModel> history) { }
            public Dictionary<string, string>? ShowImportMapping(IEnumerable<string> headers, IEnumerable<string> properties, IEnumerable<string>? requiredPropertyNames = null) => null;
            public Func<InventoryManagementApp.Models.ItemModel, IEnumerable<string>>? ShowImageImportMapping() => null;
            public void ShowPrintPreview(System.Windows.Documents.FlowDocument document, string title, string description) { }
            public void ShowPrintLabelDialog() { }
        }

        [Fact]
        public async Task SaveCommand_PersistsSettings()
        {
            var settings = new StubSettingsService();
            var config = new ConfigurationBuilder().Build();
            var vm = new DeviceSettingsViewModel(settings, config, new DummyDialogService());
            vm.Subnets = "10.0.0.0/24";
            vm.FtpPorts = "21";
            vm.AdditionalPorts = "5555:Adb";
            vm.MaxConcurrentScans = 5;
            vm.LivenessTimeoutMs = 100;
            vm.PortProbeTimeoutMs = 200;
            await vm.SaveCommand.ExecuteAsync(null);
            var all = await settings.GetAllSettingsAsync();
            Assert.Equal("10.0.0.0/24", all["DeviceDiscovery_Subnets"]);
            Assert.Equal("21", all["DeviceDiscovery_FtpPorts"]);
            Assert.Equal("5555:Adb", all["DeviceDiscovery_AdditionalPorts"]);
            Assert.Equal("5", all["DeviceDiscovery_MaxConcurrentScans"]);
            Assert.Equal("100", all["DeviceDiscovery_LivenessTimeoutMs"]);
            Assert.Equal("200", all["DeviceDiscovery_PortProbeTimeoutMs"]);
        }
    }
}
