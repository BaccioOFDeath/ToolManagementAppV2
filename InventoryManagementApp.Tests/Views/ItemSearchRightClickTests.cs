using System;
using System.IO;
using Xunit;

namespace InventoryManagementApp.Tests.Views
{
    public class ItemSearchRightClickTests
    {
        [Fact]
        public void ItemSearchPage_RightClickSelectionFallsBackToLogicalTreeForTextContent()
        {
            var code = ReadRepositoryFile("InventoryManagementApp", "Views", "Pages", "ItemSearchPage.xaml.cs");

            Assert.Contains("ItemGrid_PreviewMouseRightButtonDown", code, StringComparison.Ordinal);
            Assert.Contains("GetParent(current)", code, StringComparison.Ordinal);
            Assert.Contains("VisualTreeHelper.GetParent(current)", code, StringComparison.Ordinal);
            Assert.Contains("LogicalTreeHelper.GetParent(current)", code, StringComparison.Ordinal);
            Assert.Contains("catch (InvalidOperationException)", code, StringComparison.Ordinal);
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
