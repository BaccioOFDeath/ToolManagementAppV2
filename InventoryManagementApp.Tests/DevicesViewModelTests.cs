using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using InventoryManagementApp.Interfaces;
using InventoryManagementApp.Models;
using InventoryManagementApp.ViewModels;
using Xunit;

public class DevicesViewModelTests
{
    private sealed class StubDiscoveryService : IDeviceDiscoveryService
    {
        public List<DiscoveredDevice> Devices { get; } = new();
        public Task<IReadOnlyList<DiscoveredDevice>> DiscoverDevicesAsync(CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<DiscoveredDevice>>(Devices);
    }

    private sealed class RecordingDeviceFileService : IDeviceFileService
    {
        public List<string> Files { get; } = new();
        public Device? LastDevice { get; private set; }
        public string? LastExtensionFilter { get; private set; }
        public Task<IEnumerable<string>> ListFilesAsync(Device device, string? extensionFilter = null, CancellationToken cancellationToken = default)
        {
            LastDevice = device;
            LastExtensionFilter = extensionFilter;
            return Task.FromResult<IEnumerable<string>>(Files);
        }

        public Task<int> DownloadUnseenFilesAsync(Device device, string basePath, CancellationToken cancellationToken = default)
            => Task.FromResult(0);
    }

    private sealed class StubDialogService : IDialogService
    {
        public void ShowInfo(string message, string title) { }
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

    [Fact]
    public async Task RefreshCommand_LoadsDevices()
    {
        var discovery = new StubDiscoveryService();
        var fileService = new RecordingDeviceFileService();
        var dialog = new StubDialogService();
        discovery.Devices.Add(new DiscoveredDevice { Ip = "1.2.3.4", Hostname = "test", IsOnline = true, Protocols = new List<DeviceProtocol> { DeviceProtocol.Ftp } });
        var vm = new DevicesViewModel(discovery, fileService, dialog);

        await vm.RefreshCommand.ExecuteAsync(null);

        Assert.Single(vm.Devices);
        Assert.Equal("1.2.3.4", vm.Devices[0].Ip);
        Assert.Equal("Online", vm.Devices[0].Status);
    }

    [Fact]
    public async Task PullAllReportsCommand_LoadsFiles()
    {
        var discovery = new StubDiscoveryService();
        var fileService = new RecordingDeviceFileService();
        var dialog = new StubDialogService();
        discovery.Devices.Add(new DiscoveredDevice { Ip = "1.2.3.4", Hostname = "test", IsOnline = true, Protocols = new List<DeviceProtocol>() });
        fileService.Files.AddRange(new[] { "a.txt", "b.txt" });
        var vm = new DevicesViewModel(discovery, fileService, dialog);

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
        var dialog = new StubDialogService();
        discovery.Devices.Add(new DiscoveredDevice { Ip = "1.2.3.4", Hostname = "test", IsOnline = true, Protocols = new List<DeviceProtocol>() });
        var vm = new DevicesViewModel(discovery, fileService, dialog);

        await vm.RefreshCommand.ExecuteAsync(null);
        vm.SelectedDevice = vm.Devices[0];
        vm.FileExtensionFilter = ".txt";
        await vm.PullAllReportsCommand.ExecuteAsync(null);

        Assert.Equal(".txt", fileService.LastExtensionFilter);
    }
}
