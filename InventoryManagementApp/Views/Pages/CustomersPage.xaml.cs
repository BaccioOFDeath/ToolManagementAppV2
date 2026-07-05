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
    public partial class CustomersPage : Page
    {
        private Task? _loadCustomersTask;
        private CustomerManagementViewModel? _loadedViewModel;
        private CancellationTokenSource? _loadCustomersCancellation;
        private int _loadCustomersVersion;

        public CustomersPage()
        {
            InitializeComponent();
            Loaded += CustomersPage_Loaded;
            Unloaded += CustomersPage_Unloaded;
            DataContextChanged += CustomersPage_DataContextChanged;
            PreviewKeyDown += CustomersPage_PreviewKeyDown;
        }

        private async void CustomersPage_Loaded(object sender, RoutedEventArgs e)
        {
            Focus();
            FocusFirstSearchBox();

            if (DataContext is CustomerManagementViewModel vm)
            {
                await LoadCustomersOnceAsync(vm);
            }
        }

        private void CustomersPage_Unloaded(object sender, RoutedEventArgs e)
        {
            CancelPageOwnedLoad();
        }

        private void CustomersPage_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (!ReferenceEquals(_loadedViewModel, e.NewValue))
            {
                CancelPageOwnedLoad();
                _loadedViewModel = null;
                _loadCustomersTask = null;
            }
        }

        private async Task LoadCustomersOnceAsync(CustomerManagementViewModel vm)
        {
            if (ReferenceEquals(_loadedViewModel, vm) && _loadCustomersTask is { IsCompleted: false })
            {
                await _loadCustomersTask;
                return;
            }

            if (ReferenceEquals(_loadedViewModel, vm) && _loadCustomersTask is { IsCompletedSuccessfully: true })
            {
                return;
            }

            CancelPageOwnedLoad();
            _loadedViewModel = vm;
            var loadVersion = ++_loadCustomersVersion;
            _loadCustomersCancellation = new CancellationTokenSource();
            var cancellationToken = _loadCustomersCancellation.Token;

            await Dispatcher.Yield(DispatcherPriority.Background);

            if (cancellationToken.IsCancellationRequested || loadVersion != _loadCustomersVersion || !ReferenceEquals(DataContext, vm) || vm.IsCustomerDirectoryBusy)
            {
                return;
            }

            _loadCustomersTask = vm.LoadCustomersAsync();
            await _loadCustomersTask;

            if (cancellationToken.IsCancellationRequested || loadVersion != _loadCustomersVersion || !ReferenceEquals(DataContext, vm))
            {
                return;
            }
        }

        private void CancelPageOwnedLoad()
        {
            _loadCustomersVersion++;
            _loadCustomersCancellation?.Cancel();
            _loadCustomersCancellation?.Dispose();
            _loadCustomersCancellation = null;
        }

        private void CustomerRow_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (DataContext is not CustomerManagementViewModel vm)
                return;

            if (vm.IsCustomerDirectoryBusy)
            {
                e.Handled = true;
                return;
            }

            if (GridContextMenuSelection.SelectRow(sender, e) == null)
                return;

            if (vm.OpenCustomerDetailsCommand.CanExecute(null))
            {
                UiActionGuard.Run(this, "Customers", () => vm.OpenCustomerDetailsCommand.Execute(null));
                e.Handled = true;
                return;
            }

            e.Handled = true;
        }

        private void CustomerRow_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (DataContext is CustomerManagementViewModel { IsCustomerDirectoryBusy: true })
            {
                e.Handled = true;
                return;
            }

            GridContextMenuSelection.SelectRow(sender, e);
        }

        private void CustomerDataGrid_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            if (DataContext is CustomerManagementViewModel { IsCustomerDirectoryBusy: true })
            {
                e.Handled = true;
            }
        }

        private void CustomersPage_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (DataContext is not CustomerManagementViewModel vm)
                return;

            if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.F)
            {
                FocusFirstSearchBox();
                e.Handled = true;
                return;
            }

            if (vm.IsCustomerDirectoryBusy && IsCustomerActionShortcut(e))
            {
                e.Handled = true;
                return;
            }

            if (IsTextInputFocused() && IsCustomerActionShortcut(e))
            {
                return;
            }

            if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.N && vm.AddCustomerCommand.CanExecute(null))
            {
                UiActionGuard.RunAsync(this, "Customers", async () => await vm.AddCustomerCommand.ExecuteAsync(null));
                e.Handled = true;
                return;
            }

            if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.R && vm.SearchCustomersCommand.CanExecute(null))
            {
                UiActionGuard.RunAsync(this, "Customers", async () => await vm.SearchCustomersCommand.ExecuteAsync(null));
                e.Handled = true;
                return;
            }

            if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.E && vm.EditCustomerCommand.CanExecute(null))
            {
                UiActionGuard.RunAsync(this, "Customers", async () => await vm.EditCustomerCommand.ExecuteAsync(null));
                e.Handled = true;
                return;
            }

            if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.P && vm.PrintCustomerDirectoryCommand.CanExecute(null))
            {
                UiActionGuard.Run(this, "Customers", () => vm.PrintCustomerDirectoryCommand.Execute(null));
                e.Handled = true;
                return;
            }

            if (Keyboard.Modifiers == (ModifierKeys.Control | ModifierKeys.Shift) && e.Key == Key.P && vm.PrintSelectedCustomerCommand.CanExecute(null))
            {
                UiActionGuard.Run(this, "Customers", () => vm.PrintSelectedCustomerCommand.Execute(null));
                e.Handled = true;
                return;
            }

            if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.C && vm.CopySelectedCustomerCommand.CanExecute(null))
            {
                UiActionGuard.Run(this, "Customers", () => vm.CopySelectedCustomerCommand.Execute(null));
                e.Handled = true;
                return;
            }

            if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.D && vm.OpenCustomerDetailsCommand.CanExecute(null))
            {
                UiActionGuard.Run(this, "Customers", () => vm.OpenCustomerDetailsCommand.Execute(null));
                e.Handled = true;
                return;
            }

            if (Keyboard.Modifiers == ModifierKeys.None && e.Key == Key.Enter && vm.OpenCustomerDetailsCommand.CanExecute(null))
            {
                UiActionGuard.Run(this, "Customers", () => vm.OpenCustomerDetailsCommand.Execute(null));
                e.Handled = true;
                return;
            }

            if (Keyboard.Modifiers == ModifierKeys.None && e.Key == Key.Delete && vm.DeleteCustomerCommand.CanExecute(null))
            {
                UiActionGuard.RunAsync(this, "Customers", async () => await vm.DeleteCustomerCommand.ExecuteAsync(null));
                e.Handled = true;
            }
        }

        private static bool IsCustomerActionShortcut(KeyEventArgs e)
        {
            if (Keyboard.Modifiers == ModifierKeys.Control)
            {
                return e.Key is Key.N or Key.R or Key.E or Key.P or Key.C or Key.D;
            }

            if (Keyboard.Modifiers == (ModifierKeys.Control | ModifierKeys.Shift))
            {
                return e.Key == Key.P;
            }

            return Keyboard.Modifiers == ModifierKeys.None && (e.Key is Key.Enter or Key.Delete);
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

                for (var index = 0; index < childCount; index++)
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
            catch (System.InvalidOperationException)
            {
                return 0;
            }
        }
    }
}