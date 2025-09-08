using System;
using CommunityToolkit.Mvvm.ComponentModel;

namespace DeviceManagementApp.Models
{
    public class AssetAssignment : ObservableObject
    {
        int _assetId;
        int _userId;
        DateTime _assignedDate;
        DateTime? _returnedDate;
        int? _departmentId;

        public int AssetId
        {
            get => _assetId;
            set => SetProperty(ref _assetId, value);
        }

        public int UserId
        {
            get => _userId;
            set => SetProperty(ref _userId, value);
        }

        public DateTime AssignedDate
        {
            get => _assignedDate;
            set => SetProperty(ref _assignedDate, value);
        }

        public DateTime? ReturnedDate
        {
            get => _returnedDate;
            set => SetProperty(ref _returnedDate, value);
        }

        public int? DepartmentId
        {
            get => _departmentId;
            set => SetProperty(ref _departmentId, value);
        }
    }
}
