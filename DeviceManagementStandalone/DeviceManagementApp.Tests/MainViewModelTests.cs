using System;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Collections.Generic;
using System.Threading.Tasks;
using DeviceManagementApp.Interfaces;
using DeviceManagementApp.Models;
using DeviceManagementApp.ViewModels;
using Xunit;

namespace DeviceManagementApp.Tests
{
    public class MainViewModelTests
    {
        [Fact]
        public void OpenDevicesCommand_NavigatesToDevicesPage()
        {
            Exception? threadEx = null;
            Page? currentPage = null;
            string? currentTitle = null;

            var thread = new Thread(() =>
            {
                try
                {
                    var app = new Application();
                    var nav = new TestNavigationService();
                    var devicesVm = new DevicesViewModel(new DummyDiscoveryService(), new DummyFileService(), new DummyDialogService(), new DummyDeviceService(), new DummyDeviceGroupService());
                    var dashboardVm = new DashboardViewModel(new DummyDeviceService(), nav, devicesVm);
                    var settingsVm = new SettingsViewModel(new DummySettingsService(), new DummyDialogService());
                    var staffVm = new StaffManagementViewModel(new DummyStaffService(), new DummyDialogService());
                    var assetsVm = new AssetsViewModel(new DummyAssetService(), new DummyAssetAssignmentService(), new DummyDialogService(), new DummyStaffService());
                    var vm = new MainViewModel(nav, devicesVm, dashboardVm, settingsVm, staffVm, assetsVm);
                    vm.OpenDevicesCommand.Execute(null);
                    currentPage = vm.CurrentPage;
                    currentTitle = vm.CurrentPageTitle;
                    Application.Current?.Shutdown();
                }
                catch (Exception ex)
                {
                    threadEx = ex;
                }
            });
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            thread.Join();

            if (threadEx != null) throw threadEx;
            Assert.IsType<DeviceManagementApp.Views.Pages.DevicesPage>(currentPage);
            Assert.Equal("Devices", currentTitle);
        }

        [Fact]
        public void DevicesViewModel_ViewDetailsRequested_Navigates()
        {
            Exception? threadEx = null;
            bool navigated = false;

            var thread = new Thread(() =>
            {
                try
                {
                    var app = new Application();
                    var nav = new TestNavigationService { OnNavigate = p => navigated = p is DeviceManagementApp.Views.Pages.DeviceDetailsPage };
                    var devicesVm = new DevicesViewModel(new DummyDiscoveryService(), new DummyFileService(), new DummyDialogService(), new DummyDeviceService(), new DummyDeviceGroupService())
                    {
                        SelectedDevice = new Device()
                    };
                    var dashboardVm = new DashboardViewModel(new DummyDeviceService(), nav, devicesVm);
                    var settingsVm = new SettingsViewModel(new DummySettingsService(), new DummyDialogService());
                    var staffVm = new StaffManagementViewModel(new DummyStaffService(), new DummyDialogService());
                    var assetsVm = new AssetsViewModel(new DummyAssetService(), new DummyAssetAssignmentService(), new DummyDialogService(), new DummyStaffService());
                    var vm = new MainViewModel(nav, devicesVm, dashboardVm, settingsVm, staffVm, assetsVm);
                    devicesVm.ViewDetailsCommand.Execute(null);
                    Application.Current?.Shutdown();
                }
                catch (Exception ex)
                {
                    threadEx = ex;
                }
            });
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            thread.Join();

            if (threadEx != null) throw threadEx;
            Assert.True(navigated);
        }

