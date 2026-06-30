using System;
using System.IO;
using Xunit;

namespace InventoryManagementApp.Tests
{
    public class KitServiceQueryGuardContractTests
    {
        [Fact]
        public void KitDirectoryQueriesAreCappedWithSharedLimit()
        {
            var source = ReadRepoFile("InventoryManagementApp", "Services", "Kits", "KitService.cs");

            AssertContainsAll(
                source,
                "private const int MaxKitListCount = 500;",
                "cmd.Parameters.AddWithValue(\"@KitListLimit\", MaxKitListCount);");

            AssertCappedQuery(
                ExtractMethod(
                    source,
                    "public async Task<List<Kit>> GetAllKitsAsync()",
                    "public async Task<List<Kit>> GetActiveKitsAsync()"),
                "ORDER BY Name ASC",
                "LIMIT @KitListLimit",
                "cmd.Parameters.AddWithValue(\"@KitListLimit\", MaxKitListCount);");

            AssertCappedQuery(
                ExtractMethod(
                    source,
                    "public async Task<List<Kit>> GetActiveKitsAsync()",
                    "public async Task<Kit?> GetKitByIdAsync(int kitID)"),
                "ORDER BY Name ASC",
                "LIMIT @KitListLimit",
                "cmd.Parameters.AddWithValue(\"@KitListLimit\", MaxKitListCount);");
        }

        [Fact]
        public void KitMembershipQueryIsCappedAfterParentValidation()
        {
            var source = ReadRepoFile("InventoryManagementApp", "Services", "Kits", "KitService.cs");

            AssertContainsAll(
                source,
                "private const int MaxKitItemListCount = 500;",
                "cmd.Parameters.AddWithValue(\"@KitItemListLimit\", MaxKitItemListCount);");

            var method = ExtractMethod(
                source,
                "public async Task<List<KitItem>> GetKitItemsAsync(int kitID)",
                "public async Task<int> CreateKitAsync(Kit kit)");

            AssertContainsAll(
                method,
                "if (kitID < 1)",
                "throw new ArgumentOutOfRangeException(nameof(kitID), \"Kit ID must be greater than 0.\");",
                "using var conn = _databaseService.CreateConnection();",
                "EnsureKitExists(conn, kitID);",
                "WHERE ki.KitID = @KitID",
                "ORDER BY i.ItemNumber",
                "LIMIT @KitItemListLimit",
                "cmd.Parameters.AddWithValue(\"@KitID\", kitID);",
                "cmd.Parameters.AddWithValue(\"@KitItemListLimit\", MaxKitItemListCount);");

            Assert.True(
                method.IndexOf("EnsureKitExists(conn, kitID);", StringComparison.Ordinal) <
                method.IndexOf("var sql = @\"", StringComparison.Ordinal),
                "Expected kit item membership reads to confirm the parent kit exists before building/executing the membership query.");
            Assert.True(
                method.IndexOf("ORDER BY i.ItemNumber", StringComparison.Ordinal) <
                method.IndexOf("LIMIT @KitItemListLimit", StringComparison.Ordinal),
                "Expected kit membership caps to apply after ordering so the visible membership list remains deterministic.");
            Assert.True(
                method.IndexOf("LIMIT @KitItemListLimit", StringComparison.Ordinal) <
                method.IndexOf("cmd.Parameters.AddWithValue(\"@KitItemListLimit\", MaxKitItemListCount);", StringComparison.Ordinal),
                "Expected the kit item list cap SQL placeholder to be bound before the query is executed.");
        }

        private static void AssertContainsAll(string source, params string[] expectedSnippets)
        {
            foreach (var snippet in expectedSnippets)
            {
                Assert.Contains(snippet, source, StringComparison.Ordinal);
            }
        }

        private static void AssertCappedQuery(string method, string orderBySnippet, string limitSnippet, string parameterSnippet)
        {
            AssertContainsAll(method, orderBySnippet, limitSnippet, parameterSnippet);
            Assert.True(
                method.IndexOf(orderBySnippet, StringComparison.Ordinal) <
                method.IndexOf(limitSnippet, StringComparison.Ordinal),
                "Expected list cap to be applied after the query ordering so the visible rows remain deterministic.");
            Assert.True(
                method.IndexOf(limitSnippet, StringComparison.Ordinal) <
                method.IndexOf(parameterSnippet, StringComparison.Ordinal),
                "Expected the list cap SQL placeholder to be bound before the query is executed.");
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
