using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using InventoryManagementApp.ViewModels;

namespace InventoryManagementApp.Views.Pages
{
    public partial class CalibrationPage : Page
    {
        private Task? _loadCalibrationTask;
        private CalibrationManagementViewModel? _loadedViewModel;

        public CalibrationPage()
        {
            InitializeComponent();
            Loaded += CalibrationPage_Loaded;
            DataContextChanged += CalibrationPage_DataContextChanged;
        }

        private async void CalibrationPage_Loaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is CalibrationManagementViewModel vm)
            {
                await LoadCalibrationOnceAsync(vm);
            }
        }

        private void CalibrationPage_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (!ReferenceEquals(_loadedViewModel, e.NewValue))
            {
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

            _loadedViewModel = vm;
            await Dispatcher.Yield(DispatcherPriority.Background);
            _loadCalibrationTask = vm.LoadCalibrationCommand.ExecuteAsync(null);
            await _loadCalibrationTask;
        }

        private void CalibrationRow_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (DataContext is CalibrationManagementViewModel vm && vm.OpenCalibrationDetailsCommand.CanExecute(null))
            {
                UiActionGuard.Run(this, "Calibration", () => vm.OpenCalibrationDetailsCommand.Execute(null));
            }
        }

        private void CalibrationRow_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            GridContextMenuSelection.SelectRow(sender, e);
        }
    }
}
