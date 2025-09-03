using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;
using InventoryManagementApp.Interfaces;
using InventoryManagementApp.Models;
using InventoryManagementApp.Services.Devices;
using InventoryManagementApp.ViewModels;
using Microsoft.Extensions.Configuration;
using Xunit;

public class DevicesViewModelTests
{
    private sealed class StubDiscoveryService : IDeviceDiscoveryService
    {
        public List<DiscoveredDevice> Devices { get; } = new();
        public bool HasConfiguredSubnets { get; set; } = true;

        public int DiscoverCallCount { get; private set; }

        public Task<IReadOnlyList<DiscoveredDevice>> DiscoverDevicesAsync(CancellationToken cancellationToken = default)
        {
            DiscoverCallCount++;
            return Task.FromResult<IReadOnlyList<DiscoveredDevice>>(Devices);
        }

        public async IAsyncEnumerable<DiscoveredDevice> DiscoverDevicesAsync(IProgress<double>? progress = null,
            [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            DiscoverCallCount++;
            var total = Devices.Count;
            int processed = 0;
            foreach (var d in Devices)
            {
                await Task.Delay(10, cancellationToken);
                processed++;
                progress?.Report((double)processed / total);
                yield return d;
            }
        }
    }

    private sealed class RecordingDeviceFileService : IDeviceFileService
    {
        public List<string> Files { get; } = new();
        public Device? LastDevice { get; private set; }
        public string? LastExtensionFilter { get; private set; }
        public string? LastBasePath { get; private set; }
        public int DownloadCallCount { get; private set; }
        public Task<IEnumerable<string>> ListFilesAsync(Device device, string? extensionFilter = null, CancellationToken cancellationToken = default)
        {
            LastDevice = device;
            LastExtensionFilter = extensionFilter;
            return Task.FromResult<IEnumerable<string>>(Files);
        }

        public Task<int> DownloadUnseenFilesAsync(Device device, string basePath, CancellationToken cancellationToken = default)
        {
            LastDevice = device;
            LastBasePath = basePath;
            DownloadCallCount++;
            return Task.FromResult(0);
        }
    }

    private sealed class RecordingDialogService : IDialogService
    {
        public string? LastInfoMessage { get; private set; }
        public void ShowInfo(string message, string title) => LastInfoMessage = message;
        public Task ShowInfoAsync(string message, string title)
        {
            LastInfoMessage = message;
            return Task.CompletedTask;
        }
        public bool ShowConfirmation(string message, string title) => false;
        public ItemModel? ShowEditItemDialog(ItemModel item) => null;
        public void ShowItemDetails(ItemModel item) { }
        public (CustomerModel customer, System.DateTime dueDate)? ShowRentItemDialog(ItemModel item, IEnumerable<CustomerModel> customers) => null;
        public CustomerModel? ShowAddCustomerDialog() => null;
        public CustomerModel? ShowEditCustomerDialog(CustomerModel customer) => null;
        public void ShowRentalsFilter(ManageRentalsViewModel viewModel) { }
        public void ShowRentalHistory(ItemModel item, IEnumerable<RentalModel> history) { }
        public Dictionary<string, string>? ShowImportMapping(IEnumerable<string> headers, IEnumerable<string> properties, IEnumerable<string>? requiredPropertyNames = null) => null;
        public System.Func<ItemModel, IEnumerable<string>>? ShowImageImportMapping() => null;
        public void ShowPrintPreview(System.Windows.Documents.FlowDocument document, string title, string description) { }
        public void ShowPrintLabelDialog() { }
    }

