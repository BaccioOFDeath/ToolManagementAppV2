using System;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DeviceManagementApp.Interfaces;
using DeviceManagementApp.Models;
using DeviceManagementApp.Views.Pages;
using Application = System.Windows.Application;

namespace DeviceManagementApp.ViewModels
{
    public class MainViewModel : ObservableObject, IMainViewModel
    {
        private readonly INavigationService _navigationService;
        private readonly DevicesViewModel _devicesViewModel;
        private readonly DashboardViewModel _dashboardViewModel;
        private readonly SettingsViewModel _settingsViewModel;

        public MainViewModel(INavigationService navigationService, DevicesViewModel devicesViewModel, DashboardViewModel dashboardViewModel, SettingsViewModel settingsViewModel)
        {
            _navigationService = navigationService;
            _devicesViewModel = devicesViewModel;
            _dashboardViewModel = dashboardViewModel;
            _settingsViewModel = settingsViewModel;
            _devicesViewModel.ViewDetailsRequested += DevicesViewModel_ViewDetailsRequested;

            WindowTitle = "Device Management";
            OpenDevicesCommand = new RelayCommand(OpenDevices);
            OpenDashboardCommand = new RelayCommand(OpenDashboard);
            OpenSettingsCommand = new RelayCommand(OpenSettings);
            ExitCommand = new RelayCommand(() => Application.Current.Shutdown());

            OpenDashboard();
        }

        private Page? _currentPage;
        public Page? CurrentPage
        {
            get => _currentPage;
            private set => SetProperty(ref _currentPage, value);
        }

        private string _currentPageTitle = string.Empty;
        public string CurrentPageTitle
        {
            get => _currentPageTitle;
            private set => SetProperty(ref _currentPageTitle, value);
        }

        public string WindowTitle { get; }

        public IRelayCommand OpenDevicesCommand { get; }
        public IRelayCommand OpenDashboardCommand { get; }
        public IRelayCommand OpenSettingsCommand { get; }
        public IRelayCommand ExitCommand { get; }

        private void OpenDevices()
        {
            var page = new DevicesPage { DataContext = _devicesViewModel };
            CurrentPage = page;
            CurrentPageTitle = "Devices";
        }

        private async void OpenDashboard()
        {
            var page = new DashboardPage { DataContext = _dashboardViewModel };
            CurrentPage = page;
            CurrentPageTitle = "Dashboard";
            await _dashboardViewModel.LoadAsync(CancellationToken.None);
        }

        private async void OpenSettings()
        {
            await _settingsViewModel.InitializeAsync();
            var page = new SettingsPage { DataContext = _settingsViewModel };
            CurrentPage = page;
            CurrentPageTitle = "Settings";
        }

        private void DevicesViewModel_ViewDetailsRequested(object? sender, Device device)
        {
            var vm = new DeviceDetailsViewModel(device, _devicesViewModel.InstalledSoftware);
            _navigationService.Navigate(new DeviceDetailsPage { DataContext = vm });
        }
    }
}
