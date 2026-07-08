using System;
using System.IO;
using Xunit;

namespace InventoryManagementApp.Tests
{
    public class KitManagementPageResponsiveContractTests
    {
        [Fact]
        public void KitManagementPage_KeepsKitSummaryCardsWrappedAndBounded()
        {
            var xaml = ReadRepoFile("InventoryManagementApp", "Views", "Pages", "KitManagementPage.xaml");

            Assert.Contains("<WrapPanel Grid.Column=\"2\" HorizontalAlignment=\"Right\">", xaml, StringComparison.Ordinal);
            Assert.Contains("<Setter Property=\"MinWidth\" Value=\"150\"/>", xaml, StringComparison.Ordinal);
            Assert.Contains("<Setter Property=\"MaxWidth\" Value=\"235\"/>", xaml, StringComparison.Ordinal);
            Assert.Contains("KitStatValueText", xaml, StringComparison.Ordinal);
            Assert.Contains("<ColumnDefinition Width=\"1.15*\" MinWidth=\"0\"/>", xaml, StringComparison.Ordinal);
            Assert.Contains("{Binding KitFilterSummary}", xaml, StringComparison.Ordinal);
            Assert.Contains("{Binding KitPrintSummary}", xaml, StringComparison.Ordinal);
            Assert.DoesNotContain("<UniformGrid Grid.Column=\"2\" Columns=\"4\">", xaml, StringComparison.Ordinal);
            Assert.DoesNotContain("<ColumnDefinition Width=\"2*\" MinWidth=\"380\"/>", xaml, StringComparison.Ordinal);
            Assert.DoesNotContain("<ColumnDefinition Width=\"3*\" MinWidth=\"520\"/>", xaml, StringComparison.Ordinal);
        }

        [Fact]
        public void KitManagementPage_AvoidsLargeFixedMinimumsInMainKitSplit()
        {
            var xaml = ReadRepoFile("InventoryManagementApp", "Views", "Pages", "KitManagementPage.xaml");

            Assert.Contains("<ColumnDefinition Width=\"1.65*\" MinWidth=\"0\"/>", xaml, StringComparison.Ordinal);
            Assert.Contains("<ColumnDefinition Width=\"0.95*\" MinWidth=\"300\"/>", xaml, StringComparison.Ordinal);
            Assert.Contains("<GridSplitter Grid.Row=\"0\" Grid.RowSpan=\"3\" Grid.Column=\"1\" Width=\"6\"", xaml, StringComparison.Ordinal);
            Assert.Contains("<Border Grid.Row=\"0\" Grid.Column=\"0\" Style=\"{StaticResource Card}\" Padding=\"0\" MinWidth=\"0\">", xaml, StringComparison.Ordinal);
            Assert.Contains("<Border Grid.Row=\"2\" Grid.Column=\"0\" Style=\"{StaticResource Card}\" Padding=\"0\" MinWidth=\"0\">", xaml, StringComparison.Ordinal);
            Assert.Contains("<Border Grid.Row=\"0\" Grid.RowSpan=\"3\" Grid.Column=\"2\" Style=\"{StaticResource Card}\" Padding=\"0\" MinWidth=\"0\">", xaml, StringComparison.Ordinal);
            Assert.DoesNotContain("<ColumnDefinition Width=\"2.05*\" MinWidth=\"620\"/>", xaml, StringComparison.Ordinal);
            Assert.DoesNotContain("<ColumnDefinition Width=\"440\" MinWidth=\"380\"/>", xaml, StringComparison.Ordinal);
        }

