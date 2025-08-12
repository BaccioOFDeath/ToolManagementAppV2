using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using ToolManagementAppV2.Models.Domain;
using ToolManagementAppV2.Services.Users;

namespace ToolManagementAppV2.ViewModels
{
    public class ActivityLogsViewModel : ObservableObject
    {
        private readonly ActivityLogService _service;

        public ObservableCollection<ActivityLog> Logs { get; } = new();

        public IRelayCommand RefreshCommand { get; }

        public ActivityLogsViewModel(ActivityLogService service)
        {
            _service = service;
            RefreshCommand = new RelayCommand(LoadLogs);
            LoadLogs();
        }

        public void LoadLogs()
        {
            Logs.Clear();
            foreach (var log in _service.GetRecentLogs())
                Logs.Add(log);
        }
    }
}
