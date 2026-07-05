using System;
using System.IO;
using Xunit;

namespace InventoryManagementApp.Tests
{
    public class CheckoutHistoryWindowResponsiveContractTests
    {
        [Fact]
        public void CheckoutHistoryWindow_UsesScaledDesktopSafeBounds()
        {
            var xaml = ReadRepoFile("InventoryManagementApp", "Views", "Windows", "CheckoutHistoryWindow.xaml");
            var codeBehind = ReadRepoFile("InventoryManagementApp", "Views", "Windows", "CheckoutHistoryWindow.xaml.cs");

            Assert.Contains("Width=\"820\" Height=\"620\" MinWidth=\"640\" MinHeight=\"460\"", xaml, StringComparison.Ordinal);
            Assert.Contains("ClipToBounds=\"True\"", xaml, StringComparison.Ordinal);
            Assert.Contains("this.UseResponsiveDefaultSize(820, 620);", codeBehind, StringComparison.Ordinal);
            Assert.DoesNotContain("UseResponsiveDefaultSize(920, 820)", codeBehind, StringComparison.Ordinal);
        }

        [Fact]
        public void CheckoutHistoryWindow_UsesVirtualizedBoundedGrid()
        {
            var xaml = ReadRepoFile("InventoryManagementApp", "Views", "Windows", "CheckoutHistoryWindow.xaml");

            Assert.Contains("x:Name=\"CheckoutHistoryGrid\"", xaml, StringComparison.Ordinal);
            Assert.Contains("EnableRowVirtualization=\"True\"", xaml, StringComparison.Ordinal);
            Assert.Contains("EnableColumnVirtualization=\"True\"", xaml, StringComparison.Ordinal);
            Assert.Contains("VirtualizingPanel.IsVirtualizing=\"True\"", xaml, StringComparison.Ordinal);
            Assert.Contains("VirtualizingPanel.VirtualizationMode=\"Recycling\"", xaml, StringComparison.Ordinal);
            Assert.Contains("ScrollViewer.VerticalScrollBarVisibility=\"Auto\"", xaml, StringComparison.Ordinal);
            Assert.Contains("ScrollViewer.HorizontalScrollBarVisibility=\"Auto\"", xaml, StringComparison.Ordinal);
            Assert.Contains("SelectionUnit=\"FullRow\"", xaml, StringComparison.Ordinal);
            Assert.Contains("DataGridTextColumn Header=\"When\"", xaml, StringComparison.Ordinal);
            Assert.Contains("DataGridTextColumn Header=\"User\"", xaml, StringComparison.Ordinal);
            Assert.Contains("DataGridTextColumn Header=\"Action\"", xaml, StringComparison.Ordinal);
        }

        [Fact]
        public void CheckoutHistoryWindow_CapsRowsAndReportsOmittedHistory()
        {
            var codeBehind = ReadRepoFile("InventoryManagementApp", "Views", "Windows", "CheckoutHistoryWindow.xaml.cs");
            var xaml = ReadRepoFile("InventoryManagementApp", "Views", "Windows", "CheckoutHistoryWindow.xaml");

            Assert.Contains("const int MaxVisibleHistoryRows = 500;", codeBehind, StringComparison.Ordinal);
            Assert.Contains("const int MaxLoadedHistoryRows = MaxVisibleHistoryRows + 1;", codeBehind, StringComparison.Ordinal);
            Assert.Contains("OrderByDescending(log => log.Timestamp)", codeBehind, StringComparison.Ordinal);
            Assert.Contains(".Take(MaxLoadedHistoryRows)", codeBehind, StringComparison.Ordinal);
            Assert.Contains("orderedLogs.Take(MaxVisibleHistoryRows)", codeBehind, StringComparison.Ordinal);
            Assert.Contains("HasOmittedLogs = TotalLogCount > VisibleLogCount;", codeBehind, StringComparison.Ordinal);
            Assert.Contains("OmittedLogCount = HasOmittedLogs ? 1 : 0;", codeBehind, StringComparison.Ordinal);
            Assert.Contains("OlderHistoryIndicator = HasOmittedLogs ? \"Yes\" : \"No\";", codeBehind, StringComparison.Ordinal);
            Assert.Contains("At least one older checkout history row exists", codeBehind, StringComparison.Ordinal);
            Assert.Contains("Showing newest {VisibleLogCount:N0} checkout history rows", codeBehind, StringComparison.Ordinal);
            Assert.Contains("HasOmittedLogs", xaml, StringComparison.Ordinal);
            Assert.Contains("OmittedLogSummary", xaml, StringComparison.Ordinal);
            Assert.Contains("OlderHistoryIndicator", xaml, StringComparison.Ordinal);
            Assert.Contains("Older rows available", xaml, StringComparison.Ordinal);
            Assert.Contains("FooterStatusText", xaml, StringComparison.Ordinal);
            Assert.DoesNotContain("Showing {VisibleLogCount:N0} of {TotalLogCount:N0}", codeBehind, StringComparison.Ordinal);
        }

        [Fact]
        public void CheckoutHistoryWindow_WrapsHeaderMetricsAndFooter()
        {
            var xaml = ReadRepoFile("InventoryManagementApp", "Views", "Windows", "CheckoutHistoryWindow.xaml");

            Assert.Contains("<StackPanel DockPanel.Dock=\"Left\" MinWidth=\"220\" MaxWidth=\"520\"", xaml, StringComparison.Ordinal);
            Assert.Contains("<WrapPanel Grid.Row=\"1\" Margin=\"0,8,0,8\">", xaml, StringComparison.Ordinal);
            Assert.Contains("VisibleLogCount", xaml, StringComparison.Ordinal);
            Assert.Contains("TotalLogCount", xaml, StringComparison.Ordinal);
            Assert.Contains("OlderHistoryIndicator", xaml, StringComparison.Ordinal);
            Assert.Contains("<WrapPanel>", xaml, StringComparison.Ordinal);
            Assert.Contains("Esc closes this history window.", xaml, StringComparison.Ordinal);
        }

        [Fact]
        public void CheckoutHistoryWindow_PreservesKeyboardReviewPath()
        {
            var xaml = ReadRepoFile("InventoryManagementApp", "Views", "Windows", "CheckoutHistoryWindow.xaml");
            var codeBehind = ReadRepoFile("InventoryManagementApp", "Views", "Windows", "CheckoutHistoryWindow.xaml.cs");

            Assert.Contains("KeyDown=\"CheckoutHistoryWindow_OnKeyDown\"", xaml, StringComparison.Ordinal);
            Assert.Contains("Loaded += (_, _) => CheckoutHistoryGrid.Focus();", codeBehind, StringComparison.Ordinal);
            Assert.Contains("if (e.Key == Key.Escape)", codeBehind, StringComparison.Ordinal);
            Assert.Contains("CloseButton_Click", codeBehind, StringComparison.Ordinal);
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