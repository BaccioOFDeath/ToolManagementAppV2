using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using InventoryManagementApp.Models.Domain;
using InventoryManagementApp.ViewModels;

namespace InventoryManagementApp.Views.Pages
{
    public partial class ManageRentalsPage : Page
    {
        const double CompactHeightThreshold = 650;
        bool _isCompactHeight;
        Task? _loadRentalsTask;
        ManageRentalsViewModel? _loadedViewModel;
        int _loadVersion;

        public ManageRentalsPage()
        {
            InitializeComponent();
            Loaded += ManageRentalsPage_Loaded;
            Unloaded += ManageRentalsPage_Unloaded;
            DataContextChanged += ManageRentalsPage_DataContextChanged;
            SizeChanged += ManageRentalsPage_SizeChanged;
            PreviewKeyDown += ManageRentalsPage_PreviewKeyDown;
            RentalDeskGrid.ContextMenuOpening += RentalDeskGrid_ContextMenuOpening;
            RequestQueueGrid.ContextMenuOpening += RequestQueueGrid_ContextMenuOpening;
        }

        private async void ManageRentalsPage_Loaded(object sender, RoutedEventArgs e)
        {
            Focus();
            SearchTextBox.Focus();
            SearchTextBox.SelectAll();
            UpdateCompactHeightMode();

            if (DataContext is ManageRentalsViewModel vm)
            {
                await LoadRentalsOnceAsync(vm);
            }
        }

        private void ManageRentalsPage_Unloaded(object sender, RoutedEventArgs e)
        {
            _loadVersion++;
            _loadedViewModel = null;
            _loadRentalsTask = null;
        }

        private void ManageRentalsPage_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (!ReferenceEquals(e.NewValue, _loadedViewModel))
            {
                _loadVersion++;
                _loadedViewModel = null;
                _loadRentalsTask = null;
            }
        }

        private async Task LoadRentalsOnceAsync(ManageRentalsViewModel vm)
        {
            var loadVersion = _loadVersion;

            if (ReferenceEquals(_loadedViewModel, vm) && _loadRentalsTask is { IsCompleted: false })
            {
                await _loadRentalsTask;
                return;
            }

            if (ReferenceEquals(_loadedViewModel, vm) && _loadRentalsTask is { IsCompletedSuccessfully: true })
            {
                return;
            }

            _loadedViewModel = vm;
            await Dispatcher.Yield(DispatcherPriority.Background);

            if (!IsCurrentLoad(vm, loadVersion) || vm.IsLoading)
            {
                return;
            }

            _loadRentalsTask = vm.LoadRentalsAsync();
            try
            {
                await _loadRentalsTask;
            }
            finally
            {
                if (!IsCurrentLoad(vm, loadVersion))
                    _loadRentalsTask = null;
            }
        }

        private bool IsCurrentLoad(ManageRentalsViewModel vm, int loadVersion)
        {
            return loadVersion == _loadVersion && ReferenceEquals(DataContext, vm);
        }