    private sealed class RecordingDeviceGroupService : IDeviceGroupService
    {
        public List<DeviceGroup> Groups { get; } = new();
        public TaskCompletionSource<(string ip, int? port, int? groupId)> AssignTcs { get; } = new();
        public TaskCompletionSource<DeviceGroup> UpdateTcs { get; } = new();
        public string? CreatedGroupName { get; private set; }
        public Task<IEnumerable<DeviceGroup>> GetGroupsAsync(CancellationToken cancellationToken = default)
            => Task.FromResult<IEnumerable<DeviceGroup>>(Groups);
        public Task<int> CreateGroupAsync(string name, CancellationToken cancellationToken = default)
        {
            CreatedGroupName = name;
            return Task.FromResult(0);
        }
        public Task UpdateGroupAsync(DeviceGroup group, CancellationToken cancellationToken = default)
        {
            UpdateTcs.TrySetResult(group);
            return Task.CompletedTask;
        }
        public int? DeletedGroupId { get; private set; }
        public Task DeleteGroupAsync(int groupId, CancellationToken cancellationToken = default)
        {
            DeletedGroupId = groupId;
            return Task.CompletedTask;
        }
        public Task AssignDeviceToGroupAsync(string deviceIp, int? devicePort, int? groupId, CancellationToken cancellationToken = default)
        {
            AssignTcs.TrySetResult((deviceIp, devicePort, groupId));
            return Task.CompletedTask;
        }
        public Task<int?> GetDeviceGroupIdAsync(string deviceIp, int? devicePort, CancellationToken cancellationToken = default)
            => Task.FromResult<int?>(null);
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

    [Fact]
    public async Task RefreshCommand_LoadsDevices()
    {
        var discovery = new StubDiscoveryService();
        var fileService = new RecordingDeviceFileService();
        var dialog = new RecordingDialogService();
        discovery.Devices.Add(new DiscoveredDevice { Ip = "1.2.3.4", Hostname = "test", IsOnline = true, Protocols = new List<DeviceProtocol> { DeviceProtocol.Ftp } });
        var vm = new DevicesViewModel(discovery, fileService, dialog, new RecordingDeviceService(), new RecordingDeviceGroupService());

        await vm.RefreshCommand.ExecuteAsync(null);

        Assert.Single(vm.Devices);
        Assert.Equal("1.2.3.4", vm.Devices[0].Ip);
        Assert.Equal("Online", vm.Devices[0].Status);
        Assert.Equal("Ftp", vm.Devices[0].ProtocolsDisplay);
    }

    [Fact]
    public async Task RefreshCommand_AggregatesMultipleProtocols()
    {
        var discovery = new StubDiscoveryService();
        var fileService = new RecordingDeviceFileService();
        var dialog = new RecordingDialogService();
        discovery.Devices.Add(new DiscoveredDevice
        {
            Ip = "1.2.3.5",
            Hostname = "multi",
            IsOnline = true,
            Protocols = new List<DeviceProtocol> { DeviceProtocol.Smb, DeviceProtocol.Http }
        });
        var vm = new DevicesViewModel(discovery, fileService, dialog, new RecordingDeviceService(), new RecordingDeviceGroupService());

        await vm.RefreshCommand.ExecuteAsync(null);

        Assert.Single(vm.Devices);
        Assert.Equal("Smb, Http", vm.Devices[0].ProtocolsDisplay);
    }

    [Fact]
    public async Task RefreshCommand_CopiesMacAddress()
    {
        var discovery = new StubDiscoveryService();
        var fileService = new RecordingDeviceFileService();
        var dialog = new RecordingDialogService();
        discovery.Devices.Add(new DiscoveredDevice
        {
            Ip = "1.2.3.6",
            Hostname = "mac",
            MacAddress = "aa-bb-cc-dd-ee-ff",
            IsOnline = true,
            Protocols = new List<DeviceProtocol>()
        });
        var vm = new DevicesViewModel(discovery, fileService, dialog, new RecordingDeviceService(), new RecordingDeviceGroupService());

        await vm.RefreshCommand.ExecuteAsync(null);

        Assert.Single(vm.Devices);
        Assert.Equal("aa-bb-cc-dd-ee-ff", vm.Devices[0].MacAddress);
    }

