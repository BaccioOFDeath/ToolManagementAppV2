using System;
using CommunityToolkit.Mvvm.ComponentModel;

namespace DeviceManagementApp.Models
{
    public class Asset : ObservableObject
    {
        int _assetId;
        string _name = string.Empty;
        string _serialNumber = string.Empty;
        int? _assignedUserId;
        int? _departmentId;

        public int AssetId
        {
            get => _assetId;
            set => SetProperty(ref _assetId, value);
        }

        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }

        public string SerialNumber
        {
            get => _serialNumber;
            set => SetProperty(ref _serialNumber, value);
        }

        public int? AssignedUserId
        {
            get => _assignedUserId;
            set => SetProperty(ref _assignedUserId, value);
        }

        public int? DepartmentId
        {
            get => _departmentId;
            set => SetProperty(ref _departmentId, value);
        }
    }
}
