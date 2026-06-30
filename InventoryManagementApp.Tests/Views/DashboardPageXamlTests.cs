using System;
using System.IO;
using Xunit;

namespace InventoryManagementApp.Tests.Views
{
    public class DashboardPageXamlTests
    {
        [Fact]
        public void CheckedOutItemsGrid_ShowsItemImageColumn()
        {
            var xaml = ReadRepositoryFile("InventoryManagementApp", "Views", "Pages", "DashboardPage.xaml");
            var gridIndex = xaml.IndexOf("x:Name=\"CheckedOutItemsGrid\"", StringComparison.Ordinal);

            Assert.True(gridIndex >= 0, "Checked-out dashboard grid should exist.");
            var checkedOutGrid = xaml.Substring(gridIndex);

            Assert.Contains("<DataGridTemplateColumn Header=\"Image\" Width=\"58\">", checkedOutGrid, StringComparison.Ordinal);
            Assert.Contains("Source=\"{Binding Converter={StaticResource NullToDefaultImageConverter}, ConverterParameter=item}\"", checkedOutGrid, StringComparison.Ordinal);
            Assert.Contains("<Image.ToolTip>", checkedOutGrid, StringComparison.Ordinal);
            Assert.Contains("RowHeight=\"44\"", checkedOutGrid, StringComparison.Ordinal);
        }

        [Fact]
        public void ActiveRentalsGrid_ShowsItemImageColumn()
        {
            var xaml = ReadRepositoryFile("InventoryManagementApp", "Views", "Pages", "DashboardPage.xaml");
            var gridIndex = xaml.IndexOf("x:Name=\"RentedItemsGrid\"", StringComparison.Ordinal);
            var checkedOutGridIndex = xaml.IndexOf("x:Name=\"CheckedOutItemsGrid\"", StringComparison.Ordinal);

            Assert.True(gridIndex >= 0, "Active rentals dashboard grid should exist.");
            Assert.True(checkedOutGridIndex > gridIndex, "Checked-out grid should still follow active rentals in the XAML.");
            var activeRentalsGrid = xaml.Substring(gridIndex, checkedOutGridIndex - gridIndex);

            Assert.Contains("RowHeight=\"44\"", activeRentalsGrid, StringComparison.Ordinal);
            Assert.Contains("<DataGridTemplateColumn Header=\"Image\" Width=\"58\">", activeRentalsGrid, StringComparison.Ordinal);
            Assert.Contains("Width=\"42\" Height=\"34\"", activeRentalsGrid, StringComparison.Ordinal);
            Assert.Contains("Source=\"{Binding Converter={StaticResource NullToDefaultImageConverter}, ConverterParameter=item}\"", activeRentalsGrid, StringComparison.Ordinal);
            Assert.Contains("<Image.ToolTip>", activeRentalsGrid, StringComparison.Ordinal);
        }

        [Fact]
        public void CommonItemsGrid_MovedIntoRightSideTabGroup()
        {
            var xaml = ReadRepositoryFile("InventoryManagementApp", "Views", "Pages", "DashboardPage.xaml");
            var gridIndex = xaml.IndexOf("x:Name=\"CommonItemsGrid\"", StringComparison.Ordinal);
            var checkedOutIndex = xaml.IndexOf("x:Name=\"CheckedOutItemsGrid\"", StringComparison.Ordinal);
            var commonTabIndex = xaml.IndexOf("<TabItem Header=\"Commonly Used\">", StringComparison.Ordinal);

            Assert.True(gridIndex >= 0, "Common items dashboard grid should exist.");
            Assert.True(commonTabIndex >= 0, "Common items should be exposed as a tab beside issue workflows.");
            Assert.True(gridIndex > commonTabIndex, "Common items grid should live inside the Commonly Used tab.");
            Assert.True(checkedOutIndex >= 0 && checkedOutIndex < commonTabIndex, "Checked-out items should appear before the right-side common items tab.");
            Assert.Contains("Grid.Row=\"0\" Grid.RowSpan=\"2\" Grid.Column=\"0\"", xaml, StringComparison.Ordinal);

            var commonItemsGrid = xaml.Substring(gridIndex);

            Assert.Contains("RowHeight=\"44\"", commonItemsGrid, StringComparison.Ordinal);
            Assert.Contains("Width=\"42\" Height=\"34\"", commonItemsGrid, StringComparison.Ordinal);
        }

        [Fact]
        public void Dashboard_UsesCompactLaptopFriendlyHeaderRows()
        {
            var xaml = ReadRepositoryFile("InventoryManagementApp", "Views", "Pages", "DashboardPage.xaml");

            Assert.Contains("Padding=\"10,6\"", xaml, StringComparison.Ordinal);
            Assert.Contains("Style=\"{StaticResource PageTitleTextBlock}\"", xaml, StringComparison.Ordinal);
            Assert.Contains("MinWidth=\"92\"", xaml, StringComparison.Ordinal);
            Assert.Contains("FontSize=\"18\"", xaml, StringComparison.Ordinal);
            Assert.Contains("Padding=\"8,5\"", xaml, StringComparison.Ordinal);
            Assert.Contains("FontSize=\"15\"", xaml, StringComparison.Ordinal);
            Assert.Contains("Padding=\"6,3\"", xaml, StringComparison.Ordinal);
        }

        private static string ReadRepositoryFile(params string[] relativePath)
        {
            var directory = new DirectoryInfo(AppContext.BaseDirectory);
            while (directory != null)
            {
                var candidate = Path.Combine(directory.FullName, Path.Combine(relativePath));
                if (File.Exists(candidate))
                    return File.ReadAllText(candidate);

                directory = directory.Parent;
            }

            throw new FileNotFoundException("Could not locate repository file.", Path.Combine(relativePath));
        }
    }
}
