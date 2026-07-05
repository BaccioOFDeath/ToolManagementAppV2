using System;
using System.IO;
using Xunit;

namespace InventoryManagementApp.Tests
{
    public class ImportExportPageResponsiveContractTests
    {
        [Fact]
        public void ImportExportPage_KeepsHeaderMetricsWrappedAndBounded()
        {
            var xaml = ReadRepoFile("InventoryManagementApp", "Views", "Pages", "ImportExportPage.xaml");

            Assert.Contains("<WrapPanel Grid.Column=\"2\" HorizontalAlignment=\"Right\">", xaml, StringComparison.Ordinal);
            Assert.Contains("<Setter Property=\"MinWidth\" Value=\"150\"/>", xaml, StringComparison.Ordinal);
            Assert.Contains("<Setter Property=\"MaxWidth\" Value=\"230\"/>", xaml, StringComparison.Ordinal);
            Assert.Contains("DataOperationStatValueText", xaml, StringComparison.Ordinal);
            Assert.Contains("<ColumnDefinition Width=\"1.15*\" MinWidth=\"0\"/>", xaml, StringComparison.Ordinal);
            Assert.DoesNotContain("<UniformGrid Grid.Column=\"2\" Columns=\"4\">", xaml, StringComparison.Ordinal);
            Assert.DoesNotContain("<ColumnDefinition Width=\"2*\" MinWidth=\"390\"/>", xaml, StringComparison.Ordinal);
            Assert.DoesNotContain("<ColumnDefinition Width=\"3*\" MinWidth=\"540\"/>", xaml, StringComparison.Ordinal);
        }

        [Fact]
        public void ImportExportPage_AvoidsLargeFixedMinimumsAcrossDataSplits()
        {
            var xaml = ReadRepoFile("InventoryManagementApp", "Views", "Pages", "ImportExportPage.xaml");

            Assert.Contains("<ColumnDefinition Width=\"1.65*\" MinWidth=\"0\"/>", xaml, StringComparison.Ordinal);
            Assert.Contains("<ColumnDefinition Width=\"1.55*\" MinWidth=\"0\"/>", xaml, StringComparison.Ordinal);
            Assert.Contains("<ColumnDefinition Width=\"1.45*\" MinWidth=\"0\"/>", xaml, StringComparison.Ordinal);
            Assert.Contains("<ColumnDefinition Width=\"0.95*\" MinWidth=\"300\"/>", xaml, StringComparison.Ordinal);
            Assert.Contains("<ColumnDefinition Width=\"*\" MinWidth=\"0\"/>", xaml, StringComparison.Ordinal);
            Assert.Contains("<GridSplitter Grid.Column=\"1\" Width=\"6\"", xaml, StringComparison.Ordinal);
            Assert.Contains("Style=\"{StaticResource Card}\" Padding=\"0\" MinWidth=\"0\"", xaml, StringComparison.Ordinal);
            Assert.DoesNotContain("MinWidth=\"520\"", xaml, StringComparison.Ordinal);
            Assert.DoesNotContain("MinWidth=\"500\"", xaml, StringComparison.Ordinal);
            Assert.DoesNotContain("MinWidth=\"420\"", xaml, StringComparison.Ordinal);
            Assert.DoesNotContain("Width=\"430\" MinWidth=\"360\"", xaml, StringComparison.Ordinal);
        }