        [Fact]
        public void OpenDashboardCommand_NavigatesToDashboardPage()
        {
            Exception? threadEx = null;
            Page? currentPage = null;

            var thread = new Thread(() =>
            {
                try
                {
                    var app = new Application();
                    var nav = new TestNavigationService();
                    var devicesVm = new DevicesViewModel(new DummyDiscoveryService(), new DummyFileService(), new DummyDialogService(), new DummyDeviceService(), new DummyDeviceGroupService());
                    var dashboardVm = new DashboardViewModel(new DummyDeviceService(), nav, devicesVm);
                    var settingsVm = new SettingsViewModel(new DummySettingsService(), new DummyDialogService());
                    var staffVm = new StaffManagementViewModel(new DummyStaffService(), new DummyDialogService());
                    var assetsVm = new AssetsViewModel(new DummyAssetService(), new DummyAssetAssignmentService(), new DummyDialogService(), new DummyStaffService());
                    var vm = new MainViewModel(nav, devicesVm, dashboardVm, settingsVm, staffVm, assetsVm);
                    vm.OpenDashboardCommand.Execute(null);
                    currentPage = vm.CurrentPage;
                    Application.Current?.Shutdown();
                }
                catch (Exception ex) { threadEx = ex; }
            });
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            thread.Join();
            if (threadEx != null) throw threadEx;
            Assert.IsType<DeviceManagementApp.Views.Pages.DashboardPage>(currentPage);
        }

        [Fact]
        public void OpenSettingsCommand_NavigatesToSettingsPage()
        {
            Exception? threadEx = null;
            Page? currentPage = null;

            var thread = new Thread(() =>
            {
                try
                {
                    var app = new Application();
                    var nav = new TestNavigationService();
                    var devicesVm = new DevicesViewModel(new DummyDiscoveryService(), new DummyFileService(), new DummyDialogService(), new DummyDeviceService(), new DummyDeviceGroupService());
                    var dashboardVm = new DashboardViewModel(new DummyDeviceService(), nav, devicesVm);
                    var settingsVm = new SettingsViewModel(new DummySettingsService(), new DummyDialogService());
                    var staffVm = new StaffManagementViewModel(new DummyStaffService(), new DummyDialogService());
                    var assetsVm = new AssetsViewModel(new DummyAssetService(), new DummyAssetAssignmentService(), new DummyDialogService(), new DummyStaffService());
                    var vm = new MainViewModel(nav, devicesVm, dashboardVm, settingsVm, staffVm, assetsVm);
                    vm.OpenSettingsCommand.Execute(null);
                    currentPage = vm.CurrentPage;
                    Application.Current?.Shutdown();
                }
                catch (Exception ex) { threadEx = ex; }
            });
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            thread.Join();
            if (threadEx != null) throw threadEx;
            Assert.IsType<DeviceManagementApp.Views.Pages.SettingsPage>(currentPage);
        }

        [Fact]
        public void OpenAssetsCommand_LoadsAssetsBeforeShowingPage()
        {
            Exception? threadEx = null;
            Page? beforePage = null;
            Page? afterPage = null;
            int assetsCount = 0;

            var thread = new Thread(() =>
            {
                try
                {
                    var app = new Application();
                    var nav = new TestNavigationService();
                    var tcs = new TaskCompletionSource<IEnumerable<Asset>>();
                    var assetService = new TestAssetService(tcs.Task);
                    var assetsVm = new AssetsViewModel(assetService, new DummyAssetAssignmentService(), new DummyDialogService(), new DummyStaffService());
                    var staffVm = new StaffManagementViewModel(new DummyStaffService(), new DummyDialogService());
                    var devicesVm = new DevicesViewModel(new DummyDiscoveryService(), new DummyFileService(), new DummyDialogService(), new DummyDeviceService(), new DummyDeviceGroupService());
                    var dashboardVm = new DashboardViewModel(new DummyDeviceService(), nav, devicesVm);
                    var settingsVm = new SettingsViewModel(new DummySettingsService(), new DummyDialogService());
                    var vm = new MainViewModel(nav, devicesVm, dashboardVm, settingsVm, staffVm, assetsVm);

                    vm.OpenAssetsCommand.Execute(null);
                    beforePage = vm.CurrentPage;
                    tcs.SetResult(new[] { new Asset { AssetId = 1, Name = "A" } });
                    Thread.Sleep(50);
                    afterPage = vm.CurrentPage;
                    assetsCount = assetsVm.Assets.Count;
                    Application.Current?.Shutdown();
                }
                catch (Exception ex)
                {
                    threadEx = ex;
                }
            });
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            thread.Join();

            if (threadEx != null) throw threadEx;
            Assert.Null(beforePage);
            Assert.IsType<DeviceManagementApp.Views.Pages.AssetsPage>(afterPage);
            Assert.Equal(1, assetsCount);
        }

