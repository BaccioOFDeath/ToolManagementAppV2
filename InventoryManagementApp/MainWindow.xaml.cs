// MainWindow.xaml.cs
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using InventoryManagementApp.Utilities.Extensions;
using InventoryManagementApp.Interfaces;
using InventoryManagementApp.ViewModels;

namespace InventoryManagementApp
{
    public partial class MainWindow : Window, IMainWindow
    {
        readonly IDatabaseService? _ownedDb;
        MainViewModel? _mainViewModel;

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

            Closed += (_, __) =>
            {
                if (_mainViewModel != null)
                    _mainViewModel.PropertyChanged -= MainViewModel_PropertyChanged;
                _ownedDb?.Dispose();
            };

            if (viewModel is MainViewModel vm)
            {
                _mainViewModel = vm;
                _mainViewModel.PropertyChanged += MainViewModel_PropertyChanged;
                MouseMove += (_, __) => vm.ResetAutoLogoutTimer();
                KeyDown += (_, __) => vm.ResetAutoLogoutTimer();
                MouseDown += (_, __) => vm.ResetAutoLogoutTimer();
            }
        }

        void IMainWindow.Activate() => base.Activate();

        void IMainWindow.Focus() => base.Focus();

        void MainViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(MainViewModel.IsSidebarOpen) && sender is MainViewModel vm)
            {
                vm.IsSidebarOpen = false;
            }
        }

        void SectionMenuItem_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is not MenuItem menuItem || DataContext is not MainViewModel vm)
                return;

            if (FindSourceMenuItem(e.OriginalSource as DependencyObject) != menuItem)
                return;

            var command = menuItem.Header?.ToString() switch
            {
                "Overview" => vm.SelectOverviewSectionCommand,
                "Operations" => vm.SelectOperationsSectionCommand,
                "Insights" => vm.SelectInsightsSectionCommand,
                "Data" => vm.SelectDataSectionCommand,
                "Admin" => vm.SelectAdminSectionCommand,
                _ => null
            };

            if (command?.CanExecute(null) == true)
                command.Execute(null);

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
