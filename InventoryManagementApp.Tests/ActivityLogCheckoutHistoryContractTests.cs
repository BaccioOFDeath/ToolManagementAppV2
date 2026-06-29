using System;
using System.IO;
using Xunit;

namespace InventoryManagementApp.Tests
{
    public class ActivityLogCheckoutHistoryContractTests
    {
        [Fact]
        public void ActivityLogEntrypointsHonorCancellationBeforeSqlWork()
        {
            var source = ReadRepoFile("InventoryManagementApp", "Services", "Users", "ActivityLogService.cs");

            AssertCancellationGuardBeforeSqlWork(
                source,
                "public virtual async Task<Result> LogActionAsync",
                "public virtual async Task<Result<List<ActivityLog>>> GetRecentLogsAsync");
            AssertCancellationGuardBeforeSqlWork(
                source,
                "public virtual async Task<Result<List<ActivityLog>>> GetRecentLogsAsync",
                "public virtual async Task<Result<List<ActivityLog>>> GetCheckoutHistoryForItemAsync");
            AssertCancellationGuardBeforeSqlWork(
                source,
                "public virtual async Task<Result<List<ActivityLog>>> GetCheckoutHistoryForItemAsync",
                "public virtual async Task<Result> PurgeOldLogsAsync");
            AssertCancellationGuardBeforeSqlWork(
                source,
                "public virtual async Task<Result> PurgeOldLogsAsync",
                "ActivityLog MapLog");
        }

        [Fact]
        public void LogActionRejectsBlankUserNamesBeforeSqlWork()
        {
            var source = ReadRepoFile("InventoryManagementApp", "Services", "Users", "ActivityLogService.cs");
            var method = ExtractMethod(
                source,
                "public virtual async Task<Result> LogActionAsync",
                "public virtual async Task<Result<List<ActivityLog>>> GetRecentLogsAsync");

            Assert.Contains("if (string.IsNullOrWhiteSpace(userName))", method, StringComparison.Ordinal);
            Assert.Contains("return new Result(false, \"User name is required.\");", method, StringComparison.Ordinal);
            Assert.True(
                method.IndexOf("cancellationToken.ThrowIfCancellationRequested();", StringComparison.Ordinal) < method.IndexOf("if (string.IsNullOrWhiteSpace(userName))", StringComparison.Ordinal),
                "Cancellation should still be honored before blank-user validation.");
            Assert.True(
                method.IndexOf("if (string.IsNullOrWhiteSpace(userName))", StringComparison.Ordinal) < method.IndexOf("if (string.IsNullOrWhiteSpace(action))", StringComparison.Ordinal),
                "The blank-user guard should run before blank-action validation.");
            Assert.True(
                method.IndexOf("if (string.IsNullOrWhiteSpace(userName))", StringComparison.Ordinal) < method.IndexOf("const string sql =", StringComparison.Ordinal),
                "The blank-user guard should run before activity-log SQL text is prepared.");
            Assert.True(
                method.IndexOf("if (string.IsNullOrWhiteSpace(userName))", StringComparison.Ordinal) < method.IndexOf("new SqliteParameter(\"@UserName\", normalizedUserName)", StringComparison.Ordinal),
                "The blank-user guard should run before user-name parameters are prepared.");
            Assert.True(
                method.IndexOf("if (string.IsNullOrWhiteSpace(userName))", StringComparison.Ordinal) < method.IndexOf("using var conn = _dbService.CreateConnection()", StringComparison.Ordinal),
                "The blank-user guard should run before opening a database connection.");
        }

