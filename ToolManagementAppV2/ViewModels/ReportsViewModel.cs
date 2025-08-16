using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Windows.Documents;
using System.Threading.Tasks;
using ToolManagementAppV2.Services.Tools;

namespace ToolManagementAppV2.ViewModels
{
    public class ReportsViewModel : ObservableObject
    {
        private readonly ReportService _reportService;

        public ObservableCollection<string> ReportTypes { get; }

        private string _selectedReport;
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

        private DataTable _reportResults;
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
                "Full Rental History"
            };

            RunReportCommand = new AsyncRelayCommand(RunReportAsync, CanRunReport);
        }

        private bool CanRunReport() => !string.IsNullOrEmpty(SelectedReport);

        private async Task RunReportAsync()
        {
            FlowDocument doc = SelectedReport switch
            {
                "Summary" => await _reportService.GenerateSummaryReportAsync(),
                "Inventory" => await _reportService.GenerateInventoryReportAsync(),
                "Activity Log" => await _reportService.GenerateActivityLogReportAsync(),
                "Customers" => await _reportService.GenerateCustomerReportAsync(),
                "Users" => await _reportService.GenerateUserReportAsync(),
                "Active Rentals" => await _reportService.GenerateRentalReportAsync(true),
                "Full Rental History" => await _reportService.GenerateRentalReportAsync(false),
                _ => null
            };

            ReportResults = ConvertToTable(doc);
        }

        private static DataTable ConvertToTable(FlowDocument doc)
        {
            var table = new DataTable();
            table.Columns.Add("Line");

            if (doc == null)
                return table;

            var paragraphs = doc.Blocks
                .OfType<Paragraph>()
                .Skip(1) // Skip header
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
