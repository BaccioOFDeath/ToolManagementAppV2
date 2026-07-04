using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using InventoryManagementApp.ViewModels;

namespace InventoryManagementApp.Views.Pages
{
    public partial class MaintenancePage : Page
    {
        private Task? _loadMaintenanceTask;
        private MaintenanceManagementViewModel? _loadedViewModel;

        public MaintenancePage()
        {
            InitializeComponent();
            Loaded += MaintenancePage_Loaded;
            DataContextChanged += MaintenancePage_DataContextChanged;
        }

        private async void MaintenancePage_Loaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is MaintenanceManagementViewModel vm)
            {
                await LoadMaintenanceOnceAsync(vm);
            }
        }

        private void MaintenancePage_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (!ReferenceEquals(_loadedViewModel, e.NewValue))
            {
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

            _loadedViewModel = vm;
            await Dispatcher.Yield(DispatcherPriority.Background);
            _loadMaintenanceTask = vm.LoadMaintenanceCommand.ExecuteAsync(null);
            await _loadMaintenanceTask;
        }

        private void MaintenanceRow_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (DataContext is MaintenanceManagementViewModel vm && vm.OpenMaintenanceDetailsCommand.CanExecute(null))
            {
                UiActionGuard.Run(this, "Maintenance", () => vm.OpenMaintenanceDetailsCommand.Execute(null));
            }
        }

        private void MaintenanceRow_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            GridContextMenuSelection.SelectRow(sender, e);
        }
    }
}