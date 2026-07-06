using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Threading;
using System.Threading.Tasks;
using InventoryManagementApp.Models.Domain;
using InventoryManagementApp.ViewModels;

namespace InventoryManagementApp.Views.Pages
{
    public partial class KitManagementPage : Page
    {
        private KitManagementViewModel? _loadedViewModel;
        private Task? _loadKitsTask;

        public KitManagementPage()
        {
            InitializeComponent();
            Loaded += KitManagementPage_Loaded;
            DataContextChanged += KitManagementPage_DataContextChanged;
            PreviewKeyDown += KitManagementPage_PreviewKeyDown;
            KitsGrid.ContextMenuOpening += KitsGrid_ContextMenuOpening;
            KitItemsGrid.ContextMenuOpening += KitItemsGrid_ContextMenuOpening;
        }

        private async void KitManagementPage_Loaded(object sender, RoutedEventArgs e)
        {
            SearchTextBox.Focus();

            if (DataContext is KitManagementViewModel vm)
            {
                await LoadKitsOnceForViewModelAsync(vm);
            }
        }

        private void KitManagementPage_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (!ReferenceEquals(e.NewValue, _loadedViewModel))
            {
                _loadedViewModel = null;
                _loadKitsTask = null;
            }
        }

        private async Task LoadKitsOnceForViewModelAsync(KitManagementViewModel vm)
        {
            if (ReferenceEquals(_loadedViewModel, vm) && _loadKitsTask != null)
            {
                await _loadKitsTask;
                return;
            }

            _loadedViewModel = vm;
            _loadKitsTask = LoadKitsAfterFirstPaintAsync(vm);
            await _loadKitsTask;
        }

        private async Task LoadKitsAfterFirstPaintAsync(KitManagementViewModel vm)
        {
            await Dispatcher.InvokeAsync(() => { }, DispatcherPriority.Background);

            if (DataContext is KitManagementViewModel currentVm && ReferenceEquals(currentVm, vm) && vm.LoadKitsCommand.CanExecute(null))
            {
                await vm.LoadKitsCommand.ExecuteAsync(null);
            }
        }

        private void KitManagementPage_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (DataContext is not KitManagementViewModel vm)
                return;

