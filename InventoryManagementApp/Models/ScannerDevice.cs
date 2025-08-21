using System;
using CommunityToolkit.Mvvm.ComponentModel;

namespace InventoryManagementApp.Models
{
    public class ScannerDevice : ObservableObject
    {
        string _name = string.Empty;
        string _ip = string.Empty;
        string _status = string.Empty;
        DateTime _lastSeen;

        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }

        public string Ip
        {
            get => _ip;
            set => SetProperty(ref _ip, value);
        }

        public string Status
        {
            get => _status;
            set => SetProperty(ref _status, value);
        }

        public DateTime LastSeen
        {
            get => _lastSeen;
            set => SetProperty(ref _lastSeen, value);
        }
    }
}
