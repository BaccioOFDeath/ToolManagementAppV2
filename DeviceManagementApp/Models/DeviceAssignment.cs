using System;
using CommunityToolkit.Mvvm.ComponentModel;

namespace DeviceManagementApp.Models
{
    public class DeviceAssignment : ObservableObject
    {
        string _deviceIp = string.Empty;
        int _userId;
        DateTime _assignedDate;
        DateTime? _returnedDate;
        int? _departmentId;

        public string DeviceIp
        {
            get => _deviceIp;
            set => SetProperty(ref _deviceIp, value);
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