        [Fact]
        public void LogActionRejectsBlankActionsBeforeSqlWork()
        {
            var source = ReadRepoFile("InventoryManagementApp", "Services", "Users", "ActivityLogService.cs");
            var method = ExtractMethod(
                source,
                "public virtual async Task<Result> LogActionAsync",
                "public virtual async Task<Result<List<ActivityLog>>> GetRecentLogsAsync");

            Assert.Contains("if (string.IsNullOrWhiteSpace(action))", method, StringComparison.Ordinal);
            Assert.Contains("return new Result(false, \"Action is required.\");", method, StringComparison.Ordinal);
            Assert.True(
                method.IndexOf("cancellationToken.ThrowIfCancellationRequested();", StringComparison.Ordinal) < method.IndexOf("if (string.IsNullOrWhiteSpace(action))", StringComparison.Ordinal),
                "Cancellation should still be honored before blank-action validation.");
            Assert.True(
                method.IndexOf("if (string.IsNullOrWhiteSpace(action))", StringComparison.Ordinal) < method.IndexOf("const string sql =", StringComparison.Ordinal),
                "The blank-action guard should run before activity-log SQL text is prepared.");
            Assert.True(
                method.IndexOf("if (string.IsNullOrWhiteSpace(action))", StringComparison.Ordinal) < method.IndexOf("new SqliteParameter(\"@Action\",   normalizedAction)", StringComparison.Ordinal),
                "The blank-action guard should run before action parameters are prepared.");
            Assert.True(
                method.IndexOf("if (string.IsNullOrWhiteSpace(action))", StringComparison.Ordinal) < method.IndexOf("using var conn = _dbService.CreateConnection()", StringComparison.Ordinal),
                "The blank-action guard should run before opening a database connection.");
        }

        [Fact]
        public void LogActionNormalizesAuditFieldsBeforePersisting()
        {
            var source = ReadRepoFile("InventoryManagementApp", "Services", "Users", "ActivityLogService.cs");
            var method = ExtractMethod(
                source,
                "public virtual async Task<Result> LogActionAsync",
                "public virtual async Task<Result<List<ActivityLog>>> GetRecentLogsAsync");

            Assert.Contains("var normalizedUserName = userName.Trim();", method, StringComparison.Ordinal);
            Assert.Contains("var normalizedAction = action.Trim();", method, StringComparison.Ordinal);
            Assert.Contains("new SqliteParameter(\"@UserName\", normalizedUserName)", method, StringComparison.Ordinal);
            Assert.Contains("new SqliteParameter(\"@Action\",   normalizedAction)", method, StringComparison.Ordinal);
            Assert.DoesNotContain("new SqliteParameter(\"@UserName\", userName)", method, StringComparison.Ordinal);
            Assert.DoesNotContain("new SqliteParameter(\"@Action\",   action)", method, StringComparison.Ordinal);
            Assert.True(
                method.IndexOf("if (string.IsNullOrWhiteSpace(action))", StringComparison.Ordinal) < method.IndexOf("var normalizedUserName = userName.Trim();", StringComparison.Ordinal),
                "Audit fields should be normalized only after the blank-input guards pass.");
            Assert.True(
                method.IndexOf("var normalizedAction = action.Trim();", StringComparison.Ordinal) < method.IndexOf("const string sql =", StringComparison.Ordinal),
                "Audit fields should be normalized before activity-log SQL work starts.");
        }

        [Fact]
        public void LogActionChecksInsertAffectedRowsBeforeReturningSuccess()
        {
            var source = ReadRepoFile("InventoryManagementApp", "Services", "Users", "ActivityLogService.cs");
            var method = ExtractMethod(
                source,
                "public virtual async Task<Result> LogActionAsync",
                "public virtual async Task<Result<List<ActivityLog>>> GetRecentLogsAsync");

            Assert.Contains("var insertedRows = await SqliteHelper.ExecuteNonQueryAsync(conn, sql, p, cancellationToken).ConfigureAwait(false);", method, StringComparison.Ordinal);
            Assert.Contains("if (insertedRows == 0)", method, StringComparison.Ordinal);
            Assert.Contains("return new Result(false, \"Unable to log activity.\");", method, StringComparison.Ordinal);
            Assert.DoesNotContain("await SqliteHelper.ExecuteNonQueryAsync(conn, sql, p, cancellationToken).ConfigureAwait(false);\n                return new Result(true);", method, StringComparison.Ordinal);
            Assert.True(
                method.IndexOf("if (insertedRows == 0)", StringComparison.Ordinal) < method.IndexOf("return new Result(true);", StringComparison.Ordinal),
                "Activity logging should verify the insert affected rows before reporting success.");
        }

