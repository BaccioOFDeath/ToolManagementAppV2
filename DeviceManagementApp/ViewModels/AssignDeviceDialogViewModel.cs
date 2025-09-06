using CommunityToolkit.Mvvm.ComponentModel;

namespace DeviceManagementApp.ViewModels
{
    public class AssignDeviceDialogViewModel : ObservableObject
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
    }
}
