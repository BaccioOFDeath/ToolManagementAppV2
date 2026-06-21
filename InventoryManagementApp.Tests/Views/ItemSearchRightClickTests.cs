using System;
using System.IO;
using Xunit;

namespace InventoryManagementApp.Tests.Views
{
    public class ItemSearchRightClickTests
    {
        [Fact]
        public void ItemSearchPage_RightClickSelectionUsesSharedSafeTreeTraversal()
        {
            var code = ReadRepositoryFile("InventoryManagementApp", "Views", "Pages", "ItemSearchPage.xaml.cs");
            var helper = ReadRepositoryFile("InventoryManagementApp", "Views", "Pages", "GridContextMenuSelection.cs");

            Assert.Contains("ItemGrid_PreviewMouseRightButtonDown", code, StringComparison.Ordinal);
            Assert.Contains("GridContextMenuSelection.SelectRow(sender, e)", code, StringComparison.Ordinal);
            Assert.DoesNotContain("private static DependencyObject? GetParent", code, StringComparison.Ordinal);
            Assert.Contains("VisualTreeHelper.GetParent(current)", helper, StringComparison.Ordinal);
            Assert.Contains("LogicalTreeHelper.GetParent(current)", helper, StringComparison.Ordinal);
            Assert.Contains("catch (InvalidOperationException)", helper, StringComparison.Ordinal);
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
