using System;
using System.IO;
using Xunit;

namespace InventoryManagementApp.Tests.Views
{
    public class ImageGridRowHeightXamlTests
    {
        [Fact]
        public void ManageItemsGrid_LeavesRoomForPhotoCells()
        {
            var xaml = ReadRepositoryFile("InventoryManagementApp", "Views", "Pages", "ManageItemsPage.xaml");

            Assert.Contains("<DataGrid.RowHeight>56</DataGrid.RowHeight>", xaml, StringComparison.Ordinal);
            Assert.Contains("Width=\"48\" Height=\"48\"", xaml, StringComparison.Ordinal);
            Assert.Contains("Stretch=\"Uniform\"", xaml, StringComparison.Ordinal);
            Assert.DoesNotContain("Stretch=\"UniformToFill\"", xaml, StringComparison.Ordinal);
        }

        [Fact]
        public void ItemSearchImageGrids_KeepExplicitThumbnailRowHeights()
        {
            var xaml = ReadRepositoryFile("InventoryManagementApp", "Views", "Pages", "ItemSearchPage.xaml");

            Assert.Contains("x:Name=\"ResultsGrid\"", xaml, StringComparison.Ordinal);
            Assert.Contains("RowHeight=\"78\"", xaml, StringComparison.Ordinal);
            Assert.Contains("Width=\"62\" Height=\"62\"", xaml, StringComparison.Ordinal);
            Assert.Contains("x:Name=\"CheckedOutGrid\"", xaml, StringComparison.Ordinal);
            Assert.Contains("RowHeight=\"64\"", xaml, StringComparison.Ordinal);
            Assert.Contains("Width=\"44\" Height=\"44\"", xaml, StringComparison.Ordinal);
            Assert.DoesNotContain("UniformToFill", xaml, StringComparison.Ordinal);
        }

        [Theory]
        [InlineData("DashboardPage.xaml")]
        [InlineData("ManageRentalsPage.xaml")]
        public void OtherItemRowThumbnails_ShowTheWholePhoto(string pageName)
        {
            var xaml = ReadRepositoryFile("InventoryManagementApp", "Views", "Pages", pageName);

            Assert.Contains("Stretch=\"Uniform\"", xaml, StringComparison.Ordinal);
            Assert.DoesNotContain("Stretch=\"UniformToFill\"", xaml, StringComparison.Ordinal);
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
