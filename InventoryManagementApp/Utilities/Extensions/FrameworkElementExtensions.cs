using System;
using System.Windows;

namespace InventoryManagementApp.Utilities.Extensions
{
    /// <summary>
    /// Helper methods for view cleanup.
    /// </summary>
    public static class FrameworkElementExtensions
    {
        /// <summary>
        /// Disposes the current or previous <see cref="FrameworkElement.DataContext"/>
        /// when the element is unloaded or its data context changes.
        /// </summary>
        /// <param name="element">Element to monitor.</param>
        public static void DisposeDataContextOnUnload(this FrameworkElement element)
        {
            if (element is null) return;

            element.DataContextChanged += OnDataContextChanged;

            if (element is Window window)
                window.Closed += OnUnloaded;
            else
                element.Unloaded += OnUnloadedHandler;
        }

        static void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.OldValue is IDisposable disposable)
                disposable.Dispose();
        }

        static readonly RoutedEventHandler OnUnloadedHandler = OnUnloaded;

        static void OnUnloaded(object? sender, EventArgs e)
        {
            if (sender is FrameworkElement fe)
            {
                if (fe.DataContext is IDisposable disposable)
                    disposable.Dispose();

                fe.DataContextChanged -= OnDataContextChanged;

                if (sender is Window window)
                    window.Closed -= OnUnloaded;
                else
                    fe.Unloaded -= OnUnloadedHandler;
            }
        }
    }
}

