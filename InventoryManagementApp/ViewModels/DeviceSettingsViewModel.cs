using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using InventoryManagementApp.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using InventoryManagementApp.Models;

namespace InventoryManagementApp.ViewModels
{
    public class DeviceSettingsViewModel : ObservableObject
    {
        private readonly ISettingsService _settingsService;
        private readonly IConfiguration _configuration;
        private readonly IDialogService _dialogService;
        private readonly ILogger<DeviceSettingsViewModel> _logger;

        public DeviceSettingsViewModel(ISettingsService settingsService, IConfiguration configuration, IDialogService dialogService, ILogger<DeviceSettingsViewModel>? logger = null)
        {
            _settingsService = settingsService;
            _configuration = configuration;
            _dialogService = dialogService;
            _logger = logger ?? NullLogger<DeviceSettingsViewModel>.Instance;
            SaveCommand = new AsyncRelayCommand(SaveAsync);
        }

        public IAsyncRelayCommand SaveCommand { get; }

        private string _subnets = string.Empty;
        public string Subnets { get => _subnets; set => SetProperty(ref _subnets, value); }

        private string _ftpPorts = string.Empty;
        public string FtpPorts { get => _ftpPorts; set => SetProperty(ref _ftpPorts, value); }

        private string _additionalPorts = string.Empty;
        public string AdditionalPorts { get => _additionalPorts; set => SetProperty(ref _additionalPorts, value); }

        private int _maxConcurrentScans;
        public int MaxConcurrentScans { get => _maxConcurrentScans; set => SetProperty(ref _maxConcurrentScans, value); }

        private int _livenessTimeoutMs;
        public int LivenessTimeoutMs { get => _livenessTimeoutMs; set => SetProperty(ref _livenessTimeoutMs, value); }

        private int _portProbeTimeoutMs;
        public int PortProbeTimeoutMs { get => _portProbeTimeoutMs; set => SetProperty(ref _portProbeTimeoutMs, value); }

        bool _initialized;
        public async Task InitializeAsync()
        {
            if (_initialized) return;
            _subnets = await _settingsService.GetSettingAsync("DeviceDiscovery_Subnets").ConfigureAwait(false)
                       ?? string.Join(", ", _configuration.GetSection("DeviceDiscovery:Subnets").Get<string[]>() ?? Array.Empty<string>());
            _ftpPorts = await _settingsService.GetSettingAsync("DeviceDiscovery_FtpPorts").ConfigureAwait(false)
                       ?? string.Join(", ", _configuration.GetSection("DeviceDiscovery:FtpPorts").Get<int[]>() ?? Array.Empty<int>());
            _additionalPorts = await _settingsService.GetSettingAsync("DeviceDiscovery_AdditionalPorts").ConfigureAwait(false)
                       ?? string.Join(", ", (_configuration.GetSection("DeviceDiscovery:AdditionalPorts").Get<Dictionary<int, DeviceProtocol>>() ?? new()).Select(kv => $"{kv.Key}:{kv.Value}"));
            if (int.TryParse(await _settingsService.GetSettingAsync("DeviceDiscovery_MaxConcurrentScans").ConfigureAwait(false), out var mcs))
                _maxConcurrentScans = mcs;
            else
                _maxConcurrentScans = _configuration.GetValue<int?>("DeviceDiscovery:MaxConcurrentScans") ?? 128;
            if (int.TryParse(await _settingsService.GetSettingAsync("DeviceDiscovery_LivenessTimeoutMs").ConfigureAwait(false), out var lt))
                _livenessTimeoutMs = lt;
            else
                _livenessTimeoutMs = _configuration.GetValue<int?>("DeviceDiscovery:LivenessTimeoutMs") ?? 400;
            if (int.TryParse(await _settingsService.GetSettingAsync("DeviceDiscovery_PortProbeTimeoutMs").ConfigureAwait(false), out var ppt))
                _portProbeTimeoutMs = ppt;
            else
                _portProbeTimeoutMs = _configuration.GetValue<int?>("DeviceDiscovery:PortProbeTimeoutMs") ?? 700;
            OnPropertyChanged(nameof(Subnets));
            OnPropertyChanged(nameof(FtpPorts));
            OnPropertyChanged(nameof(AdditionalPorts));
            OnPropertyChanged(nameof(MaxConcurrentScans));
            OnPropertyChanged(nameof(LivenessTimeoutMs));
            OnPropertyChanged(nameof(PortProbeTimeoutMs));
            _initialized = true;
        }

        async Task SaveAsync()
        {
            try
            {
                await _settingsService.SaveSettingAsync("DeviceDiscovery_Subnets", _subnets).ConfigureAwait(false);
                await _settingsService.SaveSettingAsync("DeviceDiscovery_FtpPorts", _ftpPorts).ConfigureAwait(false);
                await _settingsService.SaveSettingAsync("DeviceDiscovery_AdditionalPorts", _additionalPorts).ConfigureAwait(false);
                await _settingsService.SaveSettingAsync("DeviceDiscovery_MaxConcurrentScans", _maxConcurrentScans.ToString()).ConfigureAwait(false);
                await _settingsService.SaveSettingAsync("DeviceDiscovery_LivenessTimeoutMs", _livenessTimeoutMs.ToString()).ConfigureAwait(false);
                await _settingsService.SaveSettingAsync("DeviceDiscovery_PortProbeTimeoutMs", _portProbeTimeoutMs.ToString()).ConfigureAwait(false);
                _dialogService.ShowInfo("Device settings saved.", "Device Settings");
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized to change settings.");
                _dialogService.ShowInfo("You are not authorized to change settings.", "Unauthorized");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to save device settings.");
                _dialogService.ShowInfo($"Failed to save settings: {ex.Message}", "Device Settings");
            }
        }
    }
}
