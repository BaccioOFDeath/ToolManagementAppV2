using System;
using System.IO;
using Xunit;

namespace InventoryManagementApp.Tests
{
    public class UserServiceAuthenticationWriteGuardContractTests
    {
        [Fact]
        public void AuthenticationStateWritesCheckAffectedRowsBeforeReportingSuccess()
        {
            var source = ReadRepoFile("InventoryManagementApp", "Services", "Users", "UserService.cs");

            var authenticateMethod = ExtractMethod(
                source,
                "public async Task<(AuthenticationResult Result, User? User)> AuthenticateUserAsync",
                "static bool IsLockoutActive");
            AssertContainsAll(
                authenticateMethod,
                "var upgradedRows = await SqliteHelper.ExecuteNonQueryAsync(conn, \"UPDATE Users SET PasswordHash=@Pwd, PasswordSalt=@Salt WHERE UserID=@ID\", p).ConfigureAwait(false);",
                "EnsureUserWriteSucceeded(upgradedRows, u.UserID);",
                "u.PasswordHash = upgradedResult.hash;",
                "u.PasswordSalt = upgradedResult.salt;");
            Assert.True(
                authenticateMethod.IndexOf("EnsureUserWriteSucceeded(upgradedRows, u.UserID);", StringComparison.Ordinal) <
                authenticateMethod.IndexOf("u.PasswordHash = upgradedResult.hash;", StringComparison.Ordinal),
                "Legacy password upgrades should verify the database write before mutating the in-memory user.");

            var failedLoginMethod = ExtractMethod(
                source,
                "async Task<bool> RecordFailedLoginAsync",
                "static async Task ClearLoginFailureStateAsync");
            AssertContainsAll(
                failedLoginMethod,
                "var recordedRows = await SqliteHelper.ExecuteNonQueryAsync(conn,",
                "EnsureUserWriteSucceeded(recordedRows, user.UserID);",
                "user.FailedLoginAttempts = failedAttempts;",
                "user.LockoutEndUtc = lockoutEndUtc;");
            Assert.True(
                failedLoginMethod.IndexOf("EnsureUserWriteSucceeded(recordedRows, user.UserID);", StringComparison.Ordinal) <
                failedLoginMethod.IndexOf("user.FailedLoginAttempts = failedAttempts;", StringComparison.Ordinal),
                "Failed-login recording should verify the database write before mutating in-memory failure state.");

            var clearFailureMethod = ExtractMethod(
                source,
                "static async Task ClearLoginFailureStateAsync",
                "static void EnsureUserWriteSucceeded");
            AssertContainsAll(
                clearFailureMethod,
                "var clearedRows = await SqliteHelper.ExecuteNonQueryAsync(conn,",
                "UPDATE Users SET FailedLoginAttempts=0, LockoutEndUtc=NULL WHERE UserID=@ID",
                "EnsureUserWriteSucceeded(clearedRows, userID);");
            Assert.True(
                clearFailureMethod.IndexOf("var clearedRows = await SqliteHelper.ExecuteNonQueryAsync", StringComparison.Ordinal) <
                clearFailureMethod.IndexOf("EnsureUserWriteSucceeded(clearedRows, userID);", StringComparison.Ordinal),
                "Login failure clears should inspect affected rows after executing the database write.");
        }

        private static void AssertContainsAll(string source, params string[] expectedSnippets)
        {
            foreach (var snippet in expectedSnippets)
            {
                Assert.Contains(snippet, source, StringComparison.Ordinal);
            }
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
