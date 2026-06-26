using System;
using System.IO;
using Xunit;

namespace InventoryManagementApp.Tests
{
    public class UserServiceStaleRowContractTests
    {
        [Fact]
        public void UpdateUserRejectsNullAndInvalidIdsBeforeAuthorizationAndSqlWork()
        {
            var source = ReadRepoFile("InventoryManagementApp", "Services", "Users", "UserService.cs");
            var method = ExtractMethod(
                source,
                "public async Task UpdateUserAsync(User user)",
                "public async Task<bool> ChangeUserPasswordAsync");

            Assert.Contains("if (user is null)", method, StringComparison.Ordinal);
            Assert.Contains("throw new ArgumentNullException(nameof(user));", method, StringComparison.Ordinal);
            Assert.Contains("if (user.UserID < 1)", method, StringComparison.Ordinal);
            Assert.Contains("throw new ArgumentOutOfRangeException(nameof(user), \"User ID must be greater than 0.\");", method, StringComparison.Ordinal);
            Assert.True(
                method.IndexOf("if (user.UserID < 1)", StringComparison.Ordinal) < method.IndexOf("_auth.EnsurePermission(User.PermissionManageUsers)", StringComparison.Ordinal),
                "Invalid user IDs should be rejected before authorization and SQL work are reached.");
        }

        [Fact]
        public void UpdateUserChecksTargetRowBeforePasswordFallbackAndSqlUpdate()
        {
            var source = ReadRepoFile("InventoryManagementApp", "Services", "Users", "UserService.cs");
            var method = ExtractMethod(
                source,
                "public async Task UpdateUserAsync(User user)",
                "public async Task<bool> ChangeUserPasswordAsync");

            Assert.Contains("var existing = await GetUserByIDAsync(user.UserID, CancellationToken.None);", method, StringComparison.Ordinal);
            Assert.Contains("if (existing is null)", method, StringComparison.Ordinal);
            Assert.Contains("throw new KeyNotFoundException($\"User {user.UserID} not found.\");", method, StringComparison.Ordinal);
            Assert.True(
                method.IndexOf("var existing = await GetUserByIDAsync", StringComparison.Ordinal) < method.IndexOf("string hashed = user.PasswordHash;", StringComparison.Ordinal),
                "The stale user-row guard should run before password fallback values are copied.");
            Assert.True(
                method.IndexOf("if (existing is null)", StringComparison.Ordinal) < method.IndexOf("ExecuteNonQueryAsync", StringComparison.Ordinal),
                "The missing-user guard should run before the update statement executes.");
        }

        [Fact]
        public void UpdateUserThrowsOnZeroRowsBeforeMutatingCallerPasswordFields()
        {
            var source = ReadRepoFile("InventoryManagementApp", "Services", "Users", "UserService.cs");
            var method = ExtractMethod(
                source,
                "public async Task UpdateUserAsync(User user)",
                "public async Task<bool> ChangeUserPasswordAsync");

            Assert.Contains("int rows = await SqliteHelper.ExecuteNonQueryAsync(conn, sql, p);", method, StringComparison.Ordinal);
            Assert.Contains("if (rows == 0)", method, StringComparison.Ordinal);
            Assert.Contains("throw new KeyNotFoundException($\"User {user.UserID} not found.\");", method, StringComparison.Ordinal);
            Assert.True(
                method.IndexOf("if (rows == 0)", StringComparison.Ordinal) < method.IndexOf("user.PasswordHash = hashed;", StringComparison.Ordinal),
                "A stale zero-row update should fail before the caller's password fields are rewritten.");
        }

        [Fact]
        public void UserServiceImportsGenericCollectionsForMissingRowFailures()
        {
            var source = ReadRepoFile("InventoryManagementApp", "Services", "Users", "UserService.cs");

            Assert.Contains("using System.Collections.Generic;", source, StringComparison.Ordinal);
            Assert.Contains("throw new KeyNotFoundException($\"User {user.UserID} not found.\");", source, StringComparison.Ordinal);
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
