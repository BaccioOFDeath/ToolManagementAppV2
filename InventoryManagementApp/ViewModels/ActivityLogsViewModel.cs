using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using InventoryManagementApp.Models;
using InventoryManagementApp.Models.Domain;
using InventoryManagementApp.Services.Users;

namespace InventoryManagementApp.ViewModels
{
    public class ActivityLogsViewModel : ObservableObject
    {
        private readonly ActivityLogService _service;
        private readonly ILogger<ActivityLogsViewModel> _logger;

        public ObservableCollection<ActivityLog> Logs { get; } = new();

        public IAsyncRelayCommand RefreshCommand { get; }

        public ActivityLogsViewModel(ActivityLogService service, ILogger<ActivityLogsViewModel>? logger = null)
        {
            _service = service;
            _logger = logger ?? NullLogger<ActivityLogsViewModel>.Instance;
            RefreshCommand = new AsyncRelayCommand(LoadLogsAsync);
            _ = RefreshCommand.ExecuteAsync(null);
        }

        public async Task<bool> LoadLogsAsync()
        {
            try
            {
                Logs.Clear();
                var result = await _service.GetRecentLogsAsync();
                if (!result.Success || result.Value == null)
                {
                    _logger.LogError("Failed to load activity logs: {Error}", result.ErrorMessage);
                    return false;
                }
                foreach (var log in result.Value)
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
