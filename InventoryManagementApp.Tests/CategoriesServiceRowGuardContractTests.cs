using System;
using System.IO;
using Xunit;

namespace InventoryManagementApp.Tests
{
    public class CategoriesServiceRowGuardContractTests
    {
        [Fact]
        public void CategoryInventoryQueriesValidateInventoryRowsBeforeReturningEmptyResults()
        {
            var source = ReadRepoFile("InventoryManagementApp", "Services", "Categories", "CategoriesService.cs");
            var linkMethod = ExtractMethod(
                source,
                "public async Task LinkCategoryToInventoryAsync",
                "public async Task<List<CategoryDto>> GetCategoriesForInventoryAsync");
            var listMethod = ExtractMethod(
                source,
                "public async Task<List<CategoryDto>> GetCategoriesForInventoryAsync",
                "public async Task<bool> RenameCategoryAsync");

            Assert.Contains("await EnsureInventoryExistsAsync(conn, inventoryId);", linkMethod, StringComparison.Ordinal);
            Assert.Contains("await EnsureCategoryExistsAsync(conn, categoryId);", linkMethod, StringComparison.Ordinal);
            Assert.True(
                linkMethod.IndexOf("await EnsureInventoryExistsAsync(conn, inventoryId);", StringComparison.Ordinal) < linkMethod.IndexOf("INSERT OR IGNORE INTO InventoryCategories", StringComparison.Ordinal),
                "Inventory link writes should validate the inventory row before inserting the relationship.");
            Assert.True(
                linkMethod.IndexOf("await EnsureCategoryExistsAsync(conn, categoryId);", StringComparison.Ordinal) < linkMethod.IndexOf("INSERT OR IGNORE INTO InventoryCategories", StringComparison.Ordinal),
                "Inventory link writes should validate the category row before inserting the relationship.");

            Assert.Contains("await EnsureInventoryExistsAsync(conn, inventoryId);", listMethod, StringComparison.Ordinal);
            Assert.True(
                listMethod.IndexOf("await EnsureInventoryExistsAsync(conn, inventoryId);", StringComparison.Ordinal) < listMethod.IndexOf("FROM InventoryCategories ic", StringComparison.Ordinal),
                "Category listing should reject missing inventory rows before a no-category result can look valid.");
        }

        [Fact]
        public void CategoryRenameAndDeleteFailClearlyForMissingRows()
        {
            var source = ReadRepoFile("InventoryManagementApp", "Services", "Categories", "CategoriesService.cs");
            var renameMethod = ExtractMethod(
                source,
                "public async Task<bool> RenameCategoryAsync",
                "public async Task<bool> DeleteCategoryAsync");
            var deleteMethod = ExtractMethod(
                source,
                "public async Task<bool> DeleteCategoryAsync",
                "private static async Task EnsureInventoryExistsAsync");

            Assert.Contains("await EnsureCategoryExistsAsync(conn, categoryId);", renameMethod, StringComparison.Ordinal);
            Assert.True(
                renameMethod.IndexOf("await EnsureCategoryExistsAsync(conn, categoryId);", StringComparison.Ordinal) < renameMethod.IndexOf("SELECT CategoryID FROM Categories WHERE Name=@n", StringComparison.Ordinal),
                "Rename should prove the edited category still exists before duplicate-name and update SQL work.");
            Assert.Contains("if (rows == 0)", renameMethod, StringComparison.Ordinal);
            Assert.Contains("throw new KeyNotFoundException($\"Category {categoryId} not found.\");", renameMethod, StringComparison.Ordinal);
            Assert.DoesNotContain("return rows > 0;", renameMethod, StringComparison.Ordinal);

            Assert.Contains("await EnsureCategoryExistsAsync(conn, categoryId, tx);", deleteMethod, StringComparison.Ordinal);
            Assert.True(
                deleteMethod.IndexOf("await EnsureCategoryExistsAsync(conn, categoryId, tx);", StringComparison.Ordinal) < deleteMethod.IndexOf("DELETE FROM InventoryCategories WHERE CategoryID=@id", StringComparison.Ordinal),
                "Delete should prove the category still exists before removing relationship rows.");
            Assert.Contains("if (rows == 0)", deleteMethod, StringComparison.Ordinal);
            Assert.Contains("return true;", deleteMethod, StringComparison.Ordinal);
            Assert.DoesNotContain("return rows > 0;", deleteMethod, StringComparison.Ordinal);
        }

        [Fact]
        public void CategoryAndInventoryExistenceGuardsUseExplicitMessages()
        {
            var source = ReadRepoFile("InventoryManagementApp", "Services", "Categories", "CategoriesService.cs");
            var helpers = ExtractMethod(
                source,
                "private static async Task EnsureInventoryExistsAsync",
                "    }\n\n    /// <summary>");

            Assert.Contains("SELECT InventoryID FROM Inventories WHERE InventoryID=@i", helpers, StringComparison.Ordinal);
            Assert.Contains("throw new InvalidOperationException($\"Inventory {inventoryId} not found.\");", helpers, StringComparison.Ordinal);
            Assert.Contains("SELECT CategoryID FROM Categories WHERE CategoryID=@id", helpers, StringComparison.Ordinal);
            Assert.Contains("throw new KeyNotFoundException($\"Category {categoryId} not found.\");", helpers, StringComparison.Ordinal);
        }

        private static string ExtractMethod(string source, string startMarker, string endMarker)
        {
            var start = source.IndexOf(startMarker, StringComparison.Ordinal);
            Assert.True(start >= 0, $"Could not find method start marker: {startMarker}");

            var end = source.IndexOf(endMarker, start, StringComparison.Ordinal);
            Assert.True(end > start, $"Could not find method end marker: {endMarker}");

            return source[start..end];
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
