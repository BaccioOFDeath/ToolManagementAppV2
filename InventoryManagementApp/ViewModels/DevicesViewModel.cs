using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Net.NetworkInformation;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using InventoryManagementApp.Interfaces;
using InventoryManagementApp.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualBasic;

namespace InventoryManagementApp.ViewModels
{
    public class DevicesViewModel : ObservableObject
    {
        private readonly IDeviceDiscoveryService _discoveryService;
        private readonly IDeviceFileService _fileService;
        private readonly IDialogService _dialogService;
        private readonly IDeviceService _deviceService;
        private readonly IDeviceGroupService _groupService;
        private readonly ILogger<DevicesViewModel> _logger;

        public ObservableCollection<Device> Devices { get; } = new();
        public ObservableCollection<string> DeviceFiles { get; } = new();
        public ObservableCollection<DeviceGroup> Groups { get; } = new();

        private string _fileExtensionFilter = "*.*";
        public string FileExtensionFilter
        {
            get => _fileExtensionFilter;
            set => SetProperty(ref _fileExtensionFilter, value);
        }

        private Device? _selectedDevice;
        public Device? SelectedDevice
        {
            get => _selectedDevice;
            set
            {
                if (SetProperty(ref _selectedDevice, value))
                {
                    DeviceFiles.Clear();
                    PullAllReportsCommand.NotifyCanExecuteChanged();
                    PingSelectedDeviceCommand.NotifyCanExecuteChanged();
                }
            }
        }

        public IAsyncRelayCommand RefreshCommand { get; }
        public IAsyncRelayCommand PullAllReportsCommand { get; }
        public IAsyncRelayCommand PingSelectedDeviceCommand { get; }
        public IAsyncRelayCommand AddDeviceCommand { get; }
        public IAsyncRelayCommand AddGroupCommand { get; }
        public IAsyncRelayCommand RenameGroupCommand { get; }

        private double _discoveryProgress;
        public double DiscoveryProgress
        {
            get => _discoveryProgress;
            private set => SetProperty(ref _discoveryProgress, value);
        }

        private bool _isDiscovering;
        public bool IsDiscovering
        {
            get => _isDiscovering;
            private set
            {
                if (SetProperty(ref _isDiscovering, value))
                {
                    RefreshCommand.NotifyCanExecuteChanged();
                }
            }
        }

        public Func<string?> PromptForIpPort { get; set; } = () =>
            Interaction.InputBox("Enter device IP:port:", "Add Device", string.Empty);

        public Func<string?> PromptForGroupName { get; set; } = () =>
            Interaction.InputBox("Enter group name:", "Add Group", string.Empty);

        private DeviceGroup? _selectedGroup;
        public DeviceGroup? SelectedGroup
        {
            get => _selectedGroup;
            set
            {
                if (SetProperty(ref _selectedGroup, value))
                {
                    GroupName = value?.Name ?? string.Empty;
                }
            }
        }

        private string _groupName = string.Empty;
        public string GroupName
        {
            get => _groupName;
            set => SetProperty(ref _groupName, value);
        }

        public DevicesViewModel(IDeviceDiscoveryService discoveryService,
                                 IDeviceFileService fileService,
                                 IDialogService dialogService,
                                 IDeviceService deviceService,
                                 IDeviceGroupService groupService,
                                 ILogger<DevicesViewModel>? logger = null)
        {
            _discoveryService = discoveryService;
            _fileService = fileService;
            _dialogService = dialogService;
            _deviceService = deviceService;
            _groupService = groupService;
            _logger = logger ?? NullLogger<DevicesViewModel>.Instance;

            RefreshCommand = new AsyncRelayCommand(RefreshAsync, () => !IsDiscovering);
            PullAllReportsCommand = new AsyncRelayCommand(PullAllReportsAsync, CanPullAllReports);
            PingSelectedDeviceCommand = new AsyncRelayCommand(PingSelectedDeviceAsync, () => SelectedDevice != null);
            AddDeviceCommand = new AsyncRelayCommand(AddDeviceAsync);
            AddGroupCommand = new AsyncRelayCommand(AddGroupAsync);
            RenameGroupCommand = new AsyncRelayCommand(RenameGroupAsync);
        }

