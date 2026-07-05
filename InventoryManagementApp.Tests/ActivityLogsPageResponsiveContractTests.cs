using System;
using System.IO;
using Xunit;

namespace InventoryManagementApp.Tests
{
    public class ActivityLogsPageResponsiveContractTests
    {
        [Fact]
        public void ActivityLogsPage_KeepsAuditSummaryCardsWrappedAndBounded()
        {
            var xaml = ReadRepoFile("InventoryManagementApp", "Views", "Pages", "ActivityLogsPage.xaml");

            Assert.Contains("<WrapPanel Grid.Column=\"2\" HorizontalAlignment=\"Right\">", xaml, StringComparison.Ordinal);
            Assert.Contains("<Setter Property=\"MinWidth\" Value=\"150\"/>", xaml, StringComparison.Ordinal);
            Assert.Contains("<Setter Property=\"MaxWidth\" Value=\"230\"/>", xaml, StringComparison.Ordinal);
            Assert.DoesNotContain("<UniformGrid Grid.Column=\"2\" Columns=\"4\">", xaml, StringComparison.Ordinal);
        }

        [Fact]
        public void ActivityLogsPage_AvoidsLargeFixedMinimumsInMainAuditSplit()
        {
            var xaml = ReadRepoFile("InventoryManagementApp", "Views", "Pages", "ActivityLogsPage.xaml");

            Assert.Contains("<ColumnDefinition Width=\"1.55*\" MinWidth=\"0\"/>", xaml, StringComparison.Ordinal);
            Assert.Contains("<ColumnDefinition Width=\"0.95*\" MinWidth=\"300\"/>", xaml, StringComparison.Ordinal);
            Assert.Contains("<GridSplitter Grid.Column=\"1\" Width=\"6\"", xaml, StringComparison.Ordinal);
            Assert.DoesNotContain("<ColumnDefinition Width=\"2.5*\" MinWidth=\"620\"/>", xaml, StringComparison.Ordinal);
            Assert.DoesNotContain("<ColumnDefinition Width=\"430\" MinWidth=\"380\"/>", xaml, StringComparison.Ordinal);
        }

        [Fact]
        public void ActivityLogsPage_EnablesGridVirtualizationScrollingAndFullRowSelection()
        {
            var xaml = ReadRepoFile("InventoryManagementApp", "Views", "Pages", "ActivityLogsPage.xaml");

            Assert.Contains("EnableRowVirtualization=\"True\"", xaml, StringComparison.Ordinal);
            Assert.Contains("EnableColumnVirtualization=\"True\"", xaml, StringComparison.Ordinal);
            Assert.Contains("SelectionMode=\"Single\"", xaml, StringComparison.Ordinal);
            Assert.Contains("SelectionUnit=\"FullRow\"", xaml, StringComparison.Ordinal);
            Assert.Contains("ScrollViewer.CanContentScroll=\"True\"", xaml, StringComparison.Ordinal);
            Assert.Contains("ScrollViewer.HorizontalScrollBarVisibility=\"Auto\"", xaml, StringComparison.Ordinal);
            Assert.Contains("ScrollViewer.VerticalScrollBarVisibility=\"Auto\"", xaml, StringComparison.Ordinal);
        }

        [Fact]
        public void ActivityLogsPage_BoundsEmptyStateAndHandoffTextInsteadOfForcingPageWidthOrHeight()
        {
            var xaml = ReadRepoFile("InventoryManagementApp", "Views", "Pages", "ActivityLogsPage.xaml");

            Assert.Contains("<Border Grid.Row=\"2\" MaxWidth=\"360\" Margin=\"12\"", xaml, StringComparison.Ordinal);
            Assert.Contains("MinHeight=\"130\"", xaml, StringComparison.Ordinal);
            Assert.Contains("MaxHeight=\"260\"", xaml, StringComparison.Ordinal);
            Assert.Contains("HorizontalScrollBarVisibility=\"Disabled\"", xaml, StringComparison.Ordinal);
            Assert.DoesNotContain("<Border Grid.Row=\"2\" Width=\"360\"", xaml, StringComparison.Ordinal);
            Assert.DoesNotContain("MinHeight=\"150\"", xaml, StringComparison.Ordinal);
        }

        [Fact]
        public void ActivityLogsPage_PreservesPrimaryAuditActionsAndContextMenuHandoff()
        {
            var xaml = ReadRepoFile("InventoryManagementApp", "Views", "Pages", "ActivityLogsPage.xaml");

            Assert.Contains("OpenRelatedPage_Click", xaml, StringComparison.Ordinal);
            Assert.Contains("OpenSelectedLog_Click", xaml, StringComparison.Ordinal);
            Assert.Contains("CopySelectedLog_Click", xaml, StringComparison.Ordinal);
            Assert.Contains("PrintLogs_Click", xaml, StringComparison.Ordinal);
            Assert.Contains("ActivityGridRow_PreviewMouseRightButtonDown", xaml, StringComparison.Ordinal);
            Assert.Contains("SelectedLogHandoff", xaml, StringComparison.Ordinal);
        }

