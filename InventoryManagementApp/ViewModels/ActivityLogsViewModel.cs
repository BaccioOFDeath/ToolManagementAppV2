using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using InventoryManagementApp.Models.Domain;
using InventoryManagementApp.Services.Users;

namespace InventoryManagementApp.ViewModels
{
    public class ActivityLogsViewModel : ObservableObject
    {
        const string AllUsersFilter = "All users";
        const string AllActionsFilter = "All actions";

        private readonly ActivityLogService _service;
        private readonly ILogger<ActivityLogsViewModel> _logger;

        public ObservableCollection<ActivityLog> Logs { get; } = new();
        public ObservableCollection<ActivityLog> FilteredLogs { get; } = new();
        public ObservableCollection<string> UserFilters { get; } = new();
        public ObservableCollection<string> ActionFilters { get; } = new();

        private string _searchText = string.Empty;
        public string SearchText
        {
            get => _searchText;
            set
            {
                if (SetProperty(ref _searchText, value))
                    ApplyFilters();
            }
        }

        private string _selectedUserFilter = AllUsersFilter;
        public string SelectedUserFilter
        {
            get => _selectedUserFilter;
            set
            {
                if (SetProperty(ref _selectedUserFilter, string.IsNullOrWhiteSpace(value) ? AllUsersFilter : value))
                    ApplyFilters();
            }
        }

        private string _selectedActionFilter = AllActionsFilter;
        public string SelectedActionFilter
        {
            get => _selectedActionFilter;
            set
            {
                if (SetProperty(ref _selectedActionFilter, string.IsNullOrWhiteSpace(value) ? AllActionsFilter : value))
                    ApplyFilters();
            }
        }

        private ActivityLog? _selectedLog;
        public ActivityLog? SelectedLog
        {
            get => _selectedLog;
            set
            {
                if (SetProperty(ref _selectedLog, value))
                {
                    OnPropertyChanged(nameof(SelectedLogTitle));
                    OnPropertyChanged(nameof(SelectedLogDetail));
                    OnPropertyChanged(nameof(SelectedLogActionGroup));
                    OnPropertyChanged(nameof(SelectedLogTimestamp));
                    OnPropertyChanged(nameof(SelectedLogDestinationKey));
                    OnPropertyChanged(nameof(SelectedLogDestinationName));
                    OnPropertyChanged(nameof(SelectedLogNextAction));
                    OnPropertyChanged(nameof(SelectedLogHandoff));
                    OnPropertyChanged(nameof(SelectedLogOperatorPath));
                }
            }
        }

        private string _statusMessage = "Loading recent activity...";
        public string StatusMessage
        {
            get => _statusMessage;
            set => SetProperty(ref _statusMessage, value);
        }

        private DateTime? _lastLoadedAt;
        public DateTime? LastLoadedAt
        {
            get => _lastLoadedAt;
            set
            {
                if (SetProperty(ref _lastLoadedAt, value))
                    OnPropertyChanged(nameof(LastLoadedText));
            }
        }

        private bool _isLoading;
        public bool IsLoading
        {
            get => _isLoading;
            private set
            {
                if (SetProperty(ref _isLoading, value))
                    RefreshCommand.NotifyCanExecuteChanged();
            }
        }

        public string LastLoadedText => LastLoadedAt.HasValue ? LastLoadedAt.Value.ToString("g") : "Not loaded";
        public int TotalLogCount => Logs.Count;
        public int FilteredLogCount => FilteredLogs.Count;

        public string ActivitySummary
        {
            get
            {
                if (FilteredLogs.Count == 0)
                    return "No matching activity rows. Clear filters or refresh to review recent operations.";

                var groups = FilteredLogs
                    .GroupBy(log => ClassifyAction(log.Action))
                    .OrderByDescending(group => group.Count())
                    .ThenBy(group => group.Key)
                    .Take(3)
                    .Select(group => $"{group.Key}: {group.Count()}");

                return $"{FilteredLogs.Count} visible of {Logs.Count} loaded. Top activity: {string.Join(", ", groups)}.";
            }
        }

        public string SelectedLogTitle => SelectedLog == null
            ? "No activity row selected"
            : $"{SafeText(SelectedLog.UserName, "Unknown user")} - {SelectedLogActionGroup}";

        public string SelectedLogActionGroup => SelectedLog == null
            ? "No action"
            : ClassifyAction(SelectedLog.Action);

        public string SelectedLogTimestamp => SelectedLog == null
            ? string.Empty
            : SelectedLog.Timestamp.ToString("f");

        public string SelectedLogDetail => SelectedLog == null
            ? "Select or double-click a row to inspect the full activity text."
            : SafeText(SelectedLog.Action, "No activity detail was recorded.");

        public string SelectedLogDestinationKey => SelectedLog == null
            ? "Dashboard"
            : BuildDestinationKey(SelectedLog.Action);

