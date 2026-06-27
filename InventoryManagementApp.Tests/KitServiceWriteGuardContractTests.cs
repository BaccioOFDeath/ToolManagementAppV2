using System;
using System.IO;
using Xunit;

namespace InventoryManagementApp.Tests
{
    public class KitServiceWriteGuardContractTests
    {
        [Fact]
        public void KitWritesThrowWhenNoRowsAreAffected()
        {
            var source = ReadRepoFile("InventoryManagementApp", "Services", "Kits", "KitService.cs");

            AssertWriteGuard(
                source,
                "public async Task<bool> UpdateKitAsync",
                "public async Task<bool> DeleteKitAsync",
                "var updatedRows = cmd.ExecuteNonQuery();",
                "EnsureKitWriteSucceeded(updatedRows);");
            AssertWriteGuard(
                source,
                "public async Task<bool> DeleteKitAsync",
                "public async Task<int> AddKitItemAsync",
                "var deletedRows = deleteKitCmd.ExecuteNonQuery();",
                "EnsureKitWriteSucceeded(deletedRows);");

            Assert.Contains("private static void EnsureKitWriteSucceeded(int affectedRows)", source, StringComparison.Ordinal);
            Assert.Contains("throw new InvalidOperationException(\"Kit not found.\");", source, StringComparison.Ordinal);
        }

        [Fact]
        public void KitItemWritesThrowWhenNoRowsAreAffected()
        {
            var source = ReadRepoFile("InventoryManagementApp", "Services", "Kits", "KitService.cs");

            AssertWriteGuard(
                source,
                "public async Task<bool> UpdateKitItemAsync",
                "public async Task<bool> RemoveKitItemAsync",
                "var updatedRows = cmd.ExecuteNonQuery();",
                "EnsureKitItemWriteSucceeded(updatedRows);");
            AssertWriteGuard(
                source,
                "public async Task<bool> RemoveKitItemAsync",
                "public async Task<bool> CheckKitAvailabilityAsync",
                "var removedRows = cmd.ExecuteNonQuery();",
                "EnsureKitItemWriteSucceeded(removedRows);");

            Assert.Contains("private static void EnsureKitItemWriteSucceeded(int affectedRows)", source, StringComparison.Ordinal);
            Assert.Contains("throw new InvalidOperationException(\"Kit item not found.\");", source, StringComparison.Ordinal);
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
            Assert.Contains("return true;", method, StringComparison.Ordinal);
            Assert.DoesNotContain("return cmd.ExecuteNonQuery() > 0;", method, StringComparison.Ordinal);
            Assert.True(
                method.IndexOf(executeSnippet, StringComparison.Ordinal) < method.IndexOf(guardSnippet, StringComparison.Ordinal),
                $"Expected {startMarker} to check affected rows after executing the write.");
            Assert.True(
                method.IndexOf(guardSnippet, StringComparison.Ordinal) < method.IndexOf("return true;", StringComparison.Ordinal),
                $"Expected {startMarker} to fail stale writes before reporting success.");
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
