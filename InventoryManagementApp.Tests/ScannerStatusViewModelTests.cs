using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using InventoryManagementApp.Interfaces;
using InventoryManagementApp.Models;
using InventoryManagementApp.ViewModels;
using System.Windows.Documents;
using InventoryManagementApp.ViewModels.Rental;
using Xunit;

public class ScannerStatusViewModelTests
{
    private sealed class StubScannerService : IScannerService
    {
        public List<ScannerDevice> Devices { get; } = new();
        public Task<IEnumerable<ScannerDevice>> GetScannerDevicesAsync(CancellationToken cancellationToken)
            => Task.FromResult<IEnumerable<ScannerDevice>>(Devices);
    }

    private sealed class StubSettingsService : ISettingsService
    {
        public List<string> IpAddresses { get; private set; } = new();
        public event EventHandler<IDictionary<ItemDetailField, bool>>? ItemDetailVisibilityChanged;
        public Task SaveSettingAsync(string key, string value, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task<string?> GetSettingAsync(string? key, CancellationToken cancellationToken = default) => Task.FromResult<string?>(null);
        public Task<Dictionary<string, string>> GetAllSettingsAsync(CancellationToken cancellationToken = default) => Task.FromResult(new Dictionary<string, string>());
        public Task UpdateSettingsAsync(Dictionary<string, string> settings, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task DeleteSettingAsync(string key, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task<IEnumerable<string>> GetScannerIpAddressesAsync(CancellationToken cancellationToken = default)
            => Task.FromResult<IEnumerable<string>>(IpAddresses);
        public Task<IEnumerable<string>> SaveScannerIpAddressesAsync(IEnumerable<string>? ipAddresses, CancellationToken cancellationToken = default)
        {
            IpAddresses = ipAddresses?.ToList() ?? new List<string>();
            return Task.FromResult<IEnumerable<string>>(Array.Empty<string>());
        }
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
        {
            ItemDetailVisibilityChanged?.Invoke(this, visibility);
            return Task.CompletedTask;
        }
    }

    private sealed class StubDialogService : IDialogService
    {
        public void ShowInfo(string message, string title) { }
        public bool ShowConfirmation(string message, string title) => false;
        public ItemModel? ShowEditItemDialog(ItemModel item) => null;
        public void ShowItemDetails(ItemModel item) { }
        public (CustomerModel customer, DateTime dueDate)? ShowRentItemDialog(ItemModel item, IEnumerable<CustomerModel> customers) => null;
        public CustomerModel? ShowAddCustomerDialog() => null;
        public CustomerModel? ShowEditCustomerDialog(CustomerModel customer) => null;
        public void ShowRentalsFilter(ManageRentalsViewModel viewModel) { }
        public void ShowRentalHistory(ItemModel item, IEnumerable<RentalModel> history) { }
        public Dictionary<string, string>? ShowImportMapping(IEnumerable<string> headers, IEnumerable<string> properties, IEnumerable<string>? requiredPropertyNames = null) => null;
        public Func<ItemModel, IEnumerable<string>>? ShowImageImportMapping() => null;
        public void ShowPrintPreview(FlowDocument document, string title, string description) { }
        public void ShowPrintLabelDialog() { }
    }

    [Fact]
    public async Task AddDeviceCommand_SavesIpAndRefreshesDevices()
    {
        var scannerService = new StubScannerService();
        var dialogService = new StubDialogService();
        var settingsService = new StubSettingsService();
        var vm = new ScannerStatusViewModel(scannerService, dialogService, settingsService);

        var ip = "192.168.1.10";
        vm.PromptForIp = () => ip;
        scannerService.Devices.Add(new ScannerDevice { Name = "Test", Ip = ip, Status = "Online", LastSeen = DateTime.UtcNow });

        await vm.AddDeviceCommand.ExecuteAsync(null);

        Assert.Contains(ip, settingsService.IpAddresses);
        Assert.Single(vm.Devices);
        Assert.Equal(ip, vm.Devices[0].Ip);
    }
}