    [Fact]
    public async Task RefreshCommand_ReportsProgressAndAddsDevicesIncrementally()
    {
        var discovery = new StubDiscoveryService();
        var fileService = new RecordingDeviceFileService();
        var dialog = new RecordingDialogService();
        discovery.Devices.Add(new DiscoveredDevice { Ip = "1.1.1.1", Hostname = "a", IsOnline = true, Protocols = new List<DeviceProtocol>() });
        discovery.Devices.Add(new DiscoveredDevice { Ip = "1.1.1.2", Hostname = "b", IsOnline = true, Protocols = new List<DeviceProtocol>() });
        var vm = new DevicesViewModel(discovery, fileService, dialog, new RecordingDeviceService(), new RecordingDeviceGroupService());

        var task = vm.RefreshCommand.ExecuteAsync(null);
        await Task.Delay(15);

        Assert.Equal(1, vm.Devices.Count);
        Assert.InRange(vm.DiscoveryProgress, 0.45, 0.55);

        await task;

        Assert.Equal(2, vm.Devices.Count);
        Assert.Equal(1.0, vm.DiscoveryProgress, 3);
        Assert.False(vm.IsDiscovering);
    }

    [Fact]
    public async Task RefreshCommand_DisabledDuringDiscovery()
    {
        var discovery = new StubDiscoveryService();
        var fileService = new RecordingDeviceFileService();
        var dialog = new RecordingDialogService();
        discovery.Devices.Add(new DiscoveredDevice { Ip = "1.1.1.1", Hostname = "a", IsOnline = true, Protocols = new List<DeviceProtocol>() });
        var vm = new DevicesViewModel(discovery, fileService, dialog, new RecordingDeviceService(), new RecordingDeviceGroupService());

        Assert.True(vm.RefreshCommand.CanExecute(null));

        var task = vm.RefreshCommand.ExecuteAsync(null);
        while (!vm.IsDiscovering)
            await Task.Delay(1);

        Assert.False(vm.RefreshCommand.CanExecute(null));

        await task;

        Assert.True(vm.RefreshCommand.CanExecute(null));
    }

    [Fact]
    public async Task PullAllReportsCommand_LoadsFiles()
    {
        var discovery = new StubDiscoveryService();
        var fileService = new RecordingDeviceFileService();
        var dialog = new RecordingDialogService();
        discovery.Devices.Add(new DiscoveredDevice { Ip = "1.2.3.4", Hostname = "test", IsOnline = true, Protocols = new List<DeviceProtocol>() });
        fileService.Files.AddRange(new[] { "a.txt", "b.txt" });
        var vm = new DevicesViewModel(discovery, fileService, dialog, new RecordingDeviceService(), new RecordingDeviceGroupService());

        await vm.RefreshCommand.ExecuteAsync(null);
        vm.SelectedDevice = vm.Devices[0];
        await vm.PullAllReportsCommand.ExecuteAsync(null);

        Assert.Equal(2, vm.DeviceFiles.Count);
        Assert.Contains("a.txt", vm.DeviceFiles);
        Assert.Same(vm.SelectedDevice, fileService.LastDevice);
        Assert.Null(fileService.LastExtensionFilter);
    }

    [Fact]
    public async Task PullAllReportsCommand_UsesExtensionFilter()
    {
        var discovery = new StubDiscoveryService();
        var fileService = new RecordingDeviceFileService();
        var dialog = new RecordingDialogService();
        discovery.Devices.Add(new DiscoveredDevice { Ip = "1.2.3.4", Hostname = "test", IsOnline = true, Protocols = new List<DeviceProtocol>() });
        fileService.Files.AddRange(new[] { "a.txt", "b.txt" });
        var vm = new DevicesViewModel(discovery, fileService, dialog, new RecordingDeviceService(), new RecordingDeviceGroupService());

        await vm.RefreshCommand.ExecuteAsync(null);
        vm.SelectedDevice = vm.Devices[0];
        vm.FileExtensionFilter = ".txt";
        await vm.PullAllReportsCommand.ExecuteAsync(null);

        Assert.Equal(".txt", fileService.LastExtensionFilter);
        Assert.Equal(2, vm.DeviceFiles.Count);
        Assert.Contains("a.txt", vm.DeviceFiles);
    }

