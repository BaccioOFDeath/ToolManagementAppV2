using System;
using System.IO;
using Xunit;

namespace InventoryManagementApp.Tests
{
    public class RentalHistoryWindowResponsiveContractTests
    {
        [Fact]
        public void RentalHistoryWindow_KeepsSummaryCardsWrappedAndBounded()
        {
            var xaml = ReadRepoFile("InventoryManagementApp", "Views", "Windows", "RentalHistoryWindow.xaml");

            Assert.Contains("Width=\"1040\" Height=\"660\" MinWidth=\"760\" MinHeight=\"520\"", xaml, StringComparison.Ordinal);
            Assert.Contains("x:Name=\"RentalHistoryRoot\" Margin=\"10\" MinWidth=\"0\" ClipToBounds=\"True\"", xaml, StringComparison.Ordinal);
            Assert.Contains("<WrapPanel Grid.Row=\"1\" Margin=\"0,0,0,6\">", xaml, StringComparison.Ordinal);
            Assert.Contains("x:Key=\"RentalHistoryMetricCard\"", xaml, StringComparison.Ordinal);
            Assert.Contains("<Setter Property=\"MinWidth\" Value=\"190\"/>", xaml, StringComparison.Ordinal);
            Assert.Contains("<Setter Property=\"MaxWidth\" Value=\"300\"/>", xaml, StringComparison.Ordinal);
            Assert.Contains("x:Key=\"RentalHistoryMetricValue\"", xaml, StringComparison.Ordinal);
            Assert.Contains("Text=\"{Binding SearchStatus}\" Style=\"{StaticResource CaptionTextBlock}\" TextWrapping=\"Wrap\"", xaml, StringComparison.Ordinal);
            Assert.Contains("Text=\"{Binding ExportSummary}\" Style=\"{StaticResource CaptionTextBlock}\" TextWrapping=\"Wrap\"", xaml, StringComparison.Ordinal);
            Assert.DoesNotContain("<UniformGrid Grid.Row=\"1\" Columns=\"3\"", xaml, StringComparison.Ordinal);
            Assert.DoesNotContain("Width=\"1160\" Height=\"700\" MinWidth=\"940\"", xaml, StringComparison.Ordinal);
        }

        [Fact]
        public void RentalHistoryWindow_WrapsHeaderSearchAndFooterActions()
        {
            var xaml = ReadRepoFile("InventoryManagementApp", "Views", "Windows", "RentalHistoryWindow.xaml");

            Assert.Contains("<ColumnDefinition Width=\"*\" MinWidth=\"0\"/>", xaml, StringComparison.Ordinal);
            Assert.Contains("<WrapPanel Grid.Column=\"1\" HorizontalAlignment=\"Right\" VerticalAlignment=\"Center\"", xaml, StringComparison.Ordinal);
            Assert.Contains("<pages:SearchBar x:Name=\"HistorySearchBar\"", xaml, StringComparison.Ordinal);
            Assert.Contains("Width=\"300\"", xaml, StringComparison.Ordinal);
            Assert.Contains("MinWidth=\"220\"", xaml, StringComparison.Ordinal);
            Assert.Contains("SearchCommand=\"{Binding SearchCommand}\"", xaml, StringComparison.Ordinal);
            Assert.Contains("ClearCommand=\"{Binding ClearSearchCommand}\"", xaml, StringComparison.Ordinal);
            Assert.Contains("Text=\"{Binding SearchStatus}\" Style=\"{StaticResource CaptionTextBlock}\" VerticalAlignment=\"Center\" TextWrapping=\"Wrap\" MaxWidth=\"360\"", xaml, StringComparison.Ordinal);
            Assert.Contains("<WrapPanel Grid.Column=\"1\" HorizontalAlignment=\"Right\">", xaml, StringComparison.Ordinal);
            Assert.DoesNotContain("<StackPanel DockPanel.Dock=\"Right\" Orientation=\"Horizontal\"", xaml, StringComparison.Ordinal);
            Assert.DoesNotContain("<ColumnDefinition Width=\"340\"/>", xaml, StringComparison.Ordinal);
        }

