using System;
using System.IO;
using Xunit;

namespace InventoryManagementApp.Tests
{
    public class MaintenanceCalibrationProjectionContractTests
    {
        [Fact]
        public void MaintenanceReadModelsRequireExistingItemRows()
        {
            var source = ReadRepoFile("InventoryManagementApp", "Services", "Maintenance", "MaintenanceService.cs");

            AssertContainsAll(
                source,
                "FROM MaintenanceRecords m\n                    JOIN Items i ON m.ItemID = i.ItemID",
                "public async Task<List<MaintenanceRecord>> GetAllMaintenanceRecordsAsync()",
                "public async Task<List<MaintenanceRecord>> GetMaintenanceRecordsByItemAsync(int itemID)",
                "public async Task<List<MaintenanceRecord>> GetOverdueMaintenanceAsync()",
                "public async Task<List<MaintenanceRecord>> GetUpcomingMaintenanceAsync(int days = 30)",
                "public async Task<MaintenanceRecord?> GetMaintenanceRecordByIdAsync(int maintenanceID)");
            Assert.DoesNotContain(
                "FROM MaintenanceRecords m\n                    LEFT JOIN Items i ON m.ItemID = i.ItemID",
                source,
                StringComparison.Ordinal);
        }

        [Fact]
        public void CalibrationReadModelsRequireExistingItemRows()
        {
            var source = ReadRepoFile("InventoryManagementApp", "Services", "Calibration", "CalibrationService.cs");

            AssertContainsAll(
                source,
                "FROM CalibrationRecords c\n                    JOIN Items i ON c.ItemID = i.ItemID",
                "public async Task<List<CalibrationRecord>> GetAllCalibrationRecordsAsync()",
                "public async Task<List<CalibrationRecord>> GetCalibrationRecordsByItemAsync(int itemID)",
                "public async Task<List<CalibrationRecord>> GetOverdueCalibrationAsync()",
                "public async Task<List<CalibrationRecord>> GetUpcomingCalibrationAsync(int days = 30)",
                "public async Task<CalibrationRecord?> GetLatestCalibrationForItemAsync(int itemID)",
                "public async Task<CalibrationRecord?> GetCalibrationRecordByIdAsync(int calibrationID)");
            Assert.DoesNotContain(
                "FROM CalibrationRecords c\n                    LEFT JOIN Items i ON c.ItemID = i.ItemID",
                source,
                StringComparison.Ordinal);
        }

        private static void AssertContainsAll(string source, params string[] expectedSnippets)
        {
            foreach (var snippet in expectedSnippets)
            {
                Assert.Contains(snippet, source, StringComparison.Ordinal);
            }
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
