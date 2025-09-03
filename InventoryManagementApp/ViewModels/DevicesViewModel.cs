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
                }
            }
        }

        public IAsyncRelayCommand RefreshCommand { get; }
        public IAsyncRelayCommand PullAllReportsCommand { get; }

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

        public DevicesViewModel(IDeviceDiscoveryService discoveryService,
                                 IDeviceFileService fileService,
                                 IDialogService dialogService,
                                 ILogger<DevicesViewModel>? logger = null)
        {
            _discoveryService = discoveryService;
            _fileService = fileService;
            _dialogService = dialogService;
            _logger = logger ?? NullLogger<DevicesViewModel>.Instance;

            RefreshCommand = new AsyncRelayCommand(RefreshAsync, () => !IsDiscovering);
            PullAllReportsCommand = new AsyncRelayCommand(PullAllReportsAsync, CanPullAllReports);
        }

        private async Task RefreshAsync()
        {
            try
            {
                IsDiscovering = true;
                DiscoveryProgress = 0;
                Devices.Clear();

                var progress = new Progress<double>(p => DiscoveryProgress = p);

                await foreach (var d in _discoveryService.DiscoverDevicesAsync(progress))
                {
                    Devices.Add(new Device
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
                    });
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
    }
}
