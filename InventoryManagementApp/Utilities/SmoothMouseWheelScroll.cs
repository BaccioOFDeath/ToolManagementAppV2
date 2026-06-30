using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using System.Windows.Threading;

namespace InventoryManagementApp.Utilities
{
    public static class SmoothMouseWheelScroll
    {
        const double WheelDelta = 120.0;
        const double PixelsPerWheelStep = 88.0;
        const double LogicalUnitsPerWheelStep = 1.0;
        const double AnimationMilliseconds = 180.0;
        static readonly Dictionary<ScrollViewer, ScrollAnimation> Animations = new();

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

            AnimateTo(scrollViewer, targetOffset);
            e.Handled = true;
            return true;
        }

        static void AnimateTo(ScrollViewer scrollViewer, double targetOffset)
        {
            if (!Animations.TryGetValue(scrollViewer, out var animation))
            {
                animation = new ScrollAnimation(scrollViewer);
                Animations[scrollViewer] = animation;
            }

            animation.Start(targetOffset, () => Animations.Remove(scrollViewer));
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

        sealed class ScrollAnimation
        {
            readonly ScrollViewer _scrollViewer;
            readonly DispatcherTimer _timer;
            Action? _onCompleted;
            DateTime _startedAt;
            double _startOffset;
            double _targetOffset;

            public ScrollAnimation(ScrollViewer scrollViewer)
            {
                _scrollViewer = scrollViewer;
                _timer = new DispatcherTimer(DispatcherPriority.Render, scrollViewer.Dispatcher)
                {
                    Interval = TimeSpan.FromMilliseconds(1000.0 / 60.0)
                };
                _timer.Tick += OnTick;
                _scrollViewer.Unloaded += ScrollViewer_Unloaded;
            }

            public void Start(double targetOffset, Action onCompleted)
            {
                _onCompleted = onCompleted;
                _startOffset = _scrollViewer.VerticalOffset;
                _targetOffset = Coerce(targetOffset, 0, _scrollViewer.ScrollableHeight);
                _startedAt = DateTime.UtcNow;

                if (!_timer.IsEnabled)
                    _timer.Start();
            }

            void OnTick(object? sender, EventArgs e)
            {
                if (_scrollViewer.ScrollableHeight <= 0)
                {
                    Stop();
                    return;
                }

                var elapsed = (DateTime.UtcNow - _startedAt).TotalMilliseconds;
                var progress = Coerce(elapsed / AnimationMilliseconds, 0, 1);
                var eased = EaseOutCubic(progress);
                var offset = _startOffset + (_targetOffset - _startOffset) * eased;
                _scrollViewer.ScrollToVerticalOffset(Coerce(offset, 0, _scrollViewer.ScrollableHeight));

                if (progress >= 1 || AreClose(_scrollViewer.VerticalOffset, _targetOffset))
                    Stop();
            }

            void ScrollViewer_Unloaded(object sender, RoutedEventArgs e)
                => Stop();

            void Stop()
            {
                _timer.Stop();
                _scrollViewer.Unloaded -= ScrollViewer_Unloaded;
                _onCompleted?.Invoke();
                _onCompleted = null;
            }

            static double EaseOutCubic(double value)
            {
                var inverse = 1 - value;
                return 1 - inverse * inverse * inverse;
            }
        }
    }
}
