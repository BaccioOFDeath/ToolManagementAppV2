using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using InventoryManagementApp.ViewModels;

namespace InventoryManagementApp.Views.Pages
{
    public partial class CalibrationPage : Page
    {
        private Task? _loadCalibrationTask;
        private CalibrationManagementViewModel? _loadedViewModel;
        private CancellationTokenSource? _startupLoadCancellation;
        private int _startupLoadVersion;

        public CalibrationPage()
        {
            InitializeComponent();
            Loaded += CalibrationPage_Loaded;
            Unloaded += CalibrationPage_Unloaded;
            DataContextChanged += CalibrationPage_DataContextChanged;
            PreviewKeyDown += CalibrationPage_PreviewKeyDown;
            CalibrationGrid.ContextMenuOpening += CalibrationGrid_ContextMenuOpening;
        }

        private async void CalibrationPage_Loaded(object sender, RoutedEventArgs e)
        {
            Focus();
            FocusFirstSearchBox();

            if (DataContext is CalibrationManagementViewModel vm)
            {
                await LoadCalibrationOnceAsync(vm);
            }
        }

        private void CalibrationPage_Unloaded(object sender, RoutedEventArgs e)
        {
            CancelStartupLoad();
            _loadedViewModel = null;
            _loadCalibrationTask = null;
        }

        private void CalibrationPage_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (!ReferenceEquals(_loadedViewModel, e.NewValue))
            {
                CancelStartupLoad();
                _loadedViewModel = null;
                _loadCalibrationTask = null;
            }
        }

        private async Task LoadCalibrationOnceAsync(CalibrationManagementViewModel vm)
        {
            if (ReferenceEquals(_loadedViewModel, vm) && _loadCalibrationTask is { IsCompleted: false })
            {
                await _loadCalibrationTask;
                return;
            }

            if (ReferenceEquals(_loadedViewModel, vm) && _loadCalibrationTask is { IsCompletedSuccessfully: true })
            {
                return;
            }

            CancelStartupLoad();
            var cancellation = new CancellationTokenSource();
            _startupLoadCancellation = cancellation;
            var token = cancellation.Token;
            var loadVersion = _startupLoadVersion;
            _loadedViewModel = vm;

            try
            {
                await Dispatcher.Yield(DispatcherPriority.Background);
                token.ThrowIfCancellationRequested();

                if (loadVersion != _startupLoadVersion || !ReferenceEquals(DataContext, vm) || !vm.LoadCalibrationCommand.CanExecute(null))
                {
                    return;
                }

                _loadCalibrationTask = vm.LoadCalibrationCommand.ExecuteAsync(null);
                await _loadCalibrationTask;
                token.ThrowIfCancellationRequested();

                if (loadVersion != _startupLoadVersion || !ReferenceEquals(DataContext, vm))
                {
                    return;
                }
            }
            catch (OperationCanceledException) when (token.IsCancellationRequested || !IsLoaded || !ReferenceEquals(DataContext, vm))
            {
            }
            finally
            {
                if (ReferenceEquals(_startupLoadCancellation, cancellation))
                {
                    _startupLoadCancellation.Dispose();
                    _startupLoadCancellation = null;
                }
            }
        }

        private void CancelStartupLoad()
        {
            _startupLoadVersion++;
            _startupLoadCancellation?.Cancel();
            _startupLoadCancellation?.Dispose();
            _startupLoadCancellation = null;
        }

        private void CalibrationRow_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (DataContext is CalibrationManagementViewModel { IsLoading: true })
            {
                e.Handled = true;
                return;
            }

            if (GridContextMenuSelection.SelectRow(sender, e) == null)
                return;

            if (DataContext is CalibrationManagementViewModel vm && vm.OpenCalibrationDetailsCommand.CanExecute(null))
            {
                UiActionGuard.Run(this, "Calibration", () => vm.OpenCalibrationDetailsCommand.Execute(null));
                e.Handled = true;
                return;
            }

