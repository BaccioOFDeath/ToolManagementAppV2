using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.ObjectModel;
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
        readonly ISettingsService _settingsService;
        readonly IScannerGroupService _groupService;
        readonly IScannerFileService _fileService;
        readonly IScannerRuleService _ruleService;
        readonly ILogger<ScannerStatusViewModel> _logger;
        readonly DispatcherTimer _timer;

        public ObservableCollection<ScannerDevice> Devices { get; } = new();
        public ObservableCollection<ScannerGroup> Groups { get; } = new();
        public ObservableCollection<string> DeviceFiles { get; } = new();
        public ObservableCollection<ScannerFileRule> Rules { get; } = new();

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
        public IAsyncRelayCommand LoadRulesCommand { get; }
        public IAsyncRelayCommand AddRuleCommand { get; }
        public IAsyncRelayCommand<int> DeleteRuleCommand { get; }

        public Func<string?> PromptForIp { get; set; } = () =>
            Interaction.InputBox("Enter scanner IP:", "Add Device", string.Empty);

        public Func<string?> PromptForGroupName { get; set; } = () =>
            Interaction.InputBox("Enter group name:", "Add Group", string.Empty);

        internal bool IsTimerRunning => _timer.IsEnabled;

        ScannerDevice? _selectedDevice;
        public ScannerDevice? SelectedDevice
        {
            get => _selectedDevice;
            set
            {
                if (SetProperty(ref _selectedDevice, value))
                {
                    LoadFilesCommand.Execute(null);
                    LoadRulesCommand.Execute(null);
                }
            }
        }

        public ScannerStatusViewModel(IScannerService service, IDialogService dialogService, ISettingsService settingsService, IScannerGroupService groupService, IScannerFileService fileService, IScannerRuleService ruleService, ILogger<ScannerStatusViewModel>? logger = null)
        {
            _service = service;
            _dialogService = dialogService;
            _settingsService = settingsService;
            _groupService = groupService;
            _fileService = fileService;
            _ruleService = ruleService;
            _logger = logger ?? NullLogger<ScannerStatusViewModel>.Instance;
            RefreshCommand = new AsyncRelayCommand(RefreshAsync);
            AddDeviceCommand = new AsyncRelayCommand(AddDeviceAsync);
            AddGroupCommand = new AsyncRelayCommand(AddGroupAsync);
            LoadFilesCommand = new AsyncRelayCommand(LoadFilesAsync);
            LoadRulesCommand = new AsyncRelayCommand(LoadRulesAsync);
            AddRuleCommand = new AsyncRelayCommand(AddRuleAsync);
            DeleteRuleCommand = new AsyncRelayCommand<int>(DeleteRuleAsync);
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

                var devices = await _service.GetScannerDevicesAsync(cancellationToken);
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
                _logger.LogError(ex, "Failed to refresh scanner devices");
                await _dialogService.ShowInfoAsync($"Failed to refresh scanner devices: {ex.Message}", "Error");
            }
        }

        async Task AddDeviceAsync()
        {
            try
            {
                var ip = PromptForIp?.Invoke();
                if (string.IsNullOrWhiteSpace(ip))
                    return;

                var ips = (await _settingsService.GetScannerIpAddressesAsync()).ToList();
                if (!ips.Contains(ip))
                    ips.Add(ip);

                var invalid = await _settingsService.SaveScannerIpAddressesAsync(ips);
                if (invalid.Any())
                {
                    await _dialogService.ShowInfoAsync($"Invalid IP address: {string.Join(", ", invalid)}", "Invalid IP");
                    return;
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
                var files = await _fileService.ListFilesAsync(SelectedDevice.Ip, cancellationToken);
                foreach (var f in files)
                    DeviceFiles.Add(f);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load files for device {Ip}", SelectedDevice?.Ip);
            }
        }

        async Task LoadRulesAsync(CancellationToken cancellationToken)
        {
            try
            {
                Rules.Clear();
                if (SelectedDevice is null)
                    return;
                var rules = await _ruleService.GetRulesAsync(SelectedDevice.Ip, cancellationToken);
                foreach (var r in rules)
                    Rules.Add(r);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load rules for device {Ip}", SelectedDevice?.Ip);
            }
        }

        async Task AddRuleAsync()
        {
            try
            {
                if (SelectedDevice is null)
                    return;
                var source = Interaction.InputBox("Enter source folder:", "Add Rule", string.Empty);
                if (string.IsNullOrWhiteSpace(source)) return;
                var dest = Interaction.InputBox("Enter destination folder:", "Add Rule", string.Empty);
                if (string.IsNullOrWhiteSpace(dest)) return;
                var pattern = Interaction.InputBox("Enter file pattern:", "Add Rule", "*.*");
                if (string.IsNullOrWhiteSpace(pattern)) return;
                var rule = new ScannerFileRule
                {
                    DeviceId = SelectedDevice.Ip,
                    SourcePath = source,
                    DestinationPath = dest,
                    Pattern = pattern
                };
                await _ruleService.AddRuleAsync(rule);
                await LoadRulesAsync(CancellationToken.None);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to add rule");
                await _dialogService.ShowInfoAsync($"Failed to add rule: {ex.Message}", "Error");
            }
        }

        async Task DeleteRuleAsync(int ruleId)
        {
            try
            {
                await _ruleService.DeleteRuleAsync(ruleId);
                var existing = Rules.FirstOrDefault(r => r.Id == ruleId);
                if (existing != null)
                    Rules.Remove(existing);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete rule {RuleId}", ruleId);
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
            if (e.PropertyName == nameof(ScannerDevice.GroupId) && sender is ScannerDevice device)
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
