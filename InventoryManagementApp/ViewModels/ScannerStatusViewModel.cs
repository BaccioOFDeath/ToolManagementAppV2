using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;
using InventoryManagementApp.Interfaces;
using InventoryManagementApp.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualBasic;
using System.ComponentModel;

namespace InventoryManagementApp.ViewModels
{
    public class ScannerStatusViewModel : ObservableObject, IDisposable
    {
        readonly IScannerService _service;
        readonly IDialogService _dialogService;
        readonly IDeviceService _deviceService;
        readonly IDeviceGroupService _groupService;
        readonly IDeviceFileService _fileService;
        readonly ILogger<ScannerStatusViewModel> _logger;
        readonly DispatcherTimer _timer;

        public ObservableCollection<Device> Devices { get; } = new();
        public ObservableCollection<DeviceGroup> Groups { get; } = new();
        public ObservableCollection<string> DeviceFiles { get; } = new();

        private string _fileExtensionFilter = "*.*";
        public string FileExtensionFilter
        {
            get => _fileExtensionFilter;
            set => SetProperty(ref _fileExtensionFilter, value);
        }

        bool _autoRefresh;
        public bool AutoRefresh
        {
            get => _autoRefresh;
            set
            {
                if (SetProperty(ref _autoRefresh, value))
                {
                    if (value) _timer.Start();
                    else _timer.Stop();
                }
            }
        }

        public IAsyncRelayCommand RefreshCommand { get; }
        public IAsyncRelayCommand AddDeviceCommand { get; }
        public IAsyncRelayCommand AddGroupCommand { get; }
        public IAsyncRelayCommand LoadFilesCommand { get; }

        public Func<string?> PromptForIp { get; set; } = () =>
            Interaction.InputBox("Enter scanner IP:", "Add Device", string.Empty);

        public Func<string?> PromptForGroupName { get; set; } = () =>
            Interaction.InputBox("Enter group name:", "Add Group", string.Empty);

        internal bool IsTimerRunning => _timer.IsEnabled;

        Device? _selectedDevice;
        public Device? SelectedDevice
        {
            get => _selectedDevice;
            set
            {
                if (SetProperty(ref _selectedDevice, value))
                {
                    LoadFilesCommand.Execute(null);
                }
            }
        }

        public ScannerStatusViewModel(IScannerService service, IDialogService dialogService, IDeviceService deviceService, IDeviceGroupService groupService, IDeviceFileService fileService, ILogger<ScannerStatusViewModel>? logger = null)
        {
            _service = service;
            _dialogService = dialogService;
            _deviceService = deviceService;
            _groupService = groupService;
            _fileService = fileService;
            _logger = logger ?? NullLogger<ScannerStatusViewModel>.Instance;
            RefreshCommand = new AsyncRelayCommand(RefreshAsync);
            AddDeviceCommand = new AsyncRelayCommand(AddDeviceAsync);
            AddGroupCommand = new AsyncRelayCommand(AddGroupAsync);
            LoadFilesCommand = new AsyncRelayCommand(LoadFilesAsync);
            _timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(5) };
            _timer.Tick += OnTimerTick;
        }

        void OnTimerTick(object? s, EventArgs e)
        {
            _ = RefreshAsync(CancellationToken.None);
        }

        async Task RefreshAsync(CancellationToken cancellationToken)
        {
            try
            {
                var groups = await _groupService.GetGroupsAsync(cancellationToken);
                Groups.Clear();
                foreach (var g in groups)
                    Groups.Add(g);

                var devices = await _service.GetDevicesAsync(cancellationToken);
                foreach (var d in Devices)
                    d.PropertyChanged -= Device_PropertyChanged;
                Devices.Clear();
                foreach (var d in devices)
                {
                    d.GroupId = await _groupService.GetDeviceGroupIdAsync(d.Ip, cancellationToken);
                    if (d.LastSeen.Kind != DateTimeKind.Local)
                    {
                        _logger.LogWarning("Device {Ip} reported LastSeen kind {Kind}; converting to local time", d.Ip, d.LastSeen.Kind);
                        d.LastSeen = d.LastSeen.ToLocalTime();
                    }
                    d.PropertyChanged += Device_PropertyChanged;
                    Devices.Add(d);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to refresh devices");
                await _dialogService.ShowInfoAsync($"Failed to refresh devices: {ex.Message}", "Error");
            }
        }

        async Task AddDeviceAsync()
        {
            try
            {
                var ip = PromptForIp?.Invoke();
                if (string.IsNullOrWhiteSpace(ip))
                    return;

                var existing = await _deviceService.GetDeviceAsync(ip);
                if (existing == null)
                {
                    await _deviceService.AddOrUpdateDeviceAsync(new Device { Ip = ip });
                }

                await RefreshAsync(CancellationToken.None);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to add scanner IP");
                await _dialogService.ShowInfoAsync($"Failed to add device: {ex.Message}", "Error");
            }
        }

        async Task LoadFilesAsync(CancellationToken cancellationToken)
        {
            try
            {
                DeviceFiles.Clear();
                if (SelectedDevice is null)
                    return;
                var filter = FileExtensionFilter == "*.*" ? null : FileExtensionFilter;
                await _fileService.DownloadUnseenFilesAsync(SelectedDevice, AppContext.BaseDirectory, cancellationToken);
                var deviceDirName = string.IsNullOrWhiteSpace(SelectedDevice.Hostname) ? SelectedDevice.Ip : SelectedDevice.Hostname;
                var deviceDir = Path.Combine(AppContext.BaseDirectory, "Devices", deviceDirName);
                if (Directory.Exists(deviceDir))
                {
                    var search = filter ?? "*.*";
                    foreach (var f in Directory.GetFiles(deviceDir, search))
                        DeviceFiles.Add(Path.GetFileName(f));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load files for device {Ip}", SelectedDevice?.Ip);
            }
        }

        async Task AddGroupAsync()
        {
            try
            {
                var name = PromptForGroupName?.Invoke();
                if (string.IsNullOrWhiteSpace(name))
                    return;

                await _groupService.CreateGroupAsync(name);
                await RefreshAsync(CancellationToken.None);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to add scanner group");
                await _dialogService.ShowInfoAsync($"Failed to add group: {ex.Message}", "Error");
            }
        }

        async void Device_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Device.GroupId) && sender is Device device)
            {
                try
                {
                    await _groupService.AssignDeviceToGroupAsync(device.Ip, device.GroupId);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to assign device {Ip} to group {Group}", device.Ip, device.GroupId);
                }
            }
        }

        public void Dispose()
        {
            _timer.Tick -= OnTimerTick;
            _timer.Stop();
            foreach (var d in Devices)
                d.PropertyChanged -= Device_PropertyChanged;
        }
    }
}
