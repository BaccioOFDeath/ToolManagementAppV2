using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Documents;
using InventoryManagementApp.Services.Items;

#nullable enable

namespace InventoryManagementApp.ViewModels
{
    public class ReportsViewModel : ObservableObject
    {
        internal const int MaxVisibleReportRows = 500;

        private readonly ReportService _reportService;
        private int _totalReportLineCount;
        private int _omittedReportLineCount;

        public ObservableCollection<string> ReportTypes { get; }
        public ObservableCollection<ReportLine> ReportLines { get; } = new();
        public ObservableCollection<ReportLine> ReportResults => ReportLines;

        private string _selectedReport = string.Empty;
        public string SelectedReport
        {
            get => _selectedReport;
            set
            {
                if (SetProperty(ref _selectedReport, value))
                {
                    ReportStatus = string.IsNullOrWhiteSpace(value)
                        ? "Select a report to begin."
                        : $"Ready to run {value}.";
                    ClearReportOutputForSelection(value);
                    RunReportCommand.NotifyCanExecuteChanged();
                }
            }
        }

        private ReportLine? _selectedReportLine;
        public ReportLine? SelectedReportLine
        {
            get => _selectedReportLine;
            set
            {
                if (SetProperty(ref _selectedReportLine, value))
                {
                    OnPropertyChanged(nameof(SelectedLineDetail));
                    OnPropertyChanged(nameof(SelectedLineDestination));
                    OnPropertyChanged(nameof(SelectedLineDestinationKey));
                    OnPropertyChanged(nameof(SelectedLineHandoff));
                }
            }
        }

        private string _reportTitle = "Reports";
        public string ReportTitle
        {
            get => _reportTitle;
            set => SetProperty(ref _reportTitle, value);
        }

        private string _reportSubtitle = "Run operational reports for inventory, rentals, maintenance, reservations, and usage.";
        public string ReportSubtitle
        {
            get => _reportSubtitle;
            set => SetProperty(ref _reportSubtitle, value);
        }

        private string _reportSummary = "No report has been run yet.";
        public string ReportSummary
        {
            get => _reportSummary;
            set => SetProperty(ref _reportSummary, value);
        }

        private string _reportStatus = "Select a report to begin.";
        public string ReportStatus
        {
            get => _reportStatus;
            set => SetProperty(ref _reportStatus, value);
        }

        private bool _isBusy;
        public bool IsBusy
        {
            get => _isBusy;
            set
            {
                if (SetProperty(ref _isBusy, value))
                {
                    RunReportCommand.NotifyCanExecuteChanged();
                    OnPropertyChanged(nameof(CanPrintCurrentReport));
                    OnPropertyChanged(nameof(CanUseReportRows));
                }
            }
        }

        private DateTime? _lastRunAt;
        public DateTime? LastRunAt
        {
            get => _lastRunAt;
            set
            {
                if (SetProperty(ref _lastRunAt, value))
                {
                    OnPropertyChanged(nameof(LastRunText));
                    OnPropertyChanged(nameof(CanPrintCurrentReport));
                }
            }
        }

        public string LastRunText => LastRunAt.HasValue ? LastRunAt.Value.ToString("g") : "Not run";
        public int ReportLineCount => _totalReportLineCount;
        public int VisibleReportLineCount => ReportLines.Count;
        public int OmittedReportLineCount => _omittedReportLineCount;
        public bool HasOmittedReportRows => _omittedReportLineCount > 0;
        public string ReportLineWindowSummary => HasOmittedReportRows
            ? $"Showing first {VisibleReportLineCount} of {ReportLineCount} action row(s)"
            : $"{ReportLineCount} action row(s)";
        public bool CanPrintCurrentReport => !IsBusy && LastRunAt.HasValue && ReportLineCount > 0 && ReportLines.Count > 0 && !string.Equals(ReportStatus, "Report failed.", StringComparison.Ordinal);
        public bool CanUseReportRows => !IsBusy && ReportLines.Count > 0;
        public string ReportOperatorPath => string.IsNullOrWhiteSpace(SelectedReport)
            ? "Choose a report, run it, then open the source page from any row that needs follow-up."
            : $"Run {SelectedReport}, select a row, then open {BuildDestinationName(SelectedReport, SelectedReportLine?.Category)} to continue the workflow.";
        public string SelectedLineDetail => SelectedReportLine?.Text ?? "Select or double-click a report row to inspect the detail.";
        public string SelectedLineDestination => SelectedReportLine == null
            ? BuildDestinationName(SelectedReport, null)
            : SelectedReportLine.DestinationName;
        public string SelectedLineDestinationKey => SelectedReportLine == null
            ? BuildDestinationKey(SelectedReport, null)
            : SelectedReportLine.DestinationKey;
        public string SelectedLineHandoff => SelectedReportLine == null
            ? "No report row selected."
            : $"{SelectedReportLine.Category}: {SelectedReportLine.Text}{Environment.NewLine}Next action: {SelectedReportLine.ActionHint}{Environment.NewLine}Destination: {SelectedReportLine.DestinationName}";

