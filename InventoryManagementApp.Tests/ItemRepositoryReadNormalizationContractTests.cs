using System;
using System.IO;
using Xunit;

namespace InventoryManagementApp.Tests
{
    public class ItemRepositoryReadNormalizationContractTests
    {
        [Fact]
        public void ItemProjectionNormalizesAllReadbackTextFields()
        {
            var source = ReadRepoFile("InventoryManagementApp", "Data", "ItemRepository.cs");
            var projection = ExtractProjection(source);

            AssertContainsAll(
                projection,
                "TRIM(IFNULL(ItemNumber, '')) AS ItemNumber",
                "TRIM(IFNULL(NameDescription, '')) AS Name",
                "TRIM(IFNULL(Location, '')) AS Location",
                "TRIM(IFNULL(Brand, '')) AS Brand",
                "TRIM(IFNULL(PartNumber, '')) AS PartNumber",
                "TRIM(IFNULL(Supplier, '')) AS Supplier",
                "TRIM(IFNULL(Notes, '')) AS Notes",
                "TRIM(IFNULL(Keywords, '')) AS Keywords",
                "TRIM(IFNULL(ImagePath, '')) AS ImagePath",
                "TRIM(IFNULL(CheckedOutBy, '')) AS CheckedOutBy",
                "TRIM(IFNULL(CheckedInBy, '')) AS CheckedInBy",
                "TRIM(IFNULL(MissingComponentsNotes, '')) AS MissingComponentsNotes",
                "TRIM(IFNULL(IssuesNotes, '')) AS IssuesNotes");
        }

        [Fact]
        public void ItemProjectionPreservesNonTextAndNullableTimestampReadbackContracts()
        {
            var source = ReadRepoFile("InventoryManagementApp", "Data", "ItemRepository.cs");
            var projection = ExtractProjection(source);

            AssertContainsAll(
                projection,
                "PurchasedDate",
                "AvailableQuantity AS QuantityOnHand",
                "RentedQuantity",
                "IsRentalItem",
                "Price",
                "IsCheckedOut",
                "CheckedOutTime",
                "CheckedInTime",
                "IsPowered",
                "NULLIF(TRIM(UpdatedAt), '') AS UpdatedAt",
                "IsIncomplete",
                "CheckoutCount");
        }

        [Fact]
        public void AllItemReadModelsUseTheSharedNormalizedProjection()
        {
            var source = ReadRepoFile("InventoryManagementApp", "Data", "ItemRepository.cs");

            Assert.Contains("QueryPageAsync(filter, page, useFullTextSearch, cancellationToken)", source, StringComparison.Ordinal);
            AssertMethodUsesProjection(source, "private async Task<IReadOnlyList<Item>> QueryPageAsync", "private async Task<int> CountAsync(ItemFilter filter, bool");
            AssertMethodUsesProjection(source, "public async Task<Item?> GetByIdAsync", "public async Task SaveChangesAsync");
            AssertMethodUsesProjection(source, "public async Task<List<Item>> GetItemsCheckedOutByAsync", "public async Task<List<Item>> GetCheckedOutItemsAsync");
            AssertMethodUsesProjection(source, "public async Task<List<Item>> GetCheckedOutItemsAsync", "public async Task UpdateItemImageAsync");
            AssertMethodUsesProjection(source, "public async Task<List<Item>> GetMostCommonlyUsedItemsAsync", "public async Task<List<Item>> GetIncompleteItemsAsync");
            AssertMethodUsesProjection(source, "public async Task<List<Item>> GetIncompleteItemsAsync", "private static (string WhereClause, DynamicParameters Parameters) BuildFilter");
        }

        [Fact]
        public void ItemReadbackNoLongerProjectsRawUserTextColumns()
        {
            var source = ReadRepoFile("InventoryManagementApp", "Data", "ItemRepository.cs");
            var projection = ExtractProjection(source);

            Assert.DoesNotContain("ItemNumber, NameDescription AS Name", projection, StringComparison.Ordinal);
            Assert.DoesNotContain("Location, Brand, PartNumber, Supplier", projection, StringComparison.Ordinal);
            Assert.DoesNotContain("Notes, Keywords", projection, StringComparison.Ordinal);
            Assert.DoesNotContain("ImagePath, IsCheckedOut, CheckedOutBy", projection, StringComparison.Ordinal);
            Assert.DoesNotContain("CheckedOutBy, CheckedOutTime, CheckedInBy", projection, StringComparison.Ordinal);
            Assert.DoesNotContain("IsIncomplete, MissingComponentsNotes, IssuesNotes", projection, StringComparison.Ordinal);
        }

        private static string ExtractProjection(string source)
        {
            const string marker = "private const string ItemProjection = \"";
            var start = source.IndexOf(marker, StringComparison.Ordinal);
            Assert.True(start >= 0, "Could not find ItemProjection constant.");
            start += marker.Length;

            var end = source.IndexOf("\";", start, StringComparison.Ordinal);
            Assert.True(end > start, "Could not find ItemProjection constant terminator.");

            return source[start..end];
        }

        private static void AssertMethodUsesProjection(string source, string startMarker, string endMarker)
        {
            var method = ExtractMethod(source, startMarker, endMarker);
            Assert.Contains("{ItemProjection}", method, StringComparison.Ordinal);
        }

        private static void AssertContainsAll(string source, params string[] expectedSnippets)
        {
            foreach (var snippet in expectedSnippets)
            {
                Assert.Contains(snippet, source, StringComparison.Ordinal);
            }
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
