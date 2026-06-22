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
        public void ImportExportViewModel_ShowsVisibleFeedbackForBackupStartupFailures()
        {
            var source = ReadRepositoryFile("InventoryManagementApp", "ViewModels", "ImportExportViewModel.cs");
            var backup = ExtractMethodBody(source, "async Task BackupDatabaseAsync", "    }\n}");

            Assert.Contains("string? path = null;", backup, StringComparison.Ordinal);
            Assert.Contains("var initialDirectory = _rentalConfigService == null", backup, StringComparison.Ordinal);
            Assert.Contains("path = _fileDialogService.SaveFile(\"SQLite Database|*.db\", initialDirectory);", backup, StringComparison.Ordinal);
            Assert.Contains("var failureMessage = string.IsNullOrWhiteSpace(path)", backup, StringComparison.Ordinal);
            Assert.Contains("? $\"Failed to start database backup: {ex.Message}\"", backup, StringComparison.Ordinal);
            Assert.Contains(": $\"Failed to backup database to {path}: {ex.Message}\";", backup, StringComparison.Ordinal);
            Assert.Contains("AddLog(failureMessage);", backup, StringComparison.Ordinal);
            Assert.Contains("await _dialogService.ShowInfoAsync(failureMessage, \"Database Backup\");", backup, StringComparison.Ordinal);
        }

        [Fact]
        public void ImportExportViewModel_ShowsVisibleFeedbackForFileDialogCancellations()
        {
            var source = ReadRepositoryFile("InventoryManagementApp", "ViewModels", "ImportExportViewModel.cs");

            Assert.Contains("async Task CancelFileSelectionAsync(string message, string title)", source, StringComparison.Ordinal);
            Assert.Contains("AddLog(message);", source, StringComparison.Ordinal);
            Assert.Contains("await _dialogService.ShowInfoAsync(message, title);", source, StringComparison.Ordinal);

            var itemImport = ExtractMethodBody(source, "async Task ImportItemsAsync", "async Task ExportItemsAsync");
            Assert.Contains("if (string.IsNullOrWhiteSpace(path))", itemImport, StringComparison.Ordinal);
            Assert.Contains("await CancelFileSelectionAsync($\"{plural} import file selection was cancelled.\", $\"Import {plural}\");", itemImport, StringComparison.Ordinal);

            var itemExport = ExtractMethodBody(source, "async Task ExportItemsAsync", "async Task ImportCustomersAsync");
            Assert.Contains("await CancelFileSelectionAsync($\"{plural} export destination selection was cancelled.\", $\"Export {plural}\");", itemExport, StringComparison.Ordinal);

            var customerImport = ExtractMethodBody(source, "async Task ImportCustomersAsync", "async Task ExportCustomersAsync");
            Assert.Contains("await CancelFileSelectionAsync(\"Customer import file selection was cancelled.\", \"Import Customers\");", customerImport, StringComparison.Ordinal);

            var customerExport = ExtractMethodBody(source, "async Task ExportCustomersAsync", "async Task BackupDatabaseAsync");
            Assert.Contains("await CancelFileSelectionAsync(\"Customer export destination selection was cancelled.\", \"Export Customers\");", customerExport, StringComparison.Ordinal);

            var backup = ExtractMethodBody(source, "async Task BackupDatabaseAsync", "    }\n}");
            Assert.Contains("await CancelFileSelectionAsync(\"Database backup destination selection was cancelled.\", \"Database Backup\");", backup, StringComparison.Ordinal);
        }

        [Fact]
        public void ImportExportViewModel_ShowsVisibleFeedbackForSuccessfulDataOperations()
        {
            var source = ReadRepositoryFile("InventoryManagementApp", "ViewModels", "ImportExportViewModel.cs");

            var itemImport = ExtractMethodBody(source, "async Task ImportItemsAsync", "async Task ExportItemsAsync");
            Assert.Contains("var successMessage = $\"Successfully imported {plural} from {path}.\";", itemImport, StringComparison.Ordinal);
            Assert.Contains("successMessage += $\" {skippedMessage}\";", itemImport, StringComparison.Ordinal);
            Assert.Contains("await _dialogService.ShowInfoAsync(successMessage, $\"Import {plural}\");", itemImport, StringComparison.Ordinal);

            Assert.Contains("var successMessage = $\"Successfully exported {plural} to {path} ({exporter.FormatName} format).\";", source, StringComparison.Ordinal);
            Assert.Contains("await _dialogService.ShowInfoAsync(successMessage, $\"Export {plural}\");", source, StringComparison.Ordinal);

            var customerImport = ExtractMethodBody(source, "async Task ImportCustomersAsync", "async Task ExportCustomersAsync");
            Assert.Contains("var successMessage = $\"Successfully imported customers from {path}. Imported {result.ImportedCount} customers.\";", customerImport, StringComparison.Ordinal);
            Assert.Contains("successMessage += $\" {result.SkippedRows.Count} skipped row", customerImport, StringComparison.Ordinal);
            Assert.Contains("await _dialogService.ShowInfoAsync(successMessage, \"Import Customers\");", customerImport, StringComparison.Ordinal);
            Assert.Contains("var successMessage = $\"Successfully imported {importedCount} customers from {path} ({importer.FormatName} format).\";", customerImport, StringComparison.Ordinal);

            var customerExport = ExtractMethodBody(source, "async Task ExportCustomersAsync", "async Task BackupDatabaseAsync");
            Assert.Contains("var successMessage = $\"Successfully exported customers to {path} ({exporter.FormatName} format).\";", customerExport, StringComparison.Ordinal);
            Assert.Contains("await _dialogService.ShowInfoAsync(successMessage, \"Export Customers\");", customerExport, StringComparison.Ordinal);

            var backup = ExtractMethodBody(source, "async Task BackupDatabaseAsync", "    }\n}");
            Assert.Contains("var successMessage = $\"Successfully backed up database to {path}.\";", backup, StringComparison.Ordinal);
            Assert.Contains("await _dialogService.ShowInfoAsync(successMessage, \"Database Backup\");", backup, StringComparison.Ordinal);
        }

        [Fact]
        public void ImportExportViewModel_ShowsVisibleFeedbackForItemImportFailures()
        {
            var source = ReadRepositoryFile("InventoryManagementApp", "ViewModels", "ImportExportViewModel.cs");

            Assert.Contains("var message = $\"{plural} import mapping was cancelled.\";", source, StringComparison.Ordinal);
            Assert.Contains("await _dialogService.ShowInfoAsync(message, $\"Import {plural}\");", source, StringComparison.Ordinal);
            Assert.Contains("var errorMessage = $\"Mapping for {singular} number is required.\";", source, StringComparison.Ordinal);
            Assert.Contains("await _dialogService.ShowInfoAsync(errorMessage, $\"Import {plural}\");", source, StringComparison.Ordinal);
            Assert.Contains("var errorMessage = $\"No importer found for file type: {extension}\";", source, StringComparison.Ordinal);
            Assert.Contains("await _dialogService.ShowInfoAsync($\"{plural} import was cancelled.\", $\"Import {plural}\");", source, StringComparison.Ordinal);
            Assert.Contains("await _dialogService.ShowInfoAsync($\"Failed to import {plural} from {path}: {ex.Message}\", $\"Import {plural}\");", source, StringComparison.Ordinal);
        }

        [Fact]
        public void ImportExportViewModel_ShowsVisibleFeedbackForCustomerImportFailures()
        {
            var source = ReadRepositoryFile("InventoryManagementApp", "ViewModels", "ImportExportViewModel.cs");
            var method = ExtractMethodBody(source, "async Task ImportCustomersAsync", "async Task ExportCustomersAsync");

            Assert.Contains("const string message = \"Customer import mapping was cancelled.\";", method, StringComparison.Ordinal);
            Assert.Contains("await _dialogService.ShowInfoAsync(message, \"Import Customers\");", method, StringComparison.Ordinal);
            Assert.Contains("var errorMessage = $\"No importer found for file type: {extension}\";", method, StringComparison.Ordinal);
            Assert.Contains("await _dialogService.ShowInfoAsync(errorMessage, \"Import Customers\");", method, StringComparison.Ordinal);
            Assert.Contains("const string message = \"Customer import was cancelled.\";", method, StringComparison.Ordinal);
            Assert.Contains("await _dialogService.ShowInfoAsync(message, \"Import Customers\");", method, StringComparison.Ordinal);
            Assert.Contains("var failureMessage = $\"Failed to import customers from {path}: {ex.Message}\";", method, StringComparison.Ordinal);
            Assert.Contains("await _dialogService.ShowInfoAsync(failureMessage, \"Import Customers\");", method, StringComparison.Ordinal);
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

        static string ExtractMethodBody(string source, string methodStart, string nextMethodStart)
        {
            var start = source.IndexOf(methodStart, StringComparison.Ordinal);
            Assert.True(start >= 0, $"Expected to find method starting with {methodStart}");

            var end = source.IndexOf(nextMethodStart, start, StringComparison.Ordinal);
            Assert.True(end > start, $"Expected to find next method starting with {nextMethodStart}");

            return source.Substring(start, end - start);
        }
    }
}