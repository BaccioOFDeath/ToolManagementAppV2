using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DeviceManagementApp.Interfaces;
using DeviceManagementApp.Models;
using DeviceManagementApp.ViewModels;
using DeviceManagementApp.Views.Pages;
using System.Windows.Controls;
using Xunit;

namespace DeviceManagementApp.Tests
{
    public class DashboardViewModelTests
    {
        private sealed class StubDeviceService : IDeviceService
        {
            public List<Device> Devices { get; } = new();
            public Task<IEnumerable<Device>> GetDevicesAsync(CancellationToken cancellationToken = default)
                => Task.FromResult<IEnumerable<Device>>(Devices);
            public Task<Device?> GetDeviceAsync(string ip, int? port, CancellationToken cancellationToken = default)
                => Task.FromResult<Device?>(null);
            public Task AddOrUpdateDeviceAsync(Device device, CancellationToken cancellationToken = default)
                => Task.CompletedTask;
            public Task DeleteDeviceAsync(string ip, int? port, CancellationToken cancellationToken = default)
                => Task.CompletedTask;
        }

        private sealed class DummyNavigationService : INavigationService
        {
            public void Navigate(Page page) { }
        }

        [Fact]
        public async Task LoadAsync_CalculatesStats()
        {
            var service = new StubDeviceService();
            service.Devices.Add(new Device { AssignedUserId = 1 });
            service.Devices.Add(new Device { AssignedUserId = null });
            var nav = new DummyNavigationService();
            var devicesVm = new DevicesViewModel(new DummyDiscoveryService(), new DummyFileService(), new DummyDialogService(), new StubDeviceService(), new DummyGroupService());
            var vm = new DashboardViewModel(service, nav, devicesVm);

            await vm.LoadAsync(CancellationToken.None);

            Assert.Contains(vm.StatCards, s => s.Title == "Total Devices" && s.Value == "2");
            Assert.Contains(vm.StatCards, s => s.Title == "Assigned Devices" && s.Value == "1");
            Assert.Contains(vm.StatCards, s => s.Title == "Unassigned Devices" && s.Value == "1");
        }

        private sealed class DummyDiscoveryService : IDeviceDiscoveryService
        {
            public Task<IEnumerable<Device>> DiscoverDevicesAsync(IProgress<double>? progress = null, CancellationToken cancellationToken = default)
                => Task.FromResult<IEnumerable<Device>>(Enumerable.Empty<Device>());
        }
        private sealed class DummyFileService : IDeviceFileService
        {
            public Task<IEnumerable<string>> ListFilesAsync(Device device, string? extensionFilter = null, CancellationToken cancellationToken = default)
                => Task.FromResult<IEnumerable<string>>(Enumerable.Empty<string>());
            public Task<int> DownloadUnseenFilesAsync(Device device, string basePath, CancellationToken cancellationToken = default)
                => Task.FromResult(0);
        }
        private sealed class DummyDialogService : IDialogService
        {
            public void ShowInfo(string message, string title) { }
            public bool ShowConfirmation(string message, string title) => true;
        }
        private sealed class DummyGroupService : IDeviceGroupService
        {
            public Task<IEnumerable<DeviceGroup>> GetGroupsAsync(CancellationToken cancellationToken = default) => Task.FromResult<IEnumerable<DeviceGroup>>(Enumerable.Empty<DeviceGroup>());
            public Task<int> CreateGroupAsync(string name, CancellationToken cancellationToken = default) => Task.FromResult(0);
            public Task UpdateGroupAsync(DeviceGroup group, CancellationToken cancellationToken = default) => Task.CompletedTask;
            public Task DeleteGroupAsync(int groupId, CancellationToken cancellationToken = default) => Task.CompletedTask;
            public Task AssignDeviceToGroupAsync(string deviceIp, int? devicePort, int? groupId, CancellationToken cancellationToken = default) => Task.CompletedTask;
            public Task<int?> GetDeviceGroupIdAsync(string deviceIp, int? devicePort, CancellationToken cancellationToken = default) => Task.FromResult<int?>(null);
        }
    }
}
