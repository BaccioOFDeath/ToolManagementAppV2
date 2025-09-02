using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using InventoryManagementApp.Interfaces;
using InventoryManagementApp.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace InventoryManagementApp.ViewModels
{
    public class DevicesViewModel : ObservableObject
    {
        private readonly IDeviceDiscoveryService _discoveryService;
        private readonly IDeviceFileService _fileService;
        private readonly IDialogService _dialogService;
        private readonly ILogger<DevicesViewModel> _logger;

        public ObservableCollection<Device> Devices { get; } = new();
        public ObservableCollection<string> DeviceFiles { get; } = new();

        private Device? _selectedDevice;
        public Device? SelectedDevice
        {
            get => _selectedDevice;
            set
            {
                if (SetProperty(ref _selectedDevice, value))
                {
                    PullAllReportsCommand.NotifyCanExecuteChanged();
                }
            }
        }

        public IAsyncRelayCommand RefreshCommand { get; }
        public IAsyncRelayCommand PullAllReportsCommand { get; }

        public DevicesViewModel(IDeviceDiscoveryService discoveryService,
                                 IDeviceFileService fileService,
                                 IDialogService dialogService,
                                 ILogger<DevicesViewModel>? logger = null)
        {
            _discoveryService = discoveryService;
            _fileService = fileService;
            _dialogService = dialogService;
            _logger = logger ?? NullLogger<DevicesViewModel>.Instance;

            RefreshCommand = new AsyncRelayCommand(RefreshAsync);
            PullAllReportsCommand = new AsyncRelayCommand(PullAllReportsAsync, CanPullAllReports);
        }

        private async Task RefreshAsync()
        {
            try
            {
                var devices = await _discoveryService.DiscoverDevicesAsync();
                Devices.Clear();
                foreach (var d in devices)
                {
                    Devices.Add(new Device
                    {
                        Ip = d.Ip,
                        Hostname = d.Hostname,
                        Protocol = d.Protocols.FirstOrDefault(),
                        Status = d.IsOnline ? "Online" : "Offline"
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to discover devices");
                await _dialogService.ShowInfoAsync($"Failed to discover devices: {ex.Message}", "Devices");
            }
        }

        private bool CanPullAllReports() => SelectedDevice != null;

        private async Task PullAllReportsAsync()
        {
            if (SelectedDevice == null) return;
            try
            {
                DeviceFiles.Clear();
                var files = await _fileService.ListFilesAsync(SelectedDevice, null, CancellationToken.None);
                foreach (var f in files)
                    DeviceFiles.Add(f);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load files for {Ip}", SelectedDevice.Ip);
                await _dialogService.ShowInfoAsync($"Failed to pull reports: {ex.Message}", "Devices");
            }
        }
    }
}
