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

            Assert.Contains("<TextBox Width=\"240\" MinWidth=\"190\"", xaml, StringComparison.Ordinal);
            Assert.Contains("<ComboBox Width=\"140\" MinWidth=\"120\"", xaml, StringComparison.Ordinal);
            Assert.Equal(2, CountOccurrences(xaml, "MaxWidth=\"330\" MinHeight=\"120\" Margin=\"12\""));
            Assert.Contains("<ScrollViewer Grid.Row=\"1\" VerticalScrollBarVisibility=\"Auto\" HorizontalScrollBarVisibility=\"Disabled\" Padding=\"12\">", xaml, StringComparison.Ordinal);
            Assert.Contains("<WrapPanel DockPanel.Dock=\"Right\" VerticalAlignment=\"Center\">", xaml, StringComparison.Ordinal);
            Assert.DoesNotContain("<Border Grid.Row=\"2\" Width=\"320\"", xaml, StringComparison.Ordinal);
            Assert.DoesNotContain("<Border Width=\"320\"", xaml, StringComparison.Ordinal);
            Assert.DoesNotContain("VerticalScrollBarVisibility=\"Hidden\"", xaml, StringComparison.Ordinal);
            Assert.DoesNotContain("<StackPanel DockPanel.Dock=\"Right\" Orientation=\"Horizontal\">", xaml, StringComparison.Ordinal);
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
            Assert.Contains("DeleteKitCommand", xaml, StringComparison.Ordinal);
            Assert.Contains("AddKitItemCommand", xaml, StringComparison.Ordinal);
            Assert.Contains("EditKitItemCommand", xaml, StringComparison.Ordinal);
            Assert.Contains("RemoveKitItemCommand", xaml, StringComparison.Ordinal);
            Assert.Contains("ViewKitItemsCommand", xaml, StringComparison.Ordinal);
            Assert.Contains("KitRow_MouseDoubleClick", xaml, StringComparison.Ordinal);
            Assert.Contains("KitItemRow_MouseDoubleClick", xaml, StringComparison.Ordinal);
            Assert.Contains("DataGridRow_PreviewMouseRightButtonDown", xaml, StringComparison.Ordinal);
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