        [Fact]
        public void ImportExportPage_WrapsLaneCardsAndMakesHandoffPanesScrollable()
        {
            var xaml = ReadRepoFile("InventoryManagementApp", "Views", "Pages", "ImportExportPage.xaml");

            Assert.Contains("<Setter Property=\"MinWidth\" Value=\"220\"/>", xaml, StringComparison.Ordinal);
            Assert.Contains("<Setter Property=\"MaxWidth\" Value=\"360\"/>", xaml, StringComparison.Ordinal);
            Assert.Contains("<WrapPanel Margin=\"8\">", xaml, StringComparison.Ordinal);
            Assert.Contains("<ScrollViewer Grid.Row=\"1\" VerticalScrollBarVisibility=\"Auto\" HorizontalScrollBarVisibility=\"Disabled\">", xaml, StringComparison.Ordinal);
            Assert.Contains("<ScrollViewer Grid.Column=\"2\" VerticalScrollBarVisibility=\"Auto\" HorizontalScrollBarVisibility=\"Disabled\">", xaml, StringComparison.Ordinal);
            Assert.DoesNotContain("<UniformGrid Columns=\"2\" Margin=\"8\">", xaml, StringComparison.Ordinal);
            Assert.DoesNotContain("<StackPanel Grid.Column=\"2\">", xaml, StringComparison.Ordinal);
        }

        [Fact]
        public void ImportExportPage_EnablesRunLogGridVirtualizationScrollingAndFullRowSelection()
        {
            var xaml = ReadRepoFile("InventoryManagementApp", "Views", "Pages", "ImportExportPage.xaml");

            Assert.Contains("x:Name=\"ImportExportLogGrid\"", xaml, StringComparison.Ordinal);
            Assert.Contains("EnableRowVirtualization=\"True\"", xaml, StringComparison.Ordinal);
            Assert.Contains("EnableColumnVirtualization=\"True\"", xaml, StringComparison.Ordinal);
            Assert.Contains("SelectionMode=\"Single\"", xaml, StringComparison.Ordinal);
            Assert.Contains("SelectionUnit=\"FullRow\"", xaml, StringComparison.Ordinal);
            Assert.Contains("ScrollViewer.CanContentScroll=\"True\"", xaml, StringComparison.Ordinal);
            Assert.Contains("ScrollViewer.HorizontalScrollBarVisibility=\"Auto\"", xaml, StringComparison.Ordinal);
            Assert.Contains("ScrollViewer.VerticalScrollBarVisibility=\"Auto\"", xaml, StringComparison.Ordinal);
        }

        [Fact]
        public void ImportExportPage_BoundsEmptyStateHandoffTextAndFooterProgress()
        {
            var xaml = ReadRepoFile("InventoryManagementApp", "Views", "Pages", "ImportExportPage.xaml");

            Assert.Contains("<Border Grid.Row=\"2\" MaxWidth=\"340\" MinHeight=\"120\" Margin=\"12\"", xaml, StringComparison.Ordinal);
            Assert.Contains("HorizontalScrollBarVisibility=\"Disabled\" MaxHeight=\"260\"", xaml, StringComparison.Ordinal);
            Assert.Contains("<TextBlock Text=\"{Binding LogSummary}\"", xaml, StringComparison.Ordinal);
            Assert.Contains("Text=\"{Binding DataOperationStatus}\"", xaml, StringComparison.Ordinal);
            Assert.Contains("Text=\"{Binding DataOperationSummary}\"", xaml, StringComparison.Ordinal);
            Assert.Contains("<ProgressBar Width=\"120\" Height=\"14\"", xaml, StringComparison.Ordinal);
            Assert.Contains("Visibility=\"{Binding IsDataOperationBusy, Converter={StaticResource BoolToVis}}\"", xaml, StringComparison.Ordinal);
            Assert.DoesNotContain("Text=\"Data desk ready\"", xaml, StringComparison.Ordinal);
            Assert.DoesNotContain("Text=\"Item import running\"", xaml, StringComparison.Ordinal);
            Assert.DoesNotContain("<Border Grid.Row=\"2\" Width=\"360\"", xaml, StringComparison.Ordinal);
            Assert.DoesNotContain("<UniformGrid Columns=\"2\">", xaml, StringComparison.Ordinal);
            Assert.DoesNotContain("<StackPanel DockPanel.Dock=\"Right\" Orientation=\"Horizontal\">", xaml, StringComparison.Ordinal);
        }