        [Fact]
        public void RentalHistoryWindow_EnablesHistoryGridVirtualizationScrollingAndFullRowSelection()
        {
            var xaml = ReadRepoFile("InventoryManagementApp", "Views", "Windows", "RentalHistoryWindow.xaml");

            Assert.Contains("x:Name=\"RentalHistoryDataGrid\"", xaml, StringComparison.Ordinal);
            Assert.Contains("IsEnabled=\"{Binding IsHistoryActionReady}\"", xaml, StringComparison.Ordinal);
            Assert.Contains("EnableRowVirtualization=\"True\"", xaml, StringComparison.Ordinal);
            Assert.Contains("EnableColumnVirtualization=\"True\"", xaml, StringComparison.Ordinal);
            Assert.Contains("SelectionMode=\"Single\"", xaml, StringComparison.Ordinal);
            Assert.Contains("SelectionUnit=\"FullRow\"", xaml, StringComparison.Ordinal);
            Assert.Contains("ScrollViewer.CanContentScroll=\"True\"", xaml, StringComparison.Ordinal);
            Assert.Contains("ScrollViewer.HorizontalScrollBarVisibility=\"Auto\"", xaml, StringComparison.Ordinal);
            Assert.Contains("ScrollViewer.VerticalScrollBarVisibility=\"Auto\"", xaml, StringComparison.Ordinal);
            Assert.Contains("Header=\"Location\" Binding=\"{Binding ItemLocation}\" Width=\"140\" MinWidth=\"90\"", xaml, StringComparison.Ordinal);
        }

        [Fact]
        public void RentalHistoryWindow_BoundsEmptyBusyAndOmittedRowStates()
        {
            var xaml = ReadRepoFile("InventoryManagementApp", "Views", "Windows", "RentalHistoryWindow.xaml");

            Assert.Contains("Style=\"{StaticResource Card}\" Padding=\"0\" MinWidth=\"0\"", xaml, StringComparison.Ordinal);
            Assert.Contains("<Grid MinWidth=\"0\">", xaml, StringComparison.Ordinal);
            Assert.Contains("<RowDefinition Height=\"Auto\"/>\n                    <RowDefinition Height=\"*\"/>", xaml, StringComparison.Ordinal);
            Assert.Contains("Search and CSV export run off the UI path; row actions pause while work is active.", xaml, StringComparison.Ordinal);
            Assert.Contains("MaxWidth=\"520\"", xaml, StringComparison.Ordinal);
            Assert.Contains("<Border Grid.Row=\"2\" Margin=\"8,0,8,8\" MaxWidth=\"780\" HorizontalAlignment=\"Left\">", xaml, StringComparison.Ordinal);
            Assert.Contains("Binding=\"{Binding HasOmittedHistoryRows}\" Value=\"True\"", xaml, StringComparison.Ordinal);
            Assert.Contains("<Border MaxWidth=\"340\" MinHeight=\"120\" Margin=\"12\"", xaml, StringComparison.Ordinal);
            Assert.Contains("Binding=\"{Binding IsEmptyStateVisible}\" Value=\"True\"", xaml, StringComparison.Ordinal);
            Assert.Contains("Text=\"{Binding EmptyStateTitle}\" Style=\"{StaticResource SectionHeader}\" TextAlignment=\"Center\" TextWrapping=\"Wrap\"", xaml, StringComparison.Ordinal);
            Assert.Contains("Text=\"{Binding EmptyStateMessage}\"", xaml, StringComparison.Ordinal);
            Assert.Contains("<Border MaxWidth=\"340\" Margin=\"12\" HorizontalAlignment=\"Center\" VerticalAlignment=\"Top\" IsHitTestVisible=\"True\">", xaml, StringComparison.Ordinal);
            Assert.Contains("Binding=\"{Binding IsHistoryBusy}\" Value=\"True\"", xaml, StringComparison.Ordinal);
            Assert.Contains("Working on rental history", xaml, StringComparison.Ordinal);
            Assert.Contains("Text=\"{Binding HistoryBusyStatus}\"", xaml, StringComparison.Ordinal);
            Assert.DoesNotContain("Binding=\"{Binding HasNoResults}\" Value=\"True\"", xaml, StringComparison.Ordinal);
            Assert.DoesNotContain("Binding=\"{Binding IsFiltering}\" Value=\"True\"", xaml, StringComparison.Ordinal);
            Assert.DoesNotContain("<Border Width=\"360\" HorizontalAlignment=\"Center\"", xaml, StringComparison.Ordinal);
        }

