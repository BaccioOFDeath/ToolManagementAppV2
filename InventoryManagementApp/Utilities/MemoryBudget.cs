using System;
using System.Threading;
using Timer = System.Threading.Timer;

namespace InventoryManagementApp.Utilities
{
    public sealed class MemoryBudget : IDisposable
    {
        private readonly Timer _timer;
        private readonly long _steadyThresholdBytes;
        private readonly long _peakThresholdBytes;

        public event EventHandler? SteadyExceeded;
        public event EventHandler? PeakExceeded;

        public MemoryBudget()
            : this(TimeSpan.FromSeconds(5), 400L * 1024 * 1024, 800L * 1024 * 1024)
        {
        }

        public MemoryBudget(TimeSpan interval, long steadyThresholdBytes, long peakThresholdBytes)
        {
            _steadyThresholdBytes = steadyThresholdBytes;
            _peakThresholdBytes = peakThresholdBytes;
            _timer = new Timer(CheckMemory, null, interval, interval);
        }

        private void CheckMemory(object? state)
        {
            var total = GC.GetTotalMemory(false);
            if (total > _peakThresholdBytes)
            {
                PeakExceeded?.Invoke(this, EventArgs.Empty);
            }
            else if (total > _steadyThresholdBytes)
            {
                SteadyExceeded?.Invoke(this, EventArgs.Empty);
            }
        }

        public void Dispose()
        {
            _timer.Dispose();
        }
    }
}