    [Fact]
    public async Task SelectedDeviceChange_ClearsDeviceFiles()
    {
        var discovery = new StubDiscoveryService();
        var fileService = new RecordingDeviceFileService();
        var dialog = new RecordingDialogService();
        discovery.Devices.Add(new DiscoveredDevice { Ip = "1.1.1.1", Hostname = "a", IsOnline = true, Protocols = new List<DeviceProtocol>() });
        discovery.Devices.Add(new DiscoveredDevice { Ip = "1.1.1.2", Hostname = "b", IsOnline = true, Protocols = new List<DeviceProtocol>() });
        fileService.Files.Add("a.txt");
        var vm = new DevicesViewModel(discovery, fileService, dialog, new RecordingDeviceService(), new RecordingDeviceGroupService());

        await vm.RefreshCommand.ExecuteAsync(null);
        vm.SelectedDevice = vm.Devices[0];
        await vm.PullAllReportsCommand.ExecuteAsync(null);
        Assert.Single(vm.DeviceFiles);

        vm.SelectedDevice = vm.Devices[1];

        Assert.Empty(vm.DeviceFiles);
    }

    [Fact]
    public async Task RefreshCommand_NoSubnets_ShowsWarning()
    {
        var discovery = new StubDiscoveryService { HasConfiguredSubnets = false };
        var fileService = new RecordingDeviceFileService();
        var dialog = new RecordingDialogService();

        var vm = new DevicesViewModel(discovery, fileService, dialog, new RecordingDeviceService(), new RecordingDeviceGroupService());

        await vm.RefreshCommand.ExecuteAsync(null);

        Assert.Equal("No subnets configured for device discovery.", dialog.LastInfoMessage);
    }

    [Fact]
    public async Task RefreshCommand_UnreachableHosts_NoUnobservedExceptions()
    {
        bool unobserved = false;
        void Handler(object? sender, UnobservedTaskExceptionEventArgs e) => unobserved = true;
        TaskScheduler.UnobservedTaskException += Handler;
        try
        {
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["DeviceDiscovery:Subnets:0"] = "203.0.113.0/30"
                })
                .Build();

            var discovery = new DeviceDiscoveryService(config);
            var fileService = new RecordingDeviceFileService();
            var dialog = new RecordingDialogService();
            var vm = new DevicesViewModel(discovery, fileService, dialog, new RecordingDeviceService(), new RecordingDeviceGroupService());

            await vm.RefreshCommand.ExecuteAsync(null);

            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();

            Assert.False(unobserved);
        }
        finally
        {
            TaskScheduler.UnobservedTaskException -= Handler;
        }
    }

    [Fact]
    public async Task AddDeviceCommand_AddsDeviceAndRefreshes()
    {
        var discovery = new StubDiscoveryService();
        var fileService = new RecordingDeviceFileService();
        var dialog = new RecordingDialogService();
        var deviceService = new RecordingDeviceService();
        var vm = new DevicesViewModel(discovery, fileService, dialog, deviceService, new RecordingDeviceGroupService());
        vm.PromptForIpPort = () => "1.2.3.4:1234";

        await vm.AddDeviceCommand.ExecuteAsync(null);

        Assert.Equal("1.2.3.4", deviceService.LastDevice?.Ip);
        Assert.Equal(1234, deviceService.LastDevice?.Port);
        Assert.Equal(1, discovery.DiscoverCallCount);
    }

    [Fact]
    public async Task ChangingHostname_PersistsDevice()
    {
        var discovery = new StubDiscoveryService();
        discovery.Devices.Add(new DiscoveredDevice { Ip = "1.2.3.4", Hostname = "old", IsOnline = true, Protocols = new List<DeviceProtocol>() });
        var fileService = new RecordingDeviceFileService();
        var dialog = new RecordingDialogService();
        var deviceService = new RecordingDeviceService();
        var groupService = new RecordingDeviceGroupService();
        var vm = new DevicesViewModel(discovery, fileService, dialog, deviceService, groupService);

        await vm.RefreshCommand.ExecuteAsync(null);
        vm.Devices[0].Hostname = "new";
        await deviceService.Tcs.Task;

        Assert.Equal("new", deviceService.LastDevice?.Hostname);
    }