        [Fact]
        public void ActivityLogsPage_DisablesRiskyActionsWhileRowsAreBusy()
        {
            var xaml = ReadRepoFile("InventoryManagementApp", "Views", "Pages", "ActivityLogsPage.xaml");

            Assert.Contains("IsEnabled=\"{Binding CanUseSelectedLogActions}\"", xaml, StringComparison.Ordinal);
            Assert.Contains("IsEnabled=\"{Binding CanPrintActivityRows}\"", xaml, StringComparison.Ordinal);
            Assert.Contains("IsEnabled=\"{Binding CanChangeActivityFilters}\"", xaml, StringComparison.Ordinal);
            Assert.Contains("IsEnabled=\"{Binding CanRefreshActivityRows}\"", xaml, StringComparison.Ordinal);
            Assert.Contains("<TextBlock Text=\"{Binding PrintStatusText}\"", xaml, StringComparison.Ordinal);
            Assert.Contains("DataContext=\"{Binding PlacementTarget.DataContext, RelativeSource={RelativeSource Self}}\"", xaml, StringComparison.Ordinal);
        }

        [Fact]
        public void ActivityLogsPage_ShowsDynamicBusyAndEmptyStates()
        {
            var xaml = ReadRepoFile("InventoryManagementApp", "Views", "Pages", "ActivityLogsPage.xaml");

            Assert.Contains("CanShowActivityEmptyState", xaml, StringComparison.Ordinal);
            Assert.Contains("ActivityEmptyStateTitle", xaml, StringComparison.Ordinal);
            Assert.Contains("ActivityEmptyStateMessage", xaml, StringComparison.Ordinal);
            Assert.Contains("<DataTrigger Binding=\"{Binding IsBusy}\" Value=\"True\">", xaml, StringComparison.Ordinal);
            Assert.Contains("<ProgressBar IsIndeterminate=\"True\"", xaml, StringComparison.Ordinal);
            Assert.Contains("ActivityBusyMessage", xaml, StringComparison.Ordinal);
        }

        [Fact]
        public void ActivityLogsPage_CodeBehindKeepsFirstPaintAndBusyGuards()
        {
            var codeBehind = ReadRepoFile("InventoryManagementApp", "Views", "Pages", "ActivityLogsPage.xaml.cs");

            Assert.Contains("await Dispatcher.Yield(DispatcherPriority.Background);", codeBehind, StringComparison.Ordinal);
            Assert.Contains("DataContextChanged += ActivityLogsPage_DataContextChanged;", codeBehind, StringComparison.Ordinal);
            Assert.Contains("ReferenceEquals(_loadedViewModel, vm)", codeBehind, StringComparison.Ordinal);
            Assert.Contains("ActivitySearchBox.Focus();", codeBehind, StringComparison.Ordinal);
            Assert.Contains("!vm.RefreshCommand.CanExecute(null)", codeBehind, StringComparison.Ordinal);
            Assert.Contains("DataContext is ActivityLogsViewModel { IsBusy: true }", codeBehind, StringComparison.Ordinal);
            Assert.Contains("if (vm.IsBusy)", codeBehind, StringComparison.Ordinal);
            Assert.Contains("vm.PrintStatusText", codeBehind, StringComparison.Ordinal);
        }

        [Fact]
        public void ActivityLogsPage_CodeBehindRetargetsRowsAndSuppressesBusyGestures()
        {
            var codeBehind = ReadRepoFile("InventoryManagementApp", "Views", "Pages", "ActivityLogsPage.xaml.cs");

            Assert.Contains("private bool IsActivityDirectoryBusy() =>", codeBehind, StringComparison.Ordinal);
            Assert.Contains("if (IsActivityDirectoryBusy())\n            {\n                e.Handled = true;\n                return;\n            }", codeBehind, StringComparison.Ordinal);
            Assert.Contains("RetargetActivitySelectionFromEvent(e);", codeBehind, StringComparison.Ordinal);
            Assert.Contains("ActivityGrid.SelectedItem = log;", codeBehind, StringComparison.Ordinal);
            Assert.Contains("ActivityGrid.ScrollIntoView(log);", codeBehind, StringComparison.Ordinal);
            Assert.Contains("vm.SelectedLog = log;", codeBehind, StringComparison.Ordinal);
            Assert.Contains("GridContextMenuSelection.SelectRow(sender, e);", codeBehind, StringComparison.Ordinal);
            Assert.Contains("private static T? FindAncestor<T>(DependencyObject? current)", codeBehind, StringComparison.Ordinal);
            Assert.Contains("System.Windows.Media.VisualTreeHelper.GetParent(current) ?? LogicalTreeHelper.GetParent(current)", codeBehind, StringComparison.Ordinal);
        }

