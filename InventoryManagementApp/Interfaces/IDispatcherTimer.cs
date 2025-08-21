using System;

namespace InventoryManagementApp.Interfaces
{
    /// <summary>
    /// Abstraction over a dispatcher timer to allow testable scheduling.
    /// </summary>
    public interface IDispatcherTimer
    {
        /// <summary>
        /// Occurs when the timer interval elapses.
        /// </summary>
        event EventHandler Tick;

        /// <summary>
        /// Gets or sets the amount of time between ticks.
        /// </summary>
        TimeSpan Interval { get; set; }

        /// <summary>
        /// Gets a value indicating whether the timer is enabled.
        /// </summary>
        bool IsEnabled { get; }

        /// <summary>
        /// Starts the timer.
        /// </summary>
        void Start();

        /// <summary>
        /// Stops the timer.
        /// </summary>
        void Stop();
    }
}
