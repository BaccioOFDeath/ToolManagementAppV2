using System;
using System.IO;
using Xunit;

namespace InventoryManagementApp.Tests
{
    public class CustomerServiceStaleRowContractTests
    {
        [Fact]
        public void UpdateCustomerRejectsInvalidIdsBeforeAuthorizationAndSqlWork()
        {
            var source = ReadRepoFile("InventoryManagementApp", "Services", "Customers", "CustomerService.cs");
            var method = ExtractMethod(
                source,
                "public Task UpdateCustomerAsync(CustomerModel customer",
                "public Task DeleteCustomerAsync(int customerID");

            Assert.Contains("if (customer.CustomerID < 1)", method, StringComparison.Ordinal);
            Assert.Contains("throw new ArgumentOutOfRangeException(nameof(customer), \"Customer ID must be greater than 0.\");", method, StringComparison.Ordinal);
            Assert.True(
                method.IndexOf("if (customer.CustomerID < 1)", StringComparison.Ordinal) < method.IndexOf("_auth.EnsureAdmin()", StringComparison.Ordinal),
                "Invalid customer IDs should be rejected before update authorization and SQL work are reached.");
        }

        [Fact]
        public void UpdateCustomerChecksTargetRowBeforeExecutingUpdate()
        {
            var source = ReadRepoFile("InventoryManagementApp", "Services", "Customers", "CustomerService.cs");
            var method = ExtractMethod(
                source,
                "async Task UpdateCustomerInternalAsync",
                "async Task DeleteCustomerInternalAsync");

            Assert.Contains("await EnsureCustomerRowExistsAsync(conn, customer.CustomerID, cancellationToken);", method, StringComparison.Ordinal);
            Assert.Contains("await SqliteHelper.ExecuteNonQueryAsync(conn, sql, p, cancellationToken);", method, StringComparison.Ordinal);
            Assert.True(
                method.IndexOf("EnsureCustomerRowExistsAsync", StringComparison.Ordinal) < method.IndexOf("ExecuteNonQueryAsync", StringComparison.Ordinal),
                "The stale customer-row guard should run before the update statement executes.");
        }

        [Fact]
        public void DeleteCustomerChecksTargetRowBeforeExecutingDelete()
        {
            var source = ReadRepoFile("InventoryManagementApp", "Services", "Customers", "CustomerService.cs");
            var method = ExtractMethod(
                source,
                "async Task DeleteCustomerInternalAsync",
                "async Task<CustomerModel?> GetCustomerByIDInternalAsync");

            Assert.Contains("await EnsureCustomerRowExistsAsync(conn, customerID, cancellationToken);", method, StringComparison.Ordinal);
            Assert.Contains("await SqliteHelper.ExecuteNonQueryAsync(conn, sql, p, cancellationToken);", method, StringComparison.Ordinal);
            Assert.True(
                method.IndexOf("EnsureCustomerRowExistsAsync", StringComparison.Ordinal) < method.IndexOf("ExecuteNonQueryAsync", StringComparison.Ordinal),
                "The stale customer-row guard should run before the delete statement executes.");
        }

        [Fact]
        public void CustomerRowGuardCountsByCustomerIdAndThrowsClearMissingRowFailure()
        {
            var source = ReadRepoFile("InventoryManagementApp", "Services", "Customers", "CustomerService.cs");
            var method = ExtractMethod(
                source,
                "static async Task EnsureCustomerRowExistsAsync",
                "static string? GetSkipReason");

            Assert.Contains("SELECT COUNT(*) FROM Customers WHERE CustomerID = @CustomerID", method, StringComparison.Ordinal);
            Assert.Contains("new SqliteParameter(\"@CustomerID\", customerID)", method, StringComparison.Ordinal);
            Assert.Contains("if (count == 0)", method, StringComparison.Ordinal);
            Assert.Contains("throw new KeyNotFoundException($\"Customer {customerID} not found.\");", method, StringComparison.Ordinal);
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
