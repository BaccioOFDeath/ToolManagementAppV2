using System;
using System.IO;
using Xunit;

namespace InventoryManagementApp.Tests
{
    public class ImportExportExporterCancellationContractTests
    {
        [Theory]
        [InlineData("InventoryManagementApp/Services/ImportExport/ItemCsvExporter.cs", "var items = data.ToList();", "CsvHelperUtil.ExportItemsToCsvAsync(filePath, items)")]
        [InlineData("InventoryManagementApp/Services/ImportExport/CustomerCsvExporter.cs", "var customers = data.Select(c => new CustomerModel", "CsvHelperUtil.ExportCustomersToCsv(filePath, customers)")]
        [InlineData("InventoryManagementApp/Services/ImportExport/ItemJsonExporter.cs", "var items = data.ToList();", "File.WriteAllTextAsync(filePath, json, cancellationToken)")]
        [InlineData("InventoryManagementApp/Services/ImportExport/CustomerJsonExporter.cs", "var customers = data.ToList();", "File.WriteAllTextAsync(filePath, json, cancellationToken)")]
        [InlineData("InventoryManagementApp/Services/ImportExport/ItemXmlExporter.cs", "var items = data.ToList();", "new StreamWriter(filePath)")]
        [InlineData("InventoryManagementApp/Services/ImportExport/CustomerXmlExporter.cs", "var customers = data.ToList();", "new StreamWriter(filePath)")]
        public void ExportersHonorCancellationBeforeMaterializingRowsAndWritingFiles(
            string relativePath,
            string materializeMarker,
            string writeMarker)
        {
            var source = ReadRepoFile(relativePath);

            AssertCancellationCheckBefore(source, materializeMarker);
            AssertCancellationCheckBetween(source, materializeMarker, writeMarker);
        }

        [Theory]
        [InlineData("InventoryManagementApp/Services/ImportExport/CustomerCsvExporter.cs")]
        [InlineData("InventoryManagementApp/Services/ImportExport/ItemXmlExporter.cs")]
        [InlineData("InventoryManagementApp/Services/ImportExport/CustomerXmlExporter.cs")]
        public void SynchronousExporterTasksCheckCancellationInsideTheWriterTask(string relativePath)
        {
            var source = ReadRepoFile(relativePath);
            var taskStart = source.IndexOf("await Task.Run(() =>", StringComparison.Ordinal);
            Assert.True(taskStart >= 0, $"Expected {relativePath} to use Task.Run for synchronous export work.");

            var innerCancellation = source.IndexOf("cancellationToken.ThrowIfCancellationRequested();", taskStart, StringComparison.Ordinal);
            Assert.True(innerCancellation > taskStart, $"Expected {relativePath} to check cancellation inside the synchronous export task.");
        }

        private static void AssertCancellationCheckBefore(string source, string marker)
        {
            var markerIndex = source.IndexOf(marker, StringComparison.Ordinal);
            Assert.True(markerIndex >= 0, $"Expected source marker: {marker}");

            var cancellationIndex = source.LastIndexOf("cancellationToken.ThrowIfCancellationRequested();", markerIndex, StringComparison.Ordinal);
            Assert.True(cancellationIndex >= 0, $"Expected cancellation check before marker: {marker}");
        }

        private static void AssertCancellationCheckBetween(string source, string firstMarker, string secondMarker)
        {
            var firstIndex = source.IndexOf(firstMarker, StringComparison.Ordinal);
            var secondIndex = source.IndexOf(secondMarker, StringComparison.Ordinal);

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