        [Fact]
        public void ActivityLogsPage_CodeBehindAddsKeyboardShortcutsWithoutBreakingTextEditing()
        {
            var codeBehind = ReadRepoFile("InventoryManagementApp", "Views", "Pages", "ActivityLogsPage.xaml.cs");

            Assert.Contains("PreviewKeyDown += ActivityLogsPage_PreviewKeyDown;", codeBehind, StringComparison.Ordinal);
            Assert.Contains("private void ActivityLogsPage_PreviewKeyDown", codeBehind, StringComparison.Ordinal);
            Assert.Contains("ActivitySearchBox.SelectAll();", codeBehind, StringComparison.Ordinal);
            Assert.Contains("RefreshLogs_Click(sender, e);", codeBehind, StringComparison.Ordinal);
            Assert.Contains("OpenRelatedPage_Click(sender, e);", codeBehind, StringComparison.Ordinal);
            Assert.Contains("CopySelectedLog_Click(sender, e);", codeBehind, StringComparison.Ordinal);
            Assert.Contains("PrintLogs_Click(sender, e);", codeBehind, StringComparison.Ordinal);
            Assert.Contains("OpenSelectedLog_Click(sender, e);", codeBehind, StringComparison.Ordinal);
            Assert.Contains("private static bool IsActivityLogShortcut(KeyEventArgs e)", codeBehind, StringComparison.Ordinal);
            Assert.Contains("return e.Key is Key.R or Key.O or Key.D or Key.C or Key.P;", codeBehind, StringComparison.Ordinal);
            Assert.Contains("return Keyboard.Modifiers == ModifierKeys.None && e.Key == Key.Enter;", codeBehind, StringComparison.Ordinal);
            Assert.Contains("private static bool IsTextEditingElement(object? source)", codeBehind, StringComparison.Ordinal);
            Assert.Contains("return source is TextBox or ComboBox;", codeBehind, StringComparison.Ordinal);
        }

        [Fact]
        public void ActivityLogsViewModel_CoalescesFilteringAndKeepsRowsDuringRefreshFailure()
        {
            var viewModel = ReadRepoFile("InventoryManagementApp", "ViewModels", "ActivityLogsViewModel.cs");

            Assert.Contains("const int FilterDebounceMilliseconds = 160;", viewModel, StringComparison.Ordinal);
            Assert.Contains("CancellationTokenSource? _filterRefreshCts", viewModel, StringComparison.Ordinal);
            Assert.Contains("Interlocked.Exchange(ref _filterRefreshCts, cts)", viewModel, StringComparison.Ordinal);
            Assert.Contains("await Task.Delay(FilterDebounceMilliseconds, cts.Token);", viewModel, StringComparison.Ordinal);
            Assert.Contains("await Task.Run(() => rows.Where", viewModel, StringComparison.Ordinal);
            Assert.Contains("PreserveActivityLogRowsAfterLoadFailure", viewModel, StringComparison.Ordinal);
            Assert.DoesNotContain("ClearActivityLogRowsAfterLoadFailure", viewModel, StringComparison.Ordinal);
        }

        [Fact]
        public void ActivityLogsViewModel_ExposesProfessionalDisplayAndPrintState()
        {
            var viewModel = ReadRepoFile("InventoryManagementApp", "ViewModels", "ActivityLogsViewModel.cs");

            Assert.Contains("public bool IsBusy => IsLoading || IsFiltering;", viewModel, StringComparison.Ordinal);
            Assert.Contains("public bool CanRefreshActivityRows => !IsBusy;", viewModel, StringComparison.Ordinal);
            Assert.Contains("public bool CanPrintActivityRows => !IsBusy && FilteredLogs.Count > 0;", viewModel, StringComparison.Ordinal);
            Assert.Contains("public bool CanUseSelectedLogActions => !IsBusy && SelectedLog != null;", viewModel, StringComparison.Ordinal);
            Assert.Contains("RefreshCommand = new AsyncRelayCommand(LoadLogsAsync, () => CanRefreshActivityRows);", viewModel, StringComparison.Ordinal);
            Assert.Contains("if (IsBusy)\n                return false;", viewModel, StringComparison.Ordinal);
            Assert.Contains("RefreshCommand.NotifyCanExecuteChanged();", viewModel, StringComparison.Ordinal);
            Assert.Contains("OnPropertyChanged(nameof(CanRefreshActivityRows));", viewModel, StringComparison.Ordinal);
            Assert.Contains("public string ActivityEmptyStateTitle", viewModel, StringComparison.Ordinal);
            Assert.Contains("public string ActivityEmptyStateMessage", viewModel, StringComparison.Ordinal);
            Assert.Contains("public string PrintStatusText", viewModel, StringComparison.Ordinal);
            Assert.Contains("log.UserID.ToString().Contains(search, StringComparison.OrdinalIgnoreCase)", viewModel, StringComparison.Ordinal);
            Assert.Contains("OrderByDescending(log => log.Timestamp)", viewModel, StringComparison.Ordinal);
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