        [Fact]
        public void RecentLogsRejectsInvalidCountsBeforeSqlWork()
        {
            var source = ReadRepoFile("InventoryManagementApp", "Services", "Users", "ActivityLogService.cs");
            var method = ExtractMethod(
                source,
                "public virtual async Task<Result<List<ActivityLog>>> GetRecentLogsAsync",
                "public virtual async Task<Result<List<ActivityLog>>> GetCheckoutHistoryForItemAsync");

            Assert.Contains("if (count < 1)", method, StringComparison.Ordinal);
            Assert.Contains("return new Result<List<ActivityLog>>(null, false, \"Count must be positive.\");", method, StringComparison.Ordinal);
            Assert.True(
                method.IndexOf("if (count < 1)", StringComparison.Ordinal) < method.IndexOf("using var conn = _dbService.CreateConnection()", StringComparison.Ordinal),
                "The invalid count guard should run before recent-log SQL work starts.");
        }

        [Fact]
        public void RecentLogsStillOrdersByTimestampAndAppliesTheRequestedLimit()
        {
            var source = ReadRepoFile("InventoryManagementApp", "Services", "Users", "ActivityLogService.cs");
            var method = ExtractMethod(
                source,
                "public virtual async Task<Result<List<ActivityLog>>> GetRecentLogsAsync",
                "public virtual async Task<Result<List<ActivityLog>>> GetCheckoutHistoryForItemAsync");

            Assert.Contains("ORDER BY Timestamp DESC", method, StringComparison.Ordinal);
            Assert.Contains("LIMIT @Count", method, StringComparison.Ordinal);
            Assert.Contains("new SqliteParameter(\"@Count\", count)", method, StringComparison.Ordinal);
        }

        [Fact]
        public void CheckoutHistoryRejectsInvalidItemIdsBeforeSqlWork()
        {
            var source = ReadRepoFile("InventoryManagementApp", "Services", "Users", "ActivityLogService.cs");
            var method = ExtractMethod(
                source,
                "public virtual async Task<Result<List<ActivityLog>>> GetCheckoutHistoryForItemAsync",
                "public virtual async Task<Result> PurgeOldLogsAsync");

            Assert.Contains("if (itemID < 1)", method, StringComparison.Ordinal);
            Assert.Contains("return new Result<List<ActivityLog>>(null, false, \"Item ID must be positive.\");", method, StringComparison.Ordinal);
            Assert.DoesNotContain("new Result<List<ActivityLog>>(new List<ActivityLog>(), true)", method, StringComparison.Ordinal);
            Assert.True(
                method.IndexOf("if (itemID < 1)", StringComparison.Ordinal) < method.IndexOf("using var conn = _dbService.CreateConnection()", StringComparison.Ordinal),
                "The invalid item id guard should run before checkout-history SQL work starts.");
        }

        [Fact]
        public void CheckoutHistoryStillSearchesLegacyAndItemNumberActivityFormats()
        {
            var source = ReadRepoFile("InventoryManagementApp", "Services", "Users", "ActivityLogService.cs");
            var method = ExtractMethod(
                source,
                "public virtual async Task<Result<List<ActivityLog>>> GetCheckoutHistoryForItemAsync",
                "public virtual async Task<Result> PurgeOldLogsAsync");

            Assert.Contains("%item {itemID} check-out status%", method, StringComparison.Ordinal);
            Assert.Contains("%item {itemNumber.Trim()} ({itemID})%", method, StringComparison.Ordinal);
            Assert.Contains("Action LIKE @LegacyTogglePattern", method, StringComparison.Ordinal);
            Assert.Contains("OR Action LIKE @ItemNumberPattern", method, StringComparison.Ordinal);
            Assert.Contains("ORDER BY Timestamp DESC", method, StringComparison.Ordinal);
        }

        private static void AssertCancellationGuardBeforeSqlWork(string source, string startMarker, string endMarker)
        {
            var method = ExtractMethod(source, startMarker, endMarker);

            Assert.Contains("cancellationToken.ThrowIfCancellationRequested();", method, StringComparison.Ordinal);
            Assert.True(
                method.IndexOf("cancellationToken.ThrowIfCancellationRequested();", StringComparison.Ordinal) < method.IndexOf("sql =", StringComparison.Ordinal),
                $"Expected {startMarker} to honor cancellation before SQL work starts.");
            Assert.True(
                method.IndexOf("cancellationToken.ThrowIfCancellationRequested();", StringComparison.Ordinal) < method.IndexOf("using var conn = _dbService.CreateConnection()", StringComparison.Ordinal),
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