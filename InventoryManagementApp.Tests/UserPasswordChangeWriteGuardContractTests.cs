using System;
using System.IO;
using Xunit;

namespace InventoryManagementApp.Tests
{
    public class UserPasswordChangeWriteGuardContractTests
    {
        [Fact]
        public void ChangePasswordGuardsStaleWritesBeforeReportingSuccess()
        {
            var source = ReadRepoFile("InventoryManagementApp", "Services", "Users", "UserService.cs");
            var method = ExtractMethod(
                source,
                "public async Task<bool> ChangeUserPasswordAsync",
                "async Task<bool> DeleteUserInternalAsync");

            Assert.Contains("var existing = await GetUserByIDAsync(userID, CancellationToken.None);", method, StringComparison.Ordinal);
            Assert.Contains("if (existing is null)", method, StringComparison.Ordinal);
            Assert.Contains("return false;", method, StringComparison.Ordinal);
            Assert.Contains("int rows = await SqliteHelper.ExecuteNonQueryAsync(conn, sql, p);", method, StringComparison.Ordinal);
            Assert.Contains("EnsureUserWriteSucceeded(rows, userID);", method, StringComparison.Ordinal);
            Assert.Contains("return true;", method, StringComparison.Ordinal);
            Assert.DoesNotContain("return rows > 0;", method, StringComparison.Ordinal);
            Assert.DoesNotContain("Password update affected 0 rows", method, StringComparison.Ordinal);

            Assert.True(
                method.IndexOf("if (existing is null)", StringComparison.Ordinal) < method.IndexOf("PasswordValidator.IsValid", StringComparison.Ordinal),
                "Missing password-change users should keep the friendly false result before validation and hashing work.");
            Assert.True(
                method.IndexOf("int rows = await SqliteHelper.ExecuteNonQueryAsync(conn, sql, p);", StringComparison.Ordinal) < method.IndexOf("EnsureUserWriteSucceeded(rows, userID);", StringComparison.Ordinal),
                "Password changes should capture affected rows before checking the update result.");
            Assert.True(
                method.IndexOf("EnsureUserWriteSucceeded(rows, userID);", StringComparison.Ordinal) < method.IndexOf("return true;", StringComparison.Ordinal),
                "Stale password-change writes should fail before the workflow reports success.");
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
