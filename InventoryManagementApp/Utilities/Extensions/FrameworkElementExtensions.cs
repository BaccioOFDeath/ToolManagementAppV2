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
        /// Applies a default dialog size while keeping the window inside the current work area.
        /// </summary>
        /// <param name="window">Window to size.</param>
        /// <param name="preferredWidth">Preferred width for normal desktop displays.</param>
        /// <param name="preferredHeight">Preferred height for normal desktop displays.</param>
        public static void UseResponsiveDefaultSize(this Window window, double preferredWidth, double preferredHeight)
        {
            if (window is null) return;

            window.SourceInitialized += (_, __) =>
            {
                var workArea = SystemParameters.WorkArea;
                var maxWidth = Math.Max(320, workArea.Width - 40);
                var maxHeight = Math.Max(320, workArea.Height - 40);

                window.MaxWidth = maxWidth;
                window.MaxHeight = maxHeight;
                window.MinWidth = Math.Min(window.MinWidth, maxWidth);
                window.MinHeight = Math.Min(window.MinHeight, maxHeight);
                window.Width = Math.Min(Math.Max(preferredWidth, window.MinWidth), maxWidth);
                window.Height = Math.Min(Math.Max(preferredHeight, window.MinHeight), maxHeight);
            };
        }

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