        private sealed class TestNavigationService : INavigationService
        {
            public Action<Page>? OnNavigate { get; set; }
            public void Navigate(Page page) => OnNavigate?.Invoke(page);
        }

        private sealed class DummyDiscoveryService : IDeviceDiscoveryService
        {
            public Task<IEnumerable<Device>> DiscoverDevicesAsync(IProgress<double>? progress = null, CancellationToken cancellationToken = default)
                => Task.FromResult<IEnumerable<Device>>(Array.Empty<Device>());
        }

        private sealed class DummyFileService : IDeviceFileService
        {
            public Task<IEnumerable<string>> ListFilesAsync(Device device, string? extensionFilter = null, CancellationToken cancellationToken = default)
                => Task.FromResult<IEnumerable<string>>(Array.Empty<string>());
            public Task<int> DownloadUnseenFilesAsync(Device device, string basePath, CancellationToken cancellationToken = default)
                => Task.FromResult(0);
        }

        private sealed class DummyDialogService : IDialogService
        {
            public void ShowInfo(string message, string title) { }
            public bool ShowConfirmation(string message, string title) => true;
        }

        private sealed class DummyDeviceService : IDeviceService
        {
            public Task<IEnumerable<Device>> GetDevicesAsync(CancellationToken cancellationToken = default)
                => Task.FromResult<IEnumerable<Device>>(Array.Empty<Device>());
            public Task<Device?> GetDeviceAsync(string ip, int? port, CancellationToken cancellationToken = default)
                => Task.FromResult<Device?>(null);
            public Task AddOrUpdateDeviceAsync(Device device, CancellationToken cancellationToken = default)
                => Task.CompletedTask;
            public Task DeleteDeviceAsync(string ip, int? port, CancellationToken cancellationToken = default)
                => Task.CompletedTask;
        }

        private sealed class DummyDeviceGroupService : IDeviceGroupService
        {
            public Task<IEnumerable<DeviceGroup>> GetGroupsAsync(CancellationToken cancellationToken = default)
                => Task.FromResult<IEnumerable<DeviceGroup>>(Array.Empty<DeviceGroup>());
            public Task<int> CreateGroupAsync(string name, CancellationToken cancellationToken = default)
                => Task.FromResult(0);
            public Task UpdateGroupAsync(DeviceGroup group, CancellationToken cancellationToken = default)
                => Task.CompletedTask;
            public Task DeleteGroupAsync(int groupId, CancellationToken cancellationToken = default)
                => Task.CompletedTask;
            public Task AssignDeviceToGroupAsync(string deviceIp, int? devicePort, int? groupId, CancellationToken cancellationToken = default)
                => Task.CompletedTask;
            public Task<int?> GetDeviceGroupIdAsync(string deviceIp, int? devicePort, CancellationToken cancellationToken = default)
                => Task.FromResult<int?>(null);
        }

