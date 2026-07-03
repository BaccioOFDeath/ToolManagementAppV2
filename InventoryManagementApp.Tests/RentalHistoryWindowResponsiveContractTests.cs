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
            Assert.Contains("<WrapPanel Grid.Row=\"1\" Margin=\"0,0,0,6\">", xaml, StringComparison.Ordinal);
            Assert.Contains("x:Key=\"RentalHistoryMetricCard\"", xaml, StringComparison.Ordinal);
            Assert.Contains("<Setter Property=\"MinWidth\" Value=\"190\"/>", xaml, StringComparison.Ordinal);
            Assert.Contains("<Setter Property=\"MaxWidth\" Value=\"300\"/>", xaml, StringComparison.Ordinal);
            Assert.Contains("x:Key=\"RentalHistoryMetricValue\"", xaml, StringComparison.Ordinal);
            Assert.DoesNotContain("<UniformGrid Grid.Row=\"1\" Columns=\"3\"", xaml, StringComparison.Ordinal);
            Assert.DoesNotContain("Width=\"1160\" Height=\"700\" MinWidth=\"940\"", xaml, StringComparison.Ordinal);
        }

        [Fact]
        public void RentalHistoryWindow_WrapsHeaderSearchAndFooterActions()
        {
            var xaml = ReadRepoFile("InventoryManagementApp", "Views", "Windows", "RentalHistoryWindow.xaml");

            Assert.Contains("<ColumnDefinition Width=\"*\" MinWidth=\"0\"/>", xaml, StringComparison.Ordinal);
            Assert.Contains("<WrapPanel Grid.Column=\"1\" HorizontalAlignment=\"Right\" VerticalAlignment=\"Center\"", xaml, StringComparison.Ordinal);
            Assert.Contains("<pages:SearchBar Width=\"300\"", xaml, StringComparison.Ordinal);
            Assert.Contains("MinWidth=\"220\"", xaml, StringComparison.Ordinal);
            Assert.Contains("MaxWidth=\"360\"", xaml, StringComparison.Ordinal);
            Assert.Contains("<WrapPanel Grid.Column=\"1\" HorizontalAlignment=\"Right\">", xaml, StringComparison.Ordinal);
            Assert.DoesNotContain("<StackPanel DockPanel.Dock=\"Right\" Orientation=\"Horizontal\"", xaml, StringComparison.Ordinal);
            Assert.DoesNotContain("<ColumnDefinition Width=\"340\"/>", xaml, StringComparison.Ordinal);
        }

        [Fact]
        public void RentalHistoryWindow_EnablesHistoryGridVirtualizationScrollingAndFullRowSelection()
        {
            var xaml = ReadRepoFile("InventoryManagementApp", "Views", "Windows", "RentalHistoryWindow.xaml");

            Assert.Contains("x:Name=\"RentalHistoryDataGrid\"", xaml, StringComparison.Ordinal);
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
        public void RentalHistoryWindow_BoundsEmptyStateAndPaneText()
        {
            var xaml = ReadRepoFile("InventoryManagementApp", "Views", "Windows", "RentalHistoryWindow.xaml");

            Assert.Contains("Style=\"{StaticResource Card}\" Padding=\"0\" MinWidth=\"0\"", xaml, StringComparison.Ordinal);
            Assert.Contains("<Grid MinWidth=\"0\">", xaml, StringComparison.Ordinal);
            Assert.Contains("MaxWidth=\"520\"", xaml, StringComparison.Ordinal);
            Assert.Contains("<Border MaxWidth=\"340\" MinHeight=\"120\" Margin=\"12\"", xaml, StringComparison.Ordinal);
            Assert.Contains("Text=\"No rental history records\" Style=\"{StaticResource SectionHeader}\" TextAlignment=\"Center\" TextWrapping=\"Wrap\"", xaml, StringComparison.Ordinal);
            Assert.DoesNotContain("<Border Width=\"360\" HorizontalAlignment=\"Center\"", xaml, StringComparison.Ordinal);
        }

        [Fact]
        public void RentalHistoryWindow_PreservesHistoryCommandsAndRowHandlers()
        {
            var xaml = ReadRepoFile("InventoryManagementApp", "Views", "Windows", "RentalHistoryWindow.xaml");

            Assert.Contains("OpenDetailsCommand", xaml, StringComparison.Ordinal);
            Assert.Contains("CloseCommand", xaml, StringComparison.Ordinal);
            Assert.Contains("SearchCommand", xaml, StringComparison.Ordinal);
            Assert.Contains("ClearSearchCommand", xaml, StringComparison.Ordinal);
            Assert.Contains("ExportCsvCommand", xaml, StringComparison.Ordinal);
            Assert.Contains("HistoryRow_MouseDoubleClick", xaml, StringComparison.Ordinal);
            Assert.Contains("HistoryRow_PreviewMouseRightButtonDown", xaml, StringComparison.Ordinal);
            Assert.Contains("Open Details", xaml, StringComparison.Ordinal);
            Assert.Contains("Export Current View", xaml, StringComparison.Ordinal);
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