using System;
using System.IO;
using Xunit;

namespace InventoryManagementApp.Tests
{
    public class MaintenanceCalibrationQueryGuardContractTests
    {
        [Fact]
        public void MaintenanceItemHistoryValidatesParentItemBeforeHistoryQuery()
        {
            var source = ReadRepoFile("InventoryManagementApp", "Services", "Maintenance", "MaintenanceService.cs");

            AssertContainsAll(
                source,
                "private static void EnsureItemExists(SqliteConnection conn, int itemID)",
                "SELECT COUNT(*) FROM Items WHERE ItemID = @ItemID",
                "throw new InvalidOperationException(\"Item not found.\");");

            var method = ExtractMethod(
                source,
                "public async Task<List<MaintenanceRecord>> GetMaintenanceRecordsByItemAsync(int itemID)",
                "public async Task<List<MaintenanceRecord>> GetOverdueMaintenanceAsync()");

            AssertContainsAll(
                method,
                "if (itemID < 1)",
                "throw new ArgumentOutOfRangeException(nameof(itemID), \"Item ID must be greater than 0.\");",
                "using var conn = _databaseService.CreateConnection();",
                "EnsureItemExists(conn, itemID);",
                "WHERE m.ItemID = @ItemID");
            Assert.True(
                method.IndexOf("EnsureItemExists(conn, itemID);", StringComparison.Ordinal) <
                method.IndexOf("var sql = @\"", StringComparison.Ordinal),
                "Expected maintenance item history to confirm the item row exists before building/executing the history query.");
        }

        [Fact]
        public void MaintenanceListQueriesAreCappedWithSharedLimit()
        {
            var source = ReadRepoFile("InventoryManagementApp", "Services", "Maintenance", "MaintenanceService.cs");

            AssertContainsAll(
                source,
                "private const int MaxMaintenanceListCount = 500;",
                "cmd.Parameters.AddWithValue(\"@MaintenanceListLimit\", MaxMaintenanceListCount);");

            AssertCappedQuery(
                ExtractMethod(
                    source,
                    "public async Task<List<MaintenanceRecord>> GetAllMaintenanceRecordsAsync()",
                    "public async Task<List<MaintenanceRecord>> GetMaintenanceRecordsByItemAsync(int itemID)"),
                "ORDER BY m.ScheduledDate DESC",
                "LIMIT @MaintenanceListLimit",
                "cmd.Parameters.AddWithValue(\"@MaintenanceListLimit\", MaxMaintenanceListCount);");

            AssertCappedQuery(
                ExtractMethod(
                    source,
                    "public async Task<List<MaintenanceRecord>> GetMaintenanceRecordsByItemAsync(int itemID)",
                    "public async Task<List<MaintenanceRecord>> GetOverdueMaintenanceAsync()"),
                "ORDER BY m.ScheduledDate DESC",
                "LIMIT @MaintenanceListLimit",
                "cmd.Parameters.AddWithValue(\"@MaintenanceListLimit\", MaxMaintenanceListCount);");

            AssertCappedQuery(
                ExtractMethod(
                    source,
                    "public async Task<List<MaintenanceRecord>> GetOverdueMaintenanceAsync()",
                    "public async Task<List<MaintenanceRecord>> GetUpcomingMaintenanceAsync(int days = 30)"),
                "ORDER BY m.ScheduledDate ASC",
                "LIMIT @MaintenanceListLimit",
                "cmd.Parameters.AddWithValue(\"@MaintenanceListLimit\", MaxMaintenanceListCount);");

            AssertCappedQuery(
                ExtractMethod(
                    source,
                    "public async Task<List<MaintenanceRecord>> GetUpcomingMaintenanceAsync(int days = 30)",
                    "public async Task<MaintenanceRecord?> GetMaintenanceRecordByIdAsync(int maintenanceID)"),
                "ORDER BY m.ScheduledDate ASC",
                "LIMIT @MaintenanceListLimit",
                "cmd.Parameters.AddWithValue(\"@MaintenanceListLimit\", MaxMaintenanceListCount);");
        }