        [Fact]
        public void KitManagementPage_EnablesKitGridsVirtualizationScrollingAndFullRowSelection()
        {
            var xaml = ReadRepoFile("InventoryManagementApp", "Views", "Pages", "KitManagementPage.xaml");
            var gridNames = new[] { "KitsGrid", "KitItemsGrid" };

            foreach (var gridName in gridNames)
                Assert.Contains($"x:Name=\"{gridName}\"", xaml, StringComparison.Ordinal);

            Assert.Equal(gridNames.Length, CountOccurrences(xaml, "EnableRowVirtualization=\"True\""));
            Assert.Equal(gridNames.Length, CountOccurrences(xaml, "EnableColumnVirtualization=\"True\""));
            Assert.Equal(gridNames.Length, CountOccurrences(xaml, "SelectionUnit=\"FullRow\""));
            Assert.Equal(gridNames.Length, CountOccurrences(xaml, "ScrollViewer.CanContentScroll=\"True\""));
            Assert.Equal(gridNames.Length, CountOccurrences(xaml, "ScrollViewer.HorizontalScrollBarVisibility=\"Auto\""));
            Assert.Equal(gridNames.Length, CountOccurrences(xaml, "ScrollViewer.VerticalScrollBarVisibility=\"Auto\""));
        }

        [Fact]
        public void KitManagementPage_BoundsInputsEmptyStatesAndHandoffScrolling()
        {
            var xaml = ReadRepoFile("InventoryManagementApp", "Views", "Pages", "KitManagementPage.xaml");

            Assert.Contains("<TextBox x:Name=\"SearchTextBox\" Width=\"240\" MinWidth=\"190\"", xaml, StringComparison.Ordinal);
            Assert.Contains("<ComboBox Width=\"140\" MinWidth=\"120\"", xaml, StringComparison.Ordinal);
            Assert.Equal(2, CountOccurrences(xaml, "MaxWidth=\"330\" MinHeight=\"120\" Margin=\"12\""));
            Assert.Contains("Visibility=\"{Binding IsKitDirectoryEmptyVisible, Converter={StaticResource BoolToVis}}\"", xaml, StringComparison.Ordinal);
            Assert.Contains("Visibility=\"{Binding IsKitItemsEmptyVisible, Converter={StaticResource BoolToVis}}\"", xaml, StringComparison.Ordinal);
            Assert.Contains("{Binding KitEmptyStateTitle}", xaml, StringComparison.Ordinal);
            Assert.Contains("{Binding KitItemsEmptyStateMessage}", xaml, StringComparison.Ordinal);
            Assert.Contains("<ScrollViewer Grid.Row=\"1\" VerticalScrollBarVisibility=\"Auto\" HorizontalScrollBarVisibility=\"Disabled\" Padding=\"12\">", xaml, StringComparison.Ordinal);
            Assert.Contains("<WrapPanel DockPanel.Dock=\"Right\" VerticalAlignment=\"Center\">", xaml, StringComparison.Ordinal);
            Assert.DoesNotContain("<Border Grid.Row=\"2\" Width=\"320\"", xaml, StringComparison.Ordinal);
            Assert.DoesNotContain("<Border Width=\"320\"", xaml, StringComparison.Ordinal);
            Assert.DoesNotContain("VerticalScrollBarVisibility=\"Hidden\"", xaml, StringComparison.Ordinal);
            Assert.DoesNotContain("<StackPanel DockPanel.Dock=\"Right\" Orientation=\"Horizontal\">", xaml, StringComparison.Ordinal);
        }

        [Fact]
        public void KitManagementPage_ShowsBoundedLoadingOverlaysForDirectoryAndMembership()
        {
            var xaml = ReadRepoFile("InventoryManagementApp", "Views", "Pages", "KitManagementPage.xaml");

            Assert.Contains("Visibility=\"{Binding IsLoadingKits, Converter={StaticResource BoolToVis}}\"", xaml, StringComparison.Ordinal);
            Assert.Contains("Visibility=\"{Binding IsLoadingKitItems, Converter={StaticResource BoolToVis}}\"", xaml, StringComparison.Ordinal);
            Assert.Equal(2, CountOccurrences(xaml, "MaxWidth=\"360\" MinHeight=\"118\" Margin=\"12\""));
            Assert.Equal(2, CountOccurrences(xaml, "<ProgressBar IsIndeterminate=\"True\" Height=\"6\""));
            Assert.Contains("Loading kit rows", xaml, StringComparison.Ordinal);
            Assert.Contains("Loading kit membership", xaml, StringComparison.Ordinal);
            Assert.Contains("{Binding KitItemLoadSummary}", xaml, StringComparison.Ordinal);
        }

