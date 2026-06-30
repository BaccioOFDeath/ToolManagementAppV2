using System;
using System.IO;
using Xunit;

namespace InventoryManagementApp.Tests
{
    public class UserDirectoryResultCapContractTests
    {
        [Fact]
        public void UserDirectoryCapsResultsAfterDeterministicOrdering()
        {
            var source = ReadRepoFile("InventoryManagementApp", "Services", "Users", "UserService.cs");
            var method = ExtractMethod(
                source,
                "public async Task<List<User>> GetAllUsersAsync",
                "public async Task<int> CountUsersAsync");

            Assert.Contains("private const int MaxUserListCount = 500;", source, StringComparison.Ordinal);
            Assert.Contains("ORDER BY UserName ASC, UserID ASC", method, StringComparison.Ordinal);
            Assert.Contains("LIMIT @UserListLimit", method, StringComparison.Ordinal);
            Assert.Contains("new SqliteParameter(\"@UserListLimit\", MaxUserListCount)", method, StringComparison.Ordinal);

            Assert.True(
                method.IndexOf("ORDER BY UserName ASC, UserID ASC", StringComparison.Ordinal) <
                method.IndexOf("LIMIT @UserListLimit", StringComparison.Ordinal),
                "User directory reads should apply the list cap after deterministic username ordering.");
            Assert.True(
                method.IndexOf("var parameters = new[]", StringComparison.Ordinal) <
                method.IndexOf("SqliteHelper.ExecuteReaderAsync(conn, sql, MapUser, parameters", StringComparison.Ordinal),
                "User directory reads should bind the shared cap explicitly before executing the query.");
        }

        [Fact]
        public void UserCountAndExactLookupRemainUncapped()
        {
            var source = ReadRepoFile("InventoryManagementApp", "Services", "Users", "UserService.cs");
            var countMethod = ExtractMethod(
                source,
                "public async Task<int> CountUsersAsync",
                "public async Task<User?> GetUserByIDAsync");
            var exactLookupMethod = ExtractMethod(
                source,
                "public async Task<User?> GetUserByIDAsync",
                "public async Task<(AuthenticationResult Result, User? User)> AuthenticateUserAsync");

            Assert.Contains("const string sql = \"SELECT COUNT(*) FROM Users\";", countMethod, StringComparison.Ordinal);
            Assert.DoesNotContain("LIMIT", countMethod, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("SELECT * FROM Users WHERE UserID=@ID", exactLookupMethod, StringComparison.Ordinal);
            Assert.DoesNotContain("LIMIT", exactLookupMethod, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public void UserUpdatesUseSharedStaleWriteGuardDirectly()
        {
            var source = ReadRepoFile("InventoryManagementApp", "Services", "Users", "UserService.cs");
            var method = ExtractMethod(
                source,
                "public async Task UpdateUserAsync",
                "public async Task<bool> ChangeUserPasswordAsync");

            Assert.Contains("int rows = await SqliteHelper.ExecuteNonQueryAsync(conn, sql, p);", method, StringComparison.Ordinal);
            Assert.Contains("EnsureUserWriteSucceeded(rows, user.UserID);", method, StringComparison.Ordinal);
            Assert.DoesNotContain("if (rows == 0)\n                    throw new KeyNotFoundException", method, StringComparison.Ordinal);
            Assert.True(
                method.IndexOf("int rows = await SqliteHelper.ExecuteNonQueryAsync(conn, sql, p);", StringComparison.Ordinal) <
                method.IndexOf("EnsureUserWriteSucceeded(rows, user.UserID);", StringComparison.Ordinal),
                "User updates should flow through the shared stale-write guard after capturing affected rows.");
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
