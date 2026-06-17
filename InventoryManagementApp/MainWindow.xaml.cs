// MainWindow.xaml.cs
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using InventoryManagementApp.Utilities.Extensions;
using InventoryManagementApp.Interfaces;
using InventoryManagementApp.ViewModels;

namespace InventoryManagementApp
{
    public partial class MainWindow : Window, IMainWindow
    {
        const double ScrollFactor = 0.20;
        const double OpenSidebarWidth = 214;
        const double ClosedSidebarWidth = 0;
        static readonly TimeSpan SidebarAnimationDuration = TimeSpan.FromMilliseconds(950);

        readonly IDatabaseService? _ownedDb;
        readonly DispatcherTimer _sidebarCloseTimer;
        readonly DispatcherTimer _sidebarAnimationTimer;
        DateTime _sidebarAnimationStartedAt;
        double _sidebarAnimationStartWidth;
        double _sidebarAnimationTargetWidth = OpenSidebarWidth;
        MainViewModel? _mainViewModel;

        /// <summary>
        /// Initializes a new instance of <see cref="MainWindow"/> with the provided services.
        /// When <paramref name="ownedDatabaseService"/> is supplied, the window will dispose it
        /// when closed.
        /// </summary>
        /// <param name="viewModel">View model to use as the window's data context.</param>
        /// <param name="ownedDatabaseService">Database service owned by the window; disposed on close.</param>
        public MainWindow(IMainViewModel viewModel, IDatabaseService? ownedDatabaseService = null)
        {
            InitializeComponent();

            _sidebarCloseTimer = new DispatcherTimer { Interval = TimeSpan.Zero };
            _sidebarCloseTimer.Tick += SidebarCloseTimer_Tick;
            _sidebarAnimationTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(16) };
            _sidebarAnimationTimer.Tick += SidebarAnimationTimer_Tick;

            DataContext = viewModel;
            _ownedDb = ownedDatabaseService;
            this.DisposeDataContextOnUnload();

            Closed += (_, __) =>
            {
                _sidebarCloseTimer.Stop();
                _sidebarAnimationTimer.Stop();
                if (_mainViewModel != null)
                    _mainViewModel.PropertyChanged -= MainViewModel_PropertyChanged;
                _ownedDb?.Dispose();
            };

            if (viewModel is MainViewModel vm)
            {
                _mainViewModel = vm;
                _mainViewModel.PropertyChanged += MainViewModel_PropertyChanged;
                MouseMove += (_, __) => vm.ResetAutoLogoutTimer();
                KeyDown += (_, __) => vm.ResetAutoLogoutTimer();
                MouseDown += (_, __) => vm.ResetAutoLogoutTimer();
                SetSidebarWidth(vm.IsSidebarOpen ? OpenSidebarWidth : ClosedSidebarWidth);
            }
        }

        void IMainWindow.Activate() => base.Activate();

        void IMainWindow.Focus() => base.Focus();

        void LeftNavScrollViewer_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (sender is ScrollViewer scrollViewer)
            {
                var target = scrollViewer.VerticalOffset - e.Delta * ScrollFactor;
                target = Math.Max(0, Math.Min(target, scrollViewer.ScrollableHeight));
                scrollViewer.ScrollToVerticalOffset(target);
                e.Handled = true;
            }
        }

        void LeftNavPanel_MouseEnter(object sender, MouseEventArgs e)
        {
            _sidebarCloseTimer.Stop();
        }

        void LeftNavPanel_MouseLeave(object sender, MouseEventArgs e)
        {
            _sidebarCloseTimer.Stop();
            if (_mainViewModel != null)
                _mainViewModel.IsSidebarOpen = false;
            else
                SetSidebarWidth(ClosedSidebarWidth);
        }

        void SidebarCloseTimer_Tick(object? sender, EventArgs e)
        {
            _sidebarCloseTimer.Stop();
            if (_mainViewModel != null)
                _mainViewModel.IsSidebarOpen = false;
            else
                AnimateSidebarTo(ClosedSidebarWidth);
        }

        void MainViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(MainViewModel.IsSidebarOpen) && sender is MainViewModel vm)
            {
                _sidebarCloseTimer.Stop();
                AnimateSidebarTo(vm.IsSidebarOpen ? OpenSidebarWidth : ClosedSidebarWidth);
            }
        }

        void AnimateSidebarTo(double targetWidth)
        {
            if (targetWidth <= ClosedSidebarWidth)
            {
                _sidebarAnimationTimer.Stop();
                SetSidebarWidth(ClosedSidebarWidth);
                return;
            }

            var currentWidth = LeftNavColumn.ActualWidth;
            if (double.IsNaN(currentWidth) || currentWidth <= 0 && LeftNavColumn.Width.Value > 0)
                currentWidth = LeftNavColumn.Width.Value;

            _sidebarAnimationStartWidth = currentWidth;
            _sidebarAnimationTargetWidth = targetWidth;
            _sidebarAnimationStartedAt = DateTime.UtcNow;
            _sidebarAnimationTimer.Stop();
            _sidebarAnimationTimer.Start();
        }

        void SidebarAnimationTimer_Tick(object? sender, EventArgs e)
        {
            var elapsed = DateTime.UtcNow - _sidebarAnimationStartedAt;
            var progress = Math.Clamp(elapsed.TotalMilliseconds / SidebarAnimationDuration.TotalMilliseconds, 0, 1);
            var eased = EaseInOutSine(progress);
            var width = _sidebarAnimationStartWidth + (_sidebarAnimationTargetWidth - _sidebarAnimationStartWidth) * eased;
            SetSidebarWidth(width);

            if (progress >= 1)
            {
                SetSidebarWidth(_sidebarAnimationTargetWidth);
                _sidebarAnimationTimer.Stop();
            }
        }

        void SetSidebarWidth(double width)
        {
            width = Math.Round(Math.Clamp(width, ClosedSidebarWidth, OpenSidebarWidth));
            LeftNavColumn.Width = new GridLength(width);
            LeftNavPanel.Width = width;
            LeftNavPanel.Opacity = width <= 0 ? 0 : Math.Min(1, width / OpenSidebarWidth);
            LeftNavPanel.IsHitTestVisible = width > 1;
        }

        static double EaseInOutSine(double value)
            => -(Math.Cos(Math.PI * value) - 1) / 2;
    }
}
