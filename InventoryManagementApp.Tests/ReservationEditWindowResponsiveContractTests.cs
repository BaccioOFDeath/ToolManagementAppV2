using System;
using System.IO;
using Xunit;

namespace InventoryManagementApp.Tests
{
    public class ReservationEditWindowResponsiveContractTests
    {
        [Fact]
        public void ReservationEditWindow_UsesScaledDesktopSafeDialogSizing()
        {
            var xaml = ReadRepoFile("InventoryManagementApp", "Views", "Windows", "ReservationEditWindow.xaml");
            var code = ReadRepoFile("InventoryManagementApp", "Views", "Windows", "ReservationEditWindow.xaml.cs");

            Assert.Contains("Width=\"820\"", xaml, StringComparison.Ordinal);
            Assert.Contains("Height=\"620\"", xaml, StringComparison.Ordinal);
            Assert.Contains("MinWidth=\"660\"", xaml, StringComparison.Ordinal);
            Assert.Contains("MinHeight=\"520\"", xaml, StringComparison.Ordinal);
            Assert.Contains("this.UseResponsiveDefaultSize(820, 640);", code, StringComparison.Ordinal);
            Assert.DoesNotContain("this.UseResponsiveDefaultSize(1000, 780);", code, StringComparison.Ordinal);
        }

        [Fact]
        public void ReservationEditWindow_KeepsHeaderAndSummaryCardsWrappedAndBounded()
        {
            var xaml = ReadRepoFile("InventoryManagementApp", "Views", "Windows", "ReservationEditWindow.xaml");

            Assert.Contains("<ColumnDefinition Width=\"*\" MinWidth=\"0\"/>", xaml, StringComparison.Ordinal);
            Assert.Contains("MinWidth=\"150\" MaxWidth=\"190\"", xaml, StringComparison.Ordinal);
            Assert.Contains("<WrapPanel Grid.Row=\"1\" Margin=\"0,0,0,8\">", xaml, StringComparison.Ordinal);
            Assert.Contains("x:Key=\"ReservationSummaryCard\"", xaml, StringComparison.Ordinal);
            Assert.Contains("<Setter Property=\"MinWidth\" Value=\"145\"/>", xaml, StringComparison.Ordinal);
            Assert.Contains("<Setter Property=\"MaxWidth\" Value=\"220\"/>", xaml, StringComparison.Ordinal);
            Assert.DoesNotContain("Width=\"170\" Margin=\"14,0,0,0\"", xaml, StringComparison.Ordinal);
            Assert.DoesNotContain("<ColumnDefinition Width=\"240\"/>", xaml, StringComparison.Ordinal);
        }

        [Fact]
        public void ReservationEditWindow_LowersMainSplitPressureAndKeepsPanesScrollable()
        {
            var xaml = ReadRepoFile("InventoryManagementApp", "Views", "Windows", "ReservationEditWindow.xaml");

            Assert.Contains("<ColumnDefinition Width=\"0.8*\" MinWidth=\"0\"/>", xaml, StringComparison.Ordinal);
            Assert.Contains("<ColumnDefinition Width=\"1.9*\" MinWidth=\"0\"/>", xaml, StringComparison.Ordinal);
            Assert.Contains("<GridSplitter Grid.Column=\"1\" Width=\"6\"", xaml, StringComparison.Ordinal);
            Assert.Contains("<Border Grid.Column=\"0\" Style=\"{StaticResource Card}\" Padding=\"0\" MinWidth=\"0\">", xaml, StringComparison.Ordinal);
            Assert.Contains("<Border Grid.Column=\"2\" Style=\"{StaticResource Card}\" Padding=\"0\" MinWidth=\"0\">", xaml, StringComparison.Ordinal);
            Assert.True(CountOccurrences(xaml, "VerticalScrollBarVisibility=\"Auto\" HorizontalScrollBarVisibility=\"Disabled\"") >= 2);
            Assert.DoesNotContain("<ColumnDefinition Width=\"240\"/>", xaml, StringComparison.Ordinal);
            Assert.DoesNotContain("<ColumnDefinition Width=\"8\"/>", xaml, StringComparison.Ordinal);
        }

        [Fact]
        public void ReservationEditWindow_VirtualizesItemLookupAndBoundsLookupRows()
        {
            var xaml = ReadRepoFile("InventoryManagementApp", "Views", "Windows", "ReservationEditWindow.xaml");

            Assert.Contains("MinHeight=\"150\" MaxHeight=\"220\"", xaml, StringComparison.Ordinal);
            Assert.Contains("VirtualizingPanel.IsVirtualizing=\"True\"", xaml, StringComparison.Ordinal);
            Assert.Contains("VirtualizingPanel.VirtualizationMode=\"Recycling\"", xaml, StringComparison.Ordinal);
            Assert.Contains("ScrollViewer.CanContentScroll=\"True\"", xaml, StringComparison.Ordinal);
            Assert.Contains("ScrollViewer.HorizontalScrollBarVisibility=\"Auto\"", xaml, StringComparison.Ordinal);
            Assert.Contains("ScrollViewer.VerticalScrollBarVisibility=\"Auto\"", xaml, StringComparison.Ordinal);
            Assert.Contains("<ColumnDefinition Width=\"96\"/>", xaml, StringComparison.Ordinal);
            Assert.Contains("<ColumnDefinition Width=\"110\"/>", xaml, StringComparison.Ordinal);
            Assert.DoesNotContain("<RowDefinition Height=\"210\"/>", xaml, StringComparison.Ordinal);
            Assert.DoesNotContain("<ColumnDefinition Width=\"112\"/>", xaml, StringComparison.Ordinal);
            Assert.DoesNotContain("<ColumnDefinition Width=\"130\"/>", xaml, StringComparison.Ordinal);
        }

        [Fact]
        public void ReservationEditWindow_BoundsDetailsFormAndFooterWithoutLosingCommands()
        {
            var xaml = ReadRepoFile("InventoryManagementApp", "Views", "Windows", "ReservationEditWindow.xaml");
            var requiredContracts = new[]
            {
                "ItemSearchText",
                "ItemSearchResults",
                "SelectedSearchItem",
                "ClearItemSearchCommand",
                "ApplySelectedItemCommand",
                "StatusOptions",
                "Reservation.ItemNumber",
                "Reservation.ItemName",
                "Reservation.CustomerName",
                "Reservation.StartDate",
                "Reservation.EndDate",
                "Reservation.Quantity",
                "Reservation.RentalID",
                "Reservation.Notes",
                "controls:SaveCancelBar"
            };

            Assert.Contains("<ColumnDefinition Width=\"96\"/>", xaml, StringComparison.Ordinal);
            Assert.Contains("<ColumnDefinition Width=\"78\"/>", xaml, StringComparison.Ordinal);
            Assert.Contains("<ColumnDefinition Width=\"132\"/>", xaml, StringComparison.Ordinal);
            Assert.Contains("TextAlignment=\"Right\"", xaml, StringComparison.Ordinal);

            foreach (var contract in requiredContracts)
                Assert.Contains(contract, xaml, StringComparison.Ordinal);
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