        private void ManageRentalsPage_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            UpdateCompactHeightMode();
        }

        private void UpdateCompactHeightMode()
        {
            var compactHeight = ActualHeight > 0 && ActualHeight < CompactHeightThreshold;
            if (compactHeight == _isCompactHeight)
                return;

            _isCompactHeight = compactHeight;
            RentalStatsStrip.Visibility = compactHeight ? Visibility.Collapsed : Visibility.Visible;
            RentalStatsRow.Height = compactHeight ? new GridLength(0) : GridLength.Auto;
            RentalMainRow.Height = compactHeight ? new GridLength(1.6, GridUnitType.Star) : new GridLength(1.55, GridUnitType.Star);
            RentalSplitterRow.Height = compactHeight ? new GridLength(6) : new GridLength(6);
            RequestQueueRow.Height = compactHeight ? new GridLength(1.15, GridUnitType.Star) : new GridLength(1.25, GridUnitType.Star);
            RequestDetailPanel.Visibility = compactHeight ? Visibility.Collapsed : Visibility.Visible;
            RequestDetailSplitter.Visibility = compactHeight ? Visibility.Collapsed : Visibility.Visible;
            RequestListColumn.Width = compactHeight ? new GridLength(1, GridUnitType.Star) : new GridLength(1.55, GridUnitType.Star);
            RequestDetailSplitterColumn.Width = compactHeight ? new GridLength(0) : new GridLength(6);
            RequestDetailColumn.Width = compactHeight ? new GridLength(0) : new GridLength(0.95, GridUnitType.Star);
            RequestDetailColumn.MinWidth = compactHeight ? 0 : 300;
        }

        private void RentalRow_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (DataContext is ManageRentalsViewModel { IsLoading: true })
            {
                e.Handled = true;
                return;
            }

            if (SelectRowForContextMenu(sender, e) == null)
                return;

            e.Handled = true;

            if (DataContext is ManageRentalsViewModel vm && vm.OpenRentalDetailsCommand.CanExecute(null))
            {
                UiActionGuard.Run(this, "Rentals", () => vm.OpenRentalDetailsCommand.Execute(null));
            }
        }

        private void RentalRow_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            SelectRowForContextMenu(sender, e);
        }

        private void RequestRow_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (DataContext is ManageRentalsViewModel { IsLoading: true })
            {
                e.Handled = true;
                return;
            }

            if (SelectRowForContextMenu(sender, e) == null)
                return;

            e.Handled = true;

            if (DataContext is ManageRentalsViewModel vm && vm.OpenRequestDetailsCommand.CanExecute(null))
            {
                UiActionGuard.Run(this, "Rentals", () => vm.OpenRequestDetailsCommand.Execute(null));
            }
        }

        private void RequestRow_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            SelectRowForContextMenu(sender, e);
        }

        private void RentalDeskGrid_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            SuppressContextMenuDuringLoading(e);
        }

        private void RequestQueueGrid_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            SuppressContextMenuDuringLoading(e);
        }

        private bool SuppressContextMenuDuringLoading(ContextMenuEventArgs e)
        {
            if (DataContext is ManageRentalsViewModel { IsLoading: true })
            {
                e.Handled = true;
                return true;
            }

            return false;
        }

        private void ManageRentalsPage_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (DataContext is not ManageRentalsViewModel vm)
                return;

            if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.F)
            {
                SearchTextBox.Focus();
                SearchTextBox.SelectAll();
                e.Handled = true;
                return;
            }

            if (vm.IsLoading && IsTextEditingElement(e.OriginalSource) && e.Key is not Key.Tab and not Key.Escape)
            {
                e.Handled = true;
                return;
            }

            if (IsTextEditingElement(e.OriginalSource) && IsRentalActionShortcut(e))
                return;

            if (vm.IsLoading && IsRentalActionShortcut(e))
            {
                e.Handled = true;
                return;
            }

            if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.P && vm.PrintSearchResultsCommand.CanExecute(null))
            {
                UiActionGuard.Run(this, "Rentals", () => vm.PrintSearchResultsCommand.Execute(null));
                e.Handled = true;
                return;
            }

            if (Keyboard.Modifiers == (ModifierKeys.Control | ModifierKeys.Shift) && e.Key == Key.P && vm.PrintCheckedOutCommand.CanExecute(null))
            {
                UiActionGuard.Run(this, "Rentals", () => vm.PrintCheckedOutCommand.Execute(null));
                e.Handled = true;
                return;
            }

            if (Keyboard.Modifiers == (ModifierKeys.Control | ModifierKeys.Shift) && e.Key == Key.R && vm.PrintRequestsCommand.CanExecute(null))
            {
                UiActionGuard.Run(this, "Rentals", () => vm.PrintRequestsCommand.Execute(null));
                e.Handled = true;
                return;
            }

            if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.D)
            {
                OpenFocusedDetails(vm);
                e.Handled = true;
                return;
            }

            if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.H && vm.OpenHistoryCommand.CanExecute(null))
            {
                UiActionGuard.RunAsync(this, "Rentals", async () => await vm.OpenHistoryCommand.ExecuteAsync(null));
                e.Handled = true;
                return;
            }

            if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.I && vm.CheckInCommand.CanExecute(null))
            {
                UiActionGuard.RunAsync(this, "Rentals", async () => await vm.CheckInCommand.ExecuteAsync(null));
                e.Handled = true;
                return;
            }

            if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.E && vm.ExtendCommand.CanExecute(null))
            {
                UiActionGuard.RunAsync(this, "Rentals", async () => await vm.ExtendCommand.ExecuteAsync(null));
                e.Handled = true;
                return;
            }

            if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.R && vm.PlaceRequestCommand.CanExecute(null))
            {
                UiActionGuard.RunAsync(this, "Rentals", async () => await vm.PlaceRequestCommand.ExecuteAsync(null));
                e.Handled = true;
                return;
            }

            if (Keyboard.Modifiers == ModifierKeys.None && e.Key == Key.Enter)
            {
                OpenFocusedDetails(vm);
                e.Handled = true;
                return;
            }

            if (Keyboard.Modifiers == ModifierKeys.None && e.Key == Key.Delete && vm.DeleteRentalCommand.CanExecute(null))
            {
                UiActionGuard.RunAsync(this, "Rentals", async () => await vm.DeleteRentalCommand.ExecuteAsync(null));
                e.Handled = true;
            }
        }

        private static bool IsRentalActionShortcut(KeyEventArgs e)
        {
            if (Keyboard.Modifiers == ModifierKeys.Control)
            {
                return e.Key is Key.P or Key.D or Key.H or Key.I or Key.E or Key.R;
            }

            if (Keyboard.Modifiers == (ModifierKeys.Control | ModifierKeys.Shift))
            {
                return e.Key is Key.P or Key.R;
            }

            return Keyboard.Modifiers == ModifierKeys.None && e.Key is Key.Enter or Key.Delete;
        }

        private static bool IsTextEditingElement(object? source)
        {
            if (source is TextBox or ComboBox or DatePicker or PasswordBox)
                return true;

            if (source is not DependencyObject element)
                return false;

            return GridContextMenuSelection.FindAncestor<TextBox>(element) != null
                || GridContextMenuSelection.FindAncestor<ComboBox>(element) != null
                || GridContextMenuSelection.FindAncestor<DatePicker>(element) != null
                || GridContextMenuSelection.FindAncestor<PasswordBox>(element) != null;
        }

        private void OpenFocusedDetails(ManageRentalsViewModel vm)
        {
            if (vm.IsLoading)
                return;

            UiActionGuard.Run(this, "Rentals", () =>
            {
                if (Keyboard.FocusedElement is DependencyObject focusedElement && GridContextMenuSelection.FindAncestor<System.Windows.Controls.DataGrid>(focusedElement) is System.Windows.Controls.DataGrid grid)
                {
                    if (grid.SelectedItem is Reservation && vm.OpenRequestDetailsCommand.CanExecute(null))
                    {
                        vm.OpenRequestDetailsCommand.Execute(null);
                        return;
                    }
                }

                if (vm.OpenRentalDetailsCommand.CanExecute(null))
                    vm.OpenRentalDetailsCommand.Execute(null);
                else if (vm.OpenRequestDetailsCommand.CanExecute(null))
                    vm.OpenRequestDetailsCommand.Execute(null);
            });
        }

        private DataGridRow? SelectRowForContextMenu(object sender, MouseButtonEventArgs e)
        {
            if (DataContext is ManageRentalsViewModel { IsLoading: true })
            {
                e.Handled = true;
                return null;
            }

            var row = GridContextMenuSelection.SelectRow(sender, e);
            if (row == null)
                return null;

            if (DataContext is ManageRentalsViewModel vm)
            {
                if (row.Item is RentalModel rental)
                    vm.SelectedRental = rental;
                else if (row.Item is Reservation request)
                    vm.SelectedRequest = request;
            }

            return row;
        }
    }
}
