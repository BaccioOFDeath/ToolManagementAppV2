using System;
using System.IO;
using Xunit;

namespace InventoryManagementApp.Tests
{
    public class ItemSearchPageResponsiveContractTests
    {
        [Fact]
        public void ItemSearchPage_WrapsSearchToolbarAndSummaryActions()
        {
            var xaml = NormalizeNewlines(ReadRepoFile("InventoryManagementApp", "Views", "Pages", "ItemSearchPage.xaml"));

            Assert.Contains("<DockPanel LastChildFill=\"True\">", xaml, StringComparison.Ordinal);
            Assert.Contains("<WrapPanel DockPanel.Dock=\"Left\" VerticalAlignment=\"Center\">", xaml, StringComparison.Ordinal);
            Assert.Contains("<ComboBox Width=\"150\" MinWidth=\"120\"", xaml, StringComparison.Ordinal);
            Assert.Contains("<WrapPanel DockPanel.Dock=\"Right\" Orientation=\"Horizontal\"", xaml, StringComparison.Ordinal);
            Assert.Contains("<WrapPanel VerticalAlignment=\"Center\" MinWidth=\"0\">", xaml, StringComparison.Ordinal);
            Assert.DoesNotContain("Text=\"Find item\"", xaml, StringComparison.Ordinal);
            Assert.DoesNotContain("Text=\"{Binding SearchText, UpdateSourceTrigger=PropertyChanged}\"", xaml, StringComparison.Ordinal);
            Assert.DoesNotContain("<ColumnDefinition Width=\"320\"/>", xaml, StringComparison.Ordinal);
            Assert.DoesNotContain("<ColumnDefinition Width=\"180\"/>", xaml, StringComparison.Ordinal);
            Assert.Contains("Source=\"{Binding IsAsync=True, Converter={StaticResource NullToDefaultImageConverter}, ConverterParameter=item}\"", xaml, StringComparison.Ordinal);
        }

        [Fact]
        public void ItemSearchPage_AvoidsLargeFixedMinimumsInMainSearchSplit()
        {
            var xaml = NormalizeNewlines(ReadRepoFile("InventoryManagementApp", "Views", "Pages", "ItemSearchPage.xaml"));

            Assert.Contains("<ColumnDefinition Width=\"1.7*\" MinWidth=\"0\"/>", xaml, StringComparison.Ordinal);
            Assert.Contains("<ColumnDefinition Width=\"0.95*\" MinWidth=\"300\"/>", xaml, StringComparison.Ordinal);
            Assert.Contains("<GridSplitter Grid.Column=\"1\" Width=\"5\"", xaml, StringComparison.Ordinal);
            Assert.Contains("<Border Grid.Column=\"0\" Style=\"{StaticResource Card}\" Padding=\"0\" MinWidth=\"0\">", xaml, StringComparison.Ordinal);
            Assert.Contains("<Grid Grid.Column=\"2\" MinWidth=\"0\">", xaml, StringComparison.Ordinal);
            Assert.DoesNotContain("<ColumnDefinition Width=\"2*\" MinWidth=\"560\"/>", xaml, StringComparison.Ordinal);
            Assert.DoesNotContain("<ColumnDefinition Width=\"1.25*\" MinWidth=\"430\"/>", xaml, StringComparison.Ordinal);
        }

        [Fact]
        public void ItemSearchPage_WrapsPaneHeadersAndIntelligenceActionCards()
        {
            var xaml = NormalizeNewlines(ReadRepoFile("InventoryManagementApp", "Views", "Pages", "ItemSearchPage.xaml"));

            Assert.Contains("<StackPanel DockPanel.Dock=\"Left\" VerticalAlignment=\"Center\" MinWidth=\"180\" MaxWidth=\"420\">", xaml, StringComparison.Ordinal);
            Assert.Contains("<StackPanel DockPanel.Dock=\"Left\" VerticalAlignment=\"Center\" MinWidth=\"170\" MaxWidth=\"360\">", xaml, StringComparison.Ordinal);
            Assert.Contains("<StackPanel DockPanel.Dock=\"Left\" VerticalAlignment=\"Center\" MinWidth=\"180\" MaxWidth=\"360\">", xaml, StringComparison.Ordinal);
            Assert.Contains("<Setter Property=\"MinWidth\" Value=\"145\"/>", xaml, StringComparison.Ordinal);
            Assert.Contains("<Setter Property=\"MaxWidth\" Value=\"230\"/>", xaml, StringComparison.Ordinal);
            Assert.Contains("<WrapPanel>\n                                <Border Style=\"{StaticResource InsightStatCard}\">", xaml, StringComparison.Ordinal);
            Assert.DoesNotContain("<Grid.ColumnDefinitions>\n                                    <ColumnDefinition Width=\"1.7*\"/>", xaml, StringComparison.Ordinal);
        }