        [Fact]
        public void ImportExportPage_DisablesRunLogActionsThroughSharedReadinessBindings()
        {
            var xaml = ReadRepoFile("InventoryManagementApp", "Views", "Pages", "ImportExportPage.xaml");

            Assert.True(CountOccurrences(xaml, "IsEnabled=\"{Binding CanReviewSelectedLog}\"") >= 6);
            Assert.True(CountOccurrences(xaml, "IsEnabled=\"{Binding CanPrintImportExportLogs}\"") >= 5);
            Assert.Contains("<ContextMenu DataContext=\"{Binding PlacementTarget.DataContext, RelativeSource={RelativeSource Self}}\">", xaml, StringComparison.Ordinal);
            Assert.Contains("<MenuItem Header=\"Open Log Detail\" Click=\"OpenSelectedLog_Click\" IsEnabled=\"{Binding CanReviewSelectedLog}\"/>", xaml, StringComparison.Ordinal);
            Assert.Contains("<MenuItem Header=\"Copy Selected Log\" Click=\"CopySelectedLog_Click\" IsEnabled=\"{Binding CanReviewSelectedLog}\"/>", xaml, StringComparison.Ordinal);
            Assert.Contains("<MenuItem Header=\"Print Current Log\" Click=\"PrintLogs_Click\" IsEnabled=\"{Binding CanPrintImportExportLogs}\"/>", xaml, StringComparison.Ordinal);
        }

        [Fact]
        public void ImportExportPage_ShowsBusyOverlayWithoutCompetingWithEmptyState()
        {
            var xaml = ReadRepoFile("InventoryManagementApp", "Views", "Pages", "ImportExportPage.xaml");

            Assert.Contains("<DataTrigger Binding=\"{Binding IsDataOperationBusy}\" Value=\"True\">\n                                                    <Setter Property=\"Visibility\" Value=\"Collapsed\"/>", xaml, StringComparison.Ordinal);
            Assert.Contains("<Border Grid.Row=\"2\" MaxWidth=\"380\" MinHeight=\"116\" Margin=\"12\"", xaml, StringComparison.Ordinal);
            Assert.Contains("<TextBlock Text=\"Data operation in progress\"", xaml, StringComparison.Ordinal);
            Assert.Contains("<TextBlock Text=\"{Binding DataOperationSummary}\" Style=\"{StaticResource CaptionTextBlock}\"", xaml, StringComparison.Ordinal);
            Assert.Contains("<ProgressBar IsIndeterminate=\"True\" Height=\"6\"", xaml, StringComparison.Ordinal);
        }

        [Fact]
        public void ImportExportViewModel_GatesAllDataOperationsThroughSharedBusyState()
        {
            var viewModel = ReadRepoFile("InventoryManagementApp", "ViewModels", "ImportExportViewModel.cs");

            Assert.Contains("private int _activeDataOperationCount;", viewModel, StringComparison.Ordinal);
            Assert.Contains("public bool IsDataOperationBusy => _activeDataOperationCount > 0;", viewModel, StringComparison.Ordinal);
            Assert.Contains("public bool IsDataOperationReady => !IsDataOperationBusy;", viewModel, StringComparison.Ordinal);
            Assert.Contains("public string DataOperationStatus => IsDataOperationBusy", viewModel, StringComparison.Ordinal);
            Assert.Contains("public string DataOperationSummary => IsDataOperationBusy", viewModel, StringComparison.Ordinal);
            Assert.Contains("AddLog($\"Importing {plural} from {path}...\");", viewModel, StringComparison.Ordinal);
            Assert.Contains("AddLog($\"Importing {plural} from {path} ({importer.FormatName} format)...\");", viewModel, StringComparison.Ordinal);
            Assert.DoesNotContain("await _dialogService.ShowInfoAsync($\"Importing {plural}", viewModel, StringComparison.Ordinal);
            Assert.Contains("bool TryBeginDataOperation(string operationName)", viewModel, StringComparison.Ordinal);
            Assert.Contains("void EndDataOperation()", viewModel, StringComparison.Ordinal);
            Assert.Contains("ImportItemsCommand = new AsyncRelayCommand(ct => ImportItemsAsync(ct), CanStartDataOperation);", viewModel, StringComparison.Ordinal);
            Assert.Contains("ExportItemsCommand = new AsyncRelayCommand(ct => ExportItemsAsync(ct), CanStartDataOperation);", viewModel, StringComparison.Ordinal);
            Assert.Contains("ImportCustomersCommand = new AsyncRelayCommand(ct => ImportCustomersAsync(ct), CanStartDataOperation);", viewModel, StringComparison.Ordinal);
            Assert.Contains("ExportCustomersCommand = new AsyncRelayCommand(ct => ExportCustomersAsync(ct), CanStartDataOperation);", viewModel, StringComparison.Ordinal);
            Assert.Contains("BackupDatabaseCommand = new AsyncRelayCommand(ct => BackupDatabaseAsync(ct), CanStartDataOperation);", viewModel, StringComparison.Ordinal);
            Assert.Contains("RestoreBackupCommand = new AsyncRelayCommand(ct => RestoreBackupAsync(ct), CanStartDataOperation);", viewModel, StringComparison.Ordinal);
        }

