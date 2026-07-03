using System;
using System.IO;
using Xunit;

namespace InventoryManagementApp.Tests
{
    public class DashboardPageResponsiveContractTests
    {
        [Fact]
        public void DashboardPage_KeepsOperationalMetricsWrappedAndBounded()
        {
            var xaml = ReadRepoFile("InventoryManagementApp", "Views", "Pages", "DashboardPage.xaml");

            Assert.Contains("<WrapPanel Grid.Row=\"2\" Margin=\"0,0,0,6\">", xaml, StringComparison.Ordinal);
            Assert.Contains("DashboardMetricCard", xaml, StringComparison.Ordinal);
            Assert.Contains("<Setter Property=\"MinWidth\" Value=\"150\"/>", xaml, StringComparison.Ordinal);
            Assert.Contains("<Setter Property=\"MaxWidth\" Value=\"230\"/>", xaml, StringComparison.Ordinal);
            Assert.Contains("DashboardMetricValueText", xaml, StringComparison.Ordinal);
            Assert.DoesNotContain("<UniformGrid Columns=\"4\">", xaml, StringComparison.Ordinal);
        }

        [Fact]
        public void DashboardPage_AvoidsLargeFixedMinimumsInMainWorkloadSplit()
        {
            var xaml = ReadRepoFile("InventoryManagementApp", "Views", "Pages", "DashboardPage.xaml");

            Assert.Contains("<ColumnDefinition Width=\"1.65*\" MinWidth=\"0\"/>", xaml, StringComparison.Ordinal);
            Assert.Contains("<ColumnDefinition Width=\"6\"/>", xaml, StringComparison.Ordinal);
            Assert.Contains("<ColumnDefinition Width=\"0.95*\" MinWidth=\"300\"/>", xaml, StringComparison.Ordinal);
            Assert.Contains("<GridSplitter Grid.Row=\"0\" Grid.RowSpan=\"2\" Grid.Column=\"1\" Width=\"6\"", xaml, StringComparison.Ordinal);
            Assert.Contains("<Border Grid.Row=\"0\" Grid.RowSpan=\"2\" Grid.Column=\"0\" Style=\"{StaticResource Card}\" Padding=\"0\" MinWidth=\"0\"", xaml, StringComparison.Ordinal);
            Assert.Contains("<Border Grid.Row=\"1\" Grid.Column=\"2\" Style=\"{StaticResource Card}\" Padding=\"0\" MinWidth=\"0\">", xaml, StringComparison.Ordinal);
            Assert.DoesNotContain("<ColumnDefinition Width=\"2*\" MinWidth=\"520\"/>", xaml, StringComparison.Ordinal);
            Assert.DoesNotContain("<ColumnDefinition Width=\"*\" MinWidth=\"360\"/>", xaml, StringComparison.Ordinal);
        }

        [Fact]
        public void DashboardPage_EnablesEveryDashboardGridVirtualizationScrollingAndFullRowSelection()
        {
            var xaml = ReadRepoFile("InventoryManagementApp", "Views", "Pages", "DashboardPage.xaml");
            var gridNames = new[]
            {
                "RentedItemsGrid",
                "CheckedOutItemsGrid",
                "RecentActivityGrid",
                "IncompleteItemsGrid",
                "CommonItemsGrid"
            };

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
        public void DashboardPage_WrapsHeaderAndPaneActionsForScaledDesktopWidths()
        {
            var xaml = ReadRepoFile("InventoryManagementApp", "Views", "Pages", "DashboardPage.xaml");

            Assert.Contains("<StackPanel DockPanel.Dock=\"Left\" MinWidth=\"210\" MaxWidth=\"320\"", xaml, StringComparison.Ordinal);
            Assert.Contains("<Border Style=\"{StaticResource DesktopSummaryCard}\" MinWidth=\"92\" MaxWidth=\"160\"", xaml, StringComparison.Ordinal);
            Assert.True(CountOccurrences(xaml, "<WrapPanel DockPanel.Dock=\"Right\" VerticalAlignment=\"Center\">") >= 3);
            Assert.DoesNotContain("<StackPanel Orientation=\"Horizontal\" DockPanel.Dock=\"Right\" VerticalAlignment=\"Center\">", xaml, StringComparison.Ordinal);
        }

        [Fact]
        public void DashboardPage_ExposesBoundedLoadingFeedbackAndRetrySurface()
        {
            var xaml = ReadRepoFile("InventoryManagementApp", "Views", "Pages", "DashboardPage.xaml");
            var codeBehind = ReadRepoFile("InventoryManagementApp", "Views", "Pages", "DashboardPage.xaml.cs");

            Assert.Contains("x:Name=\"DashboardRoot\"", xaml, StringComparison.Ordinal);
            Assert.Contains("x:Name=\"DashboardLoadStatusBanner\"", xaml, StringComparison.Ordinal);
            Assert.Contains("Grid.Row=\"1\"", xaml, StringComparison.Ordinal);
            Assert.Contains("Visibility=\"Collapsed\"", xaml, StringComparison.Ordinal);
            Assert.Contains("MinWidth=\"0\"", xaml, StringComparison.Ordinal);
            Assert.Contains("x:Name=\"DashboardLoadStatusText\"", xaml, StringComparison.Ordinal);
            Assert.Contains("TextWrapping=\"Wrap\"", xaml, StringComparison.Ordinal);
            Assert.Contains("x:Name=\"DashboardLoadRetryButton\"", xaml, StringComparison.Ordinal);
            Assert.Contains("Click=\"DashboardLoadRetryButton_Click\"", xaml, StringComparison.Ordinal);

            Assert.Contains("private bool _isLoadingDashboard;", codeBehind, StringComparison.Ordinal);
            Assert.Contains("LoadDashboardAsync(\"Loading dashboard data...\")", codeBehind, StringComparison.Ordinal);
            Assert.Contains("LoadDashboardAsync(\"Refreshing dashboard data...\")", codeBehind, StringComparison.Ordinal);
            Assert.Contains("await Dispatcher.Yield(DispatcherPriority.Background);", codeBehind, StringComparison.Ordinal);
            Assert.Contains("if (_isLoadingDashboard || DataContext is not DashboardViewModel vm)", codeBehind, StringComparison.Ordinal);
            Assert.Contains("_loadCts?.Cancel();", codeBehind, StringComparison.Ordinal);
            Assert.Contains("SetDashboardLoadStatus(null, showRetry: false);", codeBehind, StringComparison.Ordinal);
            Assert.Contains("DashboardLoadRetryButton.IsEnabled = DashboardLoadRetryButton.Visibility == Visibility.Visible;", codeBehind, StringComparison.Ordinal);
            Assert.Contains("Cursor = Cursors.Wait;", codeBehind, StringComparison.Ordinal);
            Assert.Contains("Cursor = previousCursor;", codeBehind, StringComparison.Ordinal);
        }

        [Fact]
        public void DashboardPage_PreservesPrimaryDashboardActionsAndRowHandoff()
        {
            var xaml = ReadRepoFile("InventoryManagementApp", "Views", "Pages", "DashboardPage.xaml");
            var codeBehind = ReadRepoFile("InventoryManagementApp", "Views", "Pages", "DashboardPage.xaml.cs");

            Assert.Contains("NewItemCommand", xaml, StringComparison.Ordinal);
            Assert.Contains("OpenItemsCommand", xaml, StringComparison.Ordinal);
            Assert.Contains("OpenRentalsCommand", xaml, StringComparison.Ordinal);
            Assert.Contains("PrintDashboardSnapshotCommand", xaml, StringComparison.Ordinal);
            Assert.Contains("PrintCheckedOutItemsCommand", xaml, StringComparison.Ordinal);
            Assert.Contains("OpenActivityDestinationCommand", xaml, StringComparison.Ordinal);
            Assert.Contains("OpenSelectedIncompleteItemCommand", xaml, StringComparison.Ordinal);
            Assert.Contains("OpenSelectedCommonItemCommand", xaml, StringComparison.Ordinal);
            Assert.Contains("DashboardGrid_PreviewMouseRightButtonDown", xaml, StringComparison.Ordinal);
            Assert.Contains("GridContextMenuSelection.SelectRow(sender, e)", codeBehind, StringComparison.Ordinal);
            Assert.Contains("OpenFocusedRow", codeBehind, StringComparison.Ordinal);
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