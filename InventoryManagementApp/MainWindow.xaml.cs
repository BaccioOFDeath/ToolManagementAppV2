// MainWindow.xaml.cs
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using InventoryManagementApp.Utilities.Extensions;
using InventoryManagementApp.Interfaces;
using InventoryManagementApp.Utilities;
using InventoryManagementApp.ViewModels;

namespace InventoryManagementApp
{
    public partial class MainWindow : Window, IMainWindow
    {
        const double CompactShellHeightThreshold = 820;
        const double CompactShellWidthThreshold = 1120;
        readonly IDatabaseService? _ownedDb;
        MainViewModel? _mainViewModel;
        bool? _isCompactShellLayout;
        double? _lastAdaptiveResourceScale;
        readonly Dictionary<string, double> _baseAdaptiveDoubleResources = new(StringComparer.Ordinal);
        readonly Dictionary<string, Thickness> _baseAdaptiveThicknessResources = new(StringComparer.Ordinal);

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
            Loaded += (_, __) => UpdateShellLayout();
            SizeChanged += (_, __) => UpdateShellLayout();

            Closed += (_, __) =>
            {
                if (_mainViewModel != null)
                {
                    _mainViewModel.PropertyChanged -= MainViewModel_PropertyChanged;
                    InputManager.Current.PreProcessInput -= InputManager_PreProcessInput;
                }
                _ownedDb?.Dispose();
            };

            if (viewModel is MainViewModel vm)
            {
                _mainViewModel = vm;
                _mainViewModel.PropertyChanged += MainViewModel_PropertyChanged;
                MouseMove += (_, __) => vm.ResetAutoLogoutTimer();
                KeyDown += (_, __) => vm.ResetAutoLogoutTimer();
                MouseDown += (_, __) => vm.ResetAutoLogoutTimer();
                InputManager.Current.PreProcessInput += InputManager_PreProcessInput;
            }
        }

        void IMainWindow.Activate() => base.Activate();

        void IMainWindow.Focus() => base.Focus();

        void UpdateShellLayout()
        {
            var availableHeight = ActualHeight > 0 ? ActualHeight : SystemParameters.WorkArea.Height;
            var availableWidth = ActualWidth > 0 ? ActualWidth : SystemParameters.WorkArea.Width;
            var compact = availableHeight < CompactShellHeightThreshold ||
                          availableWidth < CompactShellWidthThreshold ||
                          SystemParameters.WorkArea.Height < CompactShellHeightThreshold ||
                          SystemParameters.WorkArea.Width < CompactShellWidthThreshold;

            if (_isCompactShellLayout != compact)
            {
                _isCompactShellLayout = compact;
                ApplyShellLayout(compact);
            }

            ApplyAdaptiveResourceScale();
        }

        void ApplyShellLayout(bool compact)
        {
            ShellHeader.Padding = compact ? new Thickness(6, 3, 6, 3) : new Thickness(8, 5, 8, 5);
            ShellMenu.Padding = compact ? new Thickness(4, 1, 4, 1) : new Thickness(4, 3, 4, 3);
            PageHeaderBand.Padding = compact ? new Thickness(8, 3, 8, 3) : new Thickness(8, 6, 8, 6);

            ShellTitleButton.MaxWidth = compact ? 190 : 250;
            ShellTitleButton.MinWidth = 0;
            ShellSearchBar.MinWidth = compact ? 160 : 180;
            ShellSearchBar.MaxWidth = compact ? 560 : 720;
            ShellUserButton.MaxWidth = compact ? 176 : 196;

            ShellSubtitle.Visibility = compact ? Visibility.Collapsed : Visibility.Visible;
            WorkflowGuideText.Visibility = compact ? Visibility.Collapsed : Visibility.Visible;
            ShellStatusFooter.Visibility = compact ? Visibility.Collapsed : Visibility.Visible;
            ShellFooterRow.Height = compact ? new GridLength(0) : GridLength.Auto;

            if (compact)
            {
                MainFrame.Margin = new Thickness(4);
            }
            else if (TryFindResource("PagePadding") is Thickness pagePadding)
            {
                MainFrame.Margin = pagePadding;
            }
        }

        void ApplyAdaptiveResourceScale()
        {
            var scale = GetAdaptiveResourceScale(ActualWidth > 0 ? ActualWidth : Width);
            if (_lastAdaptiveResourceScale.HasValue && Math.Abs(_lastAdaptiveResourceScale.Value - scale) < 0.001)
                return;

            _lastAdaptiveResourceScale = scale;

            SetScaledDoubleResource("ThemeCaptionFontSize", scale);
            SetScaledDoubleResource("ThemeBodyFontSize", scale);
            SetScaledDoubleResource("ThemeSectionFontSize", scale);
            SetScaledDoubleResource("ThemeTitleFontSize", scale);
            SetScaledDoubleResource("ThemeControlMinHeight", scale);
            SetScaledDoubleResource("ThemeDataGridRowHeight", scale);
            SetScaledDoubleResource("ThemeDataGridHeaderHeight", scale);
            SetScaledThicknessResource("CardPadding", scale);
            SetScaledThicknessResource("ToolbarPadding", scale);
            SetScaledThicknessResource("ControlPadding", scale);
        }

        static double GetAdaptiveResourceScale(double width)
        {
            if (width >= 3400)
                return 1.22;
            if (width >= 2800)
                return 1.16;
            if (width >= 2560)
                return 1.08;
            return 1;
        }

        void SetScaledDoubleResource(string key, double scale)
        {
            if (!_baseAdaptiveDoubleResources.TryGetValue(key, out var baseValue))
            {
                if (TryFindResource(key) is not double currentValue)
                    return;

                baseValue = currentValue;
                _baseAdaptiveDoubleResources[key] = baseValue;
            }

            Resources[key] = Math.Round(baseValue * scale, 1);
        }

        void SetScaledThicknessResource(string key, double scale)
        {
            if (!_baseAdaptiveThicknessResources.TryGetValue(key, out var baseValue))
            {
                if (TryFindResource(key) is not Thickness currentValue)
                    return;

                baseValue = currentValue;
                _baseAdaptiveThicknessResources[key] = baseValue;
            }

            Resources[key] = new Thickness(
                Math.Round(baseValue.Left * scale, 1),
                Math.Round(baseValue.Top * scale, 1),
                Math.Round(baseValue.Right * scale, 1),
                Math.Round(baseValue.Bottom * scale, 1));
        }

        void MainViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(MainViewModel.IsSidebarOpen) && sender is MainViewModel vm)
            {
                vm.IsSidebarOpen = false;
            }
        }

        void InputManager_PreProcessInput(object sender, PreProcessInputEventArgs e)
        {
            if (_mainViewModel == null)
                return;

            if (e.StagingItem.Input is MouseWheelEventArgs wheelArgs && SmoothMouseWheelScroll.TryHandle(wheelArgs))
            {
                _mainViewModel.ResetAutoLogoutTimer();
                return;
            }

            if (e.StagingItem.Input is MouseEventArgs or KeyboardEventArgs)
                _mainViewModel.ResetAutoLogoutTimer();
        }

        void SectionMenuItem_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is not MenuItem menuItem)
                return;

            if (FindSourceMenuItem(e.OriginalSource as DependencyObject) != menuItem)
                return;

            menuItem.IsSubmenuOpen = true;
            e.Handled = true;
        }

        static MenuItem? FindSourceMenuItem(DependencyObject? source)
        {
            while (source != null)
            {
                if (source is MenuItem item)
                    return item;

                source = VisualTreeHelper.GetParent(source);
            }

            return null;
        }
    }
}