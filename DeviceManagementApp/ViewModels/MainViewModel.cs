using System;
using System.Windows;
using System.Windows.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DeviceManagementApp.Interfaces;
using DeviceManagementApp.Models;
using DeviceManagementApp.Views.Pages;

namespace DeviceManagementApp.ViewModels
{
    public class MainViewModel : ObservableObject, IMainViewModel
    {
        private readonly INavigationService _navigationService;
        private readonly DevicesViewModel _devicesViewModel;

        public MainViewModel(INavigationService navigationService, DevicesViewModel devicesViewModel)
        {
            _navigationService = navigationService;
            _devicesViewModel = devicesViewModel;
            _devicesViewModel.ViewDetailsRequested += DevicesViewModel_ViewDetailsRequested;

            WindowTitle = "Device Management";
            OpenDevicesCommand = new RelayCommand(OpenDevices);
            OpenDashboardCommand = new RelayCommand(OpenDashboard);
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
        public IRelayCommand ExitCommand { get; }

        private void OpenDevices()
        {
            var page = new DevicesPage { DataContext = _devicesViewModel };
            CurrentPage = page;
            CurrentPageTitle = "Devices";
        }

        private void OpenDashboard()
        {
            var page = new Page { Title = "Dashboard", Content = new TextBlock { Text = "Dashboard" } };
            CurrentPage = page;
            CurrentPageTitle = "Dashboard";
        }

        private void DevicesViewModel_ViewDetailsRequested(object? sender, Device device)
        {
            var vm = new DeviceDetailsViewModel(device, _devicesViewModel.InstalledSoftware);
            _navigationService.Navigate(new DeviceDetailsPage { DataContext = vm });
        }
    }
}
