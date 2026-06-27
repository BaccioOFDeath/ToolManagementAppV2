using System;
using System.IO;
using Xunit;

namespace InventoryManagementApp.Tests
{
    public class UserServiceEntryPointContractTests
    {
        [Fact]
        public void UserQueriesHonorCancellationBeforeSqlWork()
        {
            var source = ReadRepoFile("InventoryManagementApp", "Services", "Users", "UserService.cs");

            AssertCancellationGuardBeforeConnection(
                source,
                "public async Task<List<User>> GetAllUsersAsync",
                "public async Task<int> CountUsersAsync");
            AssertCancellationGuardBeforeConnection(
                source,
                "public async Task<int> CountUsersAsync",
                "public async Task<User?> GetUserByIDAsync");
            AssertCancellationGuardBeforeConnection(
                source,
                "public async Task<User?> GetUserByIDAsync",
                "public async Task<(AuthenticationResult Result, User? User)> AuthenticateUserAsync");
        }

        [Fact]
        public void GetUserByIdRejectsInvalidIdsBeforeCancellationAndConnectionWork()
        {
            var source = ReadRepoFile("InventoryManagementApp", "Services", "Users", "UserService.cs");
            var method = ExtractMethod(
                source,
                "public async Task<User?> GetUserByIDAsync",
                "public async Task<(AuthenticationResult Result, User? User)> AuthenticateUserAsync");

            Assert.Contains("if (userID < 1)", method, StringComparison.Ordinal);
            Assert.Contains("throw new ArgumentOutOfRangeException(nameof(userID), \"User ID must be greater than 0.\");", method, StringComparison.Ordinal);
            Assert.True(
                method.IndexOf("if (userID < 1)", StringComparison.Ordinal) < method.IndexOf("cancellationToken.ThrowIfCancellationRequested();", StringComparison.Ordinal),
                "Invalid user IDs should fail before cancellation and query work.");
            Assert.True(
                method.IndexOf("if (userID < 1)", StringComparison.Ordinal) < method.IndexOf("using var conn = _dbService.CreateConnection();", StringComparison.Ordinal),
                "Invalid user IDs should fail before opening a database connection.");
        }

        [Fact]
        public void AuthenticationStateWritesGuardAffectedRowsBeforeInMemoryMutation()
        {
            var source = ReadRepoFile("InventoryManagementApp", "Services", "Users", "UserService.cs");
            var authenticate = ExtractMethod(
                source,
                "public async Task<(AuthenticationResult Result, User? User)> AuthenticateUserAsync",
                "static bool IsLockoutActive");
            var helpers = ExtractMethod(
                source,
                "async Task<bool> RecordFailedLoginAsync",
                "public async Task<User?> GetCurrentUserAsync");

            const string upgradeWrite = "var upgradedRows = await SqliteHelper.ExecuteNonQueryAsync";
            const string upgradeGuard = "EnsureUserWriteSucceeded(upgradedRows, u.UserID);";
            Assert.Contains(upgradeWrite, authenticate, StringComparison.Ordinal);
            Assert.Contains(upgradeGuard, authenticate, StringComparison.Ordinal);
            Assert.True(
                authenticate.IndexOf(upgradeWrite, StringComparison.Ordinal) < authenticate.IndexOf(upgradeGuard, StringComparison.Ordinal),
                "Legacy password upgrades should capture affected rows before checking the write result.");
            Assert.True(
                authenticate.IndexOf(upgradeGuard, StringComparison.Ordinal) < authenticate.IndexOf("u.PasswordHash = upgradedResult.hash;", StringComparison.Ordinal),
                "Legacy password upgrades should guard stale rows before mutating the in-memory user.");

            Assert.Contains("var recordedRows = await SqliteHelper.ExecuteNonQueryAsync", helpers, StringComparison.Ordinal);
            Assert.Contains("EnsureUserWriteSucceeded(recordedRows, user.UserID);", helpers, StringComparison.Ordinal);
            Assert.True(
                helpers.IndexOf("EnsureUserWriteSucceeded(recordedRows, user.UserID);", StringComparison.Ordinal) < helpers.IndexOf("user.FailedLoginAttempts = failedAttempts;", StringComparison.Ordinal),
                "Failed-login state should guard stale rows before mutating the in-memory user.");

            Assert.Contains("var clearedRows = await SqliteHelper.ExecuteNonQueryAsync", helpers, StringComparison.Ordinal);
            Assert.Contains("EnsureUserWriteSucceeded(clearedRows, userID);", helpers, StringComparison.Ordinal);
            Assert.Contains("static void EnsureUserWriteSucceeded(int rows, int userID)", helpers, StringComparison.Ordinal);
            Assert.Contains("throw new KeyNotFoundException($\"User {userID} not found.\");", helpers, StringComparison.Ordinal);
        }

