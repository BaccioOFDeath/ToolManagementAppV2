using System;
using System.IO;
using Xunit;

namespace InventoryManagementApp.Tests
{
    public class ItemServiceImportNormalizationContractTests
    {
        [Fact]
        public void CsvItemImportNormalizesMappedTextBeforeValidationDuplicatesAndInsert()
        {
            var source = ReadRepoFile("InventoryManagementApp", "Services", "Items", "ItemService.cs");
            var method = ExtractMethod(
                source,
                "private async Task<List<int>> ImportItemsFromCsvInternalAsync",
                "protected virtual async Task<int> InsertItemAsync");

            Assert.Contains("var itemNumber = NormalizeImportedText(CsvHelperUtil.GetMapped(cols, headers, map, \"ItemNumber\"));", method, StringComparison.Ordinal);
            Assert.Contains("var name = NormalizeImportedText(CsvHelperUtil.GetMapped(cols, headers, map, nameof(ItemImportDto.Name)));", method, StringComparison.Ordinal);
            Assert.Contains("var location = NormalizeImportedText(CsvHelperUtil.GetMapped(cols, headers, map, \"Location\"));", method, StringComparison.Ordinal);
            Assert.Contains("var brand = NormalizeImportedText(CsvHelperUtil.GetMapped(cols, headers, map, \"Brand\"));", method, StringComparison.Ordinal);
            Assert.Contains("var partNumber = NormalizeImportedText(CsvHelperUtil.GetMapped(cols, headers, map, \"PartNumber\"));", method, StringComparison.Ordinal);
            Assert.Contains("var supplier = NormalizeImportedText(CsvHelperUtil.GetMapped(cols, headers, map, \"Supplier\"));", method, StringComparison.Ordinal);
            Assert.Contains("var purchased = NormalizeImportedText(CsvHelperUtil.GetMapped(cols, headers, map, \"PurchasedDate\"));", method, StringComparison.Ordinal);
            Assert.Contains("var notes = NormalizeImportedText(CsvHelperUtil.GetMapped(cols, headers, map, \"Notes\"));", method, StringComparison.Ordinal);
            Assert.Contains("var keywords = NormalizeImportedText(CsvHelperUtil.GetMapped(cols, headers, map, nameof(ItemImportDto.Keywords)));", method, StringComparison.Ordinal);
            Assert.Contains("var quantity = NormalizeImportedText(CsvHelperUtil.GetMapped(cols, headers, map, \"AvailableQuantity\"));", method, StringComparison.Ordinal);
            Assert.Contains("var powered = NormalizeImportedText(CsvHelperUtil.GetMapped(cols, headers, map, \"IsPowered\"));", method, StringComparison.Ordinal);
            Assert.Contains("var rental = NormalizeImportedText(CsvHelperUtil.GetMapped(cols, headers, map, \"IsRentalItem\"));", method, StringComparison.Ordinal);

            Assert.True(
                method.IndexOf("var itemNumber = NormalizeImportedText", StringComparison.Ordinal) < method.IndexOf("string.IsNullOrWhiteSpace(itemNumber)", StringComparison.Ordinal),
                "CSV item import should trim item numbers before missing-number validation.");
            Assert.True(
                method.IndexOf("var itemNumber = NormalizeImportedText", StringComparison.Ordinal) < method.IndexOf("existingNumbers.Contains(itemNumber)", StringComparison.Ordinal),
                "CSV item import should trim item numbers before duplicate checks.");
            Assert.True(
                method.IndexOf("var name = NormalizeImportedText", StringComparison.Ordinal) < method.IndexOf("Name = name ?? string.Empty", StringComparison.Ordinal),
                "CSV item import should trim names before insert model construction.");
            Assert.True(
                method.IndexOf("var quantity = NormalizeImportedText", StringComparison.Ordinal) < method.IndexOf("var parsedQuantity = TryParseInt(quantity);", StringComparison.Ordinal),
                "CSV item import should trim quantity text before parsing.");
        }

        [Fact]
        public void GenericItemImportNormalizesItemsBeforeGeneratedNumbersDuplicatesValidationAndInsert()
        {
            var source = ReadRepoFile("InventoryManagementApp", "Services", "Items", "ItemService.cs");
            var method = ExtractMethod(
                source,
                "public async Task<List<int>> ImportItemsAsync",
                "private static void NormalizeImportedItem");

            Assert.Contains("NormalizeImportedItem(item);", method, StringComparison.Ordinal);
            Assert.True(
                method.IndexOf("NormalizeImportedItem(item);", StringComparison.Ordinal) < method.IndexOf("string.IsNullOrWhiteSpace(item.ItemNumber)", StringComparison.Ordinal),
                "Generic item import should trim imported fields before deciding whether to generate an item number.");
            Assert.True(
                method.IndexOf("NormalizeImportedItem(item);", StringComparison.Ordinal) < method.IndexOf("existingNumbers.Contains(item.ItemNumber)", StringComparison.Ordinal),
                "Generic item import should trim item numbers before duplicate checks.");
            Assert.True(
                method.IndexOf("NormalizeImportedItem(item);", StringComparison.Ordinal) < method.IndexOf("ValidateQuantity(item.QuantityOnHand);", StringComparison.Ordinal),
                "Generic item import should normalize rows before validation and insert work.");
            Assert.True(
                method.IndexOf("NormalizeImportedItem(item);", StringComparison.Ordinal) < method.IndexOf("InsertItemAsync(conn, transaction, item", StringComparison.Ordinal),
                "Generic item import should only insert normalized item text.");
        }

        [Fact]
        public void ItemImportExistingNumberSetsUseTrimmedDatabaseValues()
        {
            var source = ReadRepoFile("InventoryManagementApp", "Services", "Items", "ItemService.cs");
            var csvMethod = ExtractMethod(
                source,
                "private async Task<List<int>> ImportItemsFromCsvInternalAsync",
                "protected virtual async Task<int> InsertItemAsync");
            var genericMethod = ExtractMethod(
                source,
                "public async Task<List<int>> ImportItemsAsync",
                "private static void NormalizeImportedItem");

            Assert.Contains("r => r.GetString(0).Trim()", csvMethod, StringComparison.Ordinal);
            Assert.Contains("r => r.GetString(0).Trim()", genericMethod, StringComparison.Ordinal);
            Assert.True(
                csvMethod.IndexOf("r => r.GetString(0).Trim()", StringComparison.Ordinal) < csvMethod.IndexOf("existingNumbers.Contains(itemNumber)", StringComparison.Ordinal),
                "CSV item imports should compare normalized imported numbers against trimmed persisted numbers.");
            Assert.True(
                genericMethod.IndexOf("r => r.GetString(0).Trim()", StringComparison.Ordinal) < genericMethod.IndexOf("existingNumbers.Contains(item.ItemNumber)", StringComparison.Ordinal),
                "Generic item imports should compare normalized imported numbers against trimmed persisted numbers.");
        }

        [Fact]
        public void ImportedItemNormalizerCoversIdentityDetailAndOperationalTextFields()
        {
            var source = ReadRepoFile("InventoryManagementApp", "Services", "Items", "ItemService.cs");
            var normalizer = ExtractMethod(
                source,
                "private static void NormalizeImportedItem",
                "private static string? NormalizeImportedText");

            Assert.Contains("item.ItemNumber = NormalizeImportedText(item.ItemNumber) ?? string.Empty;", normalizer, StringComparison.Ordinal);
            Assert.Contains("item.Name = NormalizeImportedText(item.Name) ?? string.Empty;", normalizer, StringComparison.Ordinal);
            Assert.Contains("item.Location = NormalizeImportedText(item.Location) ?? string.Empty;", normalizer, StringComparison.Ordinal);
            Assert.Contains("item.Brand = NormalizeImportedText(item.Brand) ?? string.Empty;", normalizer, StringComparison.Ordinal);
            Assert.Contains("item.PartNumber = NormalizeImportedText(item.PartNumber) ?? string.Empty;", normalizer, StringComparison.Ordinal);
            Assert.Contains("item.Supplier = NormalizeImportedText(item.Supplier) ?? string.Empty;", normalizer, StringComparison.Ordinal);
            Assert.Contains("item.Notes = NormalizeImportedText(item.Notes) ?? string.Empty;", normalizer, StringComparison.Ordinal);
            Assert.Contains("item.Keywords = NormalizeImportedText(item.Keywords) ?? string.Empty;", normalizer, StringComparison.Ordinal);
            Assert.Contains("item.ImagePath = NormalizeImportedText(item.ImagePath) ?? string.Empty;", normalizer, StringComparison.Ordinal);
            Assert.Contains("item.CheckedOutBy = NormalizeImportedText(item.CheckedOutBy) ?? string.Empty;", normalizer, StringComparison.Ordinal);
            Assert.Contains("item.CheckedInBy = NormalizeImportedText(item.CheckedInBy) ?? string.Empty;", normalizer, StringComparison.Ordinal);
            Assert.Contains("item.MissingComponentsNotes = NormalizeImportedText(item.MissingComponentsNotes) ?? string.Empty;", normalizer, StringComparison.Ordinal);
            Assert.Contains("item.IssuesNotes = NormalizeImportedText(item.IssuesNotes) ?? string.Empty;", normalizer, StringComparison.Ordinal);
            Assert.Contains("private static string? NormalizeImportedText(string? value) => value?.Trim();", source, StringComparison.Ordinal);
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
