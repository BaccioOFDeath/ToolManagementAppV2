using System.Collections.Generic;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using DeviceManagementApp.Models;

namespace DeviceManagementApp.ViewModels
{
    public class DeviceDetailsViewModel : ObservableObject
    {
        public Device Device { get; }
        public ObservableCollection<DeviceSoftware> InstalledSoftware { get; } = new();

        public DeviceDetailsViewModel(Device device, IEnumerable<DeviceSoftware> software)
        {
            Device = device;
            foreach (var s in software)
                InstalledSoftware.Add(s);
        }
    }
}