    [Fact]
    public async Task ChangingUsername_PersistsDevice()
    {
        var discovery = new StubDiscoveryService();
        discovery.Devices.Add(new DiscoveredDevice { Ip = "1.2.3.4", IsOnline = true, Protocols = new List<DeviceProtocol>() });
        var fileService = new RecordingDeviceFileService();
        var dialog = new RecordingDialogService();
        var deviceService = new RecordingDeviceService();
        var groupService = new RecordingDeviceGroupService();
        var vm = new DevicesViewModel(discovery, fileService, dialog, deviceService, groupService);

        await vm.RefreshCommand.ExecuteAsync(null);
        vm.Devices[0].Username = "user";
        await deviceService.Tcs.Task;

        Assert.Equal("user", deviceService.LastDevice?.Username);
    }

    [Fact]
    public async Task ChangingPassword_PersistsDevice()
    {
        var discovery = new StubDiscoveryService();
        discovery.Devices.Add(new DiscoveredDevice { Ip = "1.2.3.4", IsOnline = true, Protocols = new List<DeviceProtocol>() });
        var fileService = new RecordingDeviceFileService();
        var dialog = new RecordingDialogService();
        var deviceService = new RecordingDeviceService();
        var groupService = new RecordingDeviceGroupService();
        var vm = new DevicesViewModel(discovery, fileService, dialog, deviceService, groupService);

        await vm.RefreshCommand.ExecuteAsync(null);
        vm.Devices[0].Password = "pass";
        await deviceService.Tcs.Task;

        Assert.Equal("pass", deviceService.LastDevice?.Password);
    }

    [Fact]
    public async Task ChangingDomain_PersistsDevice()
    {
        var discovery = new StubDiscoveryService();
        discovery.Devices.Add(new DiscoveredDevice { Ip = "1.2.3.4", IsOnline = true, Protocols = new List<DeviceProtocol>() });
        var fileService = new RecordingDeviceFileService();
        var dialog = new RecordingDialogService();
        var deviceService = new RecordingDeviceService();
        var groupService = new RecordingDeviceGroupService();
        var vm = new DevicesViewModel(discovery, fileService, dialog, deviceService, groupService);

        await vm.RefreshCommand.ExecuteAsync(null);
        vm.Devices[0].Domain = "domain";
        await deviceService.Tcs.Task;

        Assert.Equal("domain", deviceService.LastDevice?.Domain);
    }

    [Fact]
    public async Task ChangingGroup_AssignsDevice()
    {
        var discovery = new StubDiscoveryService();
        discovery.Devices.Add(new DiscoveredDevice { Ip = "1.2.3.4", IsOnline = true, Protocols = new List<DeviceProtocol>() });
        var fileService = new RecordingDeviceFileService();
        var dialog = new RecordingDialogService();
        var deviceService = new RecordingDeviceService();
        var groupService = new RecordingDeviceGroupService();
        groupService.Groups.Add(new DeviceGroup { Id = 1, Name = "G1" });
        var vm = new DevicesViewModel(discovery, fileService, dialog, deviceService, groupService);

        await vm.RefreshCommand.ExecuteAsync(null);
        vm.Devices[0].GroupId = 1;
        var assignment = await groupService.AssignTcs.Task;

        Assert.Equal("1.2.3.4", assignment.ip);
        Assert.Null(assignment.port);
        Assert.Equal(1, assignment.groupId);
    }