        [Fact]
        public void ImportExportViewModel_UpdatesLogActionAvailabilityDuringBusyState()
        {
            var viewModel = ReadRepoFile("InventoryManagementApp", "ViewModels", "ImportExportViewModel.cs");

            Assert.Contains("public bool CanReviewSelectedLog => !IsDataOperationBusy && !string.IsNullOrWhiteSpace(SelectedImportExportLog);", viewModel, StringComparison.Ordinal);
            Assert.Contains("public bool CanPrintImportExportLogs => !IsDataOperationBusy && HasLogEntries;", viewModel, StringComparison.Ordinal);
            Assert.Contains("new RelayCommand(ClearImportExportLogs, () => HasLogEntries && !IsDataOperationBusy)", viewModel, StringComparison.Ordinal);
            Assert.Contains("if (IsDataOperationBusy)", viewModel, StringComparison.Ordinal);
            Assert.Contains("return;", viewModel, StringComparison.Ordinal);
            Assert.Contains("OnPropertyChanged(nameof(CanReviewSelectedLog));", viewModel, StringComparison.Ordinal);
            Assert.Contains("OnPropertyChanged(nameof(CanPrintImportExportLogs));", viewModel, StringComparison.Ordinal);
            Assert.Contains("ClearImportExportLogsCommand.NotifyCanExecuteChanged();", viewModel, StringComparison.Ordinal);
        }

        [Fact]
        public void ImportExportPage_CodeBehindGuardsLogActionsAndCapsPrintRows()
        {
            var codeBehind = ReadRepoFile("InventoryManagementApp", "Views", "Pages", "ImportExportPage.xaml.cs");

            Assert.Contains("private const int MaxPrintedLogRows = 250;", codeBehind, StringComparison.Ordinal);
            Assert.Contains("private bool IsDataOperationBusy() =>", codeBehind, StringComparison.Ordinal);
            Assert.Contains("vm.IsDataOperationBusy", codeBehind, StringComparison.Ordinal);
            Assert.Contains("Wait for the current import, export, backup, or restore operation to finish before opening log details.", codeBehind, StringComparison.Ordinal);
            Assert.Contains("Wait for the current import, export, backup, or restore operation to finish before copying log details.", codeBehind, StringComparison.Ordinal);
            Assert.Contains("Wait for the current import, export, backup, or restore operation to finish before generating a print preview.", codeBehind, StringComparison.Ordinal);
            Assert.Contains("var printedLogs = safeLogs.Take(MaxPrintedLogRows).ToList();", codeBehind, StringComparison.Ordinal);
            Assert.Contains("var omittedLogCount = Math.Max(0, safeLogs.Count - printedLogs.Count);", codeBehind, StringComparison.Ordinal);
            Assert.Contains("Visible Log Rows", codeBehind, StringComparison.Ordinal);
            Assert.Contains("Printed Log Rows", codeBehind, StringComparison.Ordinal);
            Assert.Contains("Omitted Log Rows", codeBehind, StringComparison.Ordinal);
        }