            e.Handled = true;
        }

        private void CalibrationRow_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (DataContext is CalibrationManagementViewModel { IsLoading: true })
            {
                e.Handled = true;
                return;
            }

            GridContextMenuSelection.SelectRow(sender, e);
        }

        private void CalibrationGrid_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            if (DataContext is CalibrationManagementViewModel { IsLoading: true })
            {
                e.Handled = true;
            }
        }

        private void CalibrationPage_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (DataContext is not CalibrationManagementViewModel vm)
                return;

            if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.F)
            {
                FocusFirstSearchBox();
                e.Handled = true;
                return;
            }

            if (IsTextInputFocused() && IsCalibrationActionShortcut(e))
            {
                return;
            }

            if (vm.IsLoading && IsCalibrationActionShortcut(e))
            {
                e.Handled = true;
                return;
            }

            if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.N && vm.AddCalibrationCommand.CanExecute(null))
            {
                UiActionGuard.RunAsync(this, "Calibration", async () => await vm.AddCalibrationCommand.ExecuteAsync(null));
                e.Handled = true;
                return;
            }

            if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.R && vm.RefreshCommand.CanExecute(null))
            {
                UiActionGuard.RunAsync(this, "Calibration", async () => await vm.RefreshCommand.ExecuteAsync(null));
                e.Handled = true;
                return;
            }

            if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.P && vm.PrintCalibrationListCommand.CanExecute(null))
            {
                UiActionGuard.Run(this, "Calibration", () => vm.PrintCalibrationListCommand.Execute(null));
                e.Handled = true;
                return;
            }

            if (Keyboard.Modifiers == (ModifierKeys.Control | ModifierKeys.Shift) && e.Key == Key.P && vm.PrintSelectedCalibrationCommand.CanExecute(null))
            {
                UiActionGuard.Run(this, "Calibration", () => vm.PrintSelectedCalibrationCommand.Execute(null));
                e.Handled = true;
                return;
            }

            if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.C && vm.CopySelectedCalibrationCommand.CanExecute(null))
            {
                UiActionGuard.Run(this, "Calibration", () => vm.CopySelectedCalibrationCommand.Execute(null));
                e.Handled = true;
                return;
            }

            if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.D && vm.OpenCalibrationDetailsCommand.CanExecute(null))
            {
                UiActionGuard.Run(this, "Calibration", () => vm.OpenCalibrationDetailsCommand.Execute(null));
                e.Handled = true;
                return;
            }

            if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.E && vm.EditCalibrationCommand.CanExecute(null))
            {
                UiActionGuard.RunAsync(this, "Calibration", async () => await vm.EditCalibrationCommand.ExecuteAsync(null));
                e.Handled = true;
                return;
            }

            if (Keyboard.Modifiers == ModifierKeys.None && e.Key == Key.Enter && vm.OpenCalibrationDetailsCommand.CanExecute(null))
            {
                UiActionGuard.Run(this, "Calibration", () => vm.OpenCalibrationDetailsCommand.Execute(null));
                e.Handled = true;
                return;
            }

            if (Keyboard.Modifiers == ModifierKeys.None && e.Key == Key.Delete && vm.DeleteCalibrationCommand.CanExecute(null))
            {
                UiActionGuard.RunAsync(this, "Calibration", async () => await vm.DeleteCalibrationCommand.ExecuteAsync(null));
                e.Handled = true;
            }
        }

        private static bool IsCalibrationActionShortcut(KeyEventArgs e)
        {
            if (Keyboard.Modifiers == ModifierKeys.Control)
            {
                return e.Key is Key.N or Key.R or Key.P or Key.C or Key.D or Key.E;
            }

            if (Keyboard.Modifiers == (ModifierKeys.Control | ModifierKeys.Shift))
            {
                return e.Key == Key.P;
            }

            return Keyboard.Modifiers == ModifierKeys.None && e.Key is Key.Enter or Key.Delete;
        }

        private void FocusFirstSearchBox()
        {
            var searchBox = FindDescendant<TextBox>(this);
            if (searchBox == null)
                return;

            searchBox.Focus();
            searchBox.SelectAll();
        }

        private static bool IsTextInputFocused()
        {
            return Keyboard.FocusedElement is TextBoxBase or PasswordBox or ComboBox;
        }

        private static T? FindDescendant<T>(DependencyObject current) where T : DependencyObject
        {
            var pending = new Stack<DependencyObject>();
            pending.Push(current);

            while (pending.Count > 0)
            {
                var parent = pending.Pop();
                var childCount = GetVisualChildCount(parent);

                for (var index = childCount - 1; index >= 0; index--)
                {
                    var child = VisualTreeHelper.GetChild(parent, index);
                    if (child is T match)
                        return match;

                    pending.Push(child);
                }
            }

            return null;
        }

        private static int GetVisualChildCount(DependencyObject current)
        {
            try
            {
                return VisualTreeHelper.GetChildrenCount(current);
            }
            catch (InvalidOperationException)
            {
                return 0;
            }
        }
    }
}