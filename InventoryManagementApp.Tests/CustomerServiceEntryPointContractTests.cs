using System;
using System.IO;
using Xunit;

namespace InventoryManagementApp.Tests
{
    public class CustomerServiceEntryPointContractTests
    {
        [Fact]
        public void CustomerCrudAndQueryHelpersHonorCancellationBeforeSqlWork()
        {
            var source = ReadRepoFile("InventoryManagementApp", "Services", "Customers", "CustomerService.cs");

            AssertCancellationGuardBeforeConnection(
                source,
                "async Task AddCustomerInternalAsync",
                "async Task UpdateCustomerInternalAsync");
            AssertCancellationGuardBeforeSqlAndConnection(
                source,
                "async Task UpdateCustomerInternalAsync",
                "async Task DeleteCustomerInternalAsync");
            AssertCancellationGuardBeforeSqlAndConnection(
                source,
                "async Task DeleteCustomerInternalAsync",
                "async Task<CustomerModel?> GetCustomerByIDInternalAsync");
            AssertCancellationGuardBeforeSqlAndConnection(
                source,
                "async Task<CustomerModel?> GetCustomerByIDInternalAsync",
                "async Task<List<CustomerModel>> GetAllCustomersInternalAsync");
            AssertCancellationGuardBeforeSqlAndConnection(
                source,
                "async Task<List<CustomerModel>> GetAllCustomersInternalAsync",
                "async Task<int> CountCustomersInternalAsync");
            AssertCancellationGuardBeforeSqlAndConnection(
                source,
                "async Task<int> CountCustomersInternalAsync",
                "async Task<List<CustomerModel>> SearchCustomersInternalAsync");
            AssertCancellationGuardBeforeSqlAndConnection(
                source,
                "async Task<List<CustomerModel>> SearchCustomersInternalAsync",
                "async Task<CustomerImportResult> ImportCustomersFromCsvInternalAsync");
        }

        [Fact]
        public void CustomerImportAndExportHelpersHonorCancellationBeforeDatabaseWork()
        {
            var source = ReadRepoFile("InventoryManagementApp", "Services", "Customers", "CustomerService.cs");

            var csvImportMethod = ExtractMethod(
                source,
                "async Task<CustomerImportResult> ImportCustomersFromCsvInternalAsync",
                "async Task ExportCustomersToCsvInternalAsync");
            Assert.Contains("cancellationToken.ThrowIfCancellationRequested();", csvImportMethod, StringComparison.Ordinal);
            Assert.True(
                csvImportMethod.IndexOf("cancellationToken.ThrowIfCancellationRequested();", StringComparison.Ordinal) < csvImportMethod.IndexOf("CsvHelperUtil.LoadCustomersFromCsvAsync", StringComparison.Ordinal),
                "CSV import should honor already-cancelled callers before file or database work begins.");
            Assert.True(
                csvImportMethod.LastIndexOf("cancellationToken.ThrowIfCancellationRequested();", csvImportMethod.IndexOf("using var conn = _dbService.CreateConnection();", StringComparison.Ordinal), StringComparison.Ordinal) >= 0,
                "CSV import should re-check cancellation before opening the import transaction connection.");

            AssertCancellationGuardBeforeMethodCall(
                source,
                "async Task ExportCustomersToCsvInternalAsync",
                "async Task InsertCustomerAsync",
                "GetAllCustomersInternalAsync");
            AssertCancellationGuardBeforeSqlAndConnection(
                source,
                "async Task InsertCustomerAsync",
                "async Task<bool> CustomerExistsAsync");
            AssertCancellationGuardBeforeSqlAndConnection(
                source,
                "async Task<bool> CustomerExistsAsync",
                "static async Task EnsureCustomerRowExistsAsync");
            AssertCancellationGuardBeforeSql(
                source,
                "static async Task EnsureCustomerRowExistsAsync",
                "static string? GetSkipReason");
        }

