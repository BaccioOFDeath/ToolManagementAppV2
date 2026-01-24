// MainWindow.xaml.cs
using System;
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
        static readonly TimeSpan SidebarAutoCloseDelay = TimeSpan.FromSeconds(3);

        readonly IDatabaseService? _ownedDb;
        readonly DispatcherTimer _sidebarAutoCloseTimer;

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

            DataContext = viewModel;
            _ownedDb = ownedDatabaseService;
            this.DisposeDataContextOnUnload();

            Closed += (_, __) => _ownedDb?.Dispose();

            if (viewModel is MainViewModel vm)
            {
                MouseMove += (_, __) => vm.ResetAutoLogoutTimer();
                KeyDown += (_, __) => vm.ResetAutoLogoutTimer();
                MouseDown += (_, __) => vm.ResetAutoLogoutTimer();
            }

            _sidebarAutoCloseTimer = new DispatcherTimer { Interval = SidebarAutoCloseDelay };
            _sidebarAutoCloseTimer.Tick += (_, __) =>
            {
                _sidebarAutoCloseTimer.Stop();
                if (DataContext is MainViewModel vm &&
                    vm.IsSidebarOpen &&
                    !ActivityBar.IsMouseOver &&
                    !SidebarPanel.IsMouseOver)
                {
                    vm.IsSidebarOpen = false;
                }
            };

            ActivityBar.MouseEnter += (_, __) => _sidebarAutoCloseTimer.Stop();
            SidebarPanel.MouseEnter += (_, __) => _sidebarAutoCloseTimer.Stop();
            ActivityBar.MouseLeave += (_, __) => TryStartSidebarAutoCloseTimer();
            SidebarPanel.MouseLeave += (_, __) => TryStartSidebarAutoCloseTimer();
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

        void TryStartSidebarAutoCloseTimer()
        {
            if (DataContext is MainViewModel vm &&
                vm.IsSidebarOpen &&
                !ActivityBar.IsMouseOver &&
                !SidebarPanel.IsMouseOver)
            {
                _sidebarAutoCloseTimer.Start();
            }
        }
    }
}
