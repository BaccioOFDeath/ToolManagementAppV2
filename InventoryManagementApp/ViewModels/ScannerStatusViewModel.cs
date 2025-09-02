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

namespace InventoryManagementApp.ViewModels
{
    public class ScannerStatusViewModel : ObservableObject, IDisposable
    {
        readonly IScannerService _service;
        readonly IDialogService _dialogService;
        readonly ISettingsService _settingsService;
        readonly ILogger<ScannerStatusViewModel> _logger;
        readonly DispatcherTimer _timer;

        public ObservableCollection<ScannerDevice> Devices { get; } = new();

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

        public Func<string?> PromptForIp { get; set; } = () =>
            Interaction.InputBox("Enter scanner IP:", "Add Device", string.Empty);

        internal bool IsTimerRunning => _timer.IsEnabled;

        public ScannerStatusViewModel(IScannerService service, IDialogService dialogService, ISettingsService settingsService, ILogger<ScannerStatusViewModel>? logger = null)
        {
            _service = service;
            _dialogService = dialogService;
            _settingsService = settingsService;
            _logger = logger ?? NullLogger<ScannerStatusViewModel>.Instance;
            RefreshCommand = new AsyncRelayCommand(RefreshAsync);
            AddDeviceCommand = new AsyncRelayCommand(AddDeviceAsync);
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
                var devices = await _service.GetScannerDevicesAsync(cancellationToken);
                Devices.Clear();
                foreach (var d in devices)
                {
                    if (d.LastSeen.Kind != DateTimeKind.Local)
                    {
                        _logger.LogWarning("Device {Ip} reported LastSeen kind {Kind}; converting to local time", d.Ip, d.LastSeen.Kind);
                        d.LastSeen = d.LastSeen.ToLocalTime();
                    }
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

        public void Dispose()
        {
            _timer.Tick -= OnTimerTick;
            _timer.Stop();
        }
    }
}
