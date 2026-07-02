using System;
using System.IO;
using Xunit;

namespace InventoryManagementApp.Tests
{
    public class ManageItemsPageResponsiveContractTests
    {
        [Fact]
        public void ManageItemsPage_KeepsDirectorySummaryCardsWrappedAndBounded()
        {
            var xaml = ReadRepoFile("InventoryManagementApp", "Views", "Pages", "ManageItemsPage.xaml");

            Assert.Contains("<WrapPanel Grid.Row=\"1\" Margin=\"0,0,0,5\">", xaml, StringComparison.Ordinal);
            Assert.Contains("<Setter Property=\"MinWidth\" Value=\"150\"/>", xaml, StringComparison.Ordinal);
            Assert.Contains("<Setter Property=\"MaxWidth\" Value=\"230\"/>", xaml, StringComparison.Ordinal);
            Assert.Contains("DirectoryStatValueText", xaml, StringComparison.Ordinal);
            Assert.DoesNotContain("<Grid Grid.Row=\"1\" Margin=\"0,0,0,5\">", xaml, StringComparison.Ordinal);
            Assert.DoesNotContain("<ColumnDefinition Width=\"1.25*\"/>", xaml, StringComparison.Ordinal);
        }

        [Fact]
        public void ManageItemsPage_WrapsHeaderActionsAndFilterControls()
        {
            var xaml = ReadRepoFile("InventoryManagementApp", "Views", "Pages", "ManageItemsPage.xaml");

            Assert.Contains("<StackPanel DockPanel.Dock=\"Left\" VerticalAlignment=\"Center\" MinWidth=\"220\" MaxWidth=\"460\">", xaml, StringComparison.Ordinal);
            Assert.Contains("<WrapPanel DockPanel.Dock=\"Right\" VerticalAlignment=\"Center\" HorizontalAlignment=\"Right\" Margin=\"12,0,0,0\">", xaml, StringComparison.Ordinal);
            Assert.Contains("<WrapPanel DockPanel.Dock=\"Left\" VerticalAlignment=\"Center\">", xaml, StringComparison.Ordinal);
            Assert.Contains("<WrapPanel DockPanel.Dock=\"Right\" VerticalAlignment=\"Center\" HorizontalAlignment=\"Right\" Margin=\"12,0,0,0\">", xaml, StringComparison.Ordinal);
            Assert.Contains("<pages:SearchBar Width=\"240\"", xaml, StringComparison.Ordinal);
            Assert.Contains("MinWidth=\"180\"", xaml, StringComparison.Ordinal);
            Assert.DoesNotContain("<StackPanel DockPanel.Dock=\"Right\" Orientation=\"Horizontal\"", xaml, StringComparison.Ordinal);
            Assert.DoesNotContain("<StackPanel DockPanel.Dock=\"Left\" Orientation=\"Horizontal\"", xaml, StringComparison.Ordinal);
        }

        [Fact]
        public void ManageItemsPage_AvoidsLargeFixedMinimumsInMainItemSplit()
        {
            var xaml = ReadRepoFile("InventoryManagementApp", "Views", "Pages", "ManageItemsPage.xaml");

            Assert.Contains("<ColumnDefinition Width=\"1.7*\" MinWidth=\"0\"/>", xaml, StringComparison.Ordinal);
            Assert.Contains("<ColumnDefinition Width=\"0.95*\" MinWidth=\"300\"/>", xaml, StringComparison.Ordinal);
            Assert.Contains("<Border Grid.Column=\"0\" Style=\"{StaticResource Card}\" Padding=\"0\" MinWidth=\"0\">", xaml, StringComparison.Ordinal);
            Assert.Contains("<Border Grid.Column=\"2\" Style=\"{StaticResource Card}\" Padding=\"0\" MinWidth=\"0\">", xaml, StringComparison.Ordinal);
            Assert.DoesNotContain("<ColumnDefinition Width=\"3.4*\" MinWidth=\"620\"/>", xaml, StringComparison.Ordinal);
            Assert.DoesNotContain("<ColumnDefinition Width=\"1*\" MinWidth=\"250\"/>", xaml, StringComparison.Ordinal);
        }