        [Fact]
        public void CalibrationItemQueriesValidateParentItemBeforeItemQueries()
        {
            var source = ReadRepoFile("InventoryManagementApp", "Services", "Calibration", "CalibrationService.cs");

            AssertContainsAll(
                source,
                "private static void EnsureItemExists(SqliteConnection conn, int itemID)",
                "SELECT COUNT(*) FROM Items WHERE ItemID = @ItemID",
                "throw new InvalidOperationException(\"Item not found.\");");

            var historyMethod = ExtractMethod(
                source,
                "public async Task<List<CalibrationRecord>> GetCalibrationRecordsByItemAsync(int itemID)",
                "public async Task<List<CalibrationRecord>> GetOverdueCalibrationAsync()");
            AssertContainsAll(
                historyMethod,
                "if (itemID < 1)",
                "throw new ArgumentOutOfRangeException(nameof(itemID), \"Item ID must be greater than 0.\");",
                "using var conn = _databaseService.CreateConnection();",
                "EnsureItemExists(conn, itemID);",
                "WHERE c.ItemID = @ItemID");
            Assert.True(
                historyMethod.IndexOf("EnsureItemExists(conn, itemID);", StringComparison.Ordinal) <
                historyMethod.IndexOf("var sql = @\"", StringComparison.Ordinal),
                "Expected calibration item history to confirm the item row exists before building/executing the history query.");

            var latestMethod = ExtractMethod(
                source,
                "public async Task<CalibrationRecord?> GetLatestCalibrationForItemAsync(int itemID)",
                "public async Task<CalibrationRecord?> GetCalibrationRecordByIdAsync(int calibrationID)");
            AssertContainsAll(
                latestMethod,
                "if (itemID < 1)",
                "throw new ArgumentOutOfRangeException(nameof(itemID), \"Item ID must be greater than 0.\");",
                "using var conn = _databaseService.CreateConnection();",
                "EnsureItemExists(conn, itemID);",
                "WHERE c.ItemID = @ItemID",
                "LIMIT 1");
            Assert.True(
                latestMethod.IndexOf("EnsureItemExists(conn, itemID);", StringComparison.Ordinal) <
                latestMethod.IndexOf("var sql = @\"", StringComparison.Ordinal),
                "Expected latest calibration lookup to confirm the item row exists before building/executing the lookup query.");
        }

        [Fact]
        public void CalibrationListQueriesAreCappedWithSharedLimit()
        {
            var source = ReadRepoFile("InventoryManagementApp", "Services", "Calibration", "CalibrationService.cs");

            AssertContainsAll(
                source,
                "private const int MaxCalibrationListCount = 500;",
                "cmd.Parameters.AddWithValue(\"@CalibrationListLimit\", MaxCalibrationListCount);");

            AssertCappedQuery(
                ExtractMethod(
                    source,
                    "public async Task<List<CalibrationRecord>> GetAllCalibrationRecordsAsync()",
                    "public async Task<List<CalibrationRecord>> GetCalibrationRecordsByItemAsync(int itemID)"),
                "ORDER BY c.CalibrationDate DESC",
                "LIMIT @CalibrationListLimit",
                "cmd.Parameters.AddWithValue(\"@CalibrationListLimit\", MaxCalibrationListCount);");

            AssertCappedQuery(
                ExtractMethod(
                    source,
                    "public async Task<List<CalibrationRecord>> GetCalibrationRecordsByItemAsync(int itemID)",
                    "public async Task<List<CalibrationRecord>> GetOverdueCalibrationAsync()"),
                "ORDER BY c.CalibrationDate DESC",
                "LIMIT @CalibrationListLimit",
                "cmd.Parameters.AddWithValue(\"@CalibrationListLimit\", MaxCalibrationListCount);");

            AssertCappedQuery(
                ExtractMethod(
                    source,
                    "public async Task<List<CalibrationRecord>> GetOverdueCalibrationAsync()",
                    "public async Task<List<CalibrationRecord>> GetUpcomingCalibrationAsync(int days = 30)"),
                "ORDER BY c.NextCalibrationDue ASC",
                "LIMIT @CalibrationListLimit",
                "cmd.Parameters.AddWithValue(\"@CalibrationListLimit\", MaxCalibrationListCount);");

            AssertCappedQuery(
                ExtractMethod(
                    source,
                    "public async Task<List<CalibrationRecord>> GetUpcomingCalibrationAsync(int days = 30)",
                    "public async Task<CalibrationRecord?> GetLatestCalibrationForItemAsync(int itemID)"),
                "ORDER BY c.NextCalibrationDue ASC",
                "LIMIT @CalibrationListLimit",
                "cmd.Parameters.AddWithValue(\"@CalibrationListLimit\", MaxCalibrationListCount);");
        }

        private static void AssertContainsAll(string source, params string[] expectedSnippets)
        {
            foreach (var snippet in expectedSnippets)
            {
                Assert.Contains(snippet, source, StringComparison.Ordinal);
            }
        }

        private static void AssertCappedQuery(string method, string orderBySnippet, string limitSnippet, string parameterSnippet)
        {
            AssertContainsAll(method, orderBySnippet, limitSnippet, parameterSnippet);
            Assert.True(
                method.IndexOf(orderBySnippet, StringComparison.Ordinal) <
                method.IndexOf(limitSnippet, StringComparison.Ordinal),
                "Expected list cap to be applied after the query ordering so the most relevant rows are retained.");
            Assert.True(
                method.IndexOf(limitSnippet, StringComparison.Ordinal) <
                method.IndexOf(parameterSnippet, StringComparison.Ordinal),
                "Expected the list cap SQL placeholder to be bound before the query is executed.");
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