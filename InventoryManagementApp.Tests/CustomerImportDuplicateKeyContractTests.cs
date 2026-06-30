using System;
using System.IO;
using Xunit;

namespace InventoryManagementApp.Tests
{
    public class CustomerImportDuplicateKeyContractTests
    {
        [Fact]
        public void PersistedDuplicateLookupIgnoresBlankPhoneAndMobileKeys()
        {
            var source = ReadRepoFile("InventoryManagementApp", "Services", "Customers", "CustomerService.cs");
            var method = ExtractMethod(
                source,
                "async Task<bool> CustomerExistsAsync",
                "static async Task EnsureCustomerRowExistsAsync");

            Assert.Contains("WHERE Contact = @Contact", method, StringComparison.Ordinal);
            Assert.Contains("(@Phone <> '' AND Phone = @Phone)", method, StringComparison.Ordinal);
            Assert.Contains("OR (@Mobile <> '' AND Mobile = @Mobile)", method, StringComparison.Ordinal);
            Assert.DoesNotContain("Phone = @Phone OR Mobile = @Mobile", method, StringComparison.Ordinal);
            Assert.Contains("new SqliteCommand(sql, conn, transaction)", method, StringComparison.Ordinal);
            Assert.DoesNotContain("_dbService.CreateConnection()", method, StringComparison.Ordinal);
        }

        [Fact]
        public void CustomerImportsCheckPersistedDuplicatesBeforeBatchReservationsAndInsertWork()
        {
            var source = ReadRepoFile("InventoryManagementApp", "Services", "Customers", "CustomerService.cs");
            var csvImportMethod = ExtractMethod(
                source,
                "async Task<CustomerImportResult> ImportCustomersFromCsvInternalAsync",
                "async Task ExportCustomersToCsvInternalAsync");
            var genericImportMethod = ExtractMethod(
                source,
                "public async Task<int> ImportCustomersAsync",
                "public async Task ExportCustomersAsync");

            AssertImportOrdering(
                csvImportMethod,
                "CustomerExistsAsync(conn, tran, c.Contact, c.Phone, c.Mobile, cancellationToken)",
                "if (!TryReserveImportedCustomer(importedCustomerKeys, c))",
                "await InsertCustomerAsync(conn, tran, c, cancellationToken);");
            AssertImportOrdering(
                genericImportMethod,
                "CustomerExistsAsync(conn, transaction, customerModel.Contact, customerModel.Phone, customerModel.Mobile, cancellationToken)",
                "if (!TryReserveImportedCustomer(importedCustomerKeys, customerModel))",
                "await InsertCustomerAsync(conn, transaction, customerModel, cancellationToken);");
        }

        private static void AssertImportOrdering(string method, string duplicateLookup, string batchReservation, string insertCall)
        {
            var lookupIndex = method.IndexOf(duplicateLookup, StringComparison.Ordinal);
            var reservationIndex = method.IndexOf(batchReservation, StringComparison.Ordinal);
            var insertIndex = method.IndexOf(insertCall, StringComparison.Ordinal);

            Assert.True(lookupIndex >= 0, $"Expected duplicate lookup call: {duplicateLookup}");
            Assert.True(reservationIndex >= 0, $"Expected batch reservation call: {batchReservation}");
            Assert.True(insertIndex >= 0, $"Expected insert call: {insertCall}");
            Assert.True(lookupIndex < reservationIndex, "Imports should check persisted duplicates before reserving identities for the current file or batch.");
            Assert.True(reservationIndex < insertIndex, "Imports should reserve nonblank duplicate keys before insert work.");
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