        [Fact]
        public void ManageItemsPage_EnablesDirectoryGridVirtualizationScrollingAndFullRowSelection()
        {
            var xaml = ReadRepoFile("InventoryManagementApp", "Views", "Pages", "ManageItemsPage.xaml");

            Assert.Contains("x:Name=\"ItemDirectoryGrid\"", xaml, StringComparison.Ordinal);
            Assert.Contains("EnableRowVirtualization=\"True\"", xaml, StringComparison.Ordinal);
            Assert.Contains("EnableColumnVirtualization=\"True\"", xaml, StringComparison.Ordinal);
            Assert.Contains("SelectionMode=\"Extended\"", xaml, StringComparison.Ordinal);
            Assert.Contains("SelectionUnit=\"FullRow\"", xaml, StringComparison.Ordinal);
            Assert.Contains("ScrollViewer.CanContentScroll=\"True\"", xaml, StringComparison.Ordinal);
            Assert.Contains("ScrollViewer.HorizontalScrollBarVisibility=\"Auto\"", xaml, StringComparison.Ordinal);
            Assert.Contains("ScrollViewer.VerticalScrollBarVisibility=\"Auto\"", xaml, StringComparison.Ordinal);
        }

        [Fact]
        public void ManageItemsPage_BoundsEmptyStateHandoffScrollingAndFooterStatus()
        {
            var xaml = ReadRepoFile("InventoryManagementApp", "Views", "Pages", "ManageItemsPage.xaml");

            Assert.Contains("<Border Grid.Row=\"2\" MaxWidth=\"320\" MinHeight=\"120\" Margin=\"12\"", xaml, StringComparison.Ordinal);
            Assert.Contains("<ScrollViewer VerticalScrollBarVisibility=\"Auto\" HorizontalScrollBarVisibility=\"Disabled\">", xaml, StringComparison.Ordinal);
            Assert.Contains("<WrapPanel>", xaml, StringComparison.Ordinal);
            Assert.DoesNotContain("<Border Grid.Row=\"2\" Width=\"300\"", xaml, StringComparison.Ordinal);
            Assert.DoesNotContain("VerticalScrollBarVisibility=\"Hidden\"", xaml, StringComparison.Ordinal);
            Assert.DoesNotContain("<DockPanel LastChildFill=\"False\">\n                <TextBlock DockPanel.Dock=\"Left\" Text=\"{Binding PendingEdits.Count", xaml, StringComparison.Ordinal);
        }

        [Fact]
        public void ManageItemsPage_PreservesPrimaryItemCommandsAndRowHandlers()
        {
            var xaml = ReadRepoFile("InventoryManagementApp", "Views", "Pages", "ManageItemsPage.xaml");

            Assert.Contains("NewItemCommand", xaml, StringComparison.Ordinal);
            Assert.Contains("OpenMobileCaptureCommand", xaml, StringComparison.Ordinal);
            Assert.Contains("EditItemCommand", xaml, StringComparison.Ordinal);
            Assert.Contains("ViewDetailsCommand", xaml, StringComparison.Ordinal);
            Assert.Contains("OpenRentalHistoryCommand", xaml, StringComparison.Ordinal);
            Assert.Contains("DeleteSelectedItemCommand", xaml, StringComparison.Ordinal);
            Assert.Contains("CommitChangesCommand", xaml, StringComparison.Ordinal);
            Assert.Contains("SearchCommand", xaml, StringComparison.Ordinal);
            Assert.Contains("DataGridRow_PreviewMouseRightButtonDown", xaml, StringComparison.Ordinal);
            Assert.Contains("DataGridRow_MouseDoubleClick", xaml, StringComparison.Ordinal);
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
