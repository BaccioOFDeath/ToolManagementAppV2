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
        public List<Device> Devices { get; } = new();
        public Task<IEnumerable<Device>> GetDevicesAsync(CancellationToken cancellationToken)
            => Task.FromResult<IEnumerable<Device>>(Devices);
    }

    private sealed class StubScannerFileService : IScannerFileService
    {
        public List<string> Files { get; } = new();
        public Task<IEnumerable<string>> ListFilesAsync(string deviceIp, CancellationToken cancellationToken = default)
            => Task.FromResult<IEnumerable<string>>(Files);
    }

    private sealed class StubScannerRuleService : IScannerRuleService
    {
        public List<ScannerFileRule> Rules { get; } = new();
        public Task<int> AddRuleAsync(ScannerFileRule rule, CancellationToken cancellationToken = default)
        {
            rule.Id = Rules.Count + 1;
            Rules.Add(rule);
            return Task.FromResult(rule.Id);
        }
        public Task<IEnumerable<ScannerFileRule>> GetRulesAsync(string deviceId, CancellationToken cancellationToken = default)
            => Task.FromResult<IEnumerable<ScannerFileRule>>(Rules.Where(r => r.DeviceId == deviceId).ToList());
        public Task DeleteRuleAsync(int ruleId, CancellationToken cancellationToken = default)
        {
            Rules.RemoveAll(r => r.Id == ruleId);
            return Task.CompletedTask;
        }
    }

    private sealed class StubDeviceGroupService : IDeviceGroupService
    {
        public List<DeviceGroup> Groups { get; } = new();
        public Dictionary<string, int?> DeviceGroups { get; } = new();
        public Task<int> CreateGroupAsync(string name, CancellationToken cancellationToken = default)
        {
            var id = Groups.Count + 1;
            Groups.Add(new DeviceGroup { Id = id, Name = name });
            return Task.FromResult(id);
        }
        public Task<IEnumerable<DeviceGroup>> GetGroupsAsync(CancellationToken cancellationToken = default)
            => Task.FromResult<IEnumerable<DeviceGroup>>(Groups);
        public Task UpdateGroupAsync(DeviceGroup group, CancellationToken cancellationToken = default)
        {
            var existing = Groups.FirstOrDefault(g => g.Id == group.Id);
            if (existing != null) existing.Name = group.Name;
            return Task.CompletedTask;
        }
        public Task DeleteGroupAsync(int groupId, CancellationToken cancellationToken = default)
        {
            Groups.RemoveAll(g => g.Id == groupId);
            return Task.CompletedTask;
        }
        public Task AssignDeviceToGroupAsync(string deviceIp, int? groupId, CancellationToken cancellationToken = default)
        {
            if (groupId == null) DeviceGroups.Remove(deviceIp);
            else DeviceGroups[deviceIp] = groupId;
            return Task.CompletedTask;
        }
        public Task<int?> GetDeviceGroupIdAsync(string deviceIp, CancellationToken cancellationToken = default)
        {
            DeviceGroups.TryGetValue(deviceIp, out var gid);
            return Task.FromResult<int?>(gid);
        }
    }

    private sealed class StubDeviceService : IDeviceService
    {
        public List<Device> Devices { get; } = new();
        public Task<IEnumerable<Device>> GetDevicesAsync(CancellationToken cancellationToken = default)
            => Task.FromResult<IEnumerable<Device>>(Devices);
        public Task<Device?> GetDeviceAsync(string ip, CancellationToken cancellationToken = default)
            => Task.FromResult<Device?>(Devices.FirstOrDefault(d => d.Ip == ip));
        public Task AddOrUpdateDeviceAsync(Device device, CancellationToken cancellationToken = default)
        {
            Devices.RemoveAll(d => d.Ip == device.Ip);
            Devices.Add(device);
            return Task.CompletedTask;
        }
        public Task DeleteDeviceAsync(string ip, CancellationToken cancellationToken = default)
        {
            Devices.RemoveAll(d => d.Ip == ip);
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
        var deviceService = new StubDeviceService();
        var groupService = new StubDeviceGroupService();
        var fileService = new StubScannerFileService();
        var ruleService = new StubScannerRuleService();
        var vm = new ScannerStatusViewModel(scannerService, dialogService, deviceService, groupService, fileService, ruleService);

        var ip = "192.168.1.10";
        vm.PromptForIp = () => ip;
        scannerService.Devices.Add(new Device { Hostname = "Test", Ip = ip, Status = "Online", LastSeen = DateTime.UtcNow });

        await vm.AddDeviceCommand.ExecuteAsync(null);

        Assert.Contains(deviceService.Devices, d => d.Ip == ip);
        Assert.Single(vm.Devices);
        Assert.Equal(ip, vm.Devices[0].Ip);
    }

    [Fact]
    public async Task SelectedDevice_LoadsFiles()
    {
        var scannerService = new StubScannerService();
        var dialogService = new StubDialogService();
        var deviceService = new StubDeviceService();
        var groupService = new StubDeviceGroupService();
        var fileService = new StubScannerFileService();
        var ruleService = new StubScannerRuleService();
        fileService.Files.AddRange(new[] { "a.txt", "b.txt" });
        var vm = new ScannerStatusViewModel(scannerService, dialogService, deviceService, groupService, fileService, ruleService);

        var device = new Device { Hostname = "Test", Ip = "1.2.3.4", Status = "Online", LastSeen = DateTime.UtcNow };
        vm.Devices.Add(device);
        vm.SelectedDevice = device;

        await vm.LoadFilesCommand.ExecuteAsync(null);

        Assert.Equal(2, vm.DeviceFiles.Count);
        Assert.Contains("a.txt", vm.DeviceFiles);
    }
}