        [Fact]
        public void ItemSearchPage_EnablesAllWorkbenchGridsVirtualizationScrollingAndFullRowSelection()
        {
            var xaml = ReadRepoFile("InventoryManagementApp", "Views", "Pages", "ItemSearchPage.xaml");

            Assert.Contains("x:Name=\"ResultsGrid\"", xaml, StringComparison.Ordinal);
            Assert.Contains("x:Name=\"CheckedOutGrid\"", xaml, StringComparison.Ordinal);
            Assert.Contains("x:Name=\"RecentSearchGrid\"", xaml, StringComparison.Ordinal);
            Assert.Contains("x:Name=\"UnavailableDemandGrid\"", xaml, StringComparison.Ordinal);
            Assert.Equal(4, CountOccurrences(xaml, "EnableRowVirtualization=\"True\""));
            Assert.Equal(4, CountOccurrences(xaml, "EnableColumnVirtualization=\"True\""));
            Assert.Equal(4, CountOccurrences(xaml, "SelectionMode=\"Single\""));
            Assert.Equal(4, CountOccurrences(xaml, "SelectionUnit=\"FullRow\""));
            Assert.Equal(4, CountOccurrences(xaml, "ScrollViewer.CanContentScroll=\"True\""));
            Assert.Equal(4, CountOccurrences(xaml, "ScrollViewer.HorizontalScrollBarVisibility=\"Auto\""));
            Assert.Equal(4, CountOccurrences(xaml, "ScrollViewer.VerticalScrollBarVisibility=\"Auto\""));
        }

        [Fact]
        public void ItemSearchPage_ReducesOversizedGridColumnsAndSidePanePressure()
        {
            var xaml = ReadRepoFile("InventoryManagementApp", "Views", "Pages", "ItemSearchPage.xaml");

            Assert.Contains("<RowDefinition Height=\"1.15*\" MinHeight=\"160\"/>", xaml, StringComparison.Ordinal);
            Assert.Contains("<RowDefinition Height=\"*\" MinHeight=\"200\"/>", xaml, StringComparison.Ordinal);
            Assert.Contains("<GridSplitter Grid.Row=\"1\" Height=\"5\"", xaml, StringComparison.Ordinal);
            Assert.Contains("Header=\"Photo\" Width=\"66\"", xaml, StringComparison.Ordinal);
            Assert.Contains("Header=\"Status\" Width=\"104\"", xaml, StringComparison.Ordinal);
            Assert.Contains("Header=\"Stock\" Width=\"86\"", xaml, StringComparison.Ordinal);
            Assert.Contains("Header=\"Activity\" Width=\"128\"", xaml, StringComparison.Ordinal);
            Assert.Contains("Header=\"Out Since\" Width=\"104\"", xaml, StringComparison.Ordinal);
            Assert.DoesNotContain("<RowDefinition Height=\"1.25*\" MinHeight=\"190\"/>", xaml, StringComparison.Ordinal);
            Assert.DoesNotContain("<RowDefinition Height=\"*\" MinHeight=\"230\"/>", xaml, StringComparison.Ordinal);
        }