            if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.F)
            {
                SearchTextBox.Focus();
                SearchTextBox.SelectAll();
                e.Handled = true;
                return;
            }

            if (vm.IsKitItemInteractionBusy && IsManagedKitShortcut(e))
            {
                e.Handled = true;
                return;
            }

            if (IsTextInputFocused() && IsManagedKitShortcut(e))
            {
                return;
            }

            if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.N && vm.AddKitCommand.CanExecute(null))
            {
                UiActionGuard.RunAsync(this, "Kit Management", async () => await vm.AddKitCommand.ExecuteAsync(null));
                e.Handled = true;
            }
            else if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.E && vm.EditKitCommand.CanExecute(null))
            {
                UiActionGuard.RunAsync(this, "Kit Management", async () => await vm.EditKitCommand.ExecuteAsync(null));
                e.Handled = true;
            }
            else if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.I && vm.AddKitItemCommand.CanExecute(null))
            {
                UiActionGuard.RunAsync(this, "Kit Management", async () => await vm.AddKitItemCommand.ExecuteAsync(null));
                e.Handled = true;
            }
            else if (Keyboard.Modifiers == (ModifierKeys.Control | ModifierKeys.Shift) && e.Key == Key.E && vm.EditKitItemCommand.CanExecute(null))
            {
                UiActionGuard.RunAsync(this, "Kit Management", async () => await vm.EditKitItemCommand.ExecuteAsync(null));
                e.Handled = true;
            }
            else if (Keyboard.Modifiers == (ModifierKeys.Control | ModifierKeys.Shift) && e.Key == Key.C && vm.CopySelectedKitCommand.CanExecute(null))
            {
                UiActionGuard.Run(this, "Kit Management", () => vm.CopySelectedKitCommand.Execute(null));
                e.Handled = true;
            }
            else if (e.Key == Key.Delete && vm.DeleteKitCommand.CanExecute(null))
            {
                UiActionGuard.RunAsync(this, "Kit Management", async () => await vm.DeleteKitCommand.ExecuteAsync(null));
                e.Handled = true;
            }
            else if (e.Key == Key.Enter && vm.OpenKitDetailsCommand.CanExecute(null))
            {
                UiActionGuard.Run(this, "Kit Management", () => vm.OpenKitDetailsCommand.Execute(null));
                e.Handled = true;
            }
            else if (e.Key == Key.F5 && vm.RefreshCommand.CanExecute(null))
            {
                UiActionGuard.RunAsync(this, "Kit Management", async () => await vm.RefreshCommand.ExecuteAsync(null));
                e.Handled = true;
            }
            else if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.P && vm.PrintSelectedKitCommand.CanExecute(null))
            {
                UiActionGuard.Run(this, "Kit Management", () => vm.PrintSelectedKitCommand.Execute(null));
                e.Handled = true;
            }
            else if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.D && vm.PrintKitListCommand.CanExecute(null))
            {
                UiActionGuard.Run(this, "Kit Management", () => vm.PrintKitListCommand.Execute(null));
                e.Handled = true;
            }
        }

        private static bool IsManagedKitShortcut(KeyEventArgs e)
        {
            if (e.Key == Key.Enter || e.Key == Key.F5 || e.Key == Key.Delete)
                return true;

            var modifiers = Keyboard.Modifiers;
            if (modifiers != ModifierKeys.Control && modifiers != (ModifierKeys.Control | ModifierKeys.Shift))
                return false;

            return e.Key == Key.N
                || e.Key == Key.E
                || e.Key == Key.I
                || e.Key == Key.C
                || e.Key == Key.P
                || e.Key == Key.D;
        }

        private void KitRow_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (DataContext is not KitManagementViewModel vm)
                return;

            if (vm.IsKitItemInteractionBusy)
            {
                e.Handled = true;
                return;
            }

            if (sender is FrameworkElement { DataContext: Kit kit })
            {
                vm.SelectedKit = kit;
            }

            if (vm.OpenKitDetailsCommand.CanExecute(null))
            {
                UiActionGuard.Run(this, "Kit Management", () => vm.OpenKitDetailsCommand.Execute(null));
                e.Handled = true;
                return;
            }

            e.Handled = true;
        }

        private void KitItemRow_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (DataContext is not KitManagementViewModel vm)
                return;

            if (vm.IsKitItemInteractionBusy)
            {
                e.Handled = true;
                return;
            }

            if (sender is FrameworkElement { DataContext: KitItem kitItem })
            {
                vm.SelectedKitItem = kitItem;
            }

            if (vm.EditKitItemCommand.CanExecute(null))
            {
                UiActionGuard.RunAsync(this, "Kit Management", async () => await vm.EditKitItemCommand.ExecuteAsync(null));
                e.Handled = true;
                return;
            }

            e.Handled = true;
        }

        private void DataGridRow_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (DataContext is KitManagementViewModel { IsKitItemInteractionBusy: true })
            {
                e.Handled = true;
                return;
            }

            GridContextMenuSelection.SelectRow(sender, e);
        }

        private void KitsGrid_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            SuppressContextMenuDuringLoading(e);
        }

        private void KitItemsGrid_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            SuppressContextMenuDuringLoading(e);
        }

        private bool SuppressContextMenuDuringLoading(ContextMenuEventArgs e)
        {
            if (DataContext is KitManagementViewModel { IsKitItemInteractionBusy: true })
            {
                e.Handled = true;
                return true;
            }

            return false;
        }

        private static bool IsTextInputFocused()
        {
            return Keyboard.FocusedElement is TextBoxBase or PasswordBox or ComboBox;
        }
    }
}