        [Fact]
        public void KitManagementPage_ShowsSelectedKitOutputSummariesInHandoffPane()
        {
            var xaml = ReadRepoFile("InventoryManagementApp", "Views", "Pages", "KitManagementPage.xaml");

            Assert.Contains("Handoff output", xaml, StringComparison.Ordinal);
            Assert.Contains("Print output", xaml, StringComparison.Ordinal);
            Assert.Contains("{Binding SelectedKitHandoffSummary}", xaml, StringComparison.Ordinal);
            Assert.Contains("{Binding SelectedKitPrintSummary}", xaml, StringComparison.Ordinal);
            Assert.Contains("<ScrollViewer Grid.Row=\"1\" VerticalScrollBarVisibility=\"Auto\" HorizontalScrollBarVisibility=\"Disabled\" Padding=\"12\">", xaml, StringComparison.Ordinal);
        }

        [Fact]
        public void KitManagementPage_PreservesPrimaryKitActionsAndRowHandoff()
        {
            var xaml = ReadRepoFile("InventoryManagementApp", "Views", "Pages", "KitManagementPage.xaml");

            Assert.Contains("AddKitCommand", xaml, StringComparison.Ordinal);
            Assert.Contains("OpenKitDetailsCommand", xaml, StringComparison.Ordinal);
            Assert.Contains("EditKitCommand", xaml, StringComparison.Ordinal);
            Assert.Contains("CheckAvailabilityCommand", xaml, StringComparison.Ordinal);
            Assert.Contains("CopySelectedKitCommand", xaml, StringComparison.Ordinal);
            Assert.Contains("PrintSelectedKitCommand", xaml, StringComparison.Ordinal);
            Assert.Contains("PrintKitListCommand", xaml, StringComparison.Ordinal);
            Assert.Contains("IsEnabled=\"{Binding IsKitDirectoryPrintAvailable}\"", xaml, StringComparison.Ordinal);
            Assert.Contains("DeleteKitCommand", xaml, StringComparison.Ordinal);
            Assert.Contains("AddKitItemCommand", xaml, StringComparison.Ordinal);
            Assert.Contains("EditKitItemCommand", xaml, StringComparison.Ordinal);
            Assert.Contains("RemoveKitItemCommand", xaml, StringComparison.Ordinal);
            Assert.Contains("ViewKitItemsCommand", xaml, StringComparison.Ordinal);
            Assert.Contains("KitRow_MouseDoubleClick", xaml, StringComparison.Ordinal);
            Assert.Contains("KitItemRow_MouseDoubleClick", xaml, StringComparison.Ordinal);
            Assert.Contains("DataGridRow_PreviewMouseRightButtonDown", xaml, StringComparison.Ordinal);
        }

        [Fact]
        public void KitManagementViewModel_GuardsLoadingCommandsAndStaleMembershipRefreshes()
        {
            var source = ReadRepoFile("InventoryManagementApp", "ViewModels", "KitManagementViewModel.cs");

            Assert.Contains("private const int MaxDirectoryPrintRows = 250;", source, StringComparison.Ordinal);
            Assert.Contains("public bool IsLoadingKits", source, StringComparison.Ordinal);
            Assert.Contains("public bool IsLoadingKitItems", source, StringComparison.Ordinal);
            Assert.Contains("public bool IsKitInteractionBusy => IsLoadingKits;", source, StringComparison.Ordinal);
            Assert.Contains("public bool IsKitItemInteractionBusy => IsLoadingKits || IsLoadingKitItems;", source, StringComparison.Ordinal);
            Assert.Contains("if (IsKitInteractionBusy)", source, StringComparison.Ordinal);
            Assert.Contains("LoadKitsCommand = new AsyncRelayCommand(LoadKitsAsync, () => !IsKitInteractionBusy);", source, StringComparison.Ordinal);
            Assert.Contains("RefreshCommand = new AsyncRelayCommand(LoadKitsAsync, () => !IsKitInteractionBusy);", source, StringComparison.Ordinal);
            Assert.Contains("ClearSearchCommand = new RelayCommand(ClearSearch, () => !IsKitInteractionBusy", source, StringComparison.Ordinal);
            Assert.Contains("var loadVersion = ++_kitItemLoadVersion;", source, StringComparison.Ordinal);
            Assert.Contains("loadVersion != _kitItemLoadVersion || SelectedKit?.KitID != kitID", source, StringComparison.Ordinal);
            Assert.Contains("if (loadVersion == _kitItemLoadVersion)", source, StringComparison.Ordinal);
        }