        [Fact]
        public void ItemSearchPage_BoundsSearchIntelligenceRefreshWork()
        {
            var codeBehind = ReadRepoFile("InventoryManagementApp", "Views", "Pages", "ItemSearchPage.xaml.cs");

            Assert.Contains("private const int SearchHistoryLimit = 10;", codeBehind, StringComparison.Ordinal);
            Assert.Contains("private const int UnavailableDemandLimit = 12;", codeBehind, StringComparison.Ordinal);
            Assert.Contains("private const int SearchSignatureItemLimit = 250;", codeBehind, StringComparison.Ordinal);
            Assert.Contains("var snapshot = CreateSearchSnapshot(_attachedViewModel.SearchResults);", codeBehind, StringComparison.Ordinal);
            Assert.Contains("if (resultIds.Count < SearchSignatureItemLimit)", codeBehind, StringComparison.Ordinal);
            Assert.Contains("if (unavailableItems.Count < UnavailableDemandLimit", codeBehind, StringComparison.Ordinal);
            Assert.Contains(".Take(UnavailableDemandLimit)", codeBehind, StringComparison.Ordinal);
            Assert.Contains("while (_searchHistory.Count > SearchHistoryLimit)", codeBehind, StringComparison.Ordinal);
            Assert.DoesNotContain("var results = _attachedViewModel.SearchResults.ToList();", codeBehind, StringComparison.Ordinal);
            Assert.DoesNotContain("var unavailable = results.Where(IsUnavailable).ToList();", codeBehind, StringComparison.Ordinal);
            Assert.DoesNotContain("results.Select(item => item.ItemID).OrderBy", codeBehind, StringComparison.Ordinal);
            Assert.DoesNotContain("unavailable.Select(item => item.ItemID).OrderBy", codeBehind, StringComparison.Ordinal);
            Assert.DoesNotContain("unavailableItems.GroupBy", codeBehind, StringComparison.Ordinal);
            Assert.DoesNotContain(".Take(12)", codeBehind, StringComparison.Ordinal);
            Assert.DoesNotContain("_searchHistory.Count > 10", codeBehind, StringComparison.Ordinal);
        }

        [Fact]
        public void ItemSearchPage_GuardsStartupLoadAndBusyActionPaths()
        {
            var codeBehind = ReadRepoFile("InventoryManagementApp", "Views", "Pages", "ItemSearchPage.xaml.cs");

            Assert.Contains("private ItemManagementViewModel? _loadedSearchViewModel;", codeBehind, StringComparison.Ordinal);
            Assert.Contains("private bool _hasLoadedSearchForViewModel;", codeBehind, StringComparison.Ordinal);
            Assert.Contains("DataContextChanged += ItemSearchPage_DataContextChanged;", codeBehind, StringComparison.Ordinal);
            Assert.Contains("ReferenceEquals(_loadedSearchViewModel, vm) && _hasLoadedSearchForViewModel", codeBehind, StringComparison.Ordinal);
            Assert.Contains("await Dispatcher.Yield(DispatcherPriority.Background);", codeBehind, StringComparison.Ordinal);
            Assert.Contains("if (!string.IsNullOrWhiteSpace(vm.SearchText) && vm.SearchResults.Count > 0)", codeBehind, StringComparison.Ordinal);
            Assert.Contains("if (!string.IsNullOrWhiteSpace(vm.SearchText))", codeBehind, StringComparison.Ordinal);
            Assert.Contains("if (!vm.SearchCommand.IsRunning && vm.SearchCommand.CanExecute(null))", codeBehind, StringComparison.Ordinal);
            Assert.Contains("_ = vm.SearchImmediatelyAsync(entry.Term);", codeBehind, StringComparison.Ordinal);
            Assert.Contains("FocusShellSearchBox();", codeBehind, StringComparison.Ordinal);
            Assert.Contains("FindName(\"ShellSearchBar\") is SearchBar shellSearchBar", codeBehind, StringComparison.Ordinal);
            Assert.Contains("shellSearchBar.FocusInput(selectAll: false);", codeBehind, StringComparison.Ordinal);
            Assert.Contains("private static bool IsSearchBusy(ItemManagementViewModel vm)", codeBehind, StringComparison.Ordinal);
            Assert.Contains("return vm.SearchCommand.IsRunning;", codeBehind, StringComparison.Ordinal);
            Assert.Contains("Wait for the item search to finish before opening item details.", codeBehind, StringComparison.Ordinal);
            Assert.Contains("Wait for the current search to finish before repeating another search.", codeBehind, StringComparison.Ordinal);
            Assert.Contains("Wait for the item search to finish before opening unavailable-demand details.", codeBehind, StringComparison.Ordinal);
            Assert.Contains("Wait for the item search to finish before clearing session intelligence.", codeBehind, StringComparison.Ordinal);
            Assert.Contains("Wait for the item search to finish before printing search intelligence.", codeBehind, StringComparison.Ordinal);
            Assert.Contains("Wait for the item search to finish before opening print preview.", codeBehind, StringComparison.Ordinal);
            Assert.Contains("if (DataContext is ItemManagementViewModel vm && IsSearchBusy(vm))\n            {\n                e.Handled = true;", codeBehind, StringComparison.Ordinal);
        }

