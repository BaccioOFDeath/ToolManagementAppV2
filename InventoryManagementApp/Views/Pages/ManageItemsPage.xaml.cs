using System;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using InventoryManagementApp.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Application = System.Windows.Application;

namespace InventoryManagementApp.Views.Pages
{
    public partial class ManageItemsPage : Page
    {
        private CancellationTokenSource _loadCts = new();
        private bool _isLoadedForCurrentLifetime;

        public ManageItemsPage()
        {
            InitializeComponent();
            DataContext = ((App)Application.Current).Host.Services.GetRequiredService<ItemsViewModel>();
            Loaded += ManageItemsPage_Loaded;
            Unloaded += ManageItemsPage_Unloaded;
            DataContextChanged += ManageItemsPage_DataContextChanged;
        }

        private void ManageItemsPage_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (!ReferenceEquals(e.OldValue, e.NewValue))
                _isLoadedForCurrentLifetime = false;
        }

        private void DataGridRow_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (IsItemDirectoryBusy())
            {
                return;
            }

            GridContextMenuSelection.SelectRow(sender, e);
        }

        private void DataGridRow_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (IsItemDirectoryBusy())
            {
                e.Handled = true;
                return;
            }

            if (GridContextMenuSelection.SelectRow(sender, e) == null)
                return;

            if (DataContext is ItemsViewModel vm && vm.ViewDetailsCommand.CanExecute(null))
            {
                UiActionGuard.Run(this, "Item Details", () => vm.ViewDetailsCommand.Execute(null));
                e.Handled = true;
            }
        }

        private async void ManageItemsPage_Loaded(object sender, RoutedEventArgs e)
        {
            if (_isLoadedForCurrentLifetime)
                return;

            _isLoadedForCurrentLifetime = true;

            if (DataContext is not ItemsViewModel vm)
            {
                vm = ((App)Application.Current).Host.Services.GetRequiredService<ItemsViewModel>();
                DataContext = vm;
            }

            try
            {
                await vm.InitializeAsync(_loadCts.Token);
                await System.Windows.Threading.Dispatcher.Yield(System.Windows.Threading.DispatcherPriority.Background);
                await vm.LoadMoreAsync(_loadCts.Token);
            }
            catch (OperationCanceledException)
            {
            }
        }

        private void ManageItemsPage_Unloaded(object sender, RoutedEventArgs e)
        {
            _loadCts.Cancel();
            _isLoadedForCurrentLifetime = false;
            if (DataContext is ItemsViewModel vm)
            {
                vm.Dispose();
                DataContext = null;
            }
            _loadCts.Dispose();
            _loadCts = new CancellationTokenSource();
        }

        private bool IsItemDirectoryBusy()
        {
            return DataContext is ItemsViewModel { Items.IsLoading: true };
        }
    }
}