        [Fact]
        public void KitManagementViewModel_CapsAndDescribesPrintPreviewOutput()
        {
            var source = ReadRepoFile("InventoryManagementApp", "ViewModels", "KitManagementViewModel.cs");

            Assert.Contains("var printedKits = visibleKits.Take(MaxDirectoryPrintRows).ToList();", source, StringComparison.Ordinal);
            Assert.Contains("var omittedCount = Math.Max(0, FullFilteredKitCount - printedKits.Count);", source, StringComparison.Ordinal);
            Assert.Contains("Matched {FullFilteredKitCount} | Grid window {visibleKits.Count} | Printed {printedKits.Count} | Omitted {omittedCount}", source, StringComparison.Ordinal);
            Assert.Contains("Large filtered directories print the first 250 matching rows to keep preview responsive.", source, StringComparison.Ordinal);
            Assert.Contains("new GridLength(1.15, GridUnitType.Star)", source, StringComparison.Ordinal);
            Assert.Contains("new GridLength(2.25, GridUnitType.Star)", source, StringComparison.Ordinal);
            Assert.DoesNotContain("new GridLength(120)", source, StringComparison.Ordinal);
            Assert.DoesNotContain("new GridLength(230)", source, StringComparison.Ordinal);
        }

        [Fact]
        public void KitManagementViewModel_CapsSelectedKitHandoffAndPrintOutput()
        {
            var source = ReadRepoFile("InventoryManagementApp", "ViewModels", "KitManagementViewModel.cs");

            Assert.Contains("private const int MaxSelectedKitHandoffRows = 100;", source, StringComparison.Ordinal);
            Assert.Contains("private const int MaxSelectedKitPrintRows = 250;", source, StringComparison.Ordinal);
            Assert.Contains("public string SelectedKitHandoffSummary", source, StringComparison.Ordinal);
            Assert.Contains("public string SelectedKitPrintSummary", source, StringComparison.Ordinal);
            Assert.Contains("var handoffItems = KitItems.Take(MaxSelectedKitHandoffRows).ToList();", source, StringComparison.Ordinal);
            Assert.Contains("additional item line", source, StringComparison.Ordinal);
            Assert.Contains("var printedItems = visibleItems.Take(MaxSelectedKitPrintRows).ToList();", source, StringComparison.Ordinal);
            Assert.Contains("Item lines {visibleItems.Count} | Printed {printedItems.Count} | Omitted {omittedCount}", source, StringComparison.Ordinal);
            Assert.Contains("Large kit membership: printing the first {printedItems.Count} item lines", source, StringComparison.Ordinal);
            Assert.Contains("_dialogService.ShowPrintPreview(doc, $\"Kit {kit.KitNumber}\", SelectedKitPrintSummary);", source, StringComparison.Ordinal);
            Assert.Contains("OnPropertyChanged(nameof(SelectedKitHandoffSummary));", source, StringComparison.Ordinal);
            Assert.Contains("OnPropertyChanged(nameof(SelectedKitPrintSummary));", source, StringComparison.Ordinal);
        }