        public string SelectedLogDestinationName => BuildDestinationName(SelectedLogDestinationKey);

        public string SelectedLogNextAction => SelectedLog == null
            ? "Select a row, then open the related page or copy a handoff for follow-up."
            : BuildNextAction(SelectedLog.Action);

        public string SelectedLogHandoff => SelectedLog == null
            ? "No activity row selected."
            : $"Activity: {SafeText(SelectedLog.Action, "No activity detail was recorded.")}{Environment.NewLine}" +
              $"User: {SafeText(SelectedLog.UserName, "Unknown user")} (ID {SelectedLog.UserID}){Environment.NewLine}" +
              $"When: {SelectedLog.Timestamp:g}{Environment.NewLine}" +
              $"Type: {SelectedLogActionGroup}{Environment.NewLine}" +
              $"Next action: {SelectedLogNextAction}{Environment.NewLine}" +
              $"Destination: {SelectedLogDestinationName}";

        public string SelectedLogOperatorPath => SelectedLog == null
            ? "Select a row to see where the audit trail should take you next."
            : $"Open {SelectedLogDestinationName} to continue the workflow from this audit event.";

        public IAsyncRelayCommand RefreshCommand { get; }
        public IRelayCommand ClearFiltersCommand { get; }

        public ActivityLogsViewModel(ActivityLogService service, ILogger<ActivityLogsViewModel>? logger = null)
        {
            _service = service;
            _logger = logger ?? NullLogger<ActivityLogsViewModel>.Instance;
            RefreshCommand = new AsyncRelayCommand(LoadLogsAsync, () => !IsLoading);
            ClearFiltersCommand = new RelayCommand(ClearFilters, HasActiveFilter);
            UserFilters.Add(AllUsersFilter);
            ActionFilters.Add(AllActionsFilter);
        }

