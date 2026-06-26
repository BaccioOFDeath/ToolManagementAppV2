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
                method.IndexOf("cancellationToken.ThrowIfCancellationRequested();", StringComparison.Ordinal) < method.IndexOf("const string sql =", StringComparison.Ordinal),
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