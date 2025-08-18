// MainWindow.xaml.cs
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;
using ToolManagementAppV2.Utilities.Extensions;
using ToolManagementAppV2.Interfaces;
using ToolManagementAppV2.ViewModels;

namespace ToolManagementAppV2
{
    public partial class MainWindow : Window, IMainWindow
    {
        readonly IDatabaseService? _ownedDb;

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
        }

        void IMainWindow.Activate() => base.Activate();

        void IMainWindow.Focus() => base.Focus();

        void LeftNavScrollViewer_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (sender is ScrollViewer scrollViewer)
            {
                var start = scrollViewer.VerticalOffset;
                var target = Math.Round(start - e.Delta / 4.0);

                var animation = new DoubleAnimation(start, target, TimeSpan.FromMilliseconds(200))
                {
                    EasingFunction = new QuadraticEase()
                };

                scrollViewer.SetValue(ScrollAnimationOffsetProperty, start);
                scrollViewer.BeginAnimation(ScrollAnimationOffsetProperty, animation);
                e.Handled = true;
            }
        }

        static readonly DependencyProperty ScrollAnimationOffsetProperty = DependencyProperty.RegisterAttached(
            "ScrollAnimationOffset",
            typeof(double),
            typeof(MainWindow),
            new PropertyMetadata(0.0, OnScrollAnimationOffsetChanged));

        static void OnScrollAnimationOffsetChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ScrollViewer scrollViewer)
            {
                scrollViewer.ScrollToVerticalOffset((double)e.NewValue);
            }
        }
    }
}
