using System;
using System.IO;
using Xunit;

namespace InventoryManagementApp.Tests
{
    public class AdminDataPrintPreviewRouteTests
    {
        [Theory]
        [InlineData("InventoryManagementApp", "Views", "Pages", "UsersPage.xaml.cs")]
        [InlineData("InventoryManagementApp", "Views", "Pages", "CategoriesPage.xaml.cs")]
        [InlineData("InventoryManagementApp", "Views", "Pages", "ImportExportPage.xaml.cs")]
        public void AdminDataPrintActionsUseSharedPreviewWindow(params string[] path)
        {
            var source = ReadRepoFile(path);

            Assert.Contains("PrintPreviewWindow", source, StringComparison.Ordinal);
            Assert.Contains("ShowPreview(document", source, StringComparison.Ordinal);
            Assert.DoesNotContain("WpfPrintDialog", source, StringComparison.Ordinal);
            Assert.DoesNotContain("new PrintDialog", source, StringComparison.Ordinal);
            Assert.DoesNotContain("PrintDocument(((IDocumentPaginatorSource)document).DocumentPaginator", source, StringComparison.Ordinal);
        }

        [Fact]
        public void CategoriesKeepBothDirectoryAndSelectedSheetPreviewRoutes()
        {
            var source = ReadRepoFile("InventoryManagementApp", "Views", "Pages", "CategoriesPage.xaml.cs");

            Assert.Contains("ShowPrintPreview(document, \"Category Directory\")", source, StringComparison.Ordinal);
            Assert.Contains("ShowPrintPreview(document, $\"Category Sheet - {category.Name}\")", source, StringComparison.Ordinal);
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