    [Fact]
    public async Task AddGroupCommand_CreatesGroupAndRefreshes()
    {
        var discovery = new StubDiscoveryService();
        var fileService = new RecordingDeviceFileService();
        var dialog = new RecordingDialogService();
        var groupService = new RecordingDeviceGroupService();
        var vm = new DevicesViewModel(discovery, fileService, dialog, new RecordingDeviceService(), groupService);
        vm.PromptForGroupName = () => "NewGroup";

        await vm.AddGroupCommand.ExecuteAsync(null);

        Assert.Equal("NewGroup", groupService.CreatedGroupName);
        Assert.Equal(1, discovery.DiscoverCallCount);
    }

    [Fact]
    public async Task RenameGroupCommand_UpdatesService()
    {
        var discovery = new StubDiscoveryService();
        var fileService = new RecordingDeviceFileService();
        var dialog = new RecordingDialogService();
        var groupService = new RecordingDeviceGroupService();
        groupService.Groups.Add(new DeviceGroup { Id = 1, Name = "Old" });
        var vm = new DevicesViewModel(discovery, fileService, dialog, new RecordingDeviceService(), groupService);

        await vm.RefreshCommand.ExecuteAsync(null);
        vm.SelectedGroup = vm.Groups[0];
        vm.GroupName = "New";
        await vm.RenameGroupCommand.ExecuteAsync(null);
        var updated = await groupService.UpdateTcs.Task;

        Assert.Equal(1, updated.Id);
        Assert.Equal("New", updated.Name);
    }

    [Fact]
    public async Task DeleteGroupCommand_DeletesGroupAndRefreshes()
    {
        var discovery = new StubDiscoveryService();
        var fileService = new RecordingDeviceFileService();
        var dialog = new RecordingDialogService();
        var groupService = new RecordingDeviceGroupService();
        var vm = new DevicesViewModel(discovery, fileService, dialog, new RecordingDeviceService(), groupService);
        vm.SelectedGroup = new DeviceGroup { Id = 1, Name = "Old" };

        await vm.DeleteGroupCommand.ExecuteAsync(null);

        Assert.Equal(1, groupService.DeletedGroupId);
        Assert.Equal(1, discovery.DiscoverCallCount);
    }

    [Fact]
    public async Task DownloadUnseenFilesCommand_UsesConfiguredPaths()
    {
        var discovery = new StubDiscoveryService();
        discovery.Devices.Add(new DiscoveredDevice { Ip = "1.2.3.4", IsOnline = true, Protocols = new List<DeviceProtocol>() });
        var fileService = new RecordingDeviceFileService();
        var dialog = new RecordingDialogService();
        var groupService = new RecordingDeviceGroupService();
        var vm = new DevicesViewModel(discovery, fileService, dialog, new RecordingDeviceService(), groupService);

        await vm.RefreshCommand.ExecuteAsync(null);
        vm.SourceFolder = "C:/src";
        vm.DestinationFolder = "C:/dest";
        vm.SelectedDevice = vm.Devices[0];

        await vm.DownloadUnseenFilesCommand.ExecuteAsync(null);

        Assert.Equal("C:/dest", fileService.LastBasePath);
        Assert.Equal(vm.SelectedDevice, fileService.LastDevice);
    }

    [Fact]
    public async Task DownloadUnseenFilesCommand_DoesNotRunIfSetupIncomplete()
    {
        var discovery = new StubDiscoveryService();
        discovery.Devices.Add(new DiscoveredDevice { Ip = "1.2.3.4", IsOnline = true, Protocols = new List<DeviceProtocol>() });
        var fileService = new RecordingDeviceFileService();
        var dialog = new RecordingDialogService();
        var groupService = new RecordingDeviceGroupService();
        var vm = new DevicesViewModel(discovery, fileService, dialog, new RecordingDeviceService(), groupService);

        await vm.RefreshCommand.ExecuteAsync(null);
        vm.SelectedDevice = vm.Devices[0];
        vm.SourceFolder = string.Empty;
        vm.DestinationFolder = "C:/dest";

        await vm.DownloadUnseenFilesCommand.ExecuteAsync(null);

        Assert.Equal(0, fileService.DownloadCallCount);
    }
}
