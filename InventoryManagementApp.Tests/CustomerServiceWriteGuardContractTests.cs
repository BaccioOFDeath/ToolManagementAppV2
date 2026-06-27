using System;
using System.IO;
using Xunit;

namespace InventoryManagementApp.Tests
{
    public class CustomerServiceWriteGuardContractTests
    {
        [Fact]
        public void CustomerWritesThrowWhenNoRowsAreAffected()
        {
            var source = ReadRepoFile("InventoryManagementApp", "Services", "Customers", "CustomerService.cs");

            AssertWriteGuard(
                source,
                "async Task UpdateCustomerInternalAsync",
                "async Task DeleteCustomerInternalAsync",
                "var updatedRows = await SqliteHelper.ExecuteNonQueryAsync(conn, sql, p, cancellationToken);",
                "EnsureCustomerWriteSucceeded(updatedRows, customer.CustomerID);");
            AssertWriteGuard(
                source,
                "async Task DeleteCustomerInternalAsync",
                "async Task<CustomerModel?> GetCustomerByIDInternalAsync",
                "var deletedRows = await SqliteHelper.ExecuteNonQueryAsync(conn, sql, p, cancellationToken);",
                "EnsureCustomerWriteSucceeded(deletedRows, customerID);");

            Assert.Contains("static void EnsureCustomerWriteSucceeded(int affectedRows, int customerID)", source, StringComparison.Ordinal);
            Assert.Contains("if (affectedRows == 0)", source, StringComparison.Ordinal);
            Assert.Contains("throw new KeyNotFoundException($\"Customer {customerID} not found.\");", source, StringComparison.Ordinal);
        }

        private static void AssertWriteGuard(
            string source,
            string startMarker,
            string endMarker,
            string executeSnippet,
            string guardSnippet)
        {
            var method = ExtractMethod(source, startMarker, endMarker);

            Assert.Contains(executeSnippet, method, StringComparison.Ordinal);
            Assert.Contains(guardSnippet, method, StringComparison.Ordinal);
            Assert.DoesNotContain("                await SqliteHelper.ExecuteNonQueryAsync(conn, sql, p, cancellationToken);", method, StringComparison.Ordinal);
            Assert.True(
                method.IndexOf(executeSnippet, StringComparison.Ordinal) < method.IndexOf(guardSnippet, StringComparison.Ordinal),
                $"Expected {startMarker} to check affected rows after executing the write.");
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
