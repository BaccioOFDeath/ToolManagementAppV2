using System;
using System.IO;
using Xunit;

namespace InventoryManagementApp.Tests
{
    public class CustomerImportExportEntrypointContractTests
    {
        [Fact]
        public void GenericCustomerImportAndExportValidateSetupBeforeAuthorizationAndWork()
        {
            var source = ReadRepoFile("InventoryManagementApp", "Services", "Customers", "CustomerService.cs");
            var importMethod = ExtractMethod(
                source,
                "public async Task<int> ImportCustomersAsync",
                "public async Task ExportCustomersAsync");
            var exportMethod = ExtractMethod(
                source,
                "public async Task ExportCustomersAsync",
                "static void NotifyChanged");

            Assert.Contains("if (string.IsNullOrWhiteSpace(filePath))", importMethod, StringComparison.Ordinal);
            Assert.Contains("throw new ArgumentNullException(nameof(filePath));", importMethod, StringComparison.Ordinal);
            Assert.Contains("if (importer is null)", importMethod, StringComparison.Ordinal);
            Assert.Contains("throw new ArgumentNullException(nameof(importer));", importMethod, StringComparison.Ordinal);
            Assert.True(
                importMethod.IndexOf("if (string.IsNullOrWhiteSpace(filePath))", StringComparison.Ordinal) < importMethod.IndexOf("_auth.EnsureAdmin();", StringComparison.Ordinal),
                "Generic customer imports should reject a missing file path before authorization or importer work.");
            Assert.True(
                importMethod.IndexOf("if (importer is null)", StringComparison.Ordinal) < importMethod.IndexOf("_auth.EnsureAdmin();", StringComparison.Ordinal),
                "Generic customer imports should reject a missing importer before authorization or importer work.");
            Assert.True(
                importMethod.IndexOf("_auth.EnsureAdmin();", StringComparison.Ordinal) < importMethod.IndexOf("importer.ImportAsync", StringComparison.Ordinal),
                "Generic customer imports should still authorize before importer file work starts.");

            Assert.Contains("if (string.IsNullOrWhiteSpace(filePath))", exportMethod, StringComparison.Ordinal);
            Assert.Contains("throw new ArgumentNullException(nameof(filePath));", exportMethod, StringComparison.Ordinal);
            Assert.Contains("if (exporter is null)", exportMethod, StringComparison.Ordinal);
            Assert.Contains("throw new ArgumentNullException(nameof(exporter));", exportMethod, StringComparison.Ordinal);
            Assert.True(
                exportMethod.IndexOf("if (string.IsNullOrWhiteSpace(filePath))", StringComparison.Ordinal) < exportMethod.IndexOf("_auth.EnsurePermission", StringComparison.Ordinal),
                "Generic customer exports should reject a missing file path before authorization or row collection.");
            Assert.True(
                exportMethod.IndexOf("if (exporter is null)", StringComparison.Ordinal) < exportMethod.IndexOf("_auth.EnsurePermission", StringComparison.Ordinal),
                "Generic customer exports should reject a missing exporter before authorization or row collection.");
            Assert.True(
                exportMethod.IndexOf("_auth.EnsurePermission", StringComparison.Ordinal) < exportMethod.IndexOf("CollectCustomersForExportAsync", StringComparison.Ordinal),
                "Generic customer exports should still authorize before collecting customer rows.");
            Assert.True(
                exportMethod.IndexOf("CollectCustomersForExportAsync", StringComparison.Ordinal) < exportMethod.IndexOf("exporter.ExportAsync", StringComparison.Ordinal),
                "Generic customer exports should collect rows before exporter handoff.");
        }

        [Fact]
        public void CustomerExportsCollectRowsWithBoundedPagesBeforeWriterWork()
        {
            var source = ReadRepoFile("InventoryManagementApp", "Services", "Customers", "CustomerService.cs");
            var csvExportMethod = ExtractMethod(
                source,
                "async Task ExportCustomersToCsvInternalAsync",
                "async Task<List<CustomerModel>> CollectCustomersForExportAsync");
            var collectorMethod = ExtractMethod(
                source,
                "async Task<List<CustomerModel>> CollectCustomersForExportAsync",
                "async Task InsertCustomerAsync");
            var genericExportMethod = ExtractMethod(
                source,
                "public async Task ExportCustomersAsync",
                "static void NotifyChanged");

            Assert.Contains("private const int CustomerExportPageSize = 500;", source, StringComparison.Ordinal);
            Assert.Contains("var offset = 0;", collectorMethod, StringComparison.Ordinal);
            Assert.Contains("while (true)", collectorMethod, StringComparison.Ordinal);
            Assert.Contains("LIMIT @CustomerExportPageSize OFFSET @CustomerExportOffset", collectorMethod, StringComparison.Ordinal);
            Assert.Contains("ORDER BY Company ASC, Contact ASC, CustomerID ASC", collectorMethod, StringComparison.Ordinal);
            Assert.Contains("new SqliteParameter(\"@CustomerExportPageSize\", CustomerExportPageSize)", collectorMethod, StringComparison.Ordinal);
            Assert.Contains("new SqliteParameter(\"@CustomerExportOffset\", offset)", collectorMethod, StringComparison.Ordinal);
            Assert.Contains("customers.AddRange(page);", collectorMethod, StringComparison.Ordinal);
            Assert.Contains("if (page.Count < CustomerExportPageSize)", collectorMethod, StringComparison.Ordinal);
            Assert.Contains("offset += CustomerExportPageSize;", collectorMethod, StringComparison.Ordinal);
            Assert.DoesNotContain("GetAllCustomersInternalAsync", csvExportMethod, StringComparison.Ordinal);
            Assert.DoesNotContain("GetAllCustomersAsync", genericExportMethod, StringComparison.Ordinal);

            Assert.True(
                csvExportMethod.IndexOf("var all = await CollectCustomersForExportAsync(cancellationToken)", StringComparison.Ordinal) < csvExportMethod.IndexOf("CsvHelperUtil.ExportCustomersToCsv", StringComparison.Ordinal),
                "CSV customer exports should finish bounded collection before handing rows to the CSV writer.");
            Assert.True(
                genericExportMethod.IndexOf("var all = await CollectCustomersForExportAsync(cancellationToken)", StringComparison.Ordinal) < genericExportMethod.IndexOf("exporter.ExportAsync", StringComparison.Ordinal),
                "Generic customer exports should finish bounded collection before handing rows to the exporter.");
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