        public IAsyncRelayCommand RunReportCommand { get; }
        public IRelayCommand ClearReportCommand { get; }

        public ReportsViewModel(ReportService reportService)
        {
            _reportService = reportService;

            ReportTypes = new ObservableCollection<string>
            {
                "Summary",
                "Inventory",
                "Activity Log",
                "Customers",
                "Users",
                "Active Rentals",
                "Full Rental History",
                "Most Rented Items",
                "Commonly Used Items",
                "Maintenance Schedule",
                "Overdue Maintenance",
                "Calibration Records",
                "Overdue Calibrations",
                "Active Reservations",
                "All Reservations",
                "Active Kits"
            };

            RunReportCommand = new AsyncRelayCommand(RunReportAsync, CanRunReport);
            ClearReportCommand = new RelayCommand(ClearReport, () => ReportLines.Count > 0);
        }

        public async Task RunSummaryReportAsync()
        {
            SelectedReport = "Summary";
            await RunReportAsync().ConfigureAwait(false);
        }

        private bool CanRunReport() => !IsBusy && !string.IsNullOrWhiteSpace(SelectedReport);

        private async Task RunReportAsync()
        {
            IsBusy = true;
            ReportStatus = $"Running {SelectedReport}...";

            try
            {
                FlowDocument? doc = SelectedReport switch
                {
                    "Summary" => await _reportService.GenerateSummaryReport(),
                    "Inventory" => await _reportService.GenerateInventoryReport(),
                    "Activity Log" => await _reportService.GenerateActivityLogReport(),
                    "Customers" => await _reportService.GenerateCustomerReport(),
                    "Users" => await _reportService.GenerateUserReport(),
                    "Active Rentals" => await _reportService.GenerateRentalReport(true),
                    "Full Rental History" => await _reportService.GenerateRentalReport(false),
                    "Most Rented Items" => await _reportService.GenerateRentalFrequencyReport(20),
                    "Commonly Used Items" => await _reportService.GenerateCommonlyUsedItemsReport(25),
                    "Maintenance Schedule" => await _reportService.GenerateMaintenanceReport(false),
                    "Overdue Maintenance" => await _reportService.GenerateMaintenanceReport(true),
                    "Calibration Records" => await _reportService.GenerateCalibrationReport(false),
                    "Overdue Calibrations" => await _reportService.GenerateCalibrationReport(true),
                    "Active Reservations" => await _reportService.GenerateReservationReport(true),
                    "All Reservations" => await _reportService.GenerateReservationReport(false),
                    "Active Kits" => await _reportService.GenerateKitReport(),
                    _ => null,
                };

                LoadReport(doc);
            }
            catch (Exception ex)
            {
                ReportLines.Clear();
                SelectedReportLine = null;
                ReportTitle = SelectedReport;
                ReportSubtitle = "The report could not be generated.";
                ReportSummary = ex.Message;
                ReportStatus = "Report failed.";
                LastRunAt = null;
                SetReportLineCounts(0, 0);
                NotifyReportOutputChanged();
                ClearReportCommand.NotifyCanExecuteChanged();
            }
            finally
            {
                IsBusy = false;
            }
        }