        private async Task RefreshAsync()
        {
            try
            {
                IsDiscovering = true;
                DiscoveryProgress = 0;

                foreach (var d in Devices)
                    d.PropertyChanged -= Device_PropertyChanged;
                Devices.Clear();

                var groups = await _groupService.GetGroupsAsync();
                Groups.Clear();
                foreach (var g in groups)
                    Groups.Add(g);

                var progress = new Progress<double>(p => DiscoveryProgress = p);

                await foreach (var d in _discoveryService.DiscoverDevicesAsync(progress))
                {
                    var device = new Device
                    {
                        Ip = d.Ip,
                        Hostname = d.Hostname,
                        MacAddress = d.MacAddress,
                        Protocol = d.Protocols.FirstOrDefault(DeviceProtocol.Unknown),
                        Protocols = d.Protocols.ToList(),
                        ProtocolsDisplay = d.Protocols.Count > 0
                            ? string.Join(", ", d.Protocols)
                            : DeviceProtocol.Unknown.ToString(),
                        Status = d.IsOnline ? "Online" : "Offline"
                    };
                    try
                    {
                        device.GroupId = await _groupService.GetDeviceGroupIdAsync(device.Ip, device.Port);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to get group for device {Ip}", device.Ip);
                    }
                    device.PropertyChanged += Device_PropertyChanged;
                    Devices.Add(device);
                }

                if (Devices.Count == 0 && !_discoveryService.HasConfiguredSubnets)
                {
                    await _dialogService.ShowInfoAsync("No subnets configured for device discovery.", "Devices");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to discover devices");
                await _dialogService.ShowInfoAsync($"Failed to discover devices: {ex.Message}", "Devices");
            }
            finally
            {
                IsDiscovering = false;
            }
        }

        private bool CanPullAllReports() => SelectedDevice != null;

        private async Task PullAllReportsAsync()
        {
            if (SelectedDevice == null) return;
            try
            {
                DeviceFiles.Clear();
                var filter = FileExtensionFilter == "*.*" ? null : FileExtensionFilter;
                var files = await _fileService.ListFilesAsync(SelectedDevice, filter, CancellationToken.None);
                foreach (var f in files)
                    DeviceFiles.Add(f);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load files for {Ip}", SelectedDevice.Ip);
                await _dialogService.ShowInfoAsync($"Failed to pull reports: {ex.Message}", "Devices");
            }
        }

        private async Task PingSelectedDeviceAsync()
        {
            if (SelectedDevice == null) return;
            try
            {
                var success = await SafePingAsync(SelectedDevice.Ip, 1000, CancellationToken.None);
                SelectedDevice.Status = success ? "Online" : "Offline";
                if (success)
                    SelectedDevice.LastSeen = DateTime.UtcNow;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to ping device {Ip}", SelectedDevice.Ip);
            }
        }

        private static async Task<bool> SafePingAsync(string ip, int timeoutMs, CancellationToken ct)
        {
            using var ping = new Ping();
            try
            {
                var pingTask = ping.SendPingAsync(ip, timeoutMs);
                var completed = await Task.WhenAny(pingTask, Task.Delay(Timeout.InfiniteTimeSpan, ct)).ConfigureAwait(false);
                if (completed != pingTask)
                    return false;
                var reply = await pingTask.ConfigureAwait(false);
                return reply.Status == IPStatus.Success;
            }
            catch (OperationCanceledException) { return false; }
            catch { return false; }
        }

        private async Task AddDeviceAsync()
        {
            try
            {
                var input = PromptForIpPort?.Invoke();
                if (string.IsNullOrWhiteSpace(input))
                    return;

                string ip;
                int? port = null;
                var parts = input.Split(':');
                ip = parts[0];
                if (parts.Length > 1 && int.TryParse(parts[1], out var parsedPort))
                    port = parsedPort;

                await _deviceService.AddOrUpdateDeviceAsync(new Device { Ip = ip, Port = port });
                await RefreshAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to add device");
                await _dialogService.ShowInfoAsync($"Failed to add device: {ex.Message}", "Devices");
            }
        }

        private async Task AddGroupAsync()
        {
            try
            {
                var name = PromptForGroupName?.Invoke();
                if (string.IsNullOrWhiteSpace(name))
                    return;

                await _groupService.CreateGroupAsync(name);
                await RefreshAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to add device group");
                await _dialogService.ShowInfoAsync($"Failed to add group: {ex.Message}", "Devices");
            }
        }

        private async Task RenameGroupAsync()
        {
            if (SelectedGroup is null || string.IsNullOrWhiteSpace(GroupName))
                return;

            try
            {
                SelectedGroup.Name = GroupName;
                await _groupService.UpdateGroupAsync(SelectedGroup);
                await RefreshAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to rename group {GroupId}", SelectedGroup.Id);
                await _dialogService.ShowInfoAsync($"Failed to rename group: {ex.Message}", "Devices");
            }
        }

        async void Device_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (sender is not Device device)
                return;

            if (e.PropertyName == nameof(Device.Hostname))
            {
                try
                {
                    await _deviceService.AddOrUpdateDeviceAsync(device);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to update device {Ip}", device.Ip);
                }
            }
            else if (e.PropertyName == nameof(Device.GroupId))
            {
                try
                {
                    await _groupService.AssignDeviceToGroupAsync(device.Ip, device.Port, device.GroupId);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to assign device {Ip} to group {Group}", device.Ip, device.GroupId);
                }
            }
        }
    }
}
