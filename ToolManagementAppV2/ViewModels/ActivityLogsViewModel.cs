using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.ObjectModel;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using ToolManagementAppV2.Models.Domain;
using ToolManagementAppV2.Services.Users;

namespace ToolManagementAppV2.ViewModels
{
    public class ActivityLogsViewModel : ObservableObject
    {
        private readonly ActivityLogService _service;
        private readonly ILogger<ActivityLogsViewModel> _logger;

        public ObservableCollection<ActivityLog> Logs { get; } = new();

        public IRelayCommand RefreshCommand { get; }

        public ActivityLogsViewModel(ActivityLogService service, ILogger<ActivityLogsViewModel>? logger = null)
        {
            _service = service;
            _logger = logger ?? NullLogger<ActivityLogsViewModel>.Instance;
            RefreshCommand = new RelayCommand(() => LoadLogs());
            LoadLogs();
        }

        public bool LoadLogs()
        {
            try
            {
                Logs.Clear();
                var logs = _service.GetRecentLogs();
                if (logs == null)
                    return false;
                foreach (var log in logs)
                    Logs.Add(log);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load activity logs");
                return false;
            }
        }
    }
}
