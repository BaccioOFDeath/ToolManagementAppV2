using System;
using System.IO;
using Xunit;

namespace InventoryManagementApp.Tests
{
    public class CustomerSearchResultCapContractTests
    {
        [Fact]
        public void CustomerSearchCapsInteractiveResultsAfterDeterministicOrdering()
        {
            var source = ReadRepoFile("InventoryManagementApp", "Services", "Customers", "CustomerService.cs");
            var searchMethod = ExtractMethod(
                source,
                "async Task<List<CustomerModel>> SearchCustomersInternalAsync",
                "async Task<CustomerImportResult> ImportCustomersFromCsvInternalAsync");

            Assert.Contains("private const int MaxCustomerSearchResults = 500;", source, StringComparison.Ordinal);
            Assert.Contains("ORDER BY Company ASC, Contact ASC, CustomerID ASC", searchMethod, StringComparison.Ordinal);
            Assert.Contains("LIMIT @CustomerSearchLimit", searchMethod, StringComparison.Ordinal);
            Assert.Contains("new SqliteParameter(\"@CustomerSearchLimit\", MaxCustomerSearchResults)", searchMethod, StringComparison.Ordinal);

            Assert.True(
                searchMethod.IndexOf("ORDER BY Company ASC, Contact ASC, CustomerID ASC", StringComparison.Ordinal) <
                searchMethod.IndexOf("LIMIT @CustomerSearchLimit", StringComparison.Ordinal),
                "Customer search should apply the interactive cap after deterministic directory ordering.");
            Assert.True(
                searchMethod.IndexOf("new SqliteParameter(\"@t\", $\"%{searchTerm}%\")", StringComparison.Ordinal) <
                searchMethod.IndexOf("new SqliteParameter(\"@CustomerSearchLimit\", MaxCustomerSearchResults)", StringComparison.Ordinal),
                "Customer search should bind the search term and the shared cap explicitly.");
        }

        [Fact]
        public void CustomerExportsKeepFullCustomerReadUncapped()
        {
            var source = ReadRepoFile("InventoryManagementApp", "Services", "Customers", "CustomerService.cs");
            var allCustomersMethod = ExtractMethod(
                source,
                "async Task<List<CustomerModel>> GetAllCustomersInternalAsync",
                "async Task<int> CountCustomersInternalAsync");
            var csvExportMethod = ExtractMethod(
                source,
                "async Task ExportCustomersToCsvInternalAsync",
                "async Task InsertCustomerAsync");
            var genericExportMethod = ExtractMethod(
                source,
                "public async Task ExportCustomersAsync",
                "static void NotifyChanged");

            Assert.Contains("const string sql = \"SELECT * FROM Customers\";", allCustomersMethod, StringComparison.Ordinal);
            Assert.DoesNotContain("LIMIT", allCustomersMethod, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("var all = await GetAllCustomersInternalAsync(cancellationToken);", csvExportMethod, StringComparison.Ordinal);
            Assert.Contains("var all = await GetAllCustomersAsync(cancellationToken).ConfigureAwait(false);", genericExportMethod, StringComparison.Ordinal);
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
