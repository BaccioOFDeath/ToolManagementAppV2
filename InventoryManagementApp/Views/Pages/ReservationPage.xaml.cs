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
    public partial class ReservationPage : Page
    {
        private Task? _loadReservationsTask;
        private ReservationManagementViewModel? _loadedViewModel;

        public ReservationPage()
        {
            InitializeComponent();
            Loaded += ReservationPage_Loaded;
            DataContextChanged += ReservationPage_DataContextChanged;
            PreviewKeyDown += ReservationPage_PreviewKeyDown;
        }

        private async void ReservationPage_Loaded(object sender, RoutedEventArgs e)
        {
            Focus();
            FocusFirstSearchBox();

            if (DataContext is ReservationManagementViewModel vm)
            {
                await LoadReservationsOnceAsync(vm);
            }
        }

        private void ReservationPage_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (!ReferenceEquals(_loadedViewModel, e.NewValue))
            {
                _loadedViewModel = null;
                _loadReservationsTask = null;
            }
        }

        private async Task LoadReservationsOnceAsync(ReservationManagementViewModel vm)
        {
            if (ReferenceEquals(_loadedViewModel, vm) && _loadReservationsTask is { IsCompleted: false })
            {
                await _loadReservationsTask;
                return;
            }

            if (ReferenceEquals(_loadedViewModel, vm) && _loadReservationsTask is { IsCompletedSuccessfully: true })
            {
                return;
            }

            _loadedViewModel = vm;
            await Dispatcher.Yield(DispatcherPriority.Background);

            if (!ReferenceEquals(DataContext, vm) || !vm.LoadReservationsCommand.CanExecute(null))
            {
                return;
            }

            _loadReservationsTask = vm.LoadReservationsCommand.ExecuteAsync(null);
            await _loadReservationsTask;
        }

        private void ReservationRow_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (DataContext is ReservationManagementViewModel vm && vm.OpenReservationDetailsCommand.CanExecute(null))
            {
                UiActionGuard.Run(this, "Reservations", () => vm.OpenReservationDetailsCommand.Execute(null));
                e.Handled = true;
            }
        }

        private void ReservationRow_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (DataContext is ReservationManagementViewModel { IsLoading: true })
            {
                e.Handled = true;
                return;
            }

            GridContextMenuSelection.SelectRow(sender, e);
        }

        private void ReservationPage_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (DataContext is not ReservationManagementViewModel vm)
                return;

            if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.F)
            {
                FocusFirstSearchBox();
                e.Handled = true;
                return;
            }

            if (vm.IsLoading && IsReservationActionShortcut(e))
            {
                e.Handled = true;
                return;
            }

            if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.N && vm.AddReservationCommand.CanExecute(null))
            {
                UiActionGuard.RunAsync(this, "Reservations", async () => await vm.AddReservationCommand.ExecuteAsync(null));
                e.Handled = true;
                return;
            }

            if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.P && vm.PrintReservationDirectoryCommand.CanExecute(null))
            {
                UiActionGuard.Run(this, "Reservations", () => vm.PrintReservationDirectoryCommand.Execute(null));
                e.Handled = true;
                return;
            }

            if (Keyboard.Modifiers == (ModifierKeys.Control | ModifierKeys.Shift) && e.Key == Key.P && vm.PrintReservationHandoffCommand.CanExecute(null))
            {
                UiActionGuard.Run(this, "Reservations", () => vm.PrintReservationHandoffCommand.Execute(null));
                e.Handled = true;
                return;
            }

            if (!IsTextInputFocused() && Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.C && vm.CopyReservationHandoffCommand.CanExecute(null))
            {
                UiActionGuard.Run(this, "Reservations", () => vm.CopyReservationHandoffCommand.Execute(null));
                e.Handled = true;
                return;
            }

            if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.D && vm.OpenReservationDetailsCommand.CanExecute(null))
            {
                UiActionGuard.Run(this, "Reservations", () => vm.OpenReservationDetailsCommand.Execute(null));
                e.Handled = true;
                return;
            }

            if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.Enter && vm.ConfirmReservationCommand.CanExecute(null))
            {
                UiActionGuard.RunAsync(this, "Reservations", async () => await vm.ConfirmReservationCommand.ExecuteAsync(null));
                e.Handled = true;
                return;
            }

            if (Keyboard.Modifiers == (ModifierKeys.Control | ModifierKeys.Shift) && e.Key == Key.Enter && vm.FulfillReservationCommand.CanExecute(null))
            {
                UiActionGuard.RunAsync(this, "Reservations", async () => await vm.FulfillReservationCommand.ExecuteAsync(null));
                e.Handled = true;
                return;
            }

            if (Keyboard.Modifiers == ModifierKeys.None && e.Key == Key.Enter && vm.OpenReservationDetailsCommand.CanExecute(null))
            {
                UiActionGuard.Run(this, "Reservations", () => vm.OpenReservationDetailsCommand.Execute(null));
                e.Handled = true;
                return;
            }

            if (Keyboard.Modifiers == ModifierKeys.None && e.Key == Key.Delete && vm.CancelReservationCommand.CanExecute(null))
            {
                UiActionGuard.RunAsync(this, "Reservations", async () => await vm.CancelReservationCommand.ExecuteAsync(null));
                e.Handled = true;
            }
        }

        private static bool IsReservationActionShortcut(KeyEventArgs e)
        {
            if (Keyboard.Modifiers == ModifierKeys.Control)
            {
                return e.Key is Key.N or Key.P or Key.C or Key.D or Key.Enter;
            }

            if (Keyboard.Modifiers == (ModifierKeys.Control | ModifierKeys.Shift))
            {
                return e.Key is Key.P or Key.Enter;
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
            return Keyboard.FocusedElement is TextBoxBase or PasswordBox;
        }

        private static T? FindDescendant<T>(DependencyObject current) where T : DependencyObject
        {
            for (var index = 0; index < VisualTreeHelper.GetChildrenCount(current); index++)
            {
                var child = VisualTreeHelper.GetChild(current, index);
                if (child is T match)
                    return match;

                var nested = FindDescendant<T>(child);
                if (nested != null)
                    return nested;
            }

            return null;
        }
    }
}