        [Fact]
        public void RentalHistoryWindow_PreservesHistoryCommandsShortcutsAndRowHandlers()
        {
            var xaml = ReadRepoFile("InventoryManagementApp", "Views", "Windows", "RentalHistoryWindow.xaml");
            var codeBehind = ReadRepoFile("InventoryManagementApp", "Views", "Windows", "RentalHistoryWindow.xaml.cs");

            Assert.Contains("Key=\"D\" Modifiers=\"Control\" Command=\"{Binding OpenDetailsCommand}\"", xaml, StringComparison.Ordinal);
            Assert.Contains("Key=\"E\" Modifiers=\"Control\" Command=\"{Binding ExportCsvCommand}\"", xaml, StringComparison.Ordinal);
            Assert.Contains("Key=\"Escape\" Command=\"{Binding CloseCommand}\"", xaml, StringComparison.Ordinal);
            Assert.Contains("InputGestureText=\"Ctrl+D\"", xaml, StringComparison.Ordinal);
            Assert.Contains("InputGestureText=\"Ctrl+E\"", xaml, StringComparison.Ordinal);
            Assert.Contains("OpenDetailsCommand", xaml, StringComparison.Ordinal);
            Assert.Contains("CloseCommand", xaml, StringComparison.Ordinal);
            Assert.Contains("SearchCommand", xaml, StringComparison.Ordinal);
            Assert.Contains("ClearSearchCommand", xaml, StringComparison.Ordinal);
            Assert.Contains("ExportCsvCommand", xaml, StringComparison.Ordinal);
            Assert.Contains("HistoryRow_MouseDoubleClick", xaml, StringComparison.Ordinal);
            Assert.Contains("HistoryRow_PreviewMouseRightButtonDown", xaml, StringComparison.Ordinal);
            Assert.Contains("Open Details", xaml, StringComparison.Ordinal);
            Assert.Contains("Export Current View", xaml, StringComparison.Ordinal);
            Assert.Contains("ToolTip=\"{Binding ExportSummary}\"", xaml, StringComparison.Ordinal);

            Assert.Contains("PreviewKeyDown += RentalHistoryWindow_PreviewKeyDown;", codeBehind, StringComparison.Ordinal);
            Assert.Contains("Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.F", codeBehind, StringComparison.Ordinal);
            Assert.Contains("HistorySearchBar.Focus();", codeBehind, StringComparison.Ordinal);
            Assert.Contains("Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.D", codeBehind, StringComparison.Ordinal);
            Assert.Contains("Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.E", codeBehind, StringComparison.Ordinal);
            Assert.Contains("Keyboard.Modifiers == ModifierKeys.None && e.Key == Key.Escape", codeBehind, StringComparison.Ordinal);
        }

        [Fact]
        public void RentalHistoryWindow_BlocksStaleRowActionsWhileBusy()
        {
            var xaml = ReadRepoFile("InventoryManagementApp", "Views", "Windows", "RentalHistoryWindow.xaml");
            var codeBehind = ReadRepoFile("InventoryManagementApp", "Views", "Windows", "RentalHistoryWindow.xaml.cs");

            Assert.Contains("IsEnabled=\"{Binding IsHistoryActionReady}\"", xaml, StringComparison.Ordinal);
            Assert.Contains("if (!vm.IsHistoryActionReady && IsRentalHistoryActionShortcut(e))", codeBehind, StringComparison.Ordinal);
            Assert.Contains("private static bool IsRentalHistoryActionShortcut(KeyEventArgs e)", codeBehind, StringComparison.Ordinal);
            Assert.Contains("return e.Key is Key.D or Key.E;", codeBehind, StringComparison.Ordinal);
            Assert.Contains("return Keyboard.Modifiers == ModifierKeys.None && e.Key == Key.Enter;", codeBehind, StringComparison.Ordinal);
            Assert.Contains("if (!vm.IsHistoryActionReady)\n            {\n                e.Handled = true;\n                return;\n            }", codeBehind, StringComparison.Ordinal);
            Assert.Contains("DataContext is RentalHistoryViewModel { IsHistoryActionReady: false }", codeBehind, StringComparison.Ordinal);
            Assert.DoesNotContain("if (vm.IsFiltering && IsRentalHistoryActionShortcut(e))", codeBehind, StringComparison.Ordinal);
            Assert.DoesNotContain("DataContext is RentalHistoryViewModel { IsFiltering: true }", codeBehind, StringComparison.Ordinal);
        }