        [Fact]
        public void AlternateCustomerImportExportEntrypointsHonorCancellationBeforeConnectionOrExporterWork()
        {
            var source = ReadRepoFile("InventoryManagementApp", "Services", "Customers", "CustomerService.cs");

            var importMethod = ExtractMethod(
                source,
                "public async Task<int> ImportCustomersAsync",
                "public async Task ExportCustomersAsync");
            Assert.Contains("cancellationToken.ThrowIfCancellationRequested();", importMethod, StringComparison.Ordinal);
            Assert.True(
                importMethod.IndexOf("cancellationToken.ThrowIfCancellationRequested();", StringComparison.Ordinal) < importMethod.IndexOf("importer.ImportAsync", StringComparison.Ordinal),
                "Generic customer import should honor already-cancelled callers before importer work begins.");
            Assert.True(
                importMethod.LastIndexOf("cancellationToken.ThrowIfCancellationRequested();", importMethod.IndexOf("using var conn = _dbService.CreateConnection();", StringComparison.Ordinal), StringComparison.Ordinal) >= 0,
                "Generic customer import should re-check cancellation before opening the database connection.");

            AssertCancellationGuardBeforeMethodCall(
                source,
                "public async Task ExportCustomersAsync",
                "    }\n}",
                "GetAllCustomersAsync");
        }

        private static void AssertCancellationGuardBeforeSqlAndConnection(string source, string startMarker, string endMarker)
        {
            var method = ExtractMethod(source, startMarker, endMarker);

            Assert.Contains("cancellationToken.ThrowIfCancellationRequested();", method, StringComparison.Ordinal);
            Assert.True(
                method.IndexOf("cancellationToken.ThrowIfCancellationRequested();", StringComparison.Ordinal) < method.IndexOf("const string sql", StringComparison.Ordinal),
                $"Expected {startMarker} to honor cancellation before SQL construction.");
            Assert.True(
                method.IndexOf("cancellationToken.ThrowIfCancellationRequested();", StringComparison.Ordinal) < method.IndexOf("CreateConnection", StringComparison.Ordinal),
                $"Expected {startMarker} to honor cancellation before opening a database connection.");
        }

        private static void AssertCancellationGuardBeforeConnection(string source, string startMarker, string endMarker)
        {
            var method = ExtractMethod(source, startMarker, endMarker);

            Assert.Contains("cancellationToken.ThrowIfCancellationRequested();", method, StringComparison.Ordinal);
            Assert.True(
                method.IndexOf("cancellationToken.ThrowIfCancellationRequested();", StringComparison.Ordinal) < method.IndexOf("CreateConnection", StringComparison.Ordinal),
                $"Expected {startMarker} to honor cancellation before opening a database connection.");
        }

        private static void AssertCancellationGuardBeforeSql(string source, string startMarker, string endMarker)
        {
            var method = ExtractMethod(source, startMarker, endMarker);

            Assert.Contains("cancellationToken.ThrowIfCancellationRequested();", method, StringComparison.Ordinal);
            Assert.True(
                method.IndexOf("cancellationToken.ThrowIfCancellationRequested();", StringComparison.Ordinal) < method.IndexOf("const string sql", StringComparison.Ordinal),
                $"Expected {startMarker} to honor cancellation before SQL construction.");
        }

        private static void AssertCancellationGuardBeforeMethodCall(string source, string startMarker, string endMarker, string methodCall)
        {
            var method = ExtractMethod(source, startMarker, endMarker);

            Assert.Contains("cancellationToken.ThrowIfCancellationRequested();", method, StringComparison.Ordinal);
            Assert.True(
                method.IndexOf("cancellationToken.ThrowIfCancellationRequested();", StringComparison.Ordinal) < method.IndexOf(methodCall, StringComparison.Ordinal),
                $"Expected {startMarker} to honor cancellation before {methodCall}.");
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
