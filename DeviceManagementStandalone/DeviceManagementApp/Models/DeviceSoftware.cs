using CommunityToolkit.Mvvm.ComponentModel;

namespace DeviceManagementApp.Models
{
    public class DeviceSoftware : ObservableObject
    {
        string _deviceIp = string.Empty;
        int? _devicePort;
        string _name = string.Empty;
        string _version = string.Empty;

        public string DeviceIp
        {
            get => _deviceIp;
            set => SetProperty(ref _deviceIp, value);
        }

        public int? DevicePort
        {
            get => _devicePort;
            set => SetProperty(ref _devicePort, value);
        }

        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }

        public string Version
        {
            get => _version;
            set => SetProperty(ref _version, value);
        }
    }
}
