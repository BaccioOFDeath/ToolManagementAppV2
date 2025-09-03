using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Documents;
using InventoryManagementApp.Interfaces;
using InventoryManagementApp.Models;
using InventoryManagementApp.ViewModels;
using Xunit;

public class ScannerStatusViewModelTests
{
    private sealed class StubScannerService : IScannerService
    {
        public List<Device> Devices { get; } = new();
        public Task<IEnumerable<Device>> GetDevicesAsync(CancellationToken cancellationToken)
            => Task.FromResult<IEnumerable<Device>>(Devices);
    }

    private sealed class RecordingDeviceService : IDeviceService
    {
        public Device? LastDevice { get; private set; }
        public TaskCompletionSource<Device> Tcs { get; } = new();
        public Task<IEnumerable<Device>> GetDevicesAsync(CancellationToken cancellationToken = default)
            => Task.FromResult<IEnumerable<Device>>(Array.Empty<Device>());
        public Task<Device?> GetDeviceAsync(string ip, int? port, CancellationToken cancellationToken = default)
            => Task.FromResult<Device?>(null);
        public Task AddOrUpdateDeviceAsync(Device device, CancellationToken cancellationToken = default)
        {
            LastDevice = device;
            Tcs.TrySetResult(device);
            return Task.CompletedTask;
        }
        public Task DeleteDeviceAsync(string ip, int? port, CancellationToken cancellationToken = default)
            => Task.CompletedTask;
    }

    private sealed class RecordingDeviceGroupService : IDeviceGroupService
    {
        public List<DeviceGroup> Groups { get; } = new();
        public TaskCompletionSource<(string ip, int? port, int? groupId)> AssignTcs { get; } = new();
        public TaskCompletionSource<DeviceGroup> UpdateTcs { get; } = new();
        public DeviceGroup? UpdatedGroup { get; private set; }
        public Task<IEnumerable<DeviceGroup>> GetGroupsAsync(CancellationToken cancellationToken = default)
            => Task.FromResult<IEnumerable<DeviceGroup>>(Groups);
        public Task<int> CreateGroupAsync(string name, CancellationToken cancellationToken = default)
            => Task.FromResult(0);
        public Task UpdateGroupAsync(DeviceGroup group, CancellationToken cancellationToken = default)
        {
            UpdatedGroup = group;
            UpdateTcs.TrySetResult(group);
            return Task.CompletedTask;
        }
        public Task DeleteGroupAsync(int groupId, CancellationToken cancellationToken = default)
            => Task.CompletedTask;
        public Task AssignDeviceToGroupAsync(string deviceIp, int? devicePort, int? groupId, CancellationToken cancellationToken = default)
        {
            AssignTcs.TrySetResult((deviceIp, devicePort, groupId));
            return Task.CompletedTask;
        }
        public Task<int?> GetDeviceGroupIdAsync(string deviceIp, int? devicePort, CancellationToken cancellationToken = default)
            => Task.FromResult<int?>(null);
    }

    private sealed class StubDeviceFileService : IDeviceFileService
    {
        public Task<IEnumerable<string>> ListFilesAsync(Device device, string? extensionFilter = null, CancellationToken cancellationToken = default)
            => Task.FromResult<IEnumerable<string>>(Array.Empty<string>());
        public Task<int> DownloadUnseenFilesAsync(Device device, string basePath, CancellationToken cancellationToken = default)
            => Task.FromResult(0);
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
    public async Task ChangingHostname_PersistsDevice()
    {
        var scanner = new StubScannerService();
        scanner.Devices.Add(new Device { Ip = "1.2.3.4", Hostname = "old" });
        var dialog = new StubDialogService();
        var deviceService = new RecordingDeviceService();
        var groupService = new RecordingDeviceGroupService();
        var fileService = new StubDeviceFileService();
        var vm = new ScannerStatusViewModel(scanner, dialog, deviceService, groupService, fileService);

        await vm.RefreshCommand.ExecuteAsync(null);
        vm.Devices[0].Hostname = "new";
        await deviceService.Tcs.Task;

        Assert.Equal("new", deviceService.LastDevice?.Hostname);
    }

    [Fact]
    public async Task ChangingGroup_AssignsDevice()
    {
        var scanner = new StubScannerService();
        scanner.Devices.Add(new Device { Ip = "1.2.3.4" });
        var dialog = new StubDialogService();
        var deviceService = new RecordingDeviceService();
        var groupService = new RecordingDeviceGroupService();
        groupService.Groups.Add(new DeviceGroup { Id = 1, Name = "G1" });
        var fileService = new StubDeviceFileService();
        var vm = new ScannerStatusViewModel(scanner, dialog, deviceService, groupService, fileService);

        await vm.RefreshCommand.ExecuteAsync(null);
        vm.Devices[0].GroupId = 1;
        var assignment = await groupService.AssignTcs.Task;

        Assert.Equal("1.2.3.4", assignment.ip);
        Assert.Null(assignment.port);
        Assert.Equal(1, assignment.groupId);
    }

    [Fact]
    public async Task RenamingGroup_UpdatesService()
    {
        var scanner = new StubScannerService();
        var dialog = new StubDialogService();
        var deviceService = new RecordingDeviceService();
        var groupService = new RecordingDeviceGroupService();
        groupService.Groups.Add(new DeviceGroup { Id = 1, Name = "Old" });
        var fileService = new StubDeviceFileService();
        var vm = new ScannerStatusViewModel(scanner, dialog, deviceService, groupService, fileService);

        await vm.RefreshCommand.ExecuteAsync(null);
        vm.SelectedGroup = vm.Groups[0];
        vm.GroupName = "New";
        await vm.RenameGroupCommand.ExecuteAsync(null);
        var updated = await groupService.UpdateTcs.Task;

        Assert.Equal(1, updated.Id);
        Assert.Equal("New", updated.Name);
    }
}
