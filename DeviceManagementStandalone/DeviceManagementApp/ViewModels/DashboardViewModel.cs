using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DeviceManagementApp.Interfaces;
using DeviceManagementApp.Models;
using DeviceManagementApp.Views.Pages;

namespace DeviceManagementApp.ViewModels
{
    public class DashboardViewModel : ObservableObject
    {
        readonly IDeviceService _deviceService;
        readonly INavigationService _navigationService;
        readonly DevicesViewModel _devicesViewModel;
        readonly IAssetService _assetService;

        public ObservableCollection<StatCard> StatCards { get; } = new();
        public ObservableCollection<Device> AssignedDevices { get; } = new();
        public ObservableCollection<Device> UnassignedDevices { get; } = new();

        public IRelayCommand NewDeviceCommand { get; }
        public IRelayCommand AssignDeviceCommand { get; }
        public IRelayCommand AssignDeviceInListCommand { get; }

        public DashboardViewModel(IDeviceService deviceService, INavigationService navigationService, DevicesViewModel devicesViewModel, IAssetService assetService)
        {
            _deviceService = deviceService;
            _navigationService = navigationService;
            _devicesViewModel = devicesViewModel;
            _assetService = assetService;
            NewDeviceCommand = new RelayCommand(OpenDevices);
            AssignDeviceCommand = new RelayCommand(OpenDevices);
            AssignDeviceInListCommand = new RelayCommand<Device>(d =>
            {
                if (d != null)
                {
                    UnassignedDevices.Remove(d);
                    AssignedDevices.Add(d);
                }
            });
        }

        void OpenDevices()
        {
            _navigationService.Navigate(new DevicesPage { DataContext = _devicesViewModel });
        }

        public async Task LoadAsync(CancellationToken cancellationToken)
        {
            StatCards.Clear();
            AssignedDevices.Clear();
            UnassignedDevices.Clear();
            var devices = await _deviceService.GetDevicesAsync(cancellationToken).ConfigureAwait(false);
            var list = devices.ToList();
            var assigned = list.Where(d => d.AssignedUserId != null).ToList();
            foreach (var d in assigned)
                AssignedDevices.Add(d);
            var unassigned = list.Where(d => d.AssignedUserId == null).ToList();
            foreach (var d in unassigned)
                UnassignedDevices.Add(d);
            StatCards.Add(new StatCard { Title = "Total Devices", Value = list.Count.ToString() });
            StatCards.Add(new StatCard { Title = "Assigned Devices", Value = assigned.Count.ToString() });
            StatCards.Add(new StatCard { Title = "Unassigned Devices", Value = unassigned.Count.ToString() });

            var assets = await _assetService.GetAssetsAsync(cancellationToken).ConfigureAwait(false);
            var assetList = assets.ToList();
            var assignedAssets = assetList.Where(a => a.AssignedUserId != null).ToList();
            var unassignedAssets = assetList.Where(a => a.AssignedUserId == null).ToList();
            StatCards.Add(new StatCard { Title = "Total Assets", Value = assetList.Count.ToString() });
            StatCards.Add(new StatCard { Title = "Assigned Assets", Value = assignedAssets.Count.ToString() });
            StatCards.Add(new StatCard { Title = "Unassigned Assets", Value = unassignedAssets.Count.ToString() });
        }

        public IRelayCommand ReturnDeviceCommand => new RelayCommand<Device>(d =>
        {
            if (d != null)
                AssignedDevices.Remove(d);
        });
    }
}
