using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace DeviceManagementApp.ViewModels
{
    public class AssignDeviceViewModel : ObservableObject
    {
        int _userId;
        int? _departmentId;

        public int UserId
        {
            get => _userId;
            set => SetProperty(ref _userId, value);
        }

        public int? DepartmentId
        {
            get => _departmentId;
            set => SetProperty(ref _departmentId, value);
        }

        public IRelayCommand OkCommand { get; }
        public IRelayCommand CancelCommand { get; }

        public AssignDeviceViewModel(Action<bool?> close)
        {
            OkCommand = new RelayCommand(() => close(true));
            CancelCommand = new RelayCommand(() => close(false));
        }
    }
}