        [Fact]
        public void RentalHistoryViewModel_CapsVisibleRowsAndReportsOmittedHistory()
        {
            var viewModel = ReadRepoFile("InventoryManagementApp", "ViewModels", "RentalHistoryViewModel.cs");

            Assert.Contains("internal const int MaxVisibleHistoryRows = 500;", viewModel, StringComparison.Ordinal);
            Assert.Contains("private int _matchedHistoryCount;", viewModel, StringComparison.Ordinal);
            Assert.Contains("_matchedHistoryCount = _allHistory.Count;", viewModel, StringComparison.Ordinal);
            Assert.Contains("History = new ObservableCollection<RentalModel>(_allHistory.Take(MaxVisibleHistoryRows).Select(r => r.Rental));", viewModel, StringComparison.Ordinal);
            Assert.Contains("public int OmittedHistoryCount => Math.Max(0, _matchedHistoryCount - History.Count);", viewModel, StringComparison.Ordinal);
            Assert.Contains("public bool HasOmittedHistoryRows => OmittedHistoryCount > 0;", viewModel, StringComparison.Ordinal);
            Assert.Contains("Showing first {History.Count} matches", viewModel, StringComparison.Ordinal);
            Assert.Contains("Export {History.Count} visible record(s); {OmittedHistoryCount} row(s) are omitted", viewModel, StringComparison.Ordinal);
            Assert.Contains("if (visibleRows.Count < MaxVisibleHistoryRows)", viewModel, StringComparison.Ordinal);
            Assert.Contains("return new FilteredHistoryResult(visibleRows, matchedCount);", viewModel, StringComparison.Ordinal);
            Assert.Contains("private sealed record FilteredHistoryResult(IReadOnlyList<RentalModel> VisibleRows, int MatchedCount);", viewModel, StringComparison.Ordinal);
        }

        [Fact]
        public void RentalHistoryViewModel_DisablesActionsAndEmptyStateDuringBusyWork()
        {
            var viewModel = ReadRepoFile("InventoryManagementApp", "ViewModels", "RentalHistoryViewModel.cs");

            Assert.Contains("public bool IsHistoryBusy => IsFiltering || IsExportingCsv;", viewModel, StringComparison.Ordinal);
            Assert.Contains("public bool IsEmptyStateVisible => HasNoResults && !IsHistoryBusy;", viewModel, StringComparison.Ordinal);
            Assert.Contains("public bool CanOpenDetails => SelectedEntry != null && !IsHistoryBusy;", viewModel, StringComparison.Ordinal);
            Assert.Contains("public bool CanExportHistory => History.Count > 0 && !IsHistoryBusy;", viewModel, StringComparison.Ordinal);
            Assert.Contains("public bool CanClearSearch => !IsHistoryBusy && (HasActiveSearch || !string.IsNullOrWhiteSpace(SearchText));", viewModel, StringComparison.Ordinal);
            Assert.Contains("public bool IsHistoryActionReady => !IsHistoryBusy;", viewModel, StringComparison.Ordinal);
            Assert.Contains("public string HistoryBusyStatus => IsExportingCsv", viewModel, StringComparison.Ordinal);
            Assert.Contains("SearchCommand = new AsyncRelayCommand(ExecuteSearchAsync, () => !IsHistoryBusy);", viewModel, StringComparison.Ordinal);
            Assert.Contains("ClearSearchCommand = new RelayCommand(ClearSearch, () => CanClearSearch);", viewModel, StringComparison.Ordinal);
            Assert.Contains("OpenDetailsCommand = new RelayCommand(OpenDetails, () => CanOpenDetails);", viewModel, StringComparison.Ordinal);
            Assert.Contains("if (!CanOpenDetails || SelectedEntry == null)", viewModel, StringComparison.Ordinal);
        }

