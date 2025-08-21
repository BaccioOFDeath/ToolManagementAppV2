using System;
using System.Windows.Threading;
using InventoryManagementApp.Interfaces;

namespace InventoryManagementApp.Utilities
{
    /// <summary>
    /// Wraps the WPF <see cref="DispatcherTimer"/> for testability.
    /// </summary>
    public class DispatcherTimerWrapper : IDispatcherTimer
    {
        readonly DispatcherTimer _timer = new();

        public event EventHandler Tick
        {
            add => _timer.Tick += value;
            remove => _timer.Tick -= value;
        }

        public TimeSpan Interval
        {
            get => _timer.Interval;
            set => _timer.Interval = value;
        }

        public bool IsEnabled => _timer.IsEnabled;

        public void Start() => _timer.Start();

        public void Stop() => _timer.Stop();
    }
}