        private void LoadReport(FlowDocument? doc)
        {
            ReportLines.Clear();
            SelectedReportLine = null;

            var lines = doc?.Blocks
                .OfType<Paragraph>()
                .Select(p => new TextRange(p.ContentStart, p.ContentEnd).Text.Trim())
                .Where(t => !string.IsNullOrWhiteSpace(t))
                .ToList() ?? new();

            ReportTitle = lines.FirstOrDefault() ?? SelectedReport;
            ReportSubtitle = BuildSubtitle(SelectedReport);

            var detailLines = lines.Skip(1).ToList();
            SetReportLineCounts(detailLines.Count, Math.Max(0, detailLines.Count - MaxVisibleReportRows));

            var visibleLines = detailLines.Take(MaxVisibleReportRows).ToList();
            for (var i = 0; i < visibleLines.Count; i++)
            {
                var category = ClassifyLine(visibleLines[i]);
                ReportLines.Add(new ReportLine(
                    i + 1,
                    category,
                    visibleLines[i],
                    BuildActionHint(visibleLines[i]),
                    BuildDestinationKey(SelectedReport, category),
                    BuildDestinationName(SelectedReport, category)));
            }

            LastRunAt = DateTime.Now;
            ReportSummary = BuildSummary(detailLines);
            ReportStatus = BuildCompletedStatus();
            if (ReportLines.Count > 0)
                SelectedReportLine = ReportLines[0];
            NotifyReportOutputChanged();
            ClearReportCommand.NotifyCanExecuteChanged();
        }

        private void ClearReportOutputForSelection(string reportName)
        {
            ReportLines.Clear();
            SelectedReportLine = null;
            SetReportLineCounts(0, 0);
            ReportTitle = string.IsNullOrWhiteSpace(reportName) ? "Reports" : reportName;
            ReportSubtitle = string.IsNullOrWhiteSpace(reportName)
                ? "Run operational reports for inventory, rentals, maintenance, reservations, and usage."
                : BuildSubtitle(reportName);
            ReportSummary = string.IsNullOrWhiteSpace(reportName)
                ? "No report has been run yet."
                : $"Run {reportName} to refresh report rows.";
            LastRunAt = null;
            NotifyReportOutputChanged();
            ClearReportCommand.NotifyCanExecuteChanged();
        }

        private void ClearReport()
        {
            ReportLines.Clear();
            SelectedReportLine = null;
            SetReportLineCounts(0, 0);
            ReportTitle = "Reports";
            ReportSubtitle = "Run operational reports for inventory, rentals, maintenance, reservations, and usage.";
            ReportSummary = "No report has been run yet.";
            ReportStatus = "Report cleared.";
            LastRunAt = null;
            NotifyReportOutputChanged();
            ClearReportCommand.NotifyCanExecuteChanged();
        }

        private void SetReportLineCounts(int totalLineCount, int omittedLineCount)
        {
            _totalReportLineCount = Math.Max(0, totalLineCount);
            _omittedReportLineCount = Math.Max(0, omittedLineCount);
        }

        private string BuildCompletedStatus()
        {
            if (ReportLineCount == 0)
                return $"{SelectedReport} completed with no rows to action.";

            if (HasOmittedReportRows)
                return $"{SelectedReport} completed with {ReportLineCount} line(s); showing first {VisibleReportLineCount} so the results grid stays responsive.";

            return $"{SelectedReport} completed with {ReportLineCount} line(s). Select a row or open the source page.";
        }

        private void NotifyReportOutputChanged()
        {
            OnPropertyChanged(nameof(ReportLineCount));
            OnPropertyChanged(nameof(VisibleReportLineCount));
            OnPropertyChanged(nameof(OmittedReportLineCount));
            OnPropertyChanged(nameof(HasOmittedReportRows));
            OnPropertyChanged(nameof(ReportLineWindowSummary));
            OnPropertyChanged(nameof(CanPrintCurrentReport));
            OnPropertyChanged(nameof(CanUseReportRows));
            OnPropertyChanged(nameof(ReportOperatorPath));
        }

        private static string BuildSubtitle(string reportName)
        {
            return reportName switch
            {
                "Most Rented Items" => "Usage intelligence for buying more high-demand items.",
                "Commonly Used Items" => "High-frequency checked-out items from the item directory.",
                "Active Rentals" => "Open rental work that may need advisor follow-up.",
                "Active Reservations" => "Pending holds and requests waiting for availability.",
                "Overdue Maintenance" => "Items requiring maintenance attention before further checkout.",
                "Overdue Calibrations" => "Calibrations that should be handled before field use.",
                "Activity Log" => "Recent inventory activity for operational review.",
                "Users" => "Admin access, lockout, and account review output.",
                _ => $"Operational output for {reportName}."
            };
        }

        private static string BuildSummary(System.Collections.Generic.IReadOnlyCollection<string> lines)
        {
            if (lines.Count == 0)
                return "The report returned no detail rows.";

            var urgentCount = lines.Count(line => ContainsAny(line, "overdue", "late", "out of stock", "checked out", "rented", "reservation", "maintenance", "calibration"));
            return urgentCount == 0
                ? $"{lines.Count} line(s) returned. No obvious overdue, unavailable, or request wording was detected."
                : $"{lines.Count} line(s) returned. {urgentCount} line(s) mention availability, overdue, request, maintenance, or calibration follow-up.";
        }

