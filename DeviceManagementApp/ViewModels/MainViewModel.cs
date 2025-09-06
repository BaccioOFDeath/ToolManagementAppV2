using System.Windows;
using System.Windows.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DeviceManagementApp.Interfaces;

namespace DeviceManagementApp.ViewModels
{
    public class MainViewModel : ObservableObject, IMainViewModel
    {
        private readonly Page _devicesPage;

        public MainViewModel(Page devicesPage)
        {
            _devicesPage = devicesPage;
            OpenDevicesCommand = new RelayCommand(OpenDevices);
            ExitCommand = new RelayCommand(() => Application.Current.Shutdown());
            WindowTitle = "Device Management";
            OpenDevices();
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
        public IRelayCommand ExitCommand { get; }

        private void OpenDevices()
        {
            CurrentPage = _devicesPage;
            CurrentPageTitle = "Devices";
        }
    }
}