        [Fact]
        public void ItemSearchPage_BoundsPrintPreviewRowsAndShowsOmittedAccounting()
        {
            var codeBehind = ReadRepoFile("InventoryManagementApp", "Views", "Pages", "ItemSearchPage.xaml.cs");

            Assert.Contains("private const int MaxItemPrintRows = 250;", codeBehind, StringComparison.Ordinal);
            Assert.Contains("var itemList = items.Take(MaxItemPrintRows).ToList();", codeBehind, StringComparison.Ordinal);
            Assert.Contains("var omittedCount = Math.Max(0, totalCount - itemList.Count);", codeBehind, StringComparison.Ordinal);
            Assert.Contains("BuildPrintDocument(title, itemList, totalCount, omittedCount)", codeBehind, StringComparison.Ordinal);
            Assert.Contains("BuildCheckedOutPrintDocument(title, items, totalCount, omittedCount)", codeBehind, StringComparison.Ordinal);
            Assert.Contains("showing {items.Count} of {totalCount} row(s); {omittedCount} omitted", codeBehind, StringComparison.Ordinal);
            Assert.Contains("showing {items.Count} of {totalCount} checked-out item(s); {omittedCount} omitted", codeBehind, StringComparison.Ordinal);
            Assert.Contains("Large result sets print the first 250 rows so preview stays responsive.", codeBehind, StringComparison.Ordinal);
            Assert.Contains("Large checked-out lists print the first 250 rows so preview stays responsive.", codeBehind, StringComparison.Ordinal);
            Assert.Contains("Review row count, omitted-row guidance, item status, holder, stock, and location details before printing.", codeBehind, StringComparison.Ordinal);
        }

        [Fact]
        public void ItemSearchPage_PreservesSearchActionsAndRowHandlers()
        {
            var xaml = ReadRepoFile("InventoryManagementApp", "Views", "Pages", "ItemSearchPage.xaml");

            Assert.Contains("ViewDetailsCommand", xaml, StringComparison.Ordinal);
            Assert.Contains("OpenRentalsCommand", xaml, StringComparison.Ordinal);
            Assert.Contains("ToggleCheckOutCommand", xaml, StringComparison.Ordinal);
            Assert.Contains("PrintSearchResults_Click", xaml, StringComparison.Ordinal);
            Assert.Contains("PrintCheckedOut_Click", xaml, StringComparison.Ordinal);
            Assert.Contains("RepeatSelectedSearch_Click", xaml, StringComparison.Ordinal);
            Assert.Contains("OpenDemandItem_Click", xaml, StringComparison.Ordinal);
            Assert.Contains("PrintSearchIntelligence_Click", xaml, StringComparison.Ordinal);
            Assert.Contains("ClearSearchIntelligence_Click", xaml, StringComparison.Ordinal);
            Assert.Contains("ItemGrid_MouseDoubleClick", xaml, StringComparison.Ordinal);
            Assert.Contains("RecentSearchGrid_MouseDoubleClick", xaml, StringComparison.Ordinal);
            Assert.Contains("UnavailableDemandGrid_MouseDoubleClick", xaml, StringComparison.Ordinal);
        }

        private static int CountOccurrences(string source, string value)
        {
            var count = 0;
            var index = 0;

            while ((index = source.IndexOf(value, index, StringComparison.Ordinal)) >= 0)
            {
                count++;
                index += value.Length;
            }

            return count;
        }

        private static string NormalizeNewlines(string source) => source.Replace("\r\n", "\n", StringComparison.Ordinal);

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
