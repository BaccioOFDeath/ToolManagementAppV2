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

            AssertContainsAll(
                xaml,
                "Data Operations Workbench",
                "DataOperationStatCard",
                "Data Control Lanes",
                "Session Handoff",
                "ItemDataSummary",
                "CustomerDataSummary",
                "BackupSummary",
                "ImageImportSummary",
                "DataOperationStatus",
                "DataOperationSummary");
        }

        [Fact]
        public void ImportExportPage_PreservesCommandsHandlersAndRunLogState()
        {
            var xaml = ReadRepositoryFile("InventoryManagementApp", "Views", "Pages", "ImportExportPage.xaml");

            AssertContainsAll(
                xaml,
                "ImportItemsCommand",
                "ExportItemsCommand",
                "ImportCustomersCommand",
                "ExportCustomersCommand",
                "BackupDatabaseCommand",
                "RestoreBackupCommand",
                "OpenImageImportMappingWindowCommand",
                "ClearImportExportLogsCommand",
                "ImportExportLogGrid_MouseDoubleClick",
                "ImportExportLogRow_PreviewMouseRightButtonDown",
                "OpenSelectedLog_Click",
                "CopySelectedLog_Click",
                "PrintLogs_Click",
                "No operation log rows yet",
                "DataRunLogCard");
        }

        [Fact]
        public void ImportExportLogRightClick_UsesSharedGuardedGridSelection()
        {
            var pageCode = ReadRepositoryFile("InventoryManagementApp", "Views", "Pages", "ImportExportPage.xaml.cs");
            var handler = ExtractMethodBody(pageCode, "private void ImportExportLogRow_PreviewMouseRightButtonDown", "private void OpenSelectedLog_Click");

            Assert.Contains("GridContextMenuSelection.SelectRow(sender, e);", handler, StringComparison.Ordinal);
            Assert.DoesNotContain("if (sender is DataGridRow row", handler, StringComparison.Ordinal);
            Assert.DoesNotContain("row.IsSelected = true;", handler, StringComparison.Ordinal);
            Assert.DoesNotContain("row.Focus();", handler, StringComparison.Ordinal);
            Assert.DoesNotContain("e.Handled = true;", handler, StringComparison.Ordinal);
        }

        [Fact]
        public void ImportExportLogActions_FallBackToViewModelSelectedLog()
        {
            var pageCode = ReadRepositoryFile("InventoryManagementApp", "Views", "Pages", "ImportExportPage.xaml.cs");

            AssertContainsAll(
                pageCode,
                "private string GetSelectedLogForAction()",
                "if (ImportExportLogGrid.SelectedItem is string gridLog && !string.IsNullOrWhiteSpace(gridLog))",
                "DataContext is ImportExportViewModel vm && !string.IsNullOrWhiteSpace(vm.SelectedImportExportLog)",
                "? vm.SelectedImportExportLog",
                "var log = GetSelectedLogForAction();");
            Assert.DoesNotContain("if (ImportExportLogGrid.SelectedItem is not string log || string.IsNullOrWhiteSpace(log))", pageCode, StringComparison.Ordinal);
        }

        [Fact]
        public void ImportExportLogPrint_PrefersSelectedResultBeforeWholeSessionLog()
        {
            var pageCode = ReadRepositoryFile("InventoryManagementApp", "Views", "Pages", "ImportExportPage.xaml.cs");
            var printMethod = ExtractMethodBody(pageCode, "private void PrintLogs_Click", "private static FlowDocument BuildPrintDocument");

            AssertContainsAll(
                printMethod,
                "var selectedLog = GetSelectedLogForAction();",
                "if (!string.IsNullOrWhiteSpace(selectedLog))",
                "new[] { selectedLog }",
                "\"Selected import/export operation result.\"",
                "\"Import / Export Selected Result\"",
                "ShowPreview(selectedDocument, \"Import / Export Selected Result\", \"Review one selected data-operation result before copying, printing, or filing the handoff.\");",
                "return;",
                "DataContext is not ImportExportViewModel vm || vm.ImportExportLogs.Count == 0",
                "BuildPrintDocument(vm.ImportExportLogs.ToList(), vm.LogSummary)");
            Assert.True(
                printMethod.IndexOf("var selectedLog = GetSelectedLogForAction();", StringComparison.Ordinal) <
                printMethod.IndexOf("DataContext is not ImportExportViewModel vm", StringComparison.Ordinal),
                "Selected result printing should be resolved before falling back to the whole session log.");
        }

        [Fact]
        public void ImportExportViewModel_ShowsVisibleFeedbackForFailedDataOperations()
        {
            var source = ReadRepositoryFile("InventoryManagementApp", "ViewModels", "ImportExportViewModel.cs");

            AssertContainsAll(
                source,
                "await _dialogService.ShowInfoAsync(errorMessage, $\"Export {plural}\");",
                "await _dialogService.ShowInfoAsync(failureMessage, $\"Export {plural}\");",
                "await _dialogService.ShowInfoAsync(message, $\"Export {plural}\");",
                "await _dialogService.ShowInfoAsync(errorMessage, \"Import Customers\");",
                "await _dialogService.ShowInfoAsync(failureMessage, \"Import Customers\");",
                "await _dialogService.ShowInfoAsync(message, \"Import Customers\");",
                "await _dialogService.ShowInfoAsync(errorMessage, \"Export Customers\");",
                "await _dialogService.ShowInfoAsync(failureMessage, \"Export Customers\");",
                "await _dialogService.ShowInfoAsync(message, \"Export Customers\");",
                "await _dialogService.ShowInfoAsync(failureMessage, \"Full Backup\");",
                "await _dialogService.ShowInfoAsync(message, \"Full Backup\");",
                "await _dialogService.ShowInfoAsync(failureMessage, \"Restore Backup\").ConfigureAwait(false);");
        }

        [Fact]
        public void ImportExportViewModel_ShowsVisibleFeedbackForBackupStartupFailures()
        {
            var source = ReadRepositoryFile("InventoryManagementApp", "ViewModels", "ImportExportViewModel.cs");

            AssertContainsAll(
                source,
                "async Task BackupDatabaseAsync(CancellationToken cancellationToken)",
                "string? path = null;",
                "var initialDirectory = _rentalConfigService == null",
                "path = _fileDialogService.SaveFile(\"Inventory Backup Package|*.inventory-backup.zip|Zip Files|*.zip\", initialDirectory);",
                "var failureMessage = string.IsNullOrWhiteSpace(path)",
                "? $\"Failed to start full backup: {ex.Message}\"",
                ": $\"Failed to create full backup package at {path}: {ex.Message}\";",
                "AddLog(failureMessage);",
                "await _dialogService.ShowInfoAsync(failureMessage, \"Full Backup\");");
        }

        [Fact]
        public void ImportExportViewModel_ShowsVisibleFeedbackForFileDialogCancellations()
        {
            var source = ReadRepositoryFile("InventoryManagementApp", "ViewModels", "ImportExportViewModel.cs");

            AssertContainsAll(
                source,
                "async Task CancelFileSelectionAsync(string message, string title)",
                "AddLog(message);",
                "await _dialogService.ShowInfoAsync(message, title);",
                "await CancelFileSelectionAsync($\"{plural} import file selection was cancelled.\", $\"Import {plural}\");",
                "await CancelFileSelectionAsync($\"{plural} export destination selection was cancelled.\", $\"Export {plural}\");",
                "await CancelFileSelectionAsync(\"Customer import file selection was cancelled.\", \"Import Customers\");",
                "await CancelFileSelectionAsync(\"Customer export destination selection was cancelled.\", \"Export Customers\");",
                "await CancelFileSelectionAsync(\"Full backup destination selection was cancelled.\", \"Full Backup\");",
                "await CancelFileSelectionAsync(\"Backup package selection was cancelled.\", \"Restore Backup\");");
        }

        [Fact]
        public void ImportExportViewModel_ShowsVisibleFeedbackForSuccessfulDataOperations()
        {
            var source = ReadRepositoryFile("InventoryManagementApp", "ViewModels", "ImportExportViewModel.cs");

            AssertContainsAll(
                source,
                "var successMessage = $\"Successfully imported {plural} from {path}.\";",
                "successMessage += $\" {skippedMessage}\";",
                "await _dialogService.ShowInfoAsync(successMessage, $\"Import {plural}\");",
                "var successMessage = $\"Successfully exported {plural} to {path} ({exporter.FormatName} format).\";",
                "await _dialogService.ShowInfoAsync(successMessage, $\"Export {plural}\");",
                "var successMessage = $\"Successfully imported customers from {path}. Imported {result.ImportedCount} customers.\";",
                "successMessage += $\" {result.SkippedRows.Count} skipped row",
                "await _dialogService.ShowInfoAsync(successMessage, \"Import Customers\");",
                "var successMessage = $\"Successfully imported {importedCount} customers from {path} ({importer.FormatName} format).\";",
                "var successMessage = $\"Successfully exported customers to {path} ({exporter.FormatName} format).\";",
                "await _dialogService.ShowInfoAsync(successMessage, \"Export Customers\");",
                "var successMessage = $\"Successfully created full backup package at {path}.\";",
                "await _dialogService.ShowInfoAsync(successMessage, \"Full Backup\");",
                "var successMessage = $\"Successfully restored backup package from {path}. Safety backup created at {safetyBackupPath}. Restart the app before continuing work.\";",
                "await _dialogService.ShowInfoAsync(successMessage, \"Restore Backup\")");
        }

        [Fact]
        public void ImportExportViewModel_ShowsVisibleFeedbackForItemImportFailures()
        {
            var source = ReadRepositoryFile("InventoryManagementApp", "ViewModels", "ImportExportViewModel.cs");

            AssertContainsAll(
                source,
                "var message = $\"{plural} import mapping was cancelled.\";",
                "await _dialogService.ShowInfoAsync(message, $\"Import {plural}\");",
                "var errorMessage = $\"Mapping for {singular} number is required.\";",
                "await _dialogService.ShowInfoAsync(errorMessage, $\"Import {plural}\");",
                "var errorMessage = $\"No importer found for file type: {extension}\";",
                "await _dialogService.ShowInfoAsync($\"{plural} import was cancelled.\", $\"Import {plural}\");",
                "await _dialogService.ShowInfoAsync($\"Failed to import {plural} from {path}: {ex.Message}\", $\"Import {plural}\");");
        }

        [Fact]
        public void ImportExportViewModel_ShowsVisibleFeedbackForCustomerImportFailures()
        {
            var source = ReadRepositoryFile("InventoryManagementApp", "ViewModels", "ImportExportViewModel.cs");

            AssertContainsAll(
                source,
                "const string message = \"Customer import mapping was cancelled.\";",
                "await _dialogService.ShowInfoAsync(message, \"Import Customers\");",
                "var errorMessage = $\"No importer found for file type: {extension}\";",
                "await _dialogService.ShowInfoAsync(errorMessage, \"Import Customers\");",
                "const string message = \"Customer import was cancelled.\";",
                "var failureMessage = $\"Failed to import customers from {path}: {ex.Message}\";",
                "await _dialogService.ShowInfoAsync(failureMessage, \"Import Customers\");");
        }

        static void AssertContainsAll(string source, params string[] expectedSnippets)
        {
            foreach (var snippet in expectedSnippets)
            {
                Assert.Contains(snippet, source, StringComparison.Ordinal);
            }
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