        [Fact]
        public void KitManagementPage_CodeBehindUsesFirstPaintLoadGuard()
        {
            var source = ReadRepoFile("InventoryManagementApp", "Views", "Pages", "KitManagementPage.xaml.cs");

            Assert.Contains("private KitManagementViewModel? _loadedViewModel;", source, StringComparison.Ordinal);
            Assert.Contains("private Task? _loadKitsTask;", source, StringComparison.Ordinal);
            Assert.Contains("DataContextChanged += KitManagementPage_DataContextChanged;", source, StringComparison.Ordinal);
            Assert.Contains("LoadKitsOnceForViewModelAsync", source, StringComparison.Ordinal);
            Assert.Contains("SearchTextBox.Focus();", source, StringComparison.Ordinal);
            Assert.Contains("await Dispatcher.InvokeAsync(() => { }, DispatcherPriority.Background);", source, StringComparison.Ordinal);
            Assert.Contains("vm.LoadKitsCommand.CanExecute(null)", source, StringComparison.Ordinal);
            Assert.Contains("ReferenceEquals(currentVm, vm)", source, StringComparison.Ordinal);
        }

        [Fact]
        public void KitManagementPage_CodeBehindGuardsBusyActionsAndRetargetsInvokedRows()
        {
            var source = ReadRepoFile("InventoryManagementApp", "Views", "Pages", "KitManagementPage.xaml.cs");

            Assert.Contains("if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.F)", source, StringComparison.Ordinal);
            Assert.Contains("SearchTextBox.SelectAll();", source, StringComparison.Ordinal);
            Assert.Contains("if (vm.IsKitItemInteractionBusy && IsManagedKitShortcut(e))", source, StringComparison.Ordinal);
            Assert.Contains("private static bool IsManagedKitShortcut(KeyEventArgs e)", source, StringComparison.Ordinal);
            Assert.Contains("Keyboard.Modifiers;", source, StringComparison.Ordinal);
            Assert.Contains("vm.AddKitCommand.CanExecute(null)", source, StringComparison.Ordinal);
            Assert.Contains("vm.EditKitCommand.CanExecute(null)", source, StringComparison.Ordinal);
            Assert.Contains("vm.AddKitItemCommand.CanExecute(null)", source, StringComparison.Ordinal);
            Assert.Contains("vm.EditKitItemCommand.CanExecute(null)", source, StringComparison.Ordinal);
            Assert.Contains("vm.CopySelectedKitCommand.CanExecute(null)", source, StringComparison.Ordinal);
            Assert.Contains("vm.DeleteKitCommand.CanExecute(null)", source, StringComparison.Ordinal);
            Assert.Contains("if (vm.IsKitItemInteractionBusy)", source, StringComparison.Ordinal);
            Assert.Contains("sender is FrameworkElement { DataContext: Kit kit }", source, StringComparison.Ordinal);
            Assert.Contains("vm.SelectedKit = kit;", source, StringComparison.Ordinal);
            Assert.Contains("sender is FrameworkElement { DataContext: KitItem kitItem }", source, StringComparison.Ordinal);
            Assert.Contains("vm.SelectedKitItem = kitItem;", source, StringComparison.Ordinal);
            Assert.Contains("DataContext is KitManagementViewModel { IsKitItemInteractionBusy: true }", source, StringComparison.Ordinal);
            Assert.Contains("GridContextMenuSelection.SelectRow(sender, e);", source, StringComparison.Ordinal);
        }

        [Fact]
        public void KitManagementPage_SuppressesGridContextMenusDuringLoading()
        {
            var source = ReadRepoFile("InventoryManagementApp", "Views", "Pages", "KitManagementPage.xaml.cs");
            var helper = ExtractSourceBlock(source, "private bool SuppressContextMenuDuringLoading", "private static bool IsTextInputFocused");

            Assert.Contains("KitsGrid.ContextMenuOpening += KitsGrid_ContextMenuOpening;", source, StringComparison.Ordinal);
            Assert.Contains("KitItemsGrid.ContextMenuOpening += KitItemsGrid_ContextMenuOpening;", source, StringComparison.Ordinal);
            Assert.Contains("private void KitsGrid_ContextMenuOpening(object sender, ContextMenuEventArgs e)", source, StringComparison.Ordinal);
            Assert.Contains("private void KitItemsGrid_ContextMenuOpening(object sender, ContextMenuEventArgs e)", source, StringComparison.Ordinal);
            Assert.Equal(2, CountOccurrences(source, "SuppressContextMenuDuringLoading(e);"));
            Assert.Contains("if (DataContext is KitManagementViewModel { IsKitItemInteractionBusy: true })", helper, StringComparison.Ordinal);
            Assert.Contains("e.Handled = true;", helper, StringComparison.Ordinal);
            Assert.Contains("return true;", helper, StringComparison.Ordinal);
            Assert.Contains("return false;", helper, StringComparison.Ordinal);
        }

        [Fact]
        public void KitManagementPage_PreservesSearchAndFilterEditingBeforeShortcutsDispatch()
        {
            var source = ReadRepoFile("InventoryManagementApp", "Views", "Pages", "KitManagementPage.xaml.cs");
            var keyDown = ExtractSourceBlock(source, "private void KitManagementPage_PreviewKeyDown", "private static bool IsManagedKitShortcut");

            Assert.Contains("using System.Windows.Controls.Primitives;", source, StringComparison.Ordinal);
            Assert.Contains("private static bool IsTextInputFocused()", source, StringComparison.Ordinal);
            Assert.Contains("Keyboard.FocusedElement is TextBoxBase or PasswordBox or ComboBox", source, StringComparison.Ordinal);
            Assert.Contains("if (IsTextInputFocused() && IsManagedKitShortcut(e))", keyDown, StringComparison.Ordinal);
            Assert.Contains("return;", keyDown, StringComparison.Ordinal);
            Assert.Contains("Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.F", keyDown, StringComparison.Ordinal);
            Assert.True(
                keyDown.IndexOf("Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.F", StringComparison.Ordinal) <
                keyDown.IndexOf("if (IsTextInputFocused() && IsManagedKitShortcut(e))", StringComparison.Ordinal),
                "Ctrl+F should keep focusing search before text-entry shortcuts are preserved.");
            Assert.True(
                keyDown.IndexOf("if (IsTextInputFocused() && IsManagedKitShortcut(e))", StringComparison.Ordinal) <
                keyDown.IndexOf("Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.N", StringComparison.Ordinal),
                "Text-entry guard should run before kit action shortcuts dispatch.");
        }

        [Fact]
        public void KitManagementPage_HandlesUnavailableDoubleClicksAfterRetargetingRows()
        {
            var source = ReadRepoFile("InventoryManagementApp", "Views", "Pages", "KitManagementPage.xaml.cs");
            var kitDoubleClick = ExtractSourceBlock(source, "private void KitRow_MouseDoubleClick", "private void KitItemRow_MouseDoubleClick");
            var itemDoubleClick = ExtractSourceBlock(source, "private void KitItemRow_MouseDoubleClick", "private void DataGridRow_PreviewMouseRightButtonDown");

            Assert.Contains("vm.SelectedKit = kit;", kitDoubleClick, StringComparison.Ordinal);
            Assert.Contains("e.Handled = true;\n                return;", kitDoubleClick, StringComparison.Ordinal);
            Assert.Contains("e.Handled = true;\n        }", kitDoubleClick, StringComparison.Ordinal);
            Assert.Contains("vm.SelectedKitItem = kitItem;", itemDoubleClick, StringComparison.Ordinal);
            Assert.Contains("e.Handled = true;\n                return;", itemDoubleClick, StringComparison.Ordinal);
            Assert.Contains("e.Handled = true;\n        }", itemDoubleClick, StringComparison.Ordinal);
        }

        private static int CountOccurrences(string text, string value)
        {
            var count = 0;
            var index = 0;

            while ((index = text.IndexOf(value, index, StringComparison.Ordinal)) >= 0)
            {
                count++;
                index += value.Length;
            }

            return count;
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

        private static string ExtractSourceBlock(string source, string startMarker, string endMarker)
        {
            var start = source.IndexOf(startMarker, StringComparison.Ordinal);
            Assert.True(start >= 0, $"Could not find start marker: {startMarker}");

            var end = source.IndexOf(endMarker, start, StringComparison.Ordinal);
            Assert.True(end > start, $"Could not find end marker after {startMarker}: {endMarker}");

            return source[start..end];
        }

        private static string NormalizeLineEndings(string text)
            => text.Replace("\r\n", "\n");
    }
}