        private static string ClassifyLine(string line)
        {
            if (ContainsAny(line, "overdue", "late"))
                return "Overdue";
            if (ContainsAny(line, "reservation", "request", "hold"))
                return "Request";
            if (ContainsAny(line, "checked out", "rented", "rental"))
                return "Rental";
            if (ContainsAny(line, "maintenance", "repair"))
                return "Maintenance";
            if (ContainsAny(line, "calibration", "calibrated"))
                return "Calibration";
            if (ContainsAny(line, "kit", "bundle"))
                return "Kit";
            if (ContainsAny(line, "item", "equipment", "inventory", "stock"))
                return "Inventory";
            if (ContainsAny(line, "customer"))
                return "Customer";
            if (ContainsAny(line, "technician", "advisor", "user", "lockout", "password", "permission"))
                return "User";
            return "Detail";
        }

        private static string BuildActionHint(string line)
        {
            if (ContainsAny(line, "overdue", "late"))
                return "Open the related rental, maintenance, or calibration workflow and follow up.";
            if (ContainsAny(line, "reservation", "request", "hold"))
                return "Check availability and contact the waiting user or customer.";
            if (ContainsAny(line, "maintenance", "repair", "calibration"))
                return "Review the item before it is rented or checked out again.";
            if (ContainsAny(line, "checked out", "rented", "rental"))
                return "Confirm holder, due-back date, and return status.";
            if (ContainsAny(line, "user", "permission", "password", "lockout"))
                return "Open Users and verify access, account state, or reset needs.";
            return "Use the source page to drill into the related record.";
        }

        private static string BuildDestinationKey(string reportName, string? category)
        {
            if (ContainsAny(reportName, "Activity"))
                return "ActivityLogs";
            if (ContainsAny(reportName, "Customer") || string.Equals(category, "Customer", StringComparison.OrdinalIgnoreCase))
                return "Customers";
            if (ContainsAny(reportName, "User") || string.Equals(category, "User", StringComparison.OrdinalIgnoreCase))
                return "Users";
            if (ContainsAny(reportName, "Rental") || string.Equals(category, "Rental", StringComparison.OrdinalIgnoreCase))
                return "Rentals";
            if (ContainsAny(reportName, "Reservation") || string.Equals(category, "Request", StringComparison.OrdinalIgnoreCase))
                return "Reservations";
            if (ContainsAny(reportName, "Maintenance") || string.Equals(category, "Maintenance", StringComparison.OrdinalIgnoreCase))
                return "Maintenance";
            if (ContainsAny(reportName, "Calibration") || string.Equals(category, "Calibration", StringComparison.OrdinalIgnoreCase))
                return "Calibration";
            if (ContainsAny(reportName, "Kit") || string.Equals(category, "Kit", StringComparison.OrdinalIgnoreCase))
                return "Kits";
            if (ContainsAny(reportName, "Inventory", "Rented Items", "Commonly Used Items") || string.Equals(category, "Inventory", StringComparison.OrdinalIgnoreCase) || string.Equals(category, "Overdue", StringComparison.OrdinalIgnoreCase))
                return "Items";
            return "Dashboard";
        }

        private static string BuildDestinationName(string reportName, string? category)
        {
            return BuildDestinationKey(reportName, category) switch
            {
                "ActivityLogs" => "Activity Logs",
                "Customers" => "Customers",
                "Users" => "Users",
                "Rentals" => "Rentals",
                "Reservations" => "Reservations",
                "Maintenance" => "Maintenance",
                "Calibration" => "Calibration",
                "Kits" => "Kits",
                "Items" => "Items",
                _ => "Dashboard"
            };
        }

        private static bool ContainsAny(string text, params string[] terms)
        {
            return terms.Any(term => text.Contains(term, StringComparison.OrdinalIgnoreCase));
        }
    }

    public sealed class ReportLine
    {
        public ReportLine(int number, string category, string text, string actionHint, string destinationKey, string destinationName)
        {
            Number = number;
            Category = category;
            Text = text;
            ActionHint = actionHint;
            DestinationKey = destinationKey;
            DestinationName = destinationName;
        }

        public int Number { get; }
        public string Category { get; }
        public string Text { get; }
        public string ActionHint { get; }
        public string DestinationKey { get; }
        public string DestinationName { get; }
    }
}