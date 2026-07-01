using System;
using System.IO;
using Xunit;

namespace InventoryManagementApp.Tests
{
    public class ItemServiceSaveNormalizationContractTests
    {
        [Fact]
        public void AddItemNormalizesTextBeforeAuthorizationDuplicateChecksInsertAndActivityLog()
        {
            var source = ReadRepoFile("InventoryManagementApp", "Services", "Items", "ItemService.cs");
            var publicMethod = ExtractMethod(
                source,
                "public async Task AddItemAsync",
                "        /// <summary>\n        /// Updates an existing item");
            var internalMethod = ExtractMethod(
                source,
                "private async Task AddItemInternalAsync",
                "private async Task UpdateItemInternalAsync");

            Assert.Contains("NormalizeItemForSave(item);", publicMethod, StringComparison.Ordinal);
            Assert.True(
                publicMethod.IndexOf("NormalizeItemForSave(item);", StringComparison.Ordinal) < publicMethod.IndexOf("_auth.EnsurePermission", StringComparison.Ordinal),
                "Add-item saves should normalize user-entered text before permission-gated persistence work starts.");
            Assert.True(
                publicMethod.IndexOf("NormalizeItemForSave(item);", StringComparison.Ordinal) < publicMethod.IndexOf("AddItemInternalAsync(item", StringComparison.Ordinal),
                "Add-item saves should normalize the model before duplicate checks and inserts.");
            Assert.True(
                publicMethod.IndexOf("NormalizeItemForSave(item);", StringComparison.Ordinal) < publicMethod.IndexOf("Added item {item.ItemNumber}", StringComparison.Ordinal),
                "Activity log messages should use the normalized item number.");

            Assert.Contains("ItemExistsAsync(itemNumber: item.ItemNumber", internalMethod, StringComparison.Ordinal);
            Assert.Contains("_repository.InsertAsync(item", internalMethod, StringComparison.Ordinal);
        }

        [Fact]
        public void UpdateItemNormalizesTextBeforeAuthorizationDuplicateChecksUpdateAndActivityLog()
        {
            var source = ReadRepoFile("InventoryManagementApp", "Services", "Items", "ItemService.cs");
            var publicMethod = ExtractMethod(
                source,
                "public async Task UpdateItemAsync",
                "        /// <summary>\n        /// Deletes an item");
            var internalMethod = ExtractMethod(
                source,
                "private async Task UpdateItemInternalAsync",
                "private async Task DeleteItemInternalAsync");

            Assert.Contains("NormalizeItemForSave(item);", publicMethod, StringComparison.Ordinal);
            Assert.True(
                publicMethod.IndexOf("NormalizeItemForSave(item);", StringComparison.Ordinal) < publicMethod.IndexOf("_auth.EnsurePermission", StringComparison.Ordinal),
                "Update-item saves should normalize user-entered text before permission-gated persistence work starts.");
            Assert.True(
                publicMethod.IndexOf("NormalizeItemForSave(item);", StringComparison.Ordinal) < publicMethod.IndexOf("UpdateItemInternalAsync(item", StringComparison.Ordinal),
                "Update-item saves should normalize the model before duplicate checks and repository updates.");
            Assert.True(
                publicMethod.IndexOf("NormalizeItemForSave(item);", StringComparison.Ordinal) < publicMethod.IndexOf("Updated item {item.ItemNumber}", StringComparison.Ordinal),
                "Activity log messages should use the normalized item number.");

            Assert.Contains("ItemExistsAsync(itemNumber: item.ItemNumber", internalMethod, StringComparison.Ordinal);
            Assert.Contains("_repository.UpdateAsync(item", internalMethod, StringComparison.Ordinal);
        }

