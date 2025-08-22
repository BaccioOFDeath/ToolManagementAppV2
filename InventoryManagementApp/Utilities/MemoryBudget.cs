using System;
using System.Threading;
using Timer = System.Threading.Timer;

namespace InventoryManagementApp.Utilities
{
    public sealed class MemoryBudget : IDisposable
    {
        private readonly Timer _timer;
        private readonly long _thresholdBytes;

        public event EventHandler? ThresholdExceeded;

        public MemoryBudget()
            : this(TimeSpan.FromSeconds(5), 600L * 1024 * 1024)
        {
        }

        public MemoryBudget(TimeSpan interval, long thresholdBytes)
        {
            _thresholdBytes = thresholdBytes;
            _timer = new Timer(CheckMemory, null, interval, interval);
        }

        private void CheckMemory(object? state)
        {
            var total = GC.GetTotalMemory(false);
            if (total > _thresholdBytes)
            {
                ThresholdExceeded?.Invoke(this, EventArgs.Empty);
            }
        }

        public void Dispose()
        {
            _timer.Dispose();
        }
    }
}
