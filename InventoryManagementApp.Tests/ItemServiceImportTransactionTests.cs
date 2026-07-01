using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using InventoryManagementApp.Data;
using InventoryManagementApp.Interfaces;
using InventoryManagementApp.Models.Domain;
using InventoryManagementApp.Services.Core;
using InventoryManagementApp.Services.Items;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace InventoryManagementApp.Tests
{
    public class ItemServiceImportTransactionTests
    {
        [Fact]
        public async Task ImportItemsAsync_RollsBackBatch_WhenInsertFails()
        {
            var dbPath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.db");

            try
            {
                using var database = new DatabaseService(dbPath, NullLogger<DatabaseService>.Instance);
                var service = new FailingImportItemService(database, Mock.Of<IItemRepository>());
                var importer = new StubItemImporter(new[]
                {
                    CreateItem("T9001", "First imported item"),
                    CreateItem("T9002", "Second imported item")
                });

                await Assert.ThrowsAsync<InvalidOperationException>(() =>
                    service.ImportItemsAsync("unused.csv", importer, CancellationToken.None));

                using var conn = database.CreateConnection();
                using var command = conn.CreateCommand();
                command.CommandText = "SELECT COUNT(*) FROM Items WHERE ItemNumber IN ('T9001', 'T9002')";

                var count = Convert.ToInt32(await command.ExecuteScalarAsync(CancellationToken.None));
                Assert.Equal(0, count);
            }
            finally
            {
                if (File.Exists(dbPath))
                    File.Delete(dbPath);
            }
        }

        [Fact]
        public void CsvImportExportEntrypointsValidateInputsBeforeAuthorizationAndWork()
        {
            var source = ReadRepoFile("InventoryManagementApp", "Services", "Items", "ItemService.cs");
            var importMethod = ExtractMethod(
                source,
                "public Task<List<int>> ImportItemsFromCsvAsync",
                "public Task ExportItemsToCsvAsync");
            var exportMethod = ExtractMethod(
                source,
                "public Task ExportItemsToCsvAsync",
                "public Task<ImageImportResult> ImportItemImagesAsync");

            AssertEntrypointPathGuardBeforeAuthorization(importMethod, "ImportItemsFromCsvAsync");
            Assert.Contains("if (map is null)", importMethod, StringComparison.Ordinal);
            Assert.Contains("throw new ArgumentNullException(nameof(map));", importMethod, StringComparison.Ordinal);
            Assert.True(
                importMethod.IndexOf("if (map is null)", StringComparison.Ordinal) < importMethod.IndexOf("_auth.EnsurePermission", StringComparison.Ordinal),
                "CSV item imports should reject a missing column map before authorization or file work.");
            Assert.True(
                importMethod.IndexOf("_auth.EnsurePermission", StringComparison.Ordinal) < importMethod.IndexOf("return ImportItemsFromCsvInternalAsync", StringComparison.Ordinal),
                "CSV item imports should keep authorization before import work starts.");

            AssertEntrypointPathGuardBeforeAuthorization(exportMethod, "ExportItemsToCsvAsync");
            Assert.True(
                exportMethod.IndexOf("_auth.EnsurePermission", StringComparison.Ordinal) < exportMethod.IndexOf("return ExportItemsToCsvInternalAsync", StringComparison.Ordinal),
                "CSV item exports should keep authorization before export work starts.");
        }

        [Fact]
        public void ImageImportEntrypointValidatesInputsBeforeAuthorizationAndCatalogWork()
        {
            var source = ReadRepoFile("InventoryManagementApp", "Services", "Items", "ItemService.cs");
            var entrypoint = ExtractMethod(
                source,
                "public Task<ImageImportResult> ImportItemImagesAsync",
                "public async Task<string> GenerateNextItemNumberAsync");
            var internalMethod = ExtractMethod(
                source,
                "private async Task<ImageImportResult> ImportItemImagesInternalAsync",
                "protected virtual Task CopyFileAsync");

            Assert.Contains("if (string.IsNullOrWhiteSpace(folderPath))", entrypoint, StringComparison.Ordinal);
            Assert.Contains("throw new ArgumentNullException(nameof(folderPath));", entrypoint, StringComparison.Ordinal);
            Assert.Contains("if (keySelector is null)", entrypoint, StringComparison.Ordinal);
            Assert.Contains("throw new ArgumentNullException(nameof(keySelector));", entrypoint, StringComparison.Ordinal);
            Assert.True(
                entrypoint.IndexOf("if (string.IsNullOrWhiteSpace(folderPath))", StringComparison.Ordinal) < entrypoint.IndexOf("_auth.EnsurePermission", StringComparison.Ordinal),
                "Image imports should reject a missing folder path before authorization or catalog work.");
            Assert.True(
                entrypoint.IndexOf("if (keySelector is null)", StringComparison.Ordinal) < entrypoint.IndexOf("_auth.EnsurePermission", StringComparison.Ordinal),
                "Image imports should reject a missing key selector before authorization or catalog work.");
            Assert.True(
                entrypoint.IndexOf("cancellationToken.ThrowIfCancellationRequested();", StringComparison.Ordinal) < entrypoint.IndexOf("Directory.Exists(folderPath)", StringComparison.Ordinal),
                "Image imports should honor cancellation before checking the image folder.");
            Assert.True(
                entrypoint.IndexOf("Directory.Exists(folderPath)", StringComparison.Ordinal) < entrypoint.IndexOf("return ImportItemImagesInternalAsync", StringComparison.Ordinal),
                "Image imports should reject missing folders before scanning the item catalog.");

            Assert.Contains("cancellationToken.ThrowIfCancellationRequested();", internalMethod, StringComparison.Ordinal);
            Assert.True(
                internalMethod.IndexOf("cancellationToken.ThrowIfCancellationRequested();", StringComparison.Ordinal) < internalMethod.IndexOf("var groups = new Dictionary", StringComparison.Ordinal),
                "Image imports should honor cancellation before collecting item match keys.");
            Assert.True(
                internalMethod.IndexOf("cancellationToken.ThrowIfCancellationRequested();", internalMethod.IndexOf("var supported = new HashSet<string>", StringComparison.Ordinal), StringComparison.Ordinal) < internalMethod.IndexOf("Directory.EnumerateFiles(folderPath)", StringComparison.Ordinal),
                "Image imports should honor cancellation before enumerating image files.");
        }

        [Fact]
        public void ImageImportCatalogBuildUsesBoundedPagesBeforeFileEnumeration()
        {
            var source = ReadRepoFile("InventoryManagementApp", "Services", "Items", "ItemService.cs");
            var internalMethod = ExtractMethod(
                source,
                "private async Task<ImageImportResult> ImportItemImagesInternalAsync",
                "protected virtual Task CopyFileAsync");

            Assert.Contains("private const int ImageImportCatalogPageSize = 500;", source, StringComparison.Ordinal);
            Assert.Contains("var pageNumber = 1;", internalMethod, StringComparison.Ordinal);
            Assert.Contains("while (true)", internalMethod, StringComparison.Ordinal);
            Assert.Contains("var pageItemCount = 0;", internalMethod, StringComparison.Ordinal);
            Assert.Contains("new ItemPage(pageNumber, ImageImportCatalogPageSize)", internalMethod, StringComparison.Ordinal);
            Assert.Contains("pageItemCount++;", internalMethod, StringComparison.Ordinal);
            Assert.Contains("if (pageItemCount < ImageImportCatalogPageSize)", internalMethod, StringComparison.Ordinal);
            Assert.Contains("pageNumber++;", internalMethod, StringComparison.Ordinal);
            Assert.DoesNotContain("new ItemPage(1, int.MaxValue)", internalMethod, StringComparison.Ordinal);
            Assert.True(
                internalMethod.IndexOf("new ItemPage(pageNumber, ImageImportCatalogPageSize)", StringComparison.Ordinal) < internalMethod.IndexOf("Directory.EnumerateFiles(folderPath)", StringComparison.Ordinal),
                "Image imports should finish paged catalog matching before file enumeration starts.");
        }

        [Fact]
        public void GenericImportExportEntrypointsValidateInputsBeforeAuthorizationAndWork()
        {
            var source = ReadRepoFile("InventoryManagementApp", "Services", "Items", "ItemService.cs");
            var importMethod = ExtractMethod(
                source,
                "public async Task<List<int>> ImportItemsAsync",
                "private static string GenerateNextImportedItemNumber");
            var exportMethod = ExtractMethod(
                source,
                "public async Task ExportItemsAsync",
                "static void NotifyChanged");

            AssertEntrypointPathGuardBeforeAuthorization(importMethod, "ImportItemsAsync");
            Assert.Contains("if (importer is null)", importMethod, StringComparison.Ordinal);
            Assert.Contains("throw new ArgumentNullException(nameof(importer));", importMethod, StringComparison.Ordinal);
            Assert.True(
                importMethod.IndexOf("if (importer is null)", StringComparison.Ordinal) < importMethod.IndexOf("_auth.EnsurePermission", StringComparison.Ordinal),
                "Generic item imports should reject a missing importer before authorization or file work.");
            Assert.True(
                importMethod.IndexOf("cancellationToken.ThrowIfCancellationRequested();", StringComparison.Ordinal) < importMethod.IndexOf("var (items, skippedRows) = await importer.ImportAsync", StringComparison.Ordinal),
                "Generic item imports should honor cancellation before importer file work starts.");
            Assert.True(
                importMethod.IndexOf("var (items, skippedRows) = await importer.ImportAsync", StringComparison.Ordinal) < importMethod.IndexOf("cancellationToken.ThrowIfCancellationRequested();", importMethod.IndexOf("var (items, skippedRows) = await importer.ImportAsync", StringComparison.Ordinal), StringComparison.Ordinal),
                "Generic item imports should honor cancellation again after importer parsing and before database work.");

            AssertEntrypointPathGuardBeforeAuthorization(exportMethod, "ExportItemsAsync");
            Assert.Contains("if (exporter is null)", exportMethod, StringComparison.Ordinal);
            Assert.Contains("throw new ArgumentNullException(nameof(exporter));", exportMethod, StringComparison.Ordinal);
            Assert.True(
                exportMethod.IndexOf("if (exporter is null)", StringComparison.Ordinal) < exportMethod.IndexOf("_auth.EnsurePermission", StringComparison.Ordinal),
                "Generic item exports should reject a missing exporter before authorization or item collection work.");
            Assert.True(
                exportMethod.IndexOf("cancellationToken.ThrowIfCancellationRequested();", StringComparison.Ordinal) < exportMethod.IndexOf("var items = new List<ItemModel>();", StringComparison.Ordinal),
                "Generic item exports should honor cancellation before collecting rows.");
        }

        [Fact]
        public void ItemNumberAndDuplicateHelpersHonorCancellationBeforeSqlWork()
        {
            var source = ReadRepoFile("InventoryManagementApp", "Services", "Items", "ItemService.cs");
            var numberMethod = ExtractMethod(
                source,
                "public async Task<string> GenerateNextItemNumberAsync",
                "private async Task<ImageImportResult> ImportItemImagesInternalAsync");
            var existsMethod = ExtractMethod(
                source,
                "private async Task<bool> ItemExistsAsync",
                "private async Task AddItemInternalAsync");

            AssertCancellationGuardBeforeSqlAndConnection(numberMethod, "GenerateNextItemNumberAsync");
            Assert.Contains("if (string.IsNullOrWhiteSpace(itemNumber))", existsMethod, StringComparison.Ordinal);
            Assert.Contains("return false;", existsMethod, StringComparison.Ordinal);
            AssertCancellationGuardBeforeSqlAndConnection(existsMethod, "ItemExistsAsync");
            Assert.True(
                existsMethod.IndexOf("if (string.IsNullOrWhiteSpace(itemNumber))", StringComparison.Ordinal) < existsMethod.IndexOf("cancellationToken.ThrowIfCancellationRequested();", StringComparison.Ordinal),
                "Blank duplicate checks should keep returning false without treating cancellation as a database operation.");
        }

        [Fact]
        public void CsvImportRejectsOutOfRangeQuantitiesBeforeInsert()
        {
            var source = ReadRepoFile("InventoryManagementApp", "Services", "Items", "ItemService.cs");
            var method = ExtractMethod(
                source,
                "private async Task<List<int>> ImportItemsFromCsvInternalAsync",
                "protected virtual async Task<int> InsertItemAsync");

            Assert.Contains("var parsedQuantity = TryParseInt(quantity);", method, StringComparison.Ordinal);
            Assert.Contains("if (!skip && (parsedQuantity < 0 || parsedQuantity > MaxQuantityOnHand))", method, StringComparison.Ordinal);
            Assert.Contains("_logger.LogWarning(\"Skipping row {Row}: AvailableQuantity {Quantity} is outside the allowed range.\", row, parsedQuantity);", method, StringComparison.Ordinal);
            Assert.Contains("invalidRows.Add(row);", method, StringComparison.Ordinal);
            Assert.Contains("QuantityOnHand = parsedQuantity", method, StringComparison.Ordinal);
            Assert.DoesNotContain("QuantityOnHand = TryParseInt(quantity)", method, StringComparison.Ordinal);

            Assert.True(
                method.IndexOf("var parsedQuantity = TryParseInt(quantity);", StringComparison.Ordinal) <
                method.IndexOf("var item = new ItemModel", StringComparison.Ordinal),
                "CSV import should parse quantity before building the item row.");
            Assert.True(
                method.IndexOf("if (!skip && (parsedQuantity < 0 || parsedQuantity > MaxQuantityOnHand))", StringComparison.Ordinal) <
                method.IndexOf("var item = new ItemModel", StringComparison.Ordinal),
                "CSV import should reject out-of-range quantities before building the item row.");
            Assert.True(
                method.IndexOf("if (!skip && (parsedQuantity < 0 || parsedQuantity > MaxQuantityOnHand))", StringComparison.Ordinal) <
                method.IndexOf("await InsertItemAsync", StringComparison.Ordinal),
                "CSV import should reject out-of-range quantities before direct insert work.");

            Assert.Contains("cancellationToken.ThrowIfCancellationRequested();", method, StringComparison.Ordinal);
            Assert.True(
                method.IndexOf("cancellationToken.ThrowIfCancellationRequested();", StringComparison.Ordinal) < method.IndexOf("using var parser = new TextFieldParser", StringComparison.Ordinal),
                "CSV imports should honor cancellation before opening the input file parser.");
            Assert.True(
                method.IndexOf("cancellationToken.ThrowIfCancellationRequested();", method.IndexOf("if (headers == null", StringComparison.Ordinal), StringComparison.Ordinal) < method.IndexOf("using var conn = _dbService.CreateConnection();", StringComparison.Ordinal),
                "CSV imports should honor cancellation again after header parsing and before database work.");
        }

        private static void AssertEntrypointPathGuardBeforeAuthorization(string method, string methodName)
        {
            Assert.Contains("if (string.IsNullOrWhiteSpace(filePath))", method, StringComparison.Ordinal);
            Assert.Contains("throw new ArgumentNullException(nameof(filePath));", method, StringComparison.Ordinal);
            Assert.True(
                method.IndexOf("if (string.IsNullOrWhiteSpace(filePath))", StringComparison.Ordinal) < method.IndexOf("_auth.EnsurePermission", StringComparison.Ordinal),
                $"{methodName} should reject missing file paths before authorization or file work.");
        }

        private static void AssertCancellationGuardBeforeSqlAndConnection(string method, string methodName)
        {
            Assert.Contains("cancellationToken.ThrowIfCancellationRequested();", method, StringComparison.Ordinal);
            var sqlIndex = method.IndexOf("const string sql", StringComparison.Ordinal);
            if (sqlIndex < 0)
                sqlIndex = method.IndexOf("var sql", StringComparison.Ordinal);
            Assert.True(sqlIndex >= 0, $"Could not find SQL declaration in {methodName}.");
            Assert.True(
                method.IndexOf("cancellationToken.ThrowIfCancellationRequested();", StringComparison.Ordinal) < sqlIndex,
                $"{methodName} should honor cancellation before SQL work starts.");
            Assert.True(
                method.IndexOf("cancellationToken.ThrowIfCancellationRequested();", StringComparison.Ordinal) < method.IndexOf("_dbService.CreateConnection()", StringComparison.Ordinal),
                $"{methodName} should honor cancellation before opening a database connection.");
        }

        private static ItemModel CreateItem(string itemNumber, string name) => new()
        {
            ItemNumber = itemNumber,
            Name = name,
            Location = "Import shelf",
            Brand = "Import brand",
            PartNumber = itemNumber,
            Supplier = "Import supplier",
            QuantityOnHand = 1
        };

        private sealed class FailingImportItemService : ItemService
        {
            public FailingImportItemService(DatabaseService dbService, IItemRepository repository)
                : base(dbService, repository, logger: NullLogger<ItemService>.Instance)
            {
            }

            protected override Task<int> InsertItemAsync(SqliteConnection conn, SqliteTransaction? transaction, ItemModel item, CancellationToken cancellationToken)
            {
                if (item.ItemNumber == "T9002")
                    throw new InvalidOperationException("Forced import failure.");

                return base.InsertItemAsync(conn, transaction, item, cancellationToken);
            }
        }

        private sealed class StubItemImporter : IDataImporter<ItemModel>
        {
            private readonly IEnumerable<ItemModel> _items;

            public StubItemImporter(IEnumerable<ItemModel> items)
            {
                _items = items;
            }

            public string FileExtension => ".csv";

            public string FileFilter => "CSV Files|*.csv";

            public string FormatName => "Test CSV";

            public Task<(IEnumerable<ItemModel> Data, List<int> SkippedRows)> ImportAsync(string filePath, CancellationToken cancellationToken = default)
            {
                return Task.FromResult((_items, new List<int>()));
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
