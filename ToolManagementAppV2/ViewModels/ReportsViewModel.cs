using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Windows.Documents;
using ToolManagementAppV2.Services.Tools;

namespace ToolManagementAppV2.ViewModels
{
    public class ReportsViewModel : ObservableObject
    {
        private readonly ReportService _reportService;

        private FlowDocument _currentReport;
        public FlowDocument CurrentReport
        {
            get => _currentReport;
            set => SetProperty(ref _currentReport, value);
        }

        public IRelayCommand GenerateSummaryReportCommand { get; }
        public IRelayCommand GenerateActivityLogReportCommand { get; }

        public ReportsViewModel(ReportService reportService)
        {
            _reportService = reportService;
            GenerateSummaryReportCommand = new RelayCommand(() => CurrentReport = _reportService.GenerateSummaryReport());
            GenerateActivityLogReportCommand = new RelayCommand(() => CurrentReport = _reportService.GenerateActivityLogReport());
        }
    }
}
