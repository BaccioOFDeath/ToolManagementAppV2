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
        public void InsightPrintActions_RouteThroughSharedPrintPreview()
        {
            var reportsCode = ReadRepositoryFile("InventoryManagementApp", "Views", "Pages", "ReportsPage.xaml.cs");
            var activityCode = ReadRepositoryFile("InventoryManagementApp", "Views", "Pages", "ActivityLogsPage.xaml.cs");

            Assert.Contains("new PrintPreviewWindow().ShowPreview(document, vm.ReportTitle, null)", reportsCode, StringComparison.Ordinal);
            Assert.Contains("new PrintPreviewWindow().ShowPreview(document, \"Activity Logs\", null)", activityCode, StringComparison.Ordinal);
            Assert.DoesNotContain("WpfPrintDialog", reportsCode, StringComparison.Ordinal);
            Assert.DoesNotContain("WpfPrintDialog", activityCode, StringComparison.Ordinal);
            Assert.DoesNotContain("PrintDocument(((IDocumentPaginatorSource)document).DocumentPaginator", reportsCode, StringComparison.Ordinal);
            Assert.DoesNotContain("PrintDocument(((IDocumentPaginatorSource)document).DocumentPaginator", activityCode, StringComparison.Ordinal);
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
