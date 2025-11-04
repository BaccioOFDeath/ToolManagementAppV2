using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Windows.Documents;
using System.Threading.Tasks;
using InventoryManagementApp.Services.Items;

#nullable enable

namespace InventoryManagementApp.ViewModels
{
    public class ReportsViewModel : ObservableObject
    {
        private readonly ReportService _reportService;

        public ObservableCollection<string> ReportTypes { get; }

        private string _selectedReport = string.Empty;
        public string SelectedReport
        {
            get => _selectedReport;
            set
            {
                if (SetProperty(ref _selectedReport, value))
                {
                    RunReportCommand.NotifyCanExecuteChanged();
                }
            }
        }

        private DataTable _reportResults = new();
        public DataTable ReportResults
        {
            get => _reportResults;
            set => SetProperty(ref _reportResults, value);
        }

        public IAsyncRelayCommand RunReportCommand { get; }

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
                "Maintenance Schedule",
                "Overdue Maintenance",
                "Calibration Records",
                "Overdue Calibrations",
                "Active Reservations",
                "All Reservations",
                "Active Kits"
            };

            RunReportCommand = new AsyncRelayCommand(RunReportAsync, CanRunReport);
        }

        private bool CanRunReport() => !string.IsNullOrEmpty(SelectedReport);

        private async Task RunReportAsync()
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
                "Maintenance Schedule" => await _reportService.GenerateMaintenanceReport(false),
                "Overdue Maintenance" => await _reportService.GenerateMaintenanceReport(true),
                "Calibration Records" => await _reportService.GenerateCalibrationReport(false),
                "Overdue Calibrations" => await _reportService.GenerateCalibrationReport(true),
                "Active Reservations" => await _reportService.GenerateReservationReport(true),
                "All Reservations" => await _reportService.GenerateReservationReport(false),
                "Active Kits" => await _reportService.GenerateKitReport(),
                _ => null,
            };

            ReportResults = ConvertToTable(doc);
        }

        private static DataTable ConvertToTable(FlowDocument? doc)
        {
            var table = new DataTable();
            table.Columns.Add("Line");

            if (doc == null)
                return table;

            var paragraphs = doc.Blocks
                .OfType<Paragraph>()
                .Skip(1)
                .Select(p => new TextRange(p.ContentStart, p.ContentEnd).Text.Trim())
                .Where(t => !string.IsNullOrWhiteSpace(t));

            foreach (var line in paragraphs)
            {
                table.Rows.Add(line);
            }

            return table;
        }
    }
}
