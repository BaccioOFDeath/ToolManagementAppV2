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
        private readonly ReportService _reportService;

        public ObservableCollection<string> ReportTypes { get; }
        public ObservableCollection<ReportLine> ReportLines { get; } = new();

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
                    OnPropertyChanged(nameof(SelectedLineDetail));
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
                    RunReportCommand.NotifyCanExecuteChanged();
            }
        }

        private DateTime? _lastRunAt;
        public DateTime? LastRunAt
        {
            get => _lastRunAt;
            set
            {
                if (SetProperty(ref _lastRunAt, value))
                    OnPropertyChanged(nameof(LastRunText));
            }
        }

        public string LastRunText => LastRunAt.HasValue ? LastRunAt.Value.ToString("g") : "Not run";
        public int ReportLineCount => ReportLines.Count;
        public string SelectedLineDetail => SelectedReportLine?.Text ?? "Select or double-click a report row to inspect the detail.";

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
                LastRunAt = DateTime.Now;
                OnPropertyChanged(nameof(ReportLineCount));
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
            for (var i = 0; i < detailLines.Count; i++)
            {
                ReportLines.Add(new ReportLine(i + 1, ClassifyLine(detailLines[i]), detailLines[i], BuildActionHint(detailLines[i])));
            }

            LastRunAt = DateTime.Now;
            ReportSummary = BuildSummary(detailLines);
            ReportStatus = $"{SelectedReport} completed with {ReportLines.Count} line(s).";
            OnPropertyChanged(nameof(ReportLineCount));
            ClearReportCommand.NotifyCanExecuteChanged();
        }

        private void ClearReport()
        {
            ReportLines.Clear();
            SelectedReportLine = null;
            ReportTitle = "Reports";
            ReportSubtitle = "Run operational reports for inventory, rentals, maintenance, reservations, and usage.";
            ReportSummary = "No report has been run yet.";
            ReportStatus = "Report cleared.";
            LastRunAt = null;
            OnPropertyChanged(nameof(ReportLineCount));
            ClearReportCommand.NotifyCanExecuteChanged();
        }

        private static string BuildSubtitle(string reportName)
        {
            return reportName switch
            {
                "Most Rented Items" => "Usage intelligence for buying more high-demand items.",
                "Active Rentals" => "Open rental work that may need advisor follow-up.",
                "Active Reservations" => "Pending holds and requests waiting for availability.",
                "Overdue Maintenance" => "Items requiring maintenance attention before further checkout.",
                "Overdue Calibrations" => "Calibrations that should be handled before field use.",
                "Activity Log" => "Recent inventory activity for operational review.",
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
            if (ContainsAny(line, "item", "equipment", "inventory", "stock"))
                return "Inventory";
            if (ContainsAny(line, "customer", "technician", "advisor", "user"))
                return "Person";
            return "Detail";
        }

        private static string BuildActionHint(string line)
        {
            if (ContainsAny(line, "overdue", "late"))
                return "Open the related rental or item record and follow up.";
            if (ContainsAny(line, "reservation", "request", "hold"))
                return "Check availability and contact the waiting user or customer.";
            if (ContainsAny(line, "maintenance", "repair", "calibration"))
                return "Review the item before it is rented or checked out again.";
            if (ContainsAny(line, "checked out", "rented", "rental"))
                return "Confirm holder, due-back date, and return status.";
            return "Use the source page to drill into the related record.";
        }

        private static bool ContainsAny(string text, params string[] terms)
        {
            return terms.Any(term => text.Contains(term, StringComparison.OrdinalIgnoreCase));
        }
    }

    public sealed class ReportLine
    {
        public ReportLine(int number, string category, string text, string actionHint)
        {
            Number = number;
            Category = category;
            Text = text;
            ActionHint = actionHint;
        }

        public int Number { get; }
        public string Category { get; }
        public string Text { get; }
        public string ActionHint { get; }
    }
}
