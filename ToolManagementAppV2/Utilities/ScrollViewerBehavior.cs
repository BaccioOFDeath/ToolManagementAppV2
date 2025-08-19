using System.Windows;
using System.Windows.Controls;

namespace ToolManagementAppV2.Utilities
{
    public static class ScrollViewerBehavior
    {
        public static readonly DependencyProperty VerticalOffsetProperty =
            DependencyProperty.RegisterAttached(
                "VerticalOffset",
                typeof(double),
                typeof(ScrollViewerBehavior),
                new PropertyMetadata(0d, OnVerticalOffsetChanged));

        public static double GetVerticalOffset(DependencyObject obj) => (double)obj.GetValue(VerticalOffsetProperty);

        public static void SetVerticalOffset(DependencyObject obj, double value) => obj.SetValue(VerticalOffsetProperty, value);

        static void OnVerticalOffsetChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ScrollViewer viewer)
            {
                viewer.ScrollToVerticalOffset((double)e.NewValue);
            }
        }
    }
}
