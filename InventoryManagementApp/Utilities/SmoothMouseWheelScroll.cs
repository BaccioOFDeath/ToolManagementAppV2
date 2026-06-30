using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace InventoryManagementApp.Utilities
{
    public static class SmoothMouseWheelScroll
    {
        const double WheelDelta = 120.0;
        const double PixelsPerWheelStep = 42.0;
        const double LogicalUnitsPerWheelStep = 0.65;

        public static bool TryHandle(MouseWheelEventArgs? e)
        {
            if (e == null || e.Handled || e.Delta == 0 || Keyboard.Modifiers.HasFlag(ModifierKeys.Control))
                return false;

            var source = e.OriginalSource as DependencyObject;
            var scrollViewer = FindScrollableViewer(source, e.Delta);
            if (scrollViewer == null)
                return false;

            var units = -e.Delta / WheelDelta;
            var step = scrollViewer.CanContentScroll ? LogicalUnitsPerWheelStep : PixelsPerWheelStep;
            var targetOffset = Coerce(scrollViewer.VerticalOffset + units * step, 0, scrollViewer.ScrollableHeight);
            if (AreClose(targetOffset, scrollViewer.VerticalOffset))
                return false;

            scrollViewer.ScrollToVerticalOffset(targetOffset);
            e.Handled = true;
            return true;
        }

        static ScrollViewer? FindScrollableViewer(DependencyObject? source, int wheelDelta)
        {
            var current = source;
            while (current != null)
            {
                if (current is ScrollViewer scrollViewer && CanScroll(scrollViewer, wheelDelta))
                    return scrollViewer;

                current = GetParent(current);
            }

            return null;
        }

        static bool CanScroll(ScrollViewer scrollViewer, int wheelDelta)
        {
            if (scrollViewer.ScrollableHeight <= 0)
                return false;

            return wheelDelta < 0
                ? scrollViewer.VerticalOffset < scrollViewer.ScrollableHeight
                : scrollViewer.VerticalOffset > 0;
        }

        static DependencyObject? GetParent(DependencyObject current)
        {
            if (current is Visual or Visual3D)
            {
                var visualParent = VisualTreeHelper.GetParent(current);
                if (visualParent != null)
                    return visualParent;
            }

            if (current is FrameworkContentElement contentElement)
                return contentElement.Parent;

            if (current is FrameworkElement element)
                return element.Parent;

            if (current is ContentElement content)
                return ContentOperations.GetParent(content);

            return null;
        }

        static double Coerce(double value, double minimum, double maximum)
            => value < minimum ? minimum : value > maximum ? maximum : value;

        static bool AreClose(double left, double right)
            => System.Math.Abs(left - right) < 0.001;
    }
}