        private sealed class DummySettingsService : ISettingsService
        {
            public event EventHandler<IDictionary<ItemDetailField, bool>>? ItemDetailVisibilityChanged;
            public Task SaveSettingAsync(string key, string value, CancellationToken cancellationToken = default) => Task.CompletedTask;
            public Task<string?> GetSettingAsync(string? key, CancellationToken cancellationToken = default) => Task.FromResult<string?>(null);
            public Task<Dictionary<string, string>> GetAllSettingsAsync(CancellationToken cancellationToken = default) => Task.FromResult(new Dictionary<string, string>());
            public Task UpdateSettingsAsync(Dictionary<string, string> settings, CancellationToken cancellationToken = default) => Task.CompletedTask;
            public Task DeleteSettingAsync(string key, CancellationToken cancellationToken = default) => Task.CompletedTask;
            public Task<string?> GetThemeAsync(CancellationToken cancellationToken = default) => Task.FromResult<string?>(null);
            public Task SaveThemeAsync(string theme, CancellationToken cancellationToken = default) => Task.CompletedTask;
            public Task<int> GetPasswordIterationsAsync(CancellationToken cancellationToken = default) => Task.FromResult(0);
            public Task SavePasswordIterationsAsync(int iterations, CancellationToken cancellationToken = default) => Task.CompletedTask;
            public Task<int> GetAutoLogoutMinutesAsync(CancellationToken cancellationToken = default) => Task.FromResult(0);
            public Task SaveAutoLogoutMinutesAsync(int minutes, CancellationToken cancellationToken = default) => Task.CompletedTask;
            public Task<string> GetItemLabelSingularAsync(CancellationToken cancellationToken = default) => Task.FromResult("Device");
            public Task SaveItemLabelSingularAsync(string label, CancellationToken cancellationToken = default) => Task.CompletedTask;
            public Task<string> GetItemLabelPluralAsync(CancellationToken cancellationToken = default) => Task.FromResult("Devices");
            public Task SaveItemLabelPluralAsync(string label, CancellationToken cancellationToken = default) => Task.CompletedTask;
            public Task<IDictionary<ItemDetailField, bool>> GetItemDetailVisibilityAsync(CancellationToken cancellationToken = default) => Task.FromResult<IDictionary<ItemDetailField, bool>>(new Dictionary<ItemDetailField, bool>());
            public Task SaveItemDetailVisibilityAsync(IDictionary<ItemDetailField, bool> visibility, CancellationToken cancellationToken = default) => Task.CompletedTask;
        }

        private sealed class DummyAssetService : IAssetService
        {
            public Task<IEnumerable<Asset>> GetAssetsAsync(CancellationToken cancellationToken = default) => Task.FromResult<IEnumerable<Asset>>(Array.Empty<Asset>());
            public Task<Asset?> GetAssetAsync(int assetId, CancellationToken cancellationToken = default) => Task.FromResult<Asset?>(null);
            public Task AddOrUpdateAssetAsync(Asset asset, CancellationToken cancellationToken = default) => Task.CompletedTask;
            public Task DeleteAssetAsync(int assetId, CancellationToken cancellationToken = default) => Task.CompletedTask;
        }

        private sealed class DummyAssetAssignmentService : IAssetAssignmentService
        {
            public Task<AssetAssignment?> GetCurrentAssignmentAsync(int assetId, CancellationToken cancellationToken = default) => Task.FromResult<AssetAssignment?>(null);
            public Task AssignAsync(AssetAssignment assignment, CancellationToken cancellationToken = default) => Task.CompletedTask;
            public Task ReturnAsync(int assetId, CancellationToken cancellationToken = default) => Task.CompletedTask;
        }

        private sealed class DummyStaffService : IStaffService
        {
            public Task<IReadOnlyList<Staff>> GetStaffAsync(CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyList<Staff>>(Array.Empty<Staff>());
            public Task<int> AddStaffAsync(Staff staff, CancellationToken cancellationToken = default) => Task.FromResult(0);
            public Task UpdateStaffAsync(Staff staff, CancellationToken cancellationToken = default) => Task.CompletedTask;
            public Task DeleteStaffAsync(int staffId, CancellationToken cancellationToken = default) => Task.CompletedTask;
        }

        private sealed class TestAssetService : IAssetService
        {
            private readonly Task<IEnumerable<Asset>> _getAssetsTask;
            public TestAssetService(Task<IEnumerable<Asset>> getAssetsTask) => _getAssetsTask = getAssetsTask;
            public Task<IEnumerable<Asset>> GetAssetsAsync(CancellationToken cancellationToken = default) => _getAssetsTask;
            public Task<Asset?> GetAssetAsync(int assetId, CancellationToken cancellationToken = default) => Task.FromResult<Asset?>(null);
            public Task AddOrUpdateAssetAsync(Asset asset, CancellationToken cancellationToken = default) => Task.CompletedTask;
            public Task DeleteAssetAsync(int assetId, CancellationToken cancellationToken = default) => Task.CompletedTask;
        }
    }
}
