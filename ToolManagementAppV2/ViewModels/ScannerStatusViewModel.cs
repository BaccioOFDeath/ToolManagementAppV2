using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;
using ToolManagementAppV2.Interfaces;
using ToolManagementAppV2.Models;

namespace ToolManagementAppV2.ViewModels
{
    public class ScannerStatusViewModel : ObservableObject
    {
        readonly IScannerService _service;
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

        public ScannerStatusViewModel(IScannerService service)
        {
            _service = service;
            RefreshCommand = new AsyncRelayCommand(RefreshAsync);
            _timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(5) };
            _timer.Tick += async (s, e) => await RefreshAsync(CancellationToken.None);
        }

        async Task RefreshAsync(CancellationToken cancellationToken)
        {
            Devices.Clear();
            var devices = await _service.GetScannerDevicesAsync(cancellationToken);
            foreach (var d in devices)
                Devices.Add(d);
        }
    }
}