        [Fact]
        public void ChangePasswordRejectsInvalidUserIdsBeforeAuthorizationPasswordAndSqlWork()
        {
            var source = ReadRepoFile("InventoryManagementApp", "Services", "Users", "UserService.cs");
            var method = ExtractMethod(
                source,
                "public async Task<bool> ChangeUserPasswordAsync",
                "async Task DeleteUserInternalAsync");

            Assert.Contains("if (userID < 1)", method, StringComparison.Ordinal);
            Assert.Contains("throw new ArgumentOutOfRangeException(nameof(userID), \"User ID must be greater than 0.\");", method, StringComparison.Ordinal);
            Assert.True(
                method.IndexOf("if (userID < 1)", StringComparison.Ordinal) < method.IndexOf("_context.CurrentUser?.UserID", StringComparison.Ordinal),
                "Invalid password-change user IDs should fail before authorization work.");
            Assert.True(
                method.IndexOf("if (userID < 1)", StringComparison.Ordinal) < method.IndexOf("PasswordValidator.IsValid", StringComparison.Ordinal),
                "Invalid password-change user IDs should fail before password validation work.");
            Assert.True(
                method.IndexOf("if (userID < 1)", StringComparison.Ordinal) < method.IndexOf("SecurityHelper.HashPasswordAsync", StringComparison.Ordinal),
                "Invalid password-change user IDs should fail before password hashing work.");
            Assert.True(
                method.IndexOf("if (userID < 1)", StringComparison.Ordinal) < method.IndexOf("using var conn = _dbService.CreateConnection();", StringComparison.Ordinal),
                "Invalid password-change user IDs should fail before opening a database connection.");
        }

        [Fact]
        public void ChangePasswordChecksUserExistsBeforePasswordValidationAndHashing()
        {
            var source = ReadRepoFile("InventoryManagementApp", "Services", "Users", "UserService.cs");
            var method = ExtractMethod(
                source,
                "public async Task<bool> ChangeUserPasswordAsync",
                "async Task DeleteUserInternalAsync");

            const string lookupSnippet = "var existing = await GetUserByIDAsync(userID, CancellationToken.None);";
            const string missingSnippet = "if (existing is null)";

            Assert.Contains(lookupSnippet, method, StringComparison.Ordinal);
            Assert.Contains(missingSnippet, method, StringComparison.Ordinal);
            Assert.Contains("return false;", method, StringComparison.Ordinal);
            Assert.True(
                method.IndexOf(lookupSnippet, StringComparison.Ordinal) < method.IndexOf(missingSnippet, StringComparison.Ordinal),
                "Password changes should inspect the target user before handling a missing account.");
            Assert.True(
                method.IndexOf(missingSnippet, StringComparison.Ordinal) < method.IndexOf("PasswordValidator.IsValid", StringComparison.Ordinal),
                "Missing password-change users should fail before password validation work.");
            Assert.True(
                method.IndexOf(missingSnippet, StringComparison.Ordinal) < method.IndexOf("SecurityHelper.HashPasswordAsync", StringComparison.Ordinal),
                "Missing password-change users should fail before password hashing work.");
            Assert.True(
                method.IndexOf(missingSnippet, StringComparison.Ordinal) < method.IndexOf("var sql = \"UPDATE Users SET PasswordHash=@Pwd", StringComparison.Ordinal),
                "Missing password-change users should fail before update SQL is prepared.");
        }

        [Fact]
        public void TryDeleteUserReturnsFalseForInvalidUserIdsBeforeAuthorizationAndLookup()
        {
            var source = ReadRepoFile("InventoryManagementApp", "Services", "Users", "UserService.cs");
            var method = ExtractMethod(
                source,
                "public async Task<bool> TryDeleteUserAsync",
                "    }\n}");

            Assert.Contains("if (userID < 1)", method, StringComparison.Ordinal);
            Assert.Contains("return false;", method, StringComparison.Ordinal);
            Assert.True(
                method.IndexOf("if (userID < 1)", StringComparison.Ordinal) < method.IndexOf("_auth.EnsurePermission", StringComparison.Ordinal),
                "Invalid delete user IDs should fail before authorization work.");
            Assert.True(
                method.IndexOf("if (userID < 1)", StringComparison.Ordinal) < method.IndexOf("GetUserByIDAsync", StringComparison.Ordinal),
                "Invalid delete user IDs should fail before user lookup work.");
            Assert.True(
                method.IndexOf("if (userID < 1)", StringComparison.Ordinal) < method.IndexOf("DeleteUserInternalAsync", StringComparison.Ordinal),
                "Invalid delete user IDs should fail before delete work.");
        }

        private static void AssertCancellationGuardBeforeConnection(string source, string startMarker, string endMarker)
        {
            var method = ExtractMethod(source, startMarker, endMarker);

            Assert.Contains("cancellationToken.ThrowIfCancellationRequested();", method, StringComparison.Ordinal);
            Assert.True(
                method.IndexOf("cancellationToken.ThrowIfCancellationRequested();", StringComparison.Ordinal) < method.IndexOf("using var conn = _dbService.CreateConnection();", StringComparison.Ordinal),
                $"Expected {startMarker} to honor cancellation before opening a database connection.");
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