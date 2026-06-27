using System;
using System.IO;
using Xunit;

namespace InventoryManagementApp.Tests
{
    public class UserServiceDeleteGuardContractTests
    {
        [Fact]
        public void DeleteUserInternalKeepsLastAdminGuardInFinalDeleteStatement()
        {
            var source = ReadRepoFile("InventoryManagementApp", "Services", "Users", "UserService.cs");
            var method = ExtractMethod(
                source,
                "async Task<bool> DeleteUserInternalAsync",
                "public async Task<bool> TryDeleteUserAsync");

            Assert.Contains("DELETE FROM Users", method, StringComparison.Ordinal);
            Assert.Contains("WHERE UserID=@ID", method, StringComparison.Ordinal);
            Assert.Contains("AND (IsAdmin = 0 OR (SELECT COUNT(*) FROM Users WHERE IsAdmin = 1) > 1)", method, StringComparison.Ordinal);
            Assert.Contains("var deletedRows = await SqliteHelper.ExecuteNonQueryAsync", method, StringComparison.Ordinal);
            Assert.Contains("return deletedRows > 0;", method, StringComparison.Ordinal);

            Assert.True(
                method.IndexOf("AND (IsAdmin = 0 OR (SELECT COUNT(*) FROM Users WHERE IsAdmin = 1) > 1)", StringComparison.Ordinal) <
                method.IndexOf("var deletedRows = await SqliteHelper.ExecuteNonQueryAsync", StringComparison.Ordinal),
                "The last-admin guard should be part of the final delete command, not only the caller pre-check.");
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