        public async Task<bool> LoadLogsAsync()
        {
            if (IsLoading)
                return false;

            try
            {
                IsLoading = true;
                StatusMessage = "Loading recent activity...";
                Logs.Clear();
                var result = await _service.GetRecentLogsAsync();
                if (!result.Success || result.Value == null)
                {
                    _logger.LogError("Failed to load activity logs: {Error}", result.ErrorMessage);
                    ClearActivityLogRowsAfterLoadFailure("Activity logs could not be loaded. Activity rows were cleared until refresh succeeds.");
                    return false;
                }

                foreach (var log in result.Value)
                    Logs.Add(log);

                LastLoadedAt = DateTime.Now;
                RebuildFilterLists();
                ApplyFilters();
                StatusMessage = $"Loaded {Logs.Count} recent activity row(s).";
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load activity logs");
                ClearActivityLogRowsAfterLoadFailure("Activity logs could not be loaded. Activity rows were cleared until refresh succeeds.");
                return false;
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void ClearActivityLogRowsAfterLoadFailure(string message)
        {
            Logs.Clear();
            FilteredLogs.Clear();
            SelectedLog = null;
            LastLoadedAt = null;
            RebuildFilterLists();
            OnPropertyChanged(nameof(TotalLogCount));
            OnPropertyChanged(nameof(FilteredLogCount));
            OnPropertyChanged(nameof(ActivitySummary));
            ClearFiltersCommand.NotifyCanExecuteChanged();
            StatusMessage = message;
        }

        private void ClearFilters()
        {
            SearchText = string.Empty;
            SelectedUserFilter = AllUsersFilter;
            SelectedActionFilter = AllActionsFilter;
            ApplyFilters();
        }

        private bool HasActiveFilter()
        {
            return !string.IsNullOrWhiteSpace(SearchText)
                || SelectedUserFilter != AllUsersFilter
                || SelectedActionFilter != AllActionsFilter;
        }

        private void RebuildFilterLists()
        {
            UserFilters.Clear();
            UserFilters.Add(AllUsersFilter);
            foreach (var userName in Logs.Select(l => l.UserName).Where(u => !string.IsNullOrWhiteSpace(u)).Distinct().OrderBy(u => u))
                UserFilters.Add(userName);

            ActionFilters.Clear();
            ActionFilters.Add(AllActionsFilter);
            foreach (var actionGroup in Logs.Select(l => ClassifyAction(l.Action)).Distinct().OrderBy(g => g))
                ActionFilters.Add(actionGroup);

            if (!UserFilters.Contains(SelectedUserFilter))
                SelectedUserFilter = AllUsersFilter;
            if (!ActionFilters.Contains(SelectedActionFilter))
                SelectedActionFilter = AllActionsFilter;
        }

        private void ApplyFilters()
        {
            var previousSelection = SelectedLog;
            var search = SearchText?.Trim() ?? string.Empty;

            var filtered = Logs.Where(log =>
                (SelectedUserFilter == AllUsersFilter || string.Equals(log.UserName, SelectedUserFilter, StringComparison.OrdinalIgnoreCase)) &&
                (SelectedActionFilter == AllActionsFilter || string.Equals(ClassifyAction(log.Action), SelectedActionFilter, StringComparison.OrdinalIgnoreCase)) &&
                (string.IsNullOrWhiteSpace(search) ||
                    SafeText(log.UserName).Contains(search, StringComparison.OrdinalIgnoreCase) ||
                    SafeText(log.Action).Contains(search, StringComparison.OrdinalIgnoreCase) ||
                    ClassifyAction(log.Action).Contains(search, StringComparison.OrdinalIgnoreCase) ||
                    BuildDestinationName(BuildDestinationKey(log.Action)).Contains(search, StringComparison.OrdinalIgnoreCase) ||
                    log.Timestamp.ToString("g").Contains(search, StringComparison.OrdinalIgnoreCase)))
                .ToList();

            FilteredLogs.Clear();
            foreach (var log in filtered)
                FilteredLogs.Add(log);

            SelectedLog = previousSelection != null && FilteredLogs.Contains(previousSelection)
                ? previousSelection
                : FilteredLogs.FirstOrDefault();

            OnPropertyChanged(nameof(TotalLogCount));
            OnPropertyChanged(nameof(FilteredLogCount));
            OnPropertyChanged(nameof(ActivitySummary));
            ClearFiltersCommand.NotifyCanExecuteChanged();

            StatusMessage = HasActiveFilter()
                ? $"{FilteredLogs.Count} of {Logs.Count} activity row(s) match the current filters."
                : $"{Logs.Count} recent activity row(s) visible.";
        }

        public static string ClassifyAction(string? action)
        {
            if (string.IsNullOrWhiteSpace(action))
                return "System";
            if (ContainsAny(action, "checkout", "checked out", "rent", "rental", "returned", "check in", "checked in"))
                return "Checkout / Rental";
            if (ContainsAny(action, "request", "reservation", "hold"))
                return "Request / Hold";
            if (ContainsAny(action, "calibration", "calibrated"))
                return "Calibration";
            if (ContainsAny(action, "maintenance", "repair"))
                return "Maintenance";
            if (ContainsAny(action, "import", "export", "backup"))
                return "Import / Export";
            if (ContainsAny(action, "user", "password", "login", "role", "permission", "lockout"))
                return "User / Admin";
            if (ContainsAny(action, "item", "equipment", "inventory", "stock", "category", "kit"))
                return "Inventory";
            return "System";
        }

        public static string BuildDestinationKey(string? action)
        {
            return ClassifyAction(action) switch
            {
                "Checkout / Rental" => "Rentals",
                "Request / Hold" => "Reservations",
                "Calibration" => "Calibration",
                "Maintenance" => "Maintenance",
                "Import / Export" => "ImportExport",
                "User / Admin" => "Users",
                "Inventory" => ContainsAny(action, "category") ? "Categories" :
                    ContainsAny(action, "kit") ? "Kits" : "Items",
                _ => "Dashboard"
            };
        }

        public static string BuildDestinationName(string destinationKey)
        {
            return destinationKey switch
            {
                "Rentals" => "Rentals",
                "Reservations" => "Reservations",
                "Calibration" => "Calibration",
                "Maintenance" => "Maintenance",
                "ImportExport" => "Import / Export",
                "Users" => "Users",
                "Categories" => "Categories",
                "Kits" => "Kits",
                "Items" => "Items",
                _ => "Dashboard"
            };
        }

        public static string BuildNextAction(string? action)
        {
            return ClassifyAction(action) switch
            {
                "Checkout / Rental" => "Confirm holder, due-back date, return state, and any shelf pickup notes.",
                "Request / Hold" => "Check availability, contact the requester, then confirm, fulfill, or cancel the hold.",
                "Calibration" => "Review certificate timing before the item is released for field use.",
                "Maintenance" => "Review the maintenance record and complete or schedule the work before reuse.",
                "Import / Export" => "Open the data workstation and verify the import/export result or backup output.",
                "User / Admin" => "Open Users and verify account state, lockout, password, role, or permissions.",
                "Inventory" => "Open the related inventory workbench and verify item, kit, stock, or category setup.",
                _ => "Open the dashboard or source page and decide whether operational follow-up is needed."
            };
        }

        private static bool ContainsAny(string? text, params string[] terms)
        {
            if (string.IsNullOrWhiteSpace(text))
                return false;

            return terms.Any(term => text.Contains(term, StringComparison.OrdinalIgnoreCase));
        }

        private static string SafeText(string? text, string fallback = "")
        {
            return string.IsNullOrWhiteSpace(text) ? fallback : text;
        }
    }
}