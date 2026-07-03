using System;
using System.IO;
using Xunit;

namespace InventoryManagementApp.Tests
{
    public class ImportExportExporterCancellationContractTests
    {
        [Theory]
        [InlineData("InventoryManagementApp/Services/ImportExport/ItemCsvExporter.cs", "var items = data as IList<ItemModel> ?? data.ToList();", "CsvHelperUtil.ExportItemsToCsvAsync(filePath, items)")]
        [InlineData("InventoryManagementApp/Services/ImportExport/CustomerCsvExporter.cs", "var customers = data.Select(c => new CustomerModel", "CsvHelperUtil.ExportCustomersToCsv(filePath, customers)")]
        [InlineData("InventoryManagementApp/Services/ImportExport/ItemJsonExporter.cs", "await using var stream = new FileStream", "JsonSerializer.SerializeAsync(stream, data, JsonOptions, cancellationToken)")]
        [InlineData("InventoryManagementApp/Services/ImportExport/CustomerJsonExporter.cs", "await using var stream = new FileStream", "JsonSerializer.SerializeAsync(stream, data, JsonOptions, cancellationToken)")]
        [InlineData("InventoryManagementApp/Services/ImportExport/ItemXmlExporter.cs", "foreach (var item in data)", "serializer.Serialize(writer, item, namespaces)")]
        [InlineData("InventoryManagementApp/Services/ImportExport/CustomerXmlExporter.cs", "foreach (var customer in data)", "serializer.Serialize(writer, customer, namespaces)")]
        public void ExportersHonorCancellationBeforePreparingRowsAndWritingFiles(
            string relativePath,
            string prepareMarker,
            string writeMarker)
        {
            var source = ReadRepoFile(relativePath);

            AssertCancellationCheckBefore(source, prepareMarker);
            AssertCancellationCheckBetween(source, prepareMarker, writeMarker);
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

        [Theory]
        [InlineData("InventoryManagementApp/Services/ImportExport/ItemJsonExporter.cs")]
        [InlineData("InventoryManagementApp/Services/ImportExport/CustomerJsonExporter.cs")]
        public void JsonExportersStreamDirectlyToFileWithoutIntermediateRowCopies(string relativePath)
        {
            var source = ReadRepoFile(relativePath);

            Assert.Contains("await using var stream = new FileStream", source, StringComparison.Ordinal);
            Assert.Contains("JsonSerializer.SerializeAsync(stream, data, JsonOptions, cancellationToken)", source, StringComparison.Ordinal);
            Assert.DoesNotContain("data.ToList()", source, StringComparison.Ordinal);
            Assert.DoesNotContain("JsonSerializer.Serialize(", source, StringComparison.Ordinal);
            Assert.DoesNotContain("File.WriteAllTextAsync", source, StringComparison.Ordinal);
        }

        [Theory]
        [InlineData("InventoryManagementApp/Services/ImportExport/ItemXmlExporter.cs", "WriteStartElement(\"Items\")", "foreach (var item in data)", "serializer.Serialize(writer, item, namespaces)")]
        [InlineData("InventoryManagementApp/Services/ImportExport/CustomerXmlExporter.cs", "WriteStartElement(\"Customers\")", "foreach (var customer in data)", "serializer.Serialize(writer, customer, namespaces)")]
        public void XmlExportersStreamRowsInsideTheExistingRootElement(
            string relativePath,
            string rootMarker,
            string loopMarker,
            string serializeMarker)
        {
            var source = ReadRepoFile(relativePath);

            Assert.Contains("XmlWriter.Create(filePath", source, StringComparison.Ordinal);
            Assert.Contains(rootMarker, source, StringComparison.Ordinal);
            Assert.Contains(loopMarker, source, StringComparison.Ordinal);
            Assert.Contains(serializeMarker, source, StringComparison.Ordinal);
            Assert.Contains("writer.WriteEndElement();", source, StringComparison.Ordinal);
            Assert.DoesNotContain("data.ToList()", source, StringComparison.Ordinal);
            Assert.DoesNotContain("typeof(List<", source, StringComparison.Ordinal);
            Assert.DoesNotContain("new StreamWriter(filePath)", source, StringComparison.Ordinal);
        }

        [Fact]
        public void ItemCsvExporterReusesExistingListInputsBeforeFallingBackToMaterialization()
        {
            var source = ReadRepoFile("InventoryManagementApp/Services/ImportExport/ItemCsvExporter.cs");

            Assert.Contains("var items = data as IList<ItemModel> ?? data.ToList();", source, StringComparison.Ordinal);
            Assert.Contains("CsvHelperUtil.ExportItemsToCsvAsync(filePath, items)", source, StringComparison.Ordinal);
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