        [Fact]
        public void BulkSaveChangesNormalizesEveryChangedItemBeforeRepositoryHandoff()
        {
            var source = ReadRepoFile("InventoryManagementApp", "Services", "Items", "ItemService.cs");
            var method = ExtractMethod(
                source,
                "public async Task SaveChangesAsync",
                "public Task<List<ItemModel>> GetMostCommonlyUsedItemsAsync");

            Assert.Contains("var changedItems = changes?.ToList() ?? new List<ItemModel>();", method, StringComparison.Ordinal);
            Assert.Contains("foreach (var item in changedItems)", method, StringComparison.Ordinal);
            Assert.Contains("ArgumentNullException.ThrowIfNull(item);", method, StringComparison.Ordinal);
            Assert.Contains("NormalizeItemForSave(item);", method, StringComparison.Ordinal);
            Assert.True(
                method.IndexOf("NormalizeItemForSave(item);", StringComparison.Ordinal) < method.IndexOf("_repository.SaveChangesAsync(changedItems", StringComparison.Ordinal),
                "Bulk item saves should normalize each model before repository persistence.");
        }

        [Fact]
        public void ItemSaveAndImportPathsShareTheSameTextNormalizationRules()
        {
            var source = ReadRepoFile("InventoryManagementApp", "Services", "Items", "ItemService.cs");
            var saveNormalizer = ExtractMethod(
                source,
                "private static void NormalizeItemForSave",
                "private static void NormalizeImportedItem");
            var importedNormalizer = ExtractMethod(
                source,
                "private static void NormalizeImportedItem",
                "private static string? NormalizeImportedText");

            Assert.Contains("NormalizeImportedItem(item);", saveNormalizer, StringComparison.Ordinal);
            Assert.Contains("item.ItemNumber = NormalizeImportedText(item.ItemNumber) ?? string.Empty;", importedNormalizer, StringComparison.Ordinal);
            Assert.Contains("item.Name = NormalizeImportedText(item.Name) ?? string.Empty;", importedNormalizer, StringComparison.Ordinal);
            Assert.Contains("item.Location = NormalizeImportedText(item.Location) ?? string.Empty;", importedNormalizer, StringComparison.Ordinal);
            Assert.Contains("item.Brand = NormalizeImportedText(item.Brand) ?? string.Empty;", importedNormalizer, StringComparison.Ordinal);
            Assert.Contains("item.PartNumber = NormalizeImportedText(item.PartNumber) ?? string.Empty;", importedNormalizer, StringComparison.Ordinal);
            Assert.Contains("item.Supplier = NormalizeImportedText(item.Supplier) ?? string.Empty;", importedNormalizer, StringComparison.Ordinal);
            Assert.Contains("item.Notes = NormalizeImportedText(item.Notes) ?? string.Empty;", importedNormalizer, StringComparison.Ordinal);
            Assert.Contains("item.Keywords = NormalizeImportedText(item.Keywords) ?? string.Empty;", importedNormalizer, StringComparison.Ordinal);
            Assert.Contains("item.ImagePath = NormalizeImportedText(item.ImagePath) ?? string.Empty;", importedNormalizer, StringComparison.Ordinal);
            Assert.Contains("item.CheckedOutBy = NormalizeImportedText(item.CheckedOutBy) ?? string.Empty;", importedNormalizer, StringComparison.Ordinal);
            Assert.Contains("item.CheckedInBy = NormalizeImportedText(item.CheckedInBy) ?? string.Empty;", importedNormalizer, StringComparison.Ordinal);
            Assert.Contains("item.MissingComponentsNotes = NormalizeImportedText(item.MissingComponentsNotes) ?? string.Empty;", importedNormalizer, StringComparison.Ordinal);
            Assert.Contains("item.IssuesNotes = NormalizeImportedText(item.IssuesNotes) ?? string.Empty;", importedNormalizer, StringComparison.Ordinal);
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
                    return NormalizeLineEndings(File.ReadAllText(candidate));

                var parent = Directory.GetParent(directory);
                if (parent is null)
                    break;

                directory = parent.FullName;
            }

            throw new FileNotFoundException($"Could not find repository file: {Path.Combine(parts)}");
        }

        private static string NormalizeLineEndings(string text) =>
            text.Replace("\r\n", "\n", StringComparison.Ordinal).Replace("\r", "\n", StringComparison.Ordinal);
    }
}
