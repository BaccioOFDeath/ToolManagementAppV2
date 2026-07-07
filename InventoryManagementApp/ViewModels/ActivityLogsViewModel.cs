using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
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
        const int FilterDebounceMilliseconds = 160;
        public const int MaxVisibleActivityRows = 500;
        const int MaxActivityPrintRows = 250;

        private readonly ActivityLogService _service;
        private readonly ILogger<ActivityLogsViewModel> _logger;
        private CancellationTokenSource? _filterRefreshCts;
        private bool _suppressFilterRefresh;
        private IReadOnlyList<ActivityLogSearchRow> _searchRows = Array.Empty<ActivityLogSearchRow>();
        private int _matchedLogCount;

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
                    QueueFilterRefresh();
            }
        }

        private string _selectedUserFilter = AllUsersFilter;
        public string SelectedUserFilter
        {
            get => _selectedUserFilter;
            set
            {
                if (SetProperty(ref _selectedUserFilter, string.IsNullOrWhiteSpace(value) ? AllUsersFilter : value))
                    QueueFilterRefresh();
            }
        }

        private string _selectedActionFilter = AllActionsFilter;
        public string SelectedActionFilter
        {
            get => _selectedActionFilter;
            set
            {
                if (SetProperty(ref _selectedActionFilter, string.IsNullOrWhiteSpace(value) ? AllActionsFilter : value))
                    QueueFilterRefresh();
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
                    NotifyActivityStateChanged();
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
                {
                    RefreshCommand.NotifyCanExecuteChanged();
                    ClearFiltersCommand.NotifyCanExecuteChanged();
                    NotifyActivityStateChanged();
                }
            }
        }

        private bool _isFiltering;
        public bool IsFiltering
        {
            get => _isFiltering;
            private set
            {
                if (SetProperty(ref _isFiltering, value))
                {
                    RefreshCommand.NotifyCanExecuteChanged();
                    NotifyActivityStateChanged();
                }
            }
        }

        public string LastLoadedText => LastLoadedAt.HasValue ? LastLoadedAt.Value.ToString("g") : "Not loaded";
        public int TotalLogCount => Logs.Count;
        public int MatchedLogCount => _matchedLogCount;
        public int FilteredLogCount => FilteredLogs.Count;
        public int OmittedLogCount => Math.Max(0, MatchedLogCount - FilteredLogCount);
        public bool HasOmittedActivityRows => OmittedLogCount > 0;
        public bool IsBusy => IsLoading || IsFiltering;
        public bool HasActiveFilters => HasActiveFilter();
        public bool CanChangeActivityFilters => !IsLoading;
        public bool CanRefreshActivityRows => !IsBusy;
        public bool CanUseSelectedLogActions => !IsBusy && SelectedLog != null;
        public bool CanPrintActivityRows => !IsBusy && FilteredLogs.Count > 0;
        public bool CanShowActivityEmptyState => !IsBusy && MatchedLogCount == 0;
        public string ActivityBusyMessage => IsLoading
            ? "Refreshing recent audit rows without blocking the rest of the screen."
            : "Applying the current search and filter choices to the loaded audit rows.";

        public string ActivityEmptyStateTitle => Logs.Count == 0
            ? "No activity rows loaded"
            : HasActiveFilter()
                ? "No activity rows match"
                : "No activity rows available";

        public string ActivityEmptyStateMessage => Logs.Count == 0
            ? "Refresh the audit trail to review recent operations, account changes, imports, rentals, and item events."
            : "Clear or adjust the current search, user, and action filters to review more of the audit trail.";

        public string ActivityWindowStatusText => HasOmittedActivityRows
            ? $"Showing first {FilteredLogCount} of {MatchedLogCount} matching activity rows to keep the live grid responsive; refine filters to reach {OmittedLogCount} more."
            : MatchedLogCount == 0
                ? "No matching activity rows are visible."
                : "All matching activity rows are visible in the live grid.";

        public string PrintStatusText
        {
            get
            {
                if (IsLoading)
                    return "Print preview will be available after the latest activity rows finish loading.";
                if (IsFiltering)
                    return "Print preview will be available after the current filters finish applying.";
                if (FilteredLogs.Count == 0)
                    return "No activity rows are ready to print.";

                var printRows = Math.Min(FilteredLogs.Count, MaxActivityPrintRows);
                if (HasOmittedActivityRows && FilteredLogs.Count > MaxActivityPrintRows)
                    return $"Print preview will include the first {printRows} of {FilteredLogs.Count} visible row(s); {OmittedLogCount} additional matching row(s) are hidden from the live grid.";
                if (HasOmittedActivityRows)
                    return $"Print preview ready for {printRows} visible row(s); {OmittedLogCount} additional matching row(s) are hidden from the live grid.";
                if (FilteredLogs.Count > MaxActivityPrintRows)
                    return $"Print preview will include the first {MaxActivityPrintRows} of {FilteredLogs.Count} visible row(s).";

                return $"Print preview ready for {FilteredLogs.Count} visible activity row(s).";
            }
        }

        public string ActivitySummary
        {
            get
            {
                if (IsLoading)
                    return Logs.Count == 0
                        ? "Loading recent activity rows."
                        : $"Refreshing activity rows; {FilteredLogs.Count} previously visible row(s) remain on screen.";
                if (IsFiltering)
                    return $"Filtering {Logs.Count} loaded activity row(s).";
                if (MatchedLogCount == 0)
                    return Logs.Count == 0
                        ? "No activity rows loaded yet. Refresh to review recent operations."
                        : "No matching activity rows. Clear filters or refine the audit search.";

                var groups = FilteredLogs
                    .GroupBy(log => ClassifyAction(log.Action))
                    .OrderByDescending(group => group.Count())
                    .ThenBy(group => group.Key)
                    .Take(3)
                    .Select(group => $"{group.Key}: {group.Count()}");

                var matchScope = HasActiveFilter() ? "matching" : "loaded";
                var omittedText = HasOmittedActivityRows
                    ? $" {OmittedLogCount} {matchScope} row(s) are hidden from the live grid."
                    : string.Empty;

                return $"{FilteredLogs.Count} visible of {MatchedLogCount} {matchScope} activity row(s).{omittedText} Top visible activity: {string.Join(", ", groups)}.";
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
            RefreshCommand = new AsyncRelayCommand(LoadLogsAsync, () => CanRefreshActivityRows);
            ClearFiltersCommand = new RelayCommand(ClearFilters, () => !IsLoading && HasActiveFilter());
            UserFilters.Add(AllUsersFilter);
            ActionFilters.Add(AllActionsFilter);
        }

        public async Task<bool> LoadLogsAsync()
        {
            if (IsBusy)
                return false;

            try
            {
                IsLoading = true;
                StatusMessage = Logs.Count == 0
                    ? "Loading recent activity..."
                    : $"Refreshing recent activity while keeping {FilteredLogs.Count} visible row(s) on screen...";

                var result = await _service.GetRecentLogsAsync();
                if (!result.Success || result.Value == null)
                {
                    _logger.LogError("Failed to load activity logs: {Error}", result.ErrorMessage);
                    PreserveActivityLogRowsAfterLoadFailure("Activity logs could not be loaded. Existing activity rows were kept on screen until refresh succeeds.");
                    return false;
                }

                var refreshedRows = result.Value
                    .OrderByDescending(log => log.Timestamp)
                    .ThenBy(log => SafeText(log.UserName, "Unknown user"), StringComparer.OrdinalIgnoreCase)
                    .ThenBy(log => SafeText(log.Action), StringComparer.OrdinalIgnoreCase)
                    .ToList();

                var refreshedSearchRows = refreshedRows
                    .Select(ActivityLogSearchRow.Create)
                    .ToList();

                Logs.Clear();
                foreach (var log in refreshedRows)
                    Logs.Add(log);
                _searchRows = refreshedSearchRows;

                LastLoadedAt = DateTime.Now;
                RebuildFilterLists();
                await ApplyFiltersAsync(false);
                StatusMessage = $"Loaded {Logs.Count} recent activity row(s); {FilteredLogs.Count} are visible in the live grid.";
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load activity logs");
                PreserveActivityLogRowsAfterLoadFailure("Activity logs could not be loaded. Existing activity rows were kept on screen until refresh succeeds.");
                return false;
            }
            finally
            {
                IsLoading = false;
            }
        }

        public void CancelPendingFilterRefresh()
        {
            var cts = Interlocked.Exchange(ref _filterRefreshCts, null);
            cts?.Cancel();
            cts?.Dispose();

            if (IsFiltering)
            {
                IsFiltering = false;
                StatusMessage = Logs.Count == 0
                    ? "Activity filtering was canceled before rows were loaded."
                    : $"{FilteredLogs.Count} visible activity row(s).";
                NotifyActivityStateChanged();
            }
        }

        private void PreserveActivityLogRowsAfterLoadFailure(string message)
        {
            if (Logs.Count == 0)
            {
                FilteredLogs.Clear();
                _matchedLogCount = 0;
                _searchRows = Array.Empty<ActivityLogSearchRow>();
                SelectedLog = null;
                LastLoadedAt = null;
                RebuildFilterLists();
            }

            NotifyActivityStateChanged();
            StatusMessage = message;
        }

        private void ClearFilters()
        {
            if (IsLoading)
                return;

            _suppressFilterRefresh = true;
            SearchText = string.Empty;
            SelectedUserFilter = AllUsersFilter;
            SelectedActionFilter = AllActionsFilter;
            _suppressFilterRefresh = false;
            _ = ApplyFiltersAsync(false);
        }

        private bool HasActiveFilter()
        {
            return !string.IsNullOrWhiteSpace(SearchText)
                || SelectedUserFilter != AllUsersFilter
                || SelectedActionFilter != AllActionsFilter;
        }

        private void RebuildFilterLists()
        {
            var searchRows = GetSearchRowsSnapshot();

            UserFilters.Clear();
            UserFilters.Add(AllUsersFilter);
            foreach (var userName in searchRows.Select(l => l.UserName).Where(u => !string.IsNullOrWhiteSpace(u)).Distinct(StringComparer.OrdinalIgnoreCase).OrderBy(u => u))
                UserFilters.Add(userName);

            ActionFilters.Clear();
            ActionFilters.Add(AllActionsFilter);
            foreach (var actionGroup in searchRows.Select(l => l.ActionGroup).Distinct().OrderBy(g => g))
                ActionFilters.Add(actionGroup);

            if (!UserFilters.Contains(SelectedUserFilter))
            {
                _selectedUserFilter = AllUsersFilter;
                OnPropertyChanged(nameof(SelectedUserFilter));
            }

            if (!ActionFilters.Contains(SelectedActionFilter))
            {
                _selectedActionFilter = AllActionsFilter;
                OnPropertyChanged(nameof(SelectedActionFilter));
            }
        }

        private void QueueFilterRefresh()
        {
            if (_suppressFilterRefresh)
                return;

            _ = ApplyFiltersAsync(true);
        }

        private async Task ApplyFiltersAsync(bool debounce)
        {
            var cts = new CancellationTokenSource();
            var previousCts = Interlocked.Exchange(ref _filterRefreshCts, cts);
            previousCts?.Cancel();
            previousCts?.Dispose();

            try
            {
                IsFiltering = true;
                StatusMessage = "Filtering loaded activity rows...";

                if (debounce)
                    await Task.Delay(FilterDebounceMilliseconds, cts.Token);

                var search = SearchText?.Trim() ?? string.Empty;
                var selectedUserFilter = SelectedUserFilter;
                var selectedActionFilter = SelectedActionFilter;
                var previousSelection = SelectedLog;
                var rows = GetSearchRowsSnapshot();

                var filterResult = await Task.Run(() => BuildFilteredLogWindow(rows, selectedUserFilter, selectedActionFilter, search, cts.Token), cts.Token);

                cts.Token.ThrowIfCancellationRequested();
                _matchedLogCount = filterResult.MatchedCount;
                ReplaceFilteredLogs(filterResult.VisibleRows, previousSelection);

                NotifyActivityStateChanged();
                StatusMessage = HasActiveFilter()
                    ? $"{FilteredLogs.Count} visible of {MatchedLogCount} matching activity row(s)."
                    : $"{FilteredLogs.Count} visible of {Logs.Count} recent activity row(s).";
            }
            catch (OperationCanceledException)
            {
            }
            finally
            {
                if (ReferenceEquals(_filterRefreshCts, cts))
                {
                    _filterRefreshCts = null;
                    IsFiltering = false;
                    NotifyActivityStateChanged();
                }

                cts.Dispose();
            }
        }

        private static FilteredActivityLogResult BuildFilteredLogWindow(IReadOnlyList<ActivityLogSearchRow> rows, string selectedUserFilter, string selectedActionFilter, string search, CancellationToken cancellationToken)
        {
            var visibleRows = new List<ActivityLog>(MaxVisibleActivityRows);
            var matchedCount = 0;

            foreach (var row in rows)
            {
                cancellationToken.ThrowIfCancellationRequested();
                if (!row.Matches(selectedUserFilter, selectedActionFilter, search))
                    continue;

                matchedCount++;
                if (visibleRows.Count < MaxVisibleActivityRows)
                    visibleRows.Add(row.Log);
            }

            return new FilteredActivityLogResult(visibleRows, matchedCount);
        }

        private IReadOnlyList<ActivityLogSearchRow> GetSearchRowsSnapshot()
        {
            if (SearchRowsMatchLogs())
                return _searchRows;

            _searchRows = Logs.Select(ActivityLogSearchRow.Create).ToList();
            return _searchRows;
        }

        private bool SearchRowsMatchLogs()
        {
            if (_searchRows.Count != Logs.Count)
                return false;

            for (var i = 0; i < Logs.Count; i++)
            {
                if (!ReferenceEquals(_searchRows[i].Log, Logs[i]))
                    return false;
            }

            return true;
        }

        private void ReplaceFilteredLogs(IReadOnlyList<ActivityLog> visibleRows, ActivityLog? previousSelection)
        {
            if (!HasSameRows(FilteredLogs, visibleRows))
            {
                FilteredLogs.Clear();
                foreach (var log in visibleRows)
                    FilteredLogs.Add(log);
            }

            SelectedLog = previousSelection != null && FilteredLogs.Contains(previousSelection)
                ? previousSelection
                : FilteredLogs.FirstOrDefault();
        }

        private static bool HasSameRows(IReadOnlyList<ActivityLog> currentRows, IReadOnlyList<ActivityLog> nextRows)
        {
            if (currentRows.Count != nextRows.Count)
                return false;

            for (var i = 0; i < currentRows.Count; i++)
            {
                if (!ReferenceEquals(currentRows[i], nextRows[i]))
                    return false;
            }

            return true;
        }

        private void NotifyActivityStateChanged()
        {
            OnPropertyChanged(nameof(TotalLogCount));
            OnPropertyChanged(nameof(MatchedLogCount));
            OnPropertyChanged(nameof(FilteredLogCount));
            OnPropertyChanged(nameof(OmittedLogCount));
            OnPropertyChanged(nameof(HasOmittedActivityRows));
            OnPropertyChanged(nameof(IsBusy));
            OnPropertyChanged(nameof(HasActiveFilters));
            OnPropertyChanged(nameof(CanChangeActivityFilters));
            OnPropertyChanged(nameof(CanRefreshActivityRows));
            OnPropertyChanged(nameof(CanUseSelectedLogActions));
            OnPropertyChanged(nameof(CanPrintActivityRows));
            OnPropertyChanged(nameof(CanShowActivityEmptyState));
            OnPropertyChanged(nameof(ActivityBusyMessage));
            OnPropertyChanged(nameof(ActivityEmptyStateTitle));
            OnPropertyChanged(nameof(ActivityEmptyStateMessage));
            OnPropertyChanged(nameof(ActivityWindowStatusText));
            OnPropertyChanged(nameof(PrintStatusText));
            OnPropertyChanged(nameof(ActivitySummary));
            ClearFiltersCommand.NotifyCanExecuteChanged();
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
            return string.IsNullOrWhiteSpace(text) ? fallback : text.Trim();
        }

        private sealed record FilteredActivityLogResult(IReadOnlyList<ActivityLog> VisibleRows, int MatchedCount);

        private sealed class ActivityLogSearchRow
        {
            private ActivityLogSearchRow(ActivityLog log, string userName, string actionGroup, string searchText)
            {
                Log = log;
                UserName = userName;
                ActionGroup = actionGroup;
                SearchText = searchText;
            }

            public ActivityLog Log { get; }
            public string UserName { get; }
            public string ActionGroup { get; }
            private string SearchText { get; }

            public static ActivityLogSearchRow Create(ActivityLog log)
            {
                var userName = SafeText(log.UserName);
                var action = SafeText(log.Action);
                var actionGroup = ClassifyAction(action);
                var destinationName = BuildDestinationName(BuildDestinationKey(action));
                var searchText = string.Join("\n", new[]
                {
                    userName,
                    action,
                    actionGroup,
                    destinationName,
                    log.UserID.ToString(),
                    log.Timestamp.ToString("g")
                });

                return new ActivityLogSearchRow(log, userName, actionGroup, searchText);
            }

            public bool Matches(string selectedUserFilter, string selectedActionFilter, string search)
            {
                return (selectedUserFilter == AllUsersFilter || string.Equals(UserName, selectedUserFilter, StringComparison.OrdinalIgnoreCase))
                    && (selectedActionFilter == AllActionsFilter || string.Equals(ActionGroup, selectedActionFilter, StringComparison.OrdinalIgnoreCase))
                    && (string.IsNullOrWhiteSpace(search) || SearchText.Contains(search, StringComparison.OrdinalIgnoreCase));
            }
        }
    }
}
