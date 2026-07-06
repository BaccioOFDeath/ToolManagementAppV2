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
    public partial class MaintenancePage : Page
    {
        private Task? _loadMaintenanceTask;
        private MaintenanceManagementViewModel? _loadedViewModel;
        private CancellationTokenSource? _startupLoadCancellation;
        private int _startupLoadVersion;

        public MaintenancePage()
        {
            InitializeComponent();
            Loaded += MaintenancePage_Loaded;
            Unloaded += MaintenancePage_Unloaded;
            DataContextChanged += MaintenancePage_DataContextChanged;
            PreviewKeyDown += MaintenancePage_PreviewKeyDown;
            MaintenanceGrid.ContextMenuOpening += MaintenanceGrid_ContextMenuOpening;
        }

        private async void MaintenancePage_Loaded(object sender, RoutedEventArgs e)
        {
            Focus();
            FocusFirstSearchBox();

            if (DataContext is MaintenanceManagementViewModel vm)
            {
                await LoadMaintenanceOnceAsync(vm);
            }
        }

        private void MaintenancePage_Unloaded(object sender, RoutedEventArgs e)
        {
            CancelStartupLoad();
            _loadedViewModel = null;
            _loadMaintenanceTask = null;
        }

        private void MaintenancePage_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (!ReferenceEquals(_loadedViewModel, e.NewValue))
            {
                CancelStartupLoad();
                _loadedViewModel = null;
                _loadMaintenanceTask = null;
            }
        }

        private async Task LoadMaintenanceOnceAsync(MaintenanceManagementViewModel vm)
        {
            if (ReferenceEquals(_loadedViewModel, vm) && _loadMaintenanceTask is { IsCompleted: false })
            {
                await _loadMaintenanceTask;
                return;
            }

            if (ReferenceEquals(_loadedViewModel, vm) && _loadMaintenanceTask is { IsCompletedSuccessfully: true })
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

                if (loadVersion != _startupLoadVersion || !ReferenceEquals(DataContext, vm) || !vm.LoadMaintenanceCommand.CanExecute(null))
                {
                    return;
                }

                _loadMaintenanceTask = vm.LoadMaintenanceCommand.ExecuteAsync(null);
                await _loadMaintenanceTask;
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

        private void MaintenanceRow_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (DataContext is MaintenanceManagementViewModel { IsLoading: true })
            {
                e.Handled = true;
                return;
            }

            if (GridContextMenuSelection.SelectRow(sender, e) == null)
                return;

            if (DataContext is MaintenanceManagementViewModel vm && vm.OpenMaintenanceDetailsCommand.CanExecute(null))
            {
                UiActionGuard.Run(this, "Maintenance", () => vm.OpenMaintenanceDetailsCommand.Execute(null));
                e.Handled = true;
                return;
            }

            e.Handled = true;
        }

        private void MaintenanceRow_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (DataContext is MaintenanceManagementViewModel { IsLoading: true })
            {
                e.Handled = true;
                return;
            }

            GridContextMenuSelection.SelectRow(sender, e);
        }

        private void MaintenanceGrid_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            if (DataContext is MaintenanceManagementViewModel { IsLoading: true })
            {
                e.Handled = true;
            }
        }

        private void MaintenancePage_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (DataContext is not MaintenanceManagementViewModel vm)
                return;

            if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.F)
            {
                FocusFirstSearchBox();
                e.Handled = true;
                return;
            }

            if (IsTextInputFocused() && IsMaintenanceActionShortcut(e))
            {
                return;
            }

            if (vm.IsLoading && IsMaintenanceActionShortcut(e))
            {
                e.Handled = true;
                return;
            }

            if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.N && vm.AddMaintenanceCommand.CanExecute(null))
            {
                UiActionGuard.RunAsync(this, "Maintenance", async () => await vm.AddMaintenanceCommand.ExecuteAsync(null));
                e.Handled = true;
                return;
            }

            if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.R && vm.RefreshCommand.CanExecute(null))
            {
                UiActionGuard.RunAsync(this, "Maintenance", async () => await vm.RefreshCommand.ExecuteAsync(null));
                e.Handled = true;
                return;
            }

            if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.P && vm.PrintMaintenanceListCommand.CanExecute(null))
            {
                UiActionGuard.Run(this, "Maintenance", () => vm.PrintMaintenanceListCommand.Execute(null));
                e.Handled = true;
                return;
            }

            if (Keyboard.Modifiers == (ModifierKeys.Control | ModifierKeys.Shift) && e.Key == Key.P && vm.PrintSelectedMaintenanceCommand.CanExecute(null))
            {
                UiActionGuard.Run(this, "Maintenance", () => vm.PrintSelectedMaintenanceCommand.Execute(null));
                e.Handled = true;
                return;
            }

            if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.C && vm.CopySelectedMaintenanceCommand.CanExecute(null))
            {
                UiActionGuard.Run(this, "Maintenance", () => vm.CopySelectedMaintenanceCommand.Execute(null));
                e.Handled = true;
                return;
            }

            if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.D && vm.OpenMaintenanceDetailsCommand.CanExecute(null))
            {
                UiActionGuard.Run(this, "Maintenance", () => vm.OpenMaintenanceDetailsCommand.Execute(null));
                e.Handled = true;
                return;
            }

            if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.E && vm.EditMaintenanceCommand.CanExecute(null))
            {
                UiActionGuard.RunAsync(this, "Maintenance", async () => await vm.EditMaintenanceCommand.ExecuteAsync(null));
                e.Handled = true;
                return;
            }

            if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.Enter && vm.CompleteMaintenanceCommand.CanExecute(null))
            {
                UiActionGuard.RunAsync(this, "Maintenance", async () => await vm.CompleteMaintenanceCommand.ExecuteAsync(null));
                e.Handled = true;
                return;
            }

            if (Keyboard.Modifiers == ModifierKeys.None && e.Key == Key.Enter && vm.OpenMaintenanceDetailsCommand.CanExecute(null))
            {
                UiActionGuard.Run(this, "Maintenance", () => vm.OpenMaintenanceDetailsCommand.Execute(null));
                e.Handled = true;
                return;
            }

            if (Keyboard.Modifiers == ModifierKeys.None && e.Key == Key.Delete && vm.DeleteMaintenanceCommand.CanExecute(null))
            {
                UiActionGuard.RunAsync(this, "Maintenance", async () => await vm.DeleteMaintenanceCommand.ExecuteAsync(null));
                e.Handled = true;
            }
        }

        private static bool IsMaintenanceActionShortcut(KeyEventArgs e)
        {
            if (Keyboard.Modifiers == ModifierKeys.Control)
            {
                return e.Key is Key.N or Key.R or Key.P or Key.C or Key.D or Key.E or Key.Enter;
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