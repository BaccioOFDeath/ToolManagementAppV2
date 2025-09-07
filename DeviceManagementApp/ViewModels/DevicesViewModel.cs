using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Net.NetworkInformation;
using System.Windows.Data;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DeviceManagementApp.Interfaces;
using DeviceManagementApp.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualBasic;
using DeviceManagementApp.Views;

namespace DeviceManagementApp.ViewModels
{
    public class DevicesViewModel : ObservableObject
    {
        private readonly IDeviceDiscoveryService _discoveryService;
        private readonly IDeviceFileService _fileService;
        private readonly IDialogService _dialogService;
        private readonly IDeviceService _deviceService;
        private readonly IDeviceGroupService _groupService;
        private readonly IDeviceAssignmentService _assignmentService;
        private readonly IDeviceSoftwareService _softwareService;
        private readonly IStaffService _staffService;
        private readonly ILogger<DevicesViewModel> _logger;

        public ObservableCollection<Device> Devices { get; } = new();
        public ObservableCollection<string> DeviceFiles { get; } = new();
        public ObservableCollection<DeviceGroup> Groups { get; } = new();
        public ObservableCollection<DeviceSoftware> InstalledSoftware { get; } = new();
        public ObservableCollection<KeyValuePair<int?, string>> Departments { get; } = new() { new(null, "All Departments") };
        public ObservableCollection<KeyValuePair<int?, string>> Staff { get; } = new() { new(null, "All Staff") };

        public event EventHandler<Device>? ViewDetailsRequested;
        public ICollectionView DevicesView { get; }

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
                    InstalledSoftware.Clear();
                    if (value != null)
                        _ = LoadInstalledSoftwareAsync(value);
                    PullAllReportsCommand.NotifyCanExecuteChanged();
                    PingSelectedDeviceCommand.NotifyCanExecuteChanged();
                    DownloadUnseenFilesCommand.NotifyCanExecuteChanged();
                    ViewDetailsCommand.NotifyCanExecuteChanged();
                    AssignDeviceCommand.NotifyCanExecuteChanged();
                    ReturnDeviceCommand.NotifyCanExecuteChanged();
                }
            }
        }

        public IAsyncRelayCommand RefreshCommand { get; }
        public IAsyncRelayCommand PullAllReportsCommand { get; }
        public IAsyncRelayCommand PingSelectedDeviceCommand { get; }
        public IAsyncRelayCommand AddDeviceCommand { get; }
        public IAsyncRelayCommand AddGroupCommand { get; }
        public IAsyncRelayCommand RenameGroupCommand { get; }
        public IAsyncRelayCommand DeleteGroupCommand { get; }
        public IAsyncRelayCommand DownloadUnseenFilesCommand { get; }
        public IAsyncRelayCommand AssignDeviceCommand { get; }
        public IAsyncRelayCommand ReturnDeviceCommand { get; }
        public IRelayCommand ViewDetailsCommand { get; }

        private string _sourceFolder = string.Empty;
        public string SourceFolder
        {
            get => _sourceFolder;
            set
            {
                if (SetProperty(ref _sourceFolder, value))
                    DownloadUnseenFilesCommand.NotifyCanExecuteChanged();
            }
        }

        private string _destinationFolder = string.Empty;
        public string DestinationFolder
        {
            get => _destinationFolder;
            set
            {
                if (SetProperty(ref _destinationFolder, value))
                    DownloadUnseenFilesCommand.NotifyCanExecuteChanged();
            }
        }

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

        public Func<Device, DeviceAssignment?> PromptForAssignment { get; set; }

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

        private int? _departmentFilter;
        public int? DepartmentFilter
        {
            get => _departmentFilter;
            set
            {
                if (SetProperty(ref _departmentFilter, value))
                    DevicesView.Refresh();
            }
        }

        private int? _assignedUserFilter;
        public int? AssignedUserFilter
        {
            get => _assignedUserFilter;
            set
            {
                if (SetProperty(ref _assignedUserFilter, value))
                    DevicesView.Refresh();
            }
        }

        public DevicesViewModel(IDeviceDiscoveryService discoveryService,
                                 IDeviceFileService fileService,
                                 IDialogService dialogService,
                                 IDeviceService deviceService,
                                 IDeviceGroupService groupService,
                                 IDeviceAssignmentService? assignmentService = null,
                                 IDeviceSoftwareService? softwareService = null,
                                 IStaffService? staffService = null,
                                 ILogger<DevicesViewModel>? logger = null)
        {
            _discoveryService = discoveryService;
            _fileService = fileService;
            _dialogService = dialogService;
            _deviceService = deviceService;
            _groupService = groupService;
            _assignmentService = assignmentService ?? NullDeviceAssignmentService.Instance;
            _softwareService = softwareService ?? NullDeviceSoftwareService.Instance;
            _staffService = staffService ?? NullStaffService.Instance;
            _logger = logger ?? NullLogger<DevicesViewModel>.Instance;

            DevicesView = CollectionViewSource.GetDefaultView(Devices);
            DevicesView.Filter = FilterDevice;

            RefreshCommand = new AsyncRelayCommand(RefreshAsync, () => !IsDiscovering);
            PullAllReportsCommand = new AsyncRelayCommand(PullAllReportsAsync, CanPullAllReports);
            PingSelectedDeviceCommand = new AsyncRelayCommand(PingSelectedDeviceAsync, () => SelectedDevice != null);
            AddDeviceCommand = new AsyncRelayCommand(AddDeviceAsync);
            AddGroupCommand = new AsyncRelayCommand(AddGroupAsync);
            RenameGroupCommand = new AsyncRelayCommand(RenameGroupAsync);
            DeleteGroupCommand = new AsyncRelayCommand(DeleteGroupAsync);
            DownloadUnseenFilesCommand = new AsyncRelayCommand(DownloadUnseenFilesAsync, CanDownloadUnseenFiles);
            AssignDeviceCommand = new AsyncRelayCommand(AssignDeviceAsync, () => SelectedDevice != null);
            ReturnDeviceCommand = new AsyncRelayCommand(ReturnDeviceAsync, () => SelectedDevice?.AssignedUserId != null);
            ViewDetailsCommand = new RelayCommand(OnViewDetails, () => SelectedDevice != null);

            PromptForAssignment = device =>
            {
                var dialog = new AssignDeviceDialog();
                AssignDeviceViewModel vm = null!;
                dialog.DataContext = vm = new AssignDeviceViewModel(r => dialog.DialogResult = r)
                {
                    UserId = device.AssignedUserId ?? 0,
                    DepartmentId = device.DepartmentId
                };
                vm.Staff.Clear();
                foreach (var s in Staff.Where(s => s.Key.HasValue))
                    vm.Staff.Add(new KeyValuePair<int, string>(s.Key!.Value, s.Value));
                return dialog.ShowDialog() == true
                    ? new DeviceAssignment
                    {
                        DeviceIp = device.Ip,
                        UserId = vm.UserId,
                        DepartmentId = vm.DepartmentId,
                        AssignedDate = DateTime.UtcNow
                    }
                    : null;
            };
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
                Departments.Clear();
                Departments.Add(new(null, "All Departments"));
                Staff.Clear();
                Staff.Add(new(null, "All Staff"));
                var staffList = (await _staffService.GetStaffAsync()).ToList();
                foreach (var s in staffList)
                    Staff.Add(new(s.Key, s.Value));

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
                        Status = d.IsOnline ? "Online" : "Offline",
                        LastSeen = d.IsOnline ? DateTime.UtcNow : default
                    };
                    try
                    {
                        device.GroupId = await _groupService.GetDeviceGroupIdAsync(device.Ip, device.Port);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to get group for device {Ip}", device.Ip);
                    }
                    try
                    {
                        var assignment = await _assignmentService.GetCurrentAssignmentAsync(device.Ip).ConfigureAwait(false);
                        if (assignment != null)
                        {
                            device.AssignedUserId = assignment.UserId;
                            device.DepartmentId = assignment.DepartmentId;
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to get assignment for device {Ip}", device.Ip);
                    }
                    device.PropertyChanged += Device_PropertyChanged;
                    Devices.Add(device);
                    if (device.DepartmentId.HasValue && !Departments.Any(dp => dp.Key == device.DepartmentId))
                        Departments.Add(new(device.DepartmentId.Value, device.DepartmentId.Value.ToString()));
                    if (device.AssignedUserId.HasValue && !Staff.Any(s => s.Key == device.AssignedUserId))
                    {
                        var name = staffList.FirstOrDefault(s => s.Key == device.AssignedUserId.Value).Value
                                   ?? device.AssignedUserId.Value.ToString();
                        Staff.Add(new(device.AssignedUserId.Value, name));
                    }
                }

                if (Devices.Count == 0 && !_discoveryService.HasConfiguredSubnets)
                {
                    await _dialogService.ShowInfoAsync("No subnets configured for device discovery.", "Devices");
                }
                DevicesView.Refresh();
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

        private bool CanDownloadUnseenFiles()
            => SelectedDevice != null
               && !string.IsNullOrWhiteSpace(SourceFolder)
               && !string.IsNullOrWhiteSpace(DestinationFolder);

        private async Task DownloadUnseenFilesAsync()
        {
            if (SelectedDevice == null) return;
            var confirm = _dialogService.ShowConfirmation(
                $"Download unseen files from '{SourceFolder}' to '{DestinationFolder}'?",
                "Confirm Download");
            if (!confirm) return;
            try
            {
                await _fileService.DownloadUnseenFilesAsync(SelectedDevice, DestinationFolder, CancellationToken.None);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to download files for {Ip}", SelectedDevice.Ip);
                await _dialogService.ShowInfoAsync($"Failed to download files: {ex.Message}", "Devices");
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

                var id = await _groupService.CreateGroupAsync(name);
                var group = new DeviceGroup { Id = id, Name = name };
                Groups.Add(group);
                SelectedGroup = group;
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

        private async Task DeleteGroupAsync()
        {
            if (SelectedGroup is null)
                return;

            var groupId = SelectedGroup.Id;

            try
            {
                await _groupService.DeleteGroupAsync(groupId);
                SelectedGroup = null;
                await RefreshAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete group {GroupId}", groupId);
                await _dialogService.ShowInfoAsync($"Failed to delete group: {ex.Message}", "Devices");
            }
        }

        private async Task AssignDeviceAsync()
        {
            if (SelectedDevice is null)
                return;

            var assignment = PromptForAssignment?.Invoke(SelectedDevice);
            if (assignment == null)
                return;

            try
            {
                await _assignmentService.AssignAsync(assignment);
                SelectedDevice.AssignedUserId = assignment.UserId;
                SelectedDevice.DepartmentId = assignment.DepartmentId;
                var staffName = Staff.FirstOrDefault(s => s.Key == assignment.UserId).Value ?? assignment.UserId.ToString();
                if (!Staff.Any(s => s.Key == assignment.UserId))
                    Staff.Add(new(assignment.UserId, staffName));
                if (assignment.DepartmentId.HasValue && !Departments.Any(d => d.Key == assignment.DepartmentId))
                    Departments.Add(new(assignment.DepartmentId.Value, assignment.DepartmentId.Value.ToString()));
                DevicesView.Refresh();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to assign device {Ip}", assignment.DeviceIp);
                await _dialogService.ShowInfoAsync($"Failed to assign device: {ex.Message}", "Devices");
            }

            ReturnDeviceCommand.NotifyCanExecuteChanged();
            AssignDeviceCommand.NotifyCanExecuteChanged();
        }

        private async Task ReturnDeviceAsync()
        {
            if (SelectedDevice is null)
                return;

            try
            {
                await _assignmentService.ReturnAsync(SelectedDevice.Ip);
                SelectedDevice.AssignedUserId = null;
                SelectedDevice.DepartmentId = null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to return device {Ip}", SelectedDevice.Ip);
                await _dialogService.ShowInfoAsync($"Failed to return device: {ex.Message}", "Devices");
            }

            ReturnDeviceCommand.NotifyCanExecuteChanged();
            AssignDeviceCommand.NotifyCanExecuteChanged();
            DevicesView.Refresh();
        }

        async Task LoadInstalledSoftwareAsync(Device device)
        {
            try
            {
                var software = await _softwareService.GetSoftwareAsync(device.Ip, device.Port).ConfigureAwait(false);
                foreach (var s in software)
                    InstalledSoftware.Add(s);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load software for {Ip}", device.Ip);
            }
        }

        async void Device_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (sender is not Device device)
                return;

            if (e.PropertyName == nameof(Device.Hostname)
                || e.PropertyName == nameof(Device.Username)
                || e.PropertyName == nameof(Device.Password)
                || e.PropertyName == nameof(Device.Domain))
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

        private void OnViewDetails()
        {
            if (SelectedDevice == null)
                return;
            ViewDetailsRequested?.Invoke(this, SelectedDevice);
        }

        private bool FilterDevice(object obj)
        {
            if (obj is not Device d)
                return false;
            if (DepartmentFilter.HasValue && d.DepartmentId != DepartmentFilter)
                return false;
            if (AssignedUserFilter.HasValue && d.AssignedUserId != AssignedUserFilter)
                return false;
            return true;
        }

        class NullDeviceAssignmentService : IDeviceAssignmentService
        {
            public static readonly IDeviceAssignmentService Instance = new NullDeviceAssignmentService();
            NullDeviceAssignmentService() { }
            public Task<DeviceAssignment?> GetCurrentAssignmentAsync(string deviceIp, CancellationToken cancellationToken = default)
                => Task.FromResult<DeviceAssignment?>(null);
            public Task AssignAsync(DeviceAssignment assignment, CancellationToken cancellationToken = default)
                => Task.CompletedTask;
            public Task ReturnAsync(string deviceIp, CancellationToken cancellationToken = default)
                => Task.CompletedTask;
        }

        class NullDeviceSoftwareService : IDeviceSoftwareService
        {
            public static readonly IDeviceSoftwareService Instance = new NullDeviceSoftwareService();
            NullDeviceSoftwareService() { }
            public Task<IEnumerable<DeviceSoftware>> GetSoftwareAsync(string deviceIp, int? devicePort, CancellationToken cancellationToken = default)
                => Task.FromResult<IEnumerable<DeviceSoftware>>(Array.Empty<DeviceSoftware>());
            public Task AddOrUpdateAsync(DeviceSoftware software, CancellationToken cancellationToken = default)
                => Task.CompletedTask;
            public Task DeleteAsync(string deviceIp, int? devicePort, string name, CancellationToken cancellationToken = default)
                => Task.CompletedTask;
        }

        class NullStaffService : IStaffService
        {
            public static readonly IStaffService Instance = new NullStaffService();
            NullStaffService() { }
            public Task<IEnumerable<KeyValuePair<int, string>>> GetStaffAsync(CancellationToken cancellationToken = default)
                => Task.FromResult<IEnumerable<KeyValuePair<int, string>>>(Array.Empty<KeyValuePair<int, string>>());
        }

    }
}
