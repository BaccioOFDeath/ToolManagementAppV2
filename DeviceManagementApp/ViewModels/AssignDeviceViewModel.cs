using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DeviceManagementApp.Models;

namespace DeviceManagementApp.ViewModels
{
    public class AssignDeviceViewModel : ObservableObject
    {
        Staff? _selectedStaff;
        int? _departmentId;

        public ObservableCollection<Staff> Staff { get; } = new();

        public Staff? SelectedStaff
        {
            get => _selectedStaff;
            set => SetProperty(ref _selectedStaff, value);
        }

        public int? DepartmentId
        {
            get => _departmentId;
            set => SetProperty(ref _departmentId, value);
        }

        public IRelayCommand OkCommand { get; }
        public IRelayCommand CancelCommand { get; }

        public AssignDeviceViewModel(Action<bool?> close, IEnumerable<Staff> staff, int? selectedStaffId = null)
        {
            foreach (var s in staff)
                Staff.Add(s);
            if (selectedStaffId.HasValue)
                SelectedStaff = Staff.FirstOrDefault(s => s.StaffId == selectedStaffId.Value);
            OkCommand = new RelayCommand(() => close(true));
            CancelCommand = new RelayCommand(() => close(false));
        }
    }
}
