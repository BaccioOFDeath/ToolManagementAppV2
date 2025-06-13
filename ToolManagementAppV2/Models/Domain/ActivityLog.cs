using CommunityToolkit.Mvvm.ComponentModel;

namespace ToolManagementAppV2.Models.Domain
{
    public class ActivityLog : ObservableObject
    {
        private int _logID;
        public int LogID
        {
            get => _logID;
            set => SetProperty(ref _logID, value);
        }

        private int _userID;
        public int UserID
        {
            get => _userID;
            set => SetProperty(ref _userID, value);
        }

        private string _userName = string.Empty;
        public string UserName
        {
            get => _userName;
            set => SetProperty(ref _userName, value);
        }

        private string _action = string.Empty;
        public string Action
        {
            get => _action;
            set => SetProperty(ref _action, value);
        }

        private DateTime _timestamp;
        public DateTime Timestamp
        {
            get => _timestamp;
            set => SetProperty(ref _timestamp, value);
        }
    }
}
