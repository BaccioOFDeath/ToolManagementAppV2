using System;
using System.IO;
using Xunit;

namespace InventoryManagementApp.Tests
{
    public class ItemSearchInputResponsivenessContractTests
    {
        [Fact]
        public void ItemSearchPage_FocusesSearchAndPreservesTextEditingKeys()
        {
            var codeBehind = ReadRepoFile("InventoryManagementApp", "Views", "Pages", "ItemSearchPage.xaml.cs");

            Assert.Contains("FocusFirstSearchBox();", codeBehind, StringComparison.Ordinal);
            Assert.Contains("Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.F", codeBehind, StringComparison.Ordinal);
            Assert.Contains("if (IsTextEditingTarget(e.OriginalSource))\n                return;", codeBehind, StringComparison.Ordinal);
            Assert.Contains("FindVisualParent<TextBoxBase>(dependencyObject) != null", codeBehind, StringComparison.Ordinal);
            Assert.Contains("FindVisualParent<PasswordBox>(dependencyObject) != null", codeBehind, StringComparison.Ordinal);
            Assert.Contains("FindVisualParent<ComboBox>(dependencyObject) != null", codeBehind, StringComparison.Ordinal);
            Assert.Contains("searchBox.SelectAll();", codeBehind, StringComparison.Ordinal);
        }

        [Fact]
        public void ItemSearchPage_RetargetsInvokedRowsBeforeOpeningDetailsOrDemandItems()
        {
            var codeBehind = ReadRepoFile("InventoryManagementApp", "Views", "Pages", "ItemSearchPage.xaml.cs");

            Assert.Contains("var item = SelectInvokedItem(grid, e) ?? grid.SelectedItem as ItemModel;", codeBehind, StringComparison.Ordinal);
            Assert.Contains("vm.SelectedItem = item;", codeBehind, StringComparison.Ordinal);
            Assert.Contains("RepeatSearch(SelectInvokedSearchHistory(e) ?? RecentSearchGrid.SelectedItem as SearchHistoryEntry);", codeBehind, StringComparison.Ordinal);
            Assert.Contains("OpenDemandItem(SelectInvokedDemand(e) ?? UnavailableDemandGrid.SelectedItem as UnavailableDemandEntry);", codeBehind, StringComparison.Ordinal);
            Assert.Contains("RecentSearchGrid.SelectedItem = entry;", codeBehind, StringComparison.Ordinal);
            Assert.Contains("UnavailableDemandGrid.SelectedItem = entry;", codeBehind, StringComparison.Ordinal);
            Assert.Contains("FindVisualParent<DataGridRow>(e.OriginalSource as DependencyObject)", codeBehind, StringComparison.Ordinal);
        }

        [Fact]
        public void ItemSearchPage_BlocksBusyMouseAndContextMenuInteraction()
        {
            var codeBehind = ReadRepoFile("InventoryManagementApp", "Views", "Pages", "ItemSearchPage.xaml.cs");

            Assert.Contains("RecentSearchGrid.PreviewMouseRightButtonDown += SearchIntelligenceGrid_PreviewMouseRightButtonDown;", codeBehind, StringComparison.Ordinal);
            Assert.Contains("UnavailableDemandGrid.PreviewMouseRightButtonDown += SearchIntelligenceGrid_PreviewMouseRightButtonDown;", codeBehind, StringComparison.Ordinal);
            Assert.Contains("private void SearchIntelligenceGrid_PreviewMouseRightButtonDown", codeBehind, StringComparison.Ordinal);
            Assert.Contains("GridContextMenuSelection.SelectRow(sender, e);", codeBehind, StringComparison.Ordinal);
            Assert.Contains("e.Handled = true;\n                ShowBusyInfo(\"Wait for the item search to finish before opening item details.\");", codeBehind, StringComparison.Ordinal);
            Assert.Contains("e.Handled = true;\n                ShowBusyInfo(\"Wait for the current search to finish before repeating another search.\");", codeBehind, StringComparison.Ordinal);
            Assert.Contains("e.Handled = true;\n                ShowBusyInfo(\"Wait for the item search to finish before opening unavailable-demand details.\");", codeBehind, StringComparison.Ordinal);
        }

        [Fact]
        public void ItemSearchPage_UsesSharedVisualTreeHelpersForScaledGridSurfaces()
        {
            var codeBehind = ReadRepoFile("InventoryManagementApp", "Views", "Pages", "ItemSearchPage.xaml.cs");

            Assert.Contains("using System.Windows.Controls.Primitives;", codeBehind, StringComparison.Ordinal);
            Assert.Contains("using System.Windows.Media;", codeBehind, StringComparison.Ordinal);
            Assert.Contains("private static T? FindVisualParent<T>(DependencyObject? current) where T : DependencyObject", codeBehind, StringComparison.Ordinal);
            Assert.Contains("current = VisualTreeHelper.GetParent(current);", codeBehind, StringComparison.Ordinal);
            Assert.Contains("private static T? FindVisualChild<T>(DependencyObject parent) where T : DependencyObject", codeBehind, StringComparison.Ordinal);
            Assert.Contains("VisualTreeHelper.GetChildrenCount(parent)", codeBehind, StringComparison.Ordinal);
            Assert.Contains("VisualTreeHelper.GetChild(parent, index)", codeBehind, StringComparison.Ordinal);
        }

        private static string ReadRepoFile(params string[] parts)
        {
            var directory = AppContext.BaseDirectory;

            while (!string.IsNullOrEmpty(directory))
            {
                var candidate = Path.Combine(directory, Path.Combine(parts));
                if (File.Exists(candidate))
                    return File.ReadAllText(candidate);

                var parent = Directory.GetParent(directory);
                if (parent is null)
                    break;

                directory = parent.FullName;
            }

            throw new FileNotFoundException($"Could not find repository file: {Path.Combine(parts)}");
        }
    }
}