        [Fact]
        public void RentalHistoryViewModel_ExportsCsvAsynchronouslyWithBusyStateAndSnapshotRows()
        {
            var viewModel = ReadRepoFile("InventoryManagementApp", "ViewModels", "RentalHistoryViewModel.cs");

            Assert.Contains("private bool _isExportingCsv;", viewModel, StringComparison.Ordinal);
            Assert.Contains("public bool IsExportingCsv", viewModel, StringComparison.Ordinal);
            Assert.Contains("public IAsyncRelayCommand ExportCsvCommand { get; }", viewModel, StringComparison.Ordinal);
            Assert.Contains("ExportCsvCommand = new AsyncRelayCommand(ExportCsvAsync, () => CanExportHistory);", viewModel, StringComparison.Ordinal);
            Assert.Contains("async Task ExportCsvAsync()", viewModel, StringComparison.Ordinal);
            Assert.Contains("var visibleRows = History.ToList();", viewModel, StringComparison.Ordinal);
            Assert.Contains("var filteredView = SearchStatus;", viewModel, StringComparison.Ordinal);
            Assert.Contains("IsExportingCsv = true;", viewModel, StringComparison.Ordinal);
            Assert.Contains("var csv = await Task.Run(() => BuildCsv(visibleRows, filteredView));", viewModel, StringComparison.Ordinal);
            Assert.Contains("await File.WriteAllTextAsync(path, csv, new UTF8Encoding(encoderShouldEmitUTF8Identifier: true));", viewModel, StringComparison.Ordinal);
            Assert.Contains("finally\n            {\n                IsExportingCsv = false;\n            }", viewModel, StringComparison.Ordinal);
            Assert.Contains("static string BuildCsv(IReadOnlyList<RentalModel> rows, string filteredView)", viewModel, StringComparison.Ordinal);
            Assert.DoesNotContain("void ExportCsv()", viewModel, StringComparison.Ordinal);
            Assert.DoesNotContain("foreach (var r in History)", viewModel, StringComparison.Ordinal);
        }

        [Fact]
        public void RentalHistoryViewModel_NotifiesCommandsAndStatusForBusyTransitions()
        {
            var viewModel = ReadRepoFile("InventoryManagementApp", "ViewModels", "RentalHistoryViewModel.cs");

            Assert.Contains("void NotifyBusyStateChanged()", viewModel, StringComparison.Ordinal);
            Assert.Contains("SearchCommand.NotifyCanExecuteChanged();", viewModel, StringComparison.Ordinal);
            Assert.Contains("ClearSearchCommand.NotifyCanExecuteChanged();", viewModel, StringComparison.Ordinal);
            Assert.Contains("OpenDetailsCommand.NotifyCanExecuteChanged();", viewModel, StringComparison.Ordinal);
            Assert.Contains("ExportCsvCommand.NotifyCanExecuteChanged();", viewModel, StringComparison.Ordinal);
            Assert.Contains("OnPropertyChanged(nameof(IsHistoryBusy));", viewModel, StringComparison.Ordinal);
            Assert.Contains("OnPropertyChanged(nameof(HistoryBusyStatus));", viewModel, StringComparison.Ordinal);
            Assert.Contains("OnPropertyChanged(nameof(IsHistoryActionReady));", viewModel, StringComparison.Ordinal);
            Assert.Contains("OnPropertyChanged(nameof(ExportSummary));", viewModel, StringComparison.Ordinal);
            Assert.Contains("SearchCommand.NotifyCanExecuteChanged();\n            ClearSearchCommand.NotifyCanExecuteChanged();\n            OpenDetailsCommand.NotifyCanExecuteChanged();\n            ExportCsvCommand.NotifyCanExecuteChanged();", viewModel, StringComparison.Ordinal);
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
        static string NormalizeLineEndings(string text)
            => text.Replace("\r\n", "\n");

    }
}
