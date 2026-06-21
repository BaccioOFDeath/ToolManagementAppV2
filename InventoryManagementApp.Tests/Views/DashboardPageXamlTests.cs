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
            Assert.Contains("Source=\"{Binding ImagePath, Converter={StaticResource NullToDefaultImageConverter}, ConverterParameter=item}\"", checkedOutGrid, StringComparison.Ordinal);
            Assert.Contains("<Image.ToolTip>", checkedOutGrid, StringComparison.Ordinal);
            Assert.Contains("RowHeight=\"44\"", checkedOutGrid, StringComparison.Ordinal);
        }

        [Fact]
        public void CommonItemsGrid_LeavesRoomForItemImage()
        {
            var xaml = ReadRepositoryFile("InventoryManagementApp", "Views", "Pages", "DashboardPage.xaml");
            var gridIndex = xaml.IndexOf("x:Name=\"CommonItemsGrid\"", StringComparison.Ordinal);

            Assert.True(gridIndex >= 0, "Common items dashboard grid should exist.");
            var commonItemsGrid = xaml.Substring(gridIndex, xaml.IndexOf("x:Name=\"RentedItemsGrid\"", StringComparison.Ordinal) - gridIndex);

            Assert.Contains("RowHeight=\"44\"", commonItemsGrid, StringComparison.Ordinal);
            Assert.Contains("Width=\"42\" Height=\"34\"", commonItemsGrid, StringComparison.Ordinal);
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