        [Fact]
        public void ImportExportPage_CodeBehindGuardsBusyRowGesturesAndKeyboardShortcuts()
        {
            var codeBehind = ReadRepoFile("InventoryManagementApp", "Views", "Pages", "ImportExportPage.xaml.cs");

            Assert.Contains("PreviewKeyDown += ImportExportPage_PreviewKeyDown;", codeBehind, StringComparison.Ordinal);
            Assert.Contains("private void ImportExportPage_PreviewKeyDown", codeBehind, StringComparison.Ordinal);
            Assert.Contains("if (IsDataOperationBusy())\n            {\n                e.Handled = true;\n                return;\n            }", codeBehind, StringComparison.Ordinal);
            Assert.Contains("RetargetLogSelectionFromEvent(e);", codeBehind, StringComparison.Ordinal);
            Assert.Contains("GridContextMenuSelection.SelectRow(sender, e);", codeBehind, StringComparison.Ordinal);
            Assert.Contains("private static bool IsRunLogShortcut(KeyEventArgs e)", codeBehind, StringComparison.Ordinal);
            Assert.Contains("return e.Key is Key.C or Key.D or Key.P;", codeBehind, StringComparison.Ordinal);
            Assert.Contains("return Keyboard.Modifiers == ModifierKeys.None && e.Key == Key.Delete;", codeBehind, StringComparison.Ordinal);
            Assert.Contains("private static bool IsTextEditingElement(object? source) =>", codeBehind, StringComparison.Ordinal);
            Assert.Contains("source is TextBoxBase or PasswordBox;", codeBehind, StringComparison.Ordinal);
            Assert.Contains("private static T? FindAncestor<T>(DependencyObject? current)", codeBehind, StringComparison.Ordinal);
        }

        [Fact]
        public void ImportExportPage_PreservesDataCommandsAndLogRowHandlers()
        {
            var xaml = ReadRepoFile("InventoryManagementApp", "Views", "Pages", "ImportExportPage.xaml");

            Assert.Contains("ImportItemsCommand", xaml, StringComparison.Ordinal);
            Assert.Contains("ExportItemsCommand", xaml, StringComparison.Ordinal);
            Assert.Contains("ImportCustomersCommand", xaml, StringComparison.Ordinal);
            Assert.Contains("ExportCustomersCommand", xaml, StringComparison.Ordinal);
            Assert.Contains("BackupDatabaseCommand", xaml, StringComparison.Ordinal);
            Assert.Contains("RestoreBackupCommand", xaml, StringComparison.Ordinal);
            Assert.Contains("OpenImageImportMappingWindowCommand", xaml, StringComparison.Ordinal);
            Assert.Contains("ClearImportExportLogsCommand", xaml, StringComparison.Ordinal);
            Assert.Contains("CopySelectedLog_Click", xaml, StringComparison.Ordinal);
            Assert.Contains("PrintLogs_Click", xaml, StringComparison.Ordinal);
            Assert.Contains("OpenSelectedLog_Click", xaml, StringComparison.Ordinal);
            Assert.Contains("ImportExportLogGrid_MouseDoubleClick", xaml, StringComparison.Ordinal);
            Assert.Contains("ImportExportLogRow_PreviewMouseRightButtonDown", xaml, StringComparison.Ordinal);
        }

        private static int CountOccurrences(string text, string value)
        {
            var count = 0;
            var startIndex = 0;
            while (true)
            {
                var index = text.IndexOf(value, startIndex, StringComparison.Ordinal);
                if (index < 0)
                    return count;

                count++;
                startIndex = index + value.Length;
            }
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