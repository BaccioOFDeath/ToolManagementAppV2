using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.ObjectModel;
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

        public IRelayCommand RefreshCommand { get; }

        internal bool IsTimerRunning => _timer.IsEnabled;

        public ScannerStatusViewModel(IScannerService service)
        {
            _service = service;
            RefreshCommand = new RelayCommand(Refresh);
            _timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(5) };
            _timer.Tick += (s, e) => Refresh();
        }

        void Refresh()
        {
            Devices.Clear();
            foreach (var d in _service.GetScannerDevices())
                Devices.Add(d);
        }
    }
}
