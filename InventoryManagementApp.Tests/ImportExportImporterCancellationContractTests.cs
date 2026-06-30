using System;
using System.IO;
using Xunit;

namespace InventoryManagementApp.Tests
{
    public class ImportExportImporterCancellationContractTests
    {
        [Theory]
        [InlineData("InventoryManagementApp/Services/ImportExport/ItemJsonImporter.cs", "File.ReadAllTextAsync(filePath, cancellationToken)", "JsonSerializer.Deserialize<List<ItemModel>>(json, JsonOptions)", "for (int i = 0; i < items.Count; i++)")]
        [InlineData("InventoryManagementApp/Services/ImportExport/CustomerJsonImporter.cs", "File.ReadAllTextAsync(filePath, cancellationToken)", "JsonSerializer.Deserialize<List<Customer>>(json, JsonOptions)", "for (int i = 0; i < customers.Count; i++)")]
        [InlineData("InventoryManagementApp/Services/ImportExport/ItemXmlImporter.cs", "await Task.Run(() =>", "serializer.Deserialize(reader) as List<ItemModel>", "for (int i = 0; i < items.Count; i++)")]
        [InlineData("InventoryManagementApp/Services/ImportExport/CustomerXmlImporter.cs", "await Task.Run(() =>", "serializer.Deserialize(reader) as List<Customer>", "for (int i = 0; i < customers.Count; i++)")]
        public void ImportersHonorCancellationBeforeParsingAndRowValidation(
            string relativePath,
            string readMarker,
            string parseMarker,
            string validationLoopMarker)
        {
            var source = ReadRepoFile(relativePath);

            AssertCancellationCheckBefore(source, readMarker);
            AssertCancellationCheckBetween(source, readMarker, parseMarker);
            AssertCancellationCheckBetween(source, parseMarker, validationLoopMarker);
            AssertCancellationCheckBetween(source, validationLoopMarker, "var item = items[i];", required: relativePath.Contains("Item", StringComparison.Ordinal));
            AssertCancellationCheckBetween(source, validationLoopMarker, "var customer = customers[i];", required: relativePath.Contains("Customer", StringComparison.Ordinal));
        }

        [Theory]
        [InlineData("InventoryManagementApp/Services/ImportExport/ItemXmlImporter.cs", "serializer.Deserialize(reader) as List<ItemModel>")]
        [InlineData("InventoryManagementApp/Services/ImportExport/CustomerXmlImporter.cs", "serializer.Deserialize(reader) as List<Customer>")]
        public void SynchronousXmlImporterTasksCheckCancellationInsideDeserializeWork(string relativePath, string deserializeMarker)
        {
            var source = ReadRepoFile(relativePath);
            var taskStart = source.IndexOf("await Task.Run(() =>", StringComparison.Ordinal);
            Assert.True(taskStart >= 0, $"Expected {relativePath} to use Task.Run for synchronous XML import work.");

            var deserializeIndex = source.IndexOf(deserializeMarker, taskStart, StringComparison.Ordinal);
            Assert.True(deserializeIndex > taskStart, $"Expected XML deserialize marker in {relativePath}.");

            var innerCancellationBeforeDeserialize = source.LastIndexOf("cancellationToken.ThrowIfCancellationRequested();", deserializeIndex, StringComparison.Ordinal);
            Assert.True(
                innerCancellationBeforeDeserialize > taskStart,
                $"Expected cancellation check inside the XML import task before deserialization in {relativePath}.");

            var taskEnd = source.IndexOf("}, cancellationToken).ConfigureAwait(false);", deserializeIndex, StringComparison.Ordinal);
            Assert.True(taskEnd > deserializeIndex, $"Expected XML import task end in {relativePath}.");

            var innerCancellationAfterDeserialize = source.IndexOf("cancellationToken.ThrowIfCancellationRequested();", deserializeIndex, StringComparison.Ordinal);
            Assert.True(
                innerCancellationAfterDeserialize > deserializeIndex && innerCancellationAfterDeserialize < taskEnd,
                $"Expected cancellation check inside the XML import task after deserialization in {relativePath}.");
        }

        private static void AssertCancellationCheckBefore(string source, string marker)
        {
            var markerIndex = source.IndexOf(marker, StringComparison.Ordinal);
            Assert.True(markerIndex >= 0, $"Expected source marker: {marker}");

            var cancellationIndex = source.LastIndexOf("cancellationToken.ThrowIfCancellationRequested();", markerIndex, StringComparison.Ordinal);
            Assert.True(cancellationIndex >= 0, $"Expected cancellation check before marker: {marker}");
        }

        private static void AssertCancellationCheckBetween(string source, string firstMarker, string secondMarker, bool required = true)
        {
            var firstIndex = source.IndexOf(firstMarker, StringComparison.Ordinal);
            var secondIndex = source.IndexOf(secondMarker, StringComparison.Ordinal);

            if (!required)
            {
                Assert.True(secondIndex < 0, $"Did not expect marker in this importer: {secondMarker}");
                return;
            }

            Assert.True(firstIndex >= 0, $"Expected first source marker: {firstMarker}");
            Assert.True(secondIndex > firstIndex, $"Expected second source marker after first marker: {secondMarker}");

            var cancellationIndex = source.IndexOf("cancellationToken.ThrowIfCancellationRequested();", firstIndex, StringComparison.Ordinal);
            Assert.True(
                cancellationIndex > firstIndex && cancellationIndex < secondIndex,
                $"Expected cancellation check between markers: {firstMarker} -> {secondMarker}");
        }

        private static string ReadRepoFile(string relativePath)
        {
            var directory = AppContext.BaseDirectory;
            var normalizedPath = relativePath.Replace('/', Path.DirectorySeparatorChar);

            while (!string.IsNullOrEmpty(directory))
            {
                var candidate = Path.Combine(directory, normalizedPath);
                if (File.Exists(candidate))
                    return File.ReadAllText(candidate);

                var parent = Directory.GetParent(directory);
                if (parent is null)
                    break;

                directory = parent.FullName;
            }

            throw new FileNotFoundException($"Could not find repository file: {relativePath}");
        }
    }
}
