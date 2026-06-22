using System;
using System.IO;
using Xunit;

namespace InventoryManagementApp.Tests.ViewModels
{
    public class ImportExportPageXamlTests
    {
        [Fact]
        public void ImportExportPage_UsesDataWorkbenchSummariesAndLanes()
        {
            var xaml = ReadRepositoryFile("InventoryManagementApp", "Views", "Pages", "ImportExportPage.xaml");

            Assert.Contains("Data Operations Workbench", xaml, StringComparison.Ordinal);
            Assert.Contains("DataOperationStatCard", xaml, StringComparison.Ordinal);
            Assert.Contains("Data Control Lanes", xaml, StringComparison.Ordinal);
            Assert.Contains("Session Handoff", xaml, StringComparison.Ordinal);
            Assert.Contains("ItemDataSummary", xaml, StringComparison.Ordinal);
            Assert.Contains("CustomerDataSummary", xaml, StringComparison.Ordinal);
            Assert.Contains("BackupSummary", xaml, StringComparison.Ordinal);
            Assert.Contains("ImageImportSummary", xaml, StringComparison.Ordinal);
            Assert.Contains("Data desk ready", xaml, StringComparison.Ordinal);
        }

        [Fact]
        public void ImportExportPage_PreservesCommandsHandlersAndRunLogState()
        {
            var xaml = ReadRepositoryFile("InventoryManagementApp", "Views", "Pages", "ImportExportPage.xaml");

            Assert.Contains("ImportItemsCommand", xaml, StringComparison.Ordinal);
            Assert.Contains("ExportItemsCommand", xaml, StringComparison.Ordinal);
            Assert.Contains("ImportCustomersCommand", xaml, StringComparison.Ordinal);
            Assert.Contains("ExportCustomersCommand", xaml, StringComparison.Ordinal);
            Assert.Contains("BackupDatabaseCommand", xaml, StringComparison.Ordinal);
            Assert.Contains("OpenImageImportMappingWindowCommand", xaml, StringComparison.Ordinal);
            Assert.Contains("ClearImportExportLogsCommand", xaml, StringComparison.Ordinal);
            Assert.Contains("ImportExportLogGrid_MouseDoubleClick", xaml, StringComparison.Ordinal);
            Assert.Contains("ImportExportLogRow_PreviewMouseRightButtonDown", xaml, StringComparison.Ordinal);
            Assert.Contains("OpenSelectedLog_Click", xaml, StringComparison.Ordinal);
            Assert.Contains("CopySelectedLog_Click", xaml, StringComparison.Ordinal);
            Assert.Contains("PrintLogs_Click", xaml, StringComparison.Ordinal);
            Assert.Contains("No operation log rows yet", xaml, StringComparison.Ordinal);
            Assert.Contains("DataRunLogCard", xaml, StringComparison.Ordinal);
        }

        [Fact]
        public void ImportExportViewModel_ShowsVisibleFeedbackForFailedDataOperations()
        {
            var source = ReadRepositoryFile("InventoryManagementApp", "ViewModels", "ImportExportViewModel.cs");

            Assert.Contains("await _dialogService.ShowInfoAsync(errorMessage, $\"Export {plural}\");", source, StringComparison.Ordinal);
            Assert.Contains("await _dialogService.ShowInfoAsync(failureMessage, $\"Export {plural}\");", source, StringComparison.Ordinal);
            Assert.Contains("await _dialogService.ShowInfoAsync(message, $\"Export {plural}\");", source, StringComparison.Ordinal);

            Assert.Contains("await _dialogService.ShowInfoAsync(errorMessage, \"Import Customers\");", source, StringComparison.Ordinal);
            Assert.Contains("await _dialogService.ShowInfoAsync(failureMessage, \"Import Customers\");", source, StringComparison.Ordinal);
            Assert.Contains("await _dialogService.ShowInfoAsync(message, \"Import Customers\");", source, StringComparison.Ordinal);

            Assert.Contains("await _dialogService.ShowInfoAsync(errorMessage, \"Export Customers\");", source, StringComparison.Ordinal);
            Assert.Contains("await _dialogService.ShowInfoAsync(failureMessage, \"Export Customers\");", source, StringComparison.Ordinal);
            Assert.Contains("await _dialogService.ShowInfoAsync(message, \"Export Customers\");", source, StringComparison.Ordinal);

            Assert.Contains("await _dialogService.ShowInfoAsync(failureMessage, \"Database Backup\");", source, StringComparison.Ordinal);
            Assert.Contains("await _dialogService.ShowInfoAsync(message, \"Database Backup\");", source, StringComparison.Ordinal);
        }

        [Fact]
        public void ImportExportViewModel_ShowsVisibleFeedbackForItemImportFailures()
        {
            var source = ReadRepositoryFile("InventoryManagementApp", "ViewModels", "ImportExportViewModel.cs");

            Assert.Contains("var errorMessage = $\"Mapping for {singular} number is required.\";", source, StringComparison.Ordinal);
            Assert.Contains("await _dialogService.ShowInfoAsync(errorMessage, $\"Import {plural}\");", source, StringComparison.Ordinal);
            Assert.Contains("var errorMessage = $\"No importer found for file type: {extension}\";", source, StringComparison.Ordinal);
            Assert.Contains("await _dialogService.ShowInfoAsync($\"{plural} import was cancelled.\", $\"Import {plural}\");", source, StringComparison.Ordinal);
            Assert.Contains("await _dialogService.ShowInfoAsync($\"Failed to import {plural} from {path}: {ex.Message}\", $\"Import {plural}\");", source, StringComparison.Ordinal);
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