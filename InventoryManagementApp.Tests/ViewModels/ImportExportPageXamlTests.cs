using System;
using System.IO;
using Xunit;

namespace InventoryManagementApp.Tests.ViewModels
{
    public class ImportExportPageXamlTests
    {
        [Fact]
        public void ImportExportPage_UsesDataOperationsWorkbenchSummariesAndCommands()
        {
            var xaml = ReadRepositoryFile("InventoryManagementApp", "Views", "Pages", "ImportExportPage.xaml");

            Assert.Contains("Data Operations Workbench", xaml, StringComparison.Ordinal);
            Assert.Contains("Import Readiness", xaml, StringComparison.Ordinal);
            Assert.Contains("Customer And Recovery Lane", xaml, StringComparison.Ordinal);
            Assert.Contains("ItemDataSummary", xaml, StringComparison.Ordinal);
            Assert.Contains("CustomerDataSummary", xaml, StringComparison.Ordinal);
            Assert.Contains("ImageImportSummary", xaml, StringComparison.Ordinal);
            Assert.Contains("BackupSummary", xaml, StringComparison.Ordinal);
            Assert.Contains("LogSummary", xaml, StringComparison.Ordinal);
            Assert.Contains("ImportItemsCommand", xaml, StringComparison.Ordinal);
            Assert.Contains("ExportItemsCommand", xaml, StringComparison.Ordinal);
            Assert.Contains("ImportCustomersCommand", xaml, StringComparison.Ordinal);
            Assert.Contains("ExportCustomersCommand", xaml, StringComparison.Ordinal);
            Assert.Contains("BackupDatabaseCommand", xaml, StringComparison.Ordinal);
        }

        [Fact]
        public void ImportExportPage_PreservesRunLogHooksAndFooterStatus()
        {
            var xaml = ReadRepositoryFile("InventoryManagementApp", "Views", "Pages", "ImportExportPage.xaml");

            Assert.Contains("Operation Results", xaml, StringComparison.Ordinal);
            Assert.Contains("Run Result", xaml, StringComparison.Ordinal);
            Assert.Contains("SelectedLogTitle", xaml, StringComparison.Ordinal);
            Assert.Contains("SelectedLogDetail", xaml, StringComparison.Ordinal);
            Assert.Contains("ImportExportLogGrid_MouseDoubleClick", xaml, StringComparison.Ordinal);
            Assert.Contains("ImportExportLogRow_PreviewMouseRightButtonDown", xaml, StringComparison.Ordinal);
            Assert.Contains("OpenSelectedLog_Click", xaml, StringComparison.Ordinal);
            Assert.Contains("CopySelectedLog_Click", xaml, StringComparison.Ordinal);
            Assert.Contains("PrintLogs_Click", xaml, StringComparison.Ordinal);
            Assert.Contains("ClearImportExportLogsCommand", xaml, StringComparison.Ordinal);
            Assert.Contains("Data desk ready", xaml, StringComparison.Ordinal);
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
