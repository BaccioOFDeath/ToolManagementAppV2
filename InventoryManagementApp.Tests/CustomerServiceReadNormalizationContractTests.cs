using System;
using System.IO;
using Xunit;

namespace InventoryManagementApp.Tests
{
    public class CustomerServiceReadNormalizationContractTests
    {
        [Fact]
        public void CustomerMapperNormalizesAllCustomerDisplayFields()
        {
            var source = ReadRepoFile("InventoryManagementApp", "Services", "Customers", "CustomerService.cs");
            var mapper = ExtractMethod(
                source,
                "CustomerModel MapCustomer(IDataRecord r)",
                "public async Task<int> ImportCustomersAsync");

            Assert.Contains("CustomerID = Convert.ToInt32(r[\"CustomerID\"])", mapper, StringComparison.Ordinal);
            Assert.Contains("Company = NormalizeCustomerReadText(r[\"Company\"]?.ToString())", mapper, StringComparison.Ordinal);
            Assert.Contains("Email = NormalizeCustomerReadText(r[\"Email\"]?.ToString())", mapper, StringComparison.Ordinal);
            Assert.Contains("Contact = NormalizeCustomerReadText(r[\"Contact\"]?.ToString())", mapper, StringComparison.Ordinal);
            Assert.Contains("Phone = NormalizeCustomerReadText(r[\"Phone\"]?.ToString())", mapper, StringComparison.Ordinal);
            Assert.Contains("Mobile = NormalizeCustomerReadText(r[\"Mobile\"]?.ToString())", mapper, StringComparison.Ordinal);
            Assert.Contains("Address = NormalizeCustomerReadText(r[\"Address\"]?.ToString())", mapper, StringComparison.Ordinal);

            Assert.DoesNotContain("r[\"Company\"]?.ToString() ?? string.Empty", mapper, StringComparison.Ordinal);
            Assert.DoesNotContain("r[\"Email\"]?.ToString() ?? string.Empty", mapper, StringComparison.Ordinal);
            Assert.DoesNotContain("r[\"Contact\"]?.ToString() ?? string.Empty", mapper, StringComparison.Ordinal);
            Assert.DoesNotContain("r[\"Phone\"]?.ToString() ?? string.Empty", mapper, StringComparison.Ordinal);
            Assert.DoesNotContain("r[\"Mobile\"]?.ToString() ?? string.Empty", mapper, StringComparison.Ordinal);
            Assert.DoesNotContain("r[\"Address\"]?.ToString() ?? string.Empty", mapper, StringComparison.Ordinal);
        }

        [Fact]
        public void CustomerReadNormalizerTrimsLegacyTextAndPreservesEmptyFallback()
        {
            var source = ReadRepoFile("InventoryManagementApp", "Services", "Customers", "CustomerService.cs");

            Assert.Contains("static string NormalizeCustomerReadText(string? value) => value?.Trim() ?? string.Empty;", source, StringComparison.Ordinal);
            Assert.Contains("static string? NormalizeImportedText(string? value) => value?.Trim();", source, StringComparison.Ordinal);
        }

        [Fact]
        public void CustomerReadAndExportPathsUseSharedMapper()
        {
            var source = ReadRepoFile("InventoryManagementApp", "Services", "Customers", "CustomerService.cs");

            AssertReaderUsesMapper(
                source,
                "async Task<CustomerModel?> GetCustomerByIDInternalAsync",
                "async Task<List<CustomerModel>> GetAllCustomersInternalAsync");
            AssertReaderUsesMapper(
                source,
                "async Task<List<CustomerModel>> GetAllCustomersInternalAsync",
                "async Task<int> CountCustomersInternalAsync");
            AssertReaderUsesMapper(
                source,
                "async Task<List<CustomerModel>> SearchCustomersInternalAsync",
                "async Task<CustomerImportResult> ImportCustomersFromCsvInternalAsync");
            AssertReaderUsesMapper(
                source,
                "async Task<List<CustomerModel>> CollectCustomersForExportAsync",
                "async Task InsertCustomerAsync");
        }

        [Fact]
        public void CustomerExportEntrypointsCollectNormalizedReadModels()
        {
            var source = ReadRepoFile("InventoryManagementApp", "Services", "Customers", "CustomerService.cs");
            var csvExport = ExtractMethod(
                source,
                "async Task ExportCustomersToCsvInternalAsync",
                "async Task<List<CustomerModel>> CollectCustomersForExportAsync");
            var genericExport = ExtractMethod(
                source,
                "public async Task ExportCustomersAsync",
                "static void NotifyChanged");

            Assert.Contains("var all = await CollectCustomersForExportAsync(cancellationToken).ConfigureAwait(false);", csvExport, StringComparison.Ordinal);
            Assert.Contains("CsvHelperUtil.ExportCustomersToCsv(filePath, all);", csvExport, StringComparison.Ordinal);
            Assert.Contains("var all = await CollectCustomersForExportAsync(cancellationToken).ConfigureAwait(false);", genericExport, StringComparison.Ordinal);
            Assert.Contains("await exporter.ExportAsync(filePath, all, cancellationToken).ConfigureAwait(false);", genericExport, StringComparison.Ordinal);
        }

        private static void AssertReaderUsesMapper(string source, string startMarker, string endMarker)
        {
            var method = ExtractMethod(source, startMarker, endMarker);
            Assert.Contains("MapCustomer", method, StringComparison.Ordinal);
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
