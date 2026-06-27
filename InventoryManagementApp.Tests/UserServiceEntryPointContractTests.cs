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
