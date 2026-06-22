using System;
using System.IO;
using Xunit;

namespace InventoryManagementApp.Tests.ViewModels
{
    public class InsightsPagesXamlTests
    {
        [Fact]
        public void ReportsPage_UsesInsightsWorkbenchSummariesAndActions()
        {
            var xaml = ReadRepositoryFile("InventoryManagementApp", "Views", "Pages", "ReportsPage.xaml");

            Assert.Contains("Reports Workbench", xaml, StringComparison.Ordinal);
            Assert.Contains("ReportsStatCard", xaml, StringComparison.Ordinal);
            Assert.Contains("ReportLineCount", xaml, StringComparison.Ordinal);
            Assert.Contains("SelectedLineDestination", xaml, StringComparison.Ordinal);
            Assert.Contains("LastRunText", xaml, StringComparison.Ordinal);
            Assert.Contains("RunReportCommand", xaml, StringComparison.Ordinal);
            Assert.Contains("ClearReportCommand", xaml, StringComparison.Ordinal);
            Assert.Contains("OpenSourcePage_Click", xaml, StringComparison.Ordinal);
            Assert.Contains("CopySelectedRow_Click", xaml, StringComparison.Ordinal);
            Assert.Contains("PrintReport_Click", xaml, StringComparison.Ordinal);
        }

        [Fact]
        public void ReportsPage_PreservesRowHooksAndStyledEmptyState()
        {
            var xaml = ReadRepositoryFile("InventoryManagementApp", "Views", "Pages", "ReportsPage.xaml");

            Assert.Contains("DesktopPaneHeader", xaml, StringComparison.Ordinal);
            Assert.Contains("DesktopPaneSubheader", xaml, StringComparison.Ordinal);
            Assert.Contains("ReportsDetailCard", xaml, StringComparison.Ordinal);
            Assert.Contains("No report rows are ready", xaml, StringComparison.Ordinal);
            Assert.Contains("ReportGrid_MouseDoubleClick", xaml, StringComparison.Ordinal);
            Assert.Contains("ReportGrid_PreviewMouseRightButtonDown", xaml, StringComparison.Ordinal);
            Assert.Contains("SelectedLineHandoff", xaml, StringComparison.Ordinal);
            Assert.Contains("ReportOperatorPath", xaml, StringComparison.Ordinal);
        }

        [Fact]
        public void ActivityLogsPage_UsesAuditWorkbenchSummariesAndFilters()
        {
            var xaml = ReadRepositoryFile("InventoryManagementApp", "Views", "Pages", "ActivityLogsPage.xaml");

            Assert.Contains("Activity Audit Workbench", xaml, StringComparison.Ordinal);
            Assert.Contains("ActivityStatCard", xaml, StringComparison.Ordinal);
            Assert.Contains("FilteredLogCount", xaml, StringComparison.Ordinal);
            Assert.Contains("TotalLogCount", xaml, StringComparison.Ordinal);
            Assert.Contains("SelectedLogActionGroup", xaml, StringComparison.Ordinal);
            Assert.Contains("SelectedLogDestinationName", xaml, StringComparison.Ordinal);
            Assert.Contains("SearchText", xaml, StringComparison.Ordinal);
            Assert.Contains("SelectedUserFilter", xaml, StringComparison.Ordinal);
            Assert.Contains("SelectedActionFilter", xaml, StringComparison.Ordinal);
            Assert.Contains("RefreshCommand", xaml, StringComparison.Ordinal);
            Assert.Contains("ClearFiltersCommand", xaml, StringComparison.Ordinal);
        }

        [Fact]
        public void ActivityLogsPage_PreservesAuditHooksAndStyledEmptyState()
        {
            var xaml = ReadRepositoryFile("InventoryManagementApp", "Views", "Pages", "ActivityLogsPage.xaml");

            Assert.Contains("DesktopPaneHeader", xaml, StringComparison.Ordinal);
            Assert.Contains("DesktopPaneSubheader", xaml, StringComparison.Ordinal);
            Assert.Contains("ActivityDetailCard", xaml, StringComparison.Ordinal);
            Assert.Contains("No activity rows match", xaml, StringComparison.Ordinal);
            Assert.Contains("ActivityGrid_MouseDoubleClick", xaml, StringComparison.Ordinal);
            Assert.Contains("ActivityGridRow_PreviewMouseRightButtonDown", xaml, StringComparison.Ordinal);
            Assert.Contains("OpenSelectedLog_Click", xaml, StringComparison.Ordinal);
            Assert.Contains("OpenRelatedPage_Click", xaml, StringComparison.Ordinal);
            Assert.Contains("CopySelectedLog_Click", xaml, StringComparison.Ordinal);
            Assert.Contains("PrintLogs_Click", xaml, StringComparison.Ordinal);
            Assert.Contains("SelectedLogHandoff", xaml, StringComparison.Ordinal);
            Assert.Contains("SelectedLogOperatorPath", xaml, StringComparison.Ordinal);
        }

        [Fact]
        public void ActivityLogsLoadFailure_ClearsRowsFiltersSelectionAndKeepsFailureStatus()
        {
            var source = ReadRepositoryFile("InventoryManagementApp", "ViewModels", "ActivityLogsViewModel.cs");

            Assert.Contains("ClearActivityLogRowsAfterLoadFailure(\"Activity logs could not be loaded. Activity rows were cleared until refresh succeeds.\");", source, StringComparison.Ordinal);
            Assert.Contains("private void ClearActivityLogRowsAfterLoadFailure(string message)", source, StringComparison.Ordinal);
            Assert.Contains("Logs.Clear();", source, StringComparison.Ordinal);
            Assert.Contains("FilteredLogs.Clear();", source, StringComparison.Ordinal);
            Assert.Contains("SelectedLog = null;", source, StringComparison.Ordinal);
            Assert.Contains("RebuildFilterLists();", source, StringComparison.Ordinal);
            Assert.Contains("OnPropertyChanged(nameof(TotalLogCount));", source, StringComparison.Ordinal);
            Assert.Contains("OnPropertyChanged(nameof(FilteredLogCount));", source, StringComparison.Ordinal);
            Assert.Contains("OnPropertyChanged(nameof(ActivitySummary));", source, StringComparison.Ordinal);
            Assert.Contains("StatusMessage = message;", source, StringComparison.Ordinal);
            Assert.DoesNotContain("StatusMessage = \"Activity logs could not be loaded.\";", source, StringComparison.Ordinal);
        }

        [Fact]
        public void ReportsGenerationFailure_ClearsRowsSelectionAndKeepsFailureStatus()
        {
            var source = ReadRepositoryFile("InventoryManagementApp", "ViewModels", "ReportsViewModel.cs");

            Assert.Contains("catch (Exception ex)", source, StringComparison.Ordinal);
            Assert.Contains("ReportLines.Clear();", source, StringComparison.Ordinal);
            Assert.Contains("SelectedReportLine = null;", source, StringComparison.Ordinal);
            Assert.Contains("ReportTitle = SelectedReport;", source, StringComparison.Ordinal);
            Assert.Contains("ReportSubtitle = \"The report could not be generated.\";", source, StringComparison.Ordinal);
            Assert.Contains("ReportSummary = ex.Message;", source, StringComparison.Ordinal);
            Assert.Contains("ReportStatus = \"Report failed.\";", source, StringComparison.Ordinal);
            Assert.Contains("LastRunAt = DateTime.Now;", source, StringComparison.Ordinal);
            Assert.Contains("OnPropertyChanged(nameof(ReportLineCount));", source, StringComparison.Ordinal);
            Assert.Contains("OnPropertyChanged(nameof(ReportOperatorPath));", source, StringComparison.Ordinal);
            Assert.Contains("ClearReportCommand.NotifyCanExecuteChanged();", source, StringComparison.Ordinal);
            Assert.DoesNotContain("catch (Exception ex)\n            {\n                LoadReport(null);", source, StringComparison.Ordinal);
        }

        [Fact]
        public void ReportsSelectionChange_ClearsStaleRowsSelectionAndRunStateBeforeNextRun()
        {
            var source = ReadRepositoryFile("InventoryManagementApp", "ViewModels", "ReportsViewModel.cs");

            Assert.Contains("ClearReportOutputForSelection(value);", source, StringComparison.Ordinal);
            Assert.Contains("private void ClearReportOutputForSelection(string reportName)", source, StringComparison.Ordinal);
            Assert.Contains("ReportLines.Clear();", source, StringComparison.Ordinal);
            Assert.Contains("SelectedReportLine = null;", source, StringComparison.Ordinal);
            Assert.Contains("ReportTitle = string.IsNullOrWhiteSpace(reportName) ? \"Reports\" : reportName;", source, StringComparison.Ordinal);
            Assert.Contains("ReportSummary = string.IsNullOrWhiteSpace(reportName)", source, StringComparison.Ordinal);
            Assert.Contains("? \"No report has been run yet.\"", source, StringComparison.Ordinal);
            Assert.Contains(": $\"Run {reportName} to refresh report rows.\";", source, StringComparison.Ordinal);
            Assert.Contains("LastRunAt = null;", source, StringComparison.Ordinal);
            Assert.Contains("OnPropertyChanged(nameof(ReportLineCount));", source, StringComparison.Ordinal);
            Assert.Contains("OnPropertyChanged(nameof(ReportOperatorPath));", source, StringComparison.Ordinal);
            Assert.Contains("ClearReportCommand.NotifyCanExecuteChanged();", source, StringComparison.Ordinal);
        }

        [Fact]
        public void ReportsPrintAction_RequiresCompletedFreshReportOutput()
        {
            var viewModel = ReadRepositoryFile("InventoryManagementApp", "ViewModels", "ReportsViewModel.cs");
            var pageCode = ReadRepositoryFile("InventoryManagementApp", "Views", "Pages", "ReportsPage.xaml.cs");

            Assert.Contains("public bool CanPrintCurrentReport => LastRunAt.HasValue && ReportLines.Count > 0 && !string.Equals(ReportStatus, \"Report failed.\", StringComparison.Ordinal);", viewModel, StringComparison.Ordinal);
            Assert.Contains("OnPropertyChanged(nameof(CanPrintCurrentReport));", viewModel, StringComparison.Ordinal);
            Assert.Contains("if (DataContext is not ReportsViewModel vm || !vm.CanPrintCurrentReport)", pageCode, StringComparison.Ordinal);
            Assert.DoesNotContain("if (DataContext is not ReportsViewModel vm || vm.ReportLines.Count == 0)", pageCode, StringComparison.Ordinal);
        }

        [Fact]
        public void ReportsSelectedRowActions_RequireActualSelectedReportLine()
        {
            var pageCode = ReadRepositoryFile("InventoryManagementApp", "Views", "Pages", "ReportsPage.xaml.cs");

            Assert.Contains("private ReportLine? GetSelectedReportLineForAction()", pageCode, StringComparison.Ordinal);
            Assert.Contains("if (ReportGrid.SelectedItem is ReportLine gridLine)", pageCode, StringComparison.Ordinal);
            Assert.Contains("return DataContext is ReportsViewModel vm", pageCode, StringComparison.Ordinal);
            Assert.Contains("? vm.SelectedReportLine", pageCode, StringComparison.Ordinal);
            Assert.Contains("var line = GetSelectedReportLineForAction();", pageCode, StringComparison.Ordinal);
            Assert.Contains("if (line == null || string.IsNullOrWhiteSpace(line.DestinationKey))", pageCode, StringComparison.Ordinal);
            Assert.Contains("switch (line.DestinationKey)", pageCode, StringComparison.Ordinal);
            Assert.DoesNotContain("key = vm.SelectedLineDestinationKey;", pageCode, StringComparison.Ordinal);
            Assert.DoesNotContain("var key = line?.DestinationKey;", pageCode, StringComparison.Ordinal);
        }

        [Fact]
        public void InsightPrintActions_RouteThroughSharedPrintPreview()
        {
            var reportsCode = ReadRepositoryFile("InventoryManagementApp", "Views", "Pages", "ReportsPage.xaml.cs");
            var activityCode = ReadRepositoryFile("InventoryManagementApp", "Views", "Pages", "ActivityLogsPage.xaml.cs");

            Assert.Contains("new PrintPreviewWindow().ShowPreview(", reportsCode, StringComparison.Ordinal);
            Assert.Contains("vm.ReportTitle", reportsCode, StringComparison.Ordinal);
            Assert.Contains("Review the report summary, destination routing, and next-action handoff before printing.", reportsCode, StringComparison.Ordinal);
            Assert.Contains("new PrintPreviewWindow().ShowPreview(", activityCode, StringComparison.Ordinal);
            Assert.Contains("\"Activity Logs\"", activityCode, StringComparison.Ordinal);
            Assert.Contains("Review the filtered audit trail, destination routing, and operator handoff before printing.", activityCode, StringComparison.Ordinal);
            Assert.DoesNotContain("WpfPrintDialog", reportsCode, StringComparison.Ordinal);
            Assert.DoesNotContain("PrintDocument(((IDocumentPaginatorSource)document).DocumentPaginator", reportsCode, StringComparison.Ordinal);
            Assert.DoesNotContain("WpfPrintDialog", activityCode, StringComparison.Ordinal);
            Assert.DoesNotContain("PrintDocument(((IDocumentPaginatorSource)document).DocumentPaginator", activityCode, StringComparison.Ordinal);
        }

        [Fact]
        public void ReportsNavigation_RunsSummaryReportBeforeShowingPage()
        {
            var source = ReadRepositoryFile("InventoryManagementApp", "ViewModels", "MainViewModel.cs");

            Assert.Contains("await Reports.RunSummaryReportAsync();", source, StringComparison.Ordinal);
            Assert.Contains("var page = new ReportsPage { DataContext = Reports, Title = \"Reports\" };", source, StringComparison.Ordinal);
        }

        static string ReadRepositoryFile(params string[] relativePathParts)
        {
            var directory = new DirectoryInfo(AppContext.BaseDirectory);
            while (directory != null && !File.Exists(Path.Combine(directory.FullName, "InventoryManagementApp.sln")))
                directory = directory.Parent;

            Assert.NotNull(directory);
            var path = Path.Combine(directory!.FullName, Path.Combine(relativePathParts));
            Assert.True(File.Exists(path), $"Expected repository file at {path}");
            return File.ReadAllText(path);
        }
    }
}
