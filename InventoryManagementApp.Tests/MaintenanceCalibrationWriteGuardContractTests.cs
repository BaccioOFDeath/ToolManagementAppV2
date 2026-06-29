using System;
using System.IO;
using Xunit;

namespace InventoryManagementApp.Tests
{
    public class MaintenanceCalibrationWriteGuardContractTests
    {
        [Fact]
        public void MaintenanceCreateChecksInsertedRowsBeforeReturningId()
        {
            var source = ReadRepoFile("InventoryManagementApp", "Services", "Maintenance", "MaintenanceService.cs");
            var createMethod = ExtractMethod(
                source,
                "public async Task<int> CreateMaintenanceRecordAsync(MaintenanceRecord record)",
                "public async Task<bool> UpdateMaintenanceRecordAsync");

            AssertCreateGuard(
                createMethod,
                "EnsureMaintenanceCreateSucceeded(insertedRows);",
                "Unable to create maintenance record.");
        }

        [Fact]
        public void CalibrationCreateChecksInsertedRowsBeforeReturningId()
        {
            var source = ReadRepoFile("InventoryManagementApp", "Services", "Calibration", "CalibrationService.cs");
            var createMethod = ExtractMethod(
                source,
                "public async Task<int> CreateCalibrationRecordAsync(CalibrationRecord record)",
                "public async Task<bool> UpdateCalibrationRecordAsync");

            AssertCreateGuard(
                createMethod,
                "EnsureCalibrationCreateSucceeded(insertedRows);",
                "Unable to create calibration record.");
        }

        [Fact]
        public void CreateWriteGuardsKeepWorkflowSpecificFailureMessages()
        {
            var maintenanceSource = ReadRepoFile("InventoryManagementApp", "Services", "Maintenance", "MaintenanceService.cs");
            var calibrationSource = ReadRepoFile("InventoryManagementApp", "Services", "Calibration", "CalibrationService.cs");

            AssertContainsAll(
                maintenanceSource,
                "private static void EnsureMaintenanceCreateSucceeded(int affectedRows)",
                "throw new InvalidOperationException(\"Unable to create maintenance record.\");");
            AssertContainsAll(
                calibrationSource,
                "private static void EnsureCalibrationCreateSucceeded(int affectedRows)",
                "throw new InvalidOperationException(\"Unable to create calibration record.\");");
        }

        private static void AssertCreateGuard(
            string createMethod,
            string guardSnippet,
            string failureMessage)
        {
            AssertContainsAll(
                createMethod,
                "var insertedRows = cmd.ExecuteNonQuery();",
                guardSnippet,
                "using var idCmd = new SqliteCommand(\"SELECT last_insert_rowid();\", conn);",
                "if (id < 1)",
                $"throw new InvalidOperationException(\"{failureMessage}\");",
                "return id;");
            Assert.DoesNotContain("SELECT last_insert_rowid();\";", createMethod, StringComparison.Ordinal);

            Assert.True(
                createMethod.IndexOf("var insertedRows = cmd.ExecuteNonQuery();", StringComparison.Ordinal) <
                createMethod.IndexOf(guardSnippet, StringComparison.Ordinal),
                "Expected create methods to inspect affected rows immediately after executing the insert.");
            Assert.True(
                createMethod.IndexOf(guardSnippet, StringComparison.Ordinal) <
                createMethod.IndexOf("using var idCmd = new SqliteCommand(\"SELECT last_insert_rowid();\", conn);", StringComparison.Ordinal),
                "Expected failed creates to stop before reading the inserted id.");
            Assert.True(
                createMethod.IndexOf("if (id < 1)", StringComparison.Ordinal) <
                createMethod.IndexOf("return id;", StringComparison.Ordinal),
                "Expected create methods to reject invalid inserted ids before reporting success.");
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
