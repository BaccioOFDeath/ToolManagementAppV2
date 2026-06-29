using System;
using System.IO;
using Xunit;

namespace InventoryManagementApp.Tests
{
    public class KitServiceWriteGuardContractTests
    {
        [Fact]
        public void KitCreateChecksInsertedRowsBeforeReturningId()
        {
            var source = ReadRepoFile("InventoryManagementApp", "Services", "Kits", "KitService.cs");
            var createMethod = ExtractMethod(
                source,
                "public async Task<int> CreateKitAsync",
                "public async Task<bool> UpdateKitAsync");

            AssertCreateGuard(
                createMethod,
                "EnsureKitCreateSucceeded(insertedRows);",
                "Unable to create kit.");
            Assert.Contains("private static void EnsureKitCreateSucceeded(int affectedRows)", source, StringComparison.Ordinal);
        }

        [Fact]
        public void KitItemCreateChecksInsertedRowsBeforeReturningId()
        {
            var source = ReadRepoFile("InventoryManagementApp", "Services", "Kits", "KitService.cs");
            var createMethod = ExtractMethod(
                source,
                "public async Task<int> AddKitItemAsync",
                "public async Task<bool> UpdateKitItemAsync");

            AssertCreateGuard(
                createMethod,
                "EnsureKitItemCreateSucceeded(insertedRows);",
                "Unable to add kit item.");
            Assert.Contains("private static void EnsureKitItemCreateSucceeded(int affectedRows)", source, StringComparison.Ordinal);
        }

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

        private static void AssertCreateGuard(
            string createMethod,
            string guardSnippet,
            string failureMessage)
        {
            Assert.Contains("var insertedRows = cmd.ExecuteNonQuery();", createMethod, StringComparison.Ordinal);
            Assert.Contains(guardSnippet, createMethod, StringComparison.Ordinal);
            Assert.Contains("using var idCmd = new SqliteCommand(\"SELECT last_insert_rowid();\", conn);", createMethod, StringComparison.Ordinal);
            Assert.Contains("if (id < 1)", createMethod, StringComparison.Ordinal);
            Assert.Contains($"throw new InvalidOperationException(\"{failureMessage}\");", createMethod, StringComparison.Ordinal);
            Assert.Contains("return id;", createMethod, StringComparison.Ordinal);
            Assert.DoesNotContain("cmd.ExecuteScalar()", createMethod, StringComparison.Ordinal);
            Assert.DoesNotContain("SELECT last_insert_rowid();\";", createMethod, StringComparison.Ordinal);

            Assert.True(
                createMethod.IndexOf("var insertedRows = cmd.ExecuteNonQuery();", StringComparison.Ordinal) <
                createMethod.IndexOf(guardSnippet, StringComparison.Ordinal),
                "Expected create methods to inspect affected rows immediately after executing the insert.");
            Assert.True(
                createMethod.IndexOf(guardSnippet, StringComparison.Ordinal) <
                createMethod.IndexOf("using var idCmd = new SqliteCommand(\"SELECT last_insert_rowid();\", conn);", StringComparison.Ordinal),
                "Expected failed creates to stop before reading the inserted id.");
            Assert.True(
                createMethod.IndexOf("if (id < 1)", StringComparison.Ordinal) <
                createMethod.IndexOf("return id;", StringComparison.Ordinal),
                "Expected create methods to reject invalid inserted ids before reporting success.");
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