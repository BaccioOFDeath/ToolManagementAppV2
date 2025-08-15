using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;
using ToolManagementAppV2.Interfaces;
using ToolManagementAppV2.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace ToolManagementAppV2.ViewModels
{
    public class ScannerStatusViewModel : ObservableObject, IDisposable
    {
        readonly IScannerService _service;
        readonly IDialogService _dialogService;
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

        internal bool IsTimerRunning => _timer.IsEnabled;

        public ScannerStatusViewModel(IScannerService service, IDialogService dialogService, ILogger<ScannerStatusViewModel>? logger = null)
        {
            _service = service;
            _dialogService = dialogService;
            _logger = logger ?? NullLogger<ScannerStatusViewModel>.Instance;
            RefreshCommand = new AsyncRelayCommand(RefreshAsync);
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
                    Devices.Add(d);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to refresh scanner devices");
                await _dialogService.ShowInfoAsync($"Failed to refresh scanner devices: {ex.Message}", "Error");
            }
        }

        public void Dispose()
        {
            _timer.Tick -= OnTimerTick;
            _timer.Stop();
        }
    }
}
