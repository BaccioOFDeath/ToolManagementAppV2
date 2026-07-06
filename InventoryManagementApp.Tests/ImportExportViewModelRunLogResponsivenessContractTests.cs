using System;
using System.IO;
using Xunit;

namespace InventoryManagementApp.Tests
{
    public class ImportExportViewModelRunLogResponsivenessContractTests
    {
        [Fact]
        public void ImportExportViewModel_BoundsVisibleRunLogRowsForGridResponsiveness()
        {
            var viewModel = ReadRepoFile("InventoryManagementApp", "ViewModels", "ImportExportViewModel.cs");

            Assert.Contains("private const int MaxVisibleImportExportLogRows = 500;", viewModel, StringComparison.Ordinal);
            Assert.Contains("public int VisibleImportExportLogCount => ImportExportLogs.Count;", viewModel, StringComparison.Ordinal);
            Assert.Contains("public int OmittedImportExportLogCount => _omittedImportExportLogCount;", viewModel, StringComparison.Ordinal);
            Assert.Contains("public bool HasOmittedImportExportLogs => _omittedImportExportLogCount > 0;", viewModel, StringComparison.Ordinal);
            Assert.Contains("while (ImportExportLogs.Count >= MaxVisibleImportExportLogRows)", viewModel, StringComparison.Ordinal);
            Assert.Contains("ImportExportLogs.RemoveAt(0);", viewModel, StringComparison.Ordinal);
            Assert.Contains("_omittedImportExportLogCount++;", viewModel, StringComparison.Ordinal);
            Assert.DoesNotContain("ImportExportLogs.Add(message);\n            SelectedImportExportLog = message;\n            ClearImportExportLogsCommand.NotifyCanExecuteChanged();", viewModel, StringComparison.Ordinal);
        }

        [Fact]
        public void ImportExportViewModel_ReportsVisibleAndOmittedRunLogCounts()
        {
            var viewModel = ReadRepoFile("InventoryManagementApp", "ViewModels", "ImportExportViewModel.cs");

            Assert.Contains("var visibleCount = VisibleImportExportLogCount;", viewModel, StringComparison.Ordinal);
            Assert.Contains("var totalCount = visibleCount + OmittedImportExportLogCount;", viewModel, StringComparison.Ordinal);
            Assert.Contains("if (HasOmittedImportExportLogs)", viewModel, StringComparison.Ordinal);
            Assert.Contains("visible of {totalCount} operation log", viewModel, StringComparison.Ordinal);
            Assert.Contains("kept out of the grid for responsiveness", viewModel, StringComparison.Ordinal);
            Assert.Contains("OnPropertyChanged(nameof(VisibleImportExportLogCount));", viewModel, StringComparison.Ordinal);
            Assert.Contains("OnPropertyChanged(nameof(OmittedImportExportLogCount));", viewModel, StringComparison.Ordinal);
            Assert.Contains("OnPropertyChanged(nameof(HasOmittedImportExportLogs));", viewModel, StringComparison.Ordinal);
            Assert.Contains("OnPropertyChanged(nameof(LogSummary));", viewModel, StringComparison.Ordinal);
        }

        [Fact]
        public void ImportExportViewModel_ClearsOmittedRunLogCountWithVisibleRows()
        {
            var viewModel = ReadRepoFile("InventoryManagementApp", "ViewModels", "ImportExportViewModel.cs");

            Assert.Contains("void ClearImportExportLogs()", viewModel, StringComparison.Ordinal);
            Assert.Contains("ImportExportLogs.Clear();", viewModel, StringComparison.Ordinal);
            Assert.Contains("_omittedImportExportLogCount = 0;", viewModel, StringComparison.Ordinal);
            Assert.Contains("SelectedImportExportLog = null;", viewModel, StringComparison.Ordinal);
            Assert.Contains("ClearImportExportLogsCommand.NotifyCanExecuteChanged();", viewModel, StringComparison.Ordinal);
        }

        [Fact]
        public void ImportExportViewModel_BoundsSelectedLogInlinePreviewButPreservesFullSelection()
        {
            var viewModel = ReadRepoFile("InventoryManagementApp", "ViewModels", "ImportExportViewModel.cs");

            Assert.Contains("private const int MaxSelectedLogDetailCharacters = 1800;", viewModel, StringComparison.Ordinal);
            Assert.Contains("public string SelectedLogDetail", viewModel, StringComparison.Ordinal);
            Assert.Contains("BuildSelectedLogDetailPreview(SelectedImportExportLog)", viewModel, StringComparison.Ordinal);
            Assert.Contains("private static string BuildSelectedLogDetailPreview(string? value)", viewModel, StringComparison.Ordinal);
            Assert.Contains("if (text.Length <= MaxSelectedLogDetailCharacters)", viewModel, StringComparison.Ordinal);
            Assert.Contains("text.Substring(0, MaxSelectedLogDetailCharacters).TrimEnd();", viewModel, StringComparison.Ordinal);
            Assert.Contains("characters omitted from this inline preview", viewModel, StringComparison.Ordinal);
            Assert.Contains("Use Copy Result or Open Log Detail for the complete operation text.", viewModel, StringComparison.Ordinal);
        }

        [Fact]
        public void ImportExportPage_StillUsesCompleteSelectedLogForCopyDetailAndPrintHandoff()
        {
            var codeBehind = ReadRepoFile("InventoryManagementApp", "Views", "Pages", "ImportExportPage.xaml.cs");

            Assert.Contains("private string GetSelectedLogForAction()", codeBehind, StringComparison.Ordinal);
            Assert.Contains("ImportExportLogGrid.SelectedItem is string gridLog", codeBehind, StringComparison.Ordinal);
            Assert.Contains("vm.SelectedImportExportLog", codeBehind, StringComparison.Ordinal);
            Assert.Contains("Clipboard.SetText(log);", codeBehind, StringComparison.Ordinal);
            Assert.Contains("BuildBoundedLogText(log, MaxDetailLogCharacters", codeBehind, StringComparison.Ordinal);
            Assert.Contains("new[] { selectedLog }", codeBehind, StringComparison.Ordinal);
        }

        private static string ReadRepoFile(params string[] parts)
        {
            var directory = AppContext.BaseDirectory;

            while (!string.IsNullOrEmpty(directory))
            {
                var candidate = Path.Combine(directory, Path.Combine(parts));
                if (File.Exists(candidate))
                    return NormalizeLineEndings(File.ReadAllText(candidate));

                var parent = Directory.GetParent(directory);
                if (parent is null)
                    break;

                directory = parent.FullName;
            }

            throw new FileNotFoundException($"Could not find repository file: {Path.Combine(parts)}");
        }

        private static string NormalizeLineEndings(string text) =>
            text.Replace("\r\n", "\n", StringComparison.Ordinal).Replace("\r", "\n", StringComparison.Ordinal);
    }
}
