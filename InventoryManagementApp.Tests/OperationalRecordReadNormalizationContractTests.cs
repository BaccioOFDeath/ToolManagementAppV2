using System;
using System.IO;
using Xunit;

namespace InventoryManagementApp.Tests
{
    public class OperationalRecordReadNormalizationContractTests
    {
        [Fact]
        public void MaintenanceMapperNormalizesAllReadbackTextFields()
        {
            var source = ReadRepoFile("InventoryManagementApp", "Services", "Maintenance", "MaintenanceService.cs");
            var mapper = ExtractMethod(source, "private MaintenanceRecord MapMaintenanceRecord(SqliteDataReader reader)", "private static string NormalizeMaintenanceReadText");

            AssertContainsAll(
                mapper,
                "ItemNumber = NormalizeMaintenanceReadText(reader, \"ItemNumber\")",
                "ItemName = NormalizeMaintenanceReadText(reader, \"ItemName\")",
                "MaintenanceType = NormalizeMaintenanceReadText(reader, \"MaintenanceType\")",
                "Description = NormalizeMaintenanceReadText(reader, \"Description\")",
                "PerformedBy = NormalizeMaintenanceReadText(reader, \"PerformedBy\")",
                "Status = NormalizeMaintenanceReadText(reader, \"Status\")",
                "Notes = NormalizeMaintenanceReadText(reader, \"Notes\")");

            Assert.DoesNotContain("reader.GetString(reader.GetOrdinal(\"ItemNumber\"))", mapper, StringComparison.Ordinal);
            Assert.DoesNotContain("reader.GetString(reader.GetOrdinal(\"ItemName\"))", mapper, StringComparison.Ordinal);
            Assert.DoesNotContain("reader.GetString(reader.GetOrdinal(\"MaintenanceType\"))", mapper, StringComparison.Ordinal);
            Assert.DoesNotContain("Description = reader.IsDBNull", mapper, StringComparison.Ordinal);
            Assert.DoesNotContain("PerformedBy = reader.IsDBNull", mapper, StringComparison.Ordinal);
            Assert.DoesNotContain("Status = reader.GetString", mapper, StringComparison.Ordinal);
            Assert.DoesNotContain("Notes = reader.IsDBNull", mapper, StringComparison.Ordinal);
        }

        [Fact]
        public void MaintenanceReadNormalizerTrimsValuesAndPreservesEmptyFallback()
        {
            var source = ReadRepoFile("InventoryManagementApp", "Services", "Maintenance", "MaintenanceService.cs");
            var normalizer = ExtractMethod(source, "private static string NormalizeMaintenanceReadText(SqliteDataReader reader, string columnName)", "private static void NormalizeMaintenanceRecordForSave");

            AssertContainsAll(
                normalizer,
                "var ordinal = reader.GetOrdinal(columnName);",
                "return reader.IsDBNull(ordinal) ? string.Empty : reader.GetString(ordinal).Trim();");
        }

        [Fact]
        public void MaintenanceReadMethodsShareTheNormalizedMapper()
        {
            var source = ReadRepoFile("InventoryManagementApp", "Services", "Maintenance", "MaintenanceService.cs");

            AssertReadMethodUsesMapper(source, "public async Task<List<MaintenanceRecord>> GetAllMaintenanceRecordsAsync()", "public async Task<int> CountMaintenanceRecordsAsync()");
            AssertReadMethodUsesMapper(source, "public async Task<List<MaintenanceRecord>> GetMaintenanceRecordsByItemAsync(int itemID)", "public async Task<List<MaintenanceRecord>> GetOverdueMaintenanceAsync()");
            AssertReadMethodUsesMapper(source, "public async Task<List<MaintenanceRecord>> GetOverdueMaintenanceAsync()", "public async Task<int> CountOverdueMaintenanceAsync()");
            AssertReadMethodUsesMapper(source, "public async Task<List<MaintenanceRecord>> GetUpcomingMaintenanceAsync(int days = 30)", "public async Task<int> CountUpcomingMaintenanceAsync(int days = 30)");
            AssertReadMethodUsesMapper(source, "public async Task<MaintenanceRecord?> GetMaintenanceRecordByIdAsync(int maintenanceID)", "public async Task<int> CreateMaintenanceRecordAsync(MaintenanceRecord record)");
        }

        [Fact]
        public void MaintenanceScheduledFiltersNormalizeLegacyPaddedStatusText()
        {
            var source = ReadRepoFile("InventoryManagementApp", "Services", "Maintenance", "MaintenanceService.cs");

            AssertScheduledFilterNormalizesStatus(source, "public async Task<List<MaintenanceRecord>> GetOverdueMaintenanceAsync()", "public async Task<int> CountOverdueMaintenanceAsync()");
            AssertScheduledFilterNormalizesStatus(source, "public async Task<int> CountOverdueMaintenanceAsync()", "public async Task<List<MaintenanceRecord>> GetUpcomingMaintenanceAsync(int days = 30)");
            AssertScheduledFilterNormalizesStatus(source, "public async Task<List<MaintenanceRecord>> GetUpcomingMaintenanceAsync(int days = 30)", "public async Task<int> CountUpcomingMaintenanceAsync(int days = 30)");
            AssertScheduledFilterNormalizesStatus(source, "public async Task<int> CountUpcomingMaintenanceAsync(int days = 30)", "public async Task<MaintenanceRecord?> GetMaintenanceRecordByIdAsync(int maintenanceID)");
        }

        [Fact]
        public void CalibrationMapperNormalizesAllReadbackTextFields()
        {
            var source = ReadRepoFile("InventoryManagementApp", "Services", "Calibration", "CalibrationService.cs");
            var mapper = ExtractMethod(source, "private CalibrationRecord MapCalibrationRecord(SqliteDataReader reader)", "private static string NormalizeCalibrationReadText");

            AssertContainsAll(
                mapper,
                "ItemNumber = NormalizeCalibrationReadText(reader, \"ItemNumber\")",
                "ItemName = NormalizeCalibrationReadText(reader, \"ItemName\")",
                "CalibratedBy = NormalizeCalibrationReadText(reader, \"CalibratedBy\")",
                "CertificateNumber = NormalizeCalibrationReadText(reader, \"CertificateNumber\")",
                "Standard = NormalizeCalibrationReadText(reader, \"Standard\")",
                "Result = NormalizeCalibrationReadText(reader, \"Result\")",
                "Notes = NormalizeCalibrationReadText(reader, \"Notes\")");

            Assert.DoesNotContain("reader.GetString(reader.GetOrdinal(\"ItemNumber\"))", mapper, StringComparison.Ordinal);
            Assert.DoesNotContain("reader.GetString(reader.GetOrdinal(\"ItemName\"))", mapper, StringComparison.Ordinal);
            Assert.DoesNotContain("CalibratedBy = reader.IsDBNull", mapper, StringComparison.Ordinal);
            Assert.DoesNotContain("CertificateNumber = reader.IsDBNull", mapper, StringComparison.Ordinal);
            Assert.DoesNotContain("Standard = reader.IsDBNull", mapper, StringComparison.Ordinal);
            Assert.DoesNotContain("Result = reader.IsDBNull", mapper, StringComparison.Ordinal);
            Assert.DoesNotContain("Notes = reader.IsDBNull", mapper, StringComparison.Ordinal);
        }

        [Fact]
        public void CalibrationReadNormalizerTrimsValuesAndPreservesEmptyFallback()
        {
            var source = ReadRepoFile("InventoryManagementApp", "Services", "Calibration", "CalibrationService.cs");
            var normalizer = ExtractMethod(source, "private static string NormalizeCalibrationReadText(SqliteDataReader reader, string columnName)", "private static void NormalizeCalibrationRecordForSave");

            AssertContainsAll(
                normalizer,
                "var ordinal = reader.GetOrdinal(columnName);",
                "return reader.IsDBNull(ordinal) ? string.Empty : reader.GetString(ordinal).Trim();");
        }

        [Fact]
        public void CalibrationReadMethodsShareTheNormalizedMapper()
        {
            var source = ReadRepoFile("InventoryManagementApp", "Services", "Calibration", "CalibrationService.cs");

            AssertReadMethodUsesMapper(source, "public async Task<List<CalibrationRecord>> GetAllCalibrationRecordsAsync()", "public async Task<int> CountCalibrationRecordsAsync()");
            AssertReadMethodUsesMapper(source, "public async Task<List<CalibrationRecord>> GetCalibrationRecordsByItemAsync(int itemID)", "public async Task<List<CalibrationRecord>> GetOverdueCalibrationAsync()");
            AssertReadMethodUsesMapper(source, "public async Task<List<CalibrationRecord>> GetOverdueCalibrationAsync()", "public async Task<int> CountOverdueCalibrationAsync()");
            AssertReadMethodUsesMapper(source, "public async Task<List<CalibrationRecord>> GetUpcomingCalibrationAsync(int days = 30)", "public async Task<int> CountUpcomingCalibrationAsync(int days = 30)");
            AssertReadMethodUsesMapper(source, "public async Task<CalibrationRecord?> GetLatestCalibrationForItemAsync(int itemID)", "public async Task<CalibrationRecord?> GetCalibrationRecordByIdAsync(int calibrationID)");
            AssertReadMethodUsesMapper(source, "public async Task<CalibrationRecord?> GetCalibrationRecordByIdAsync(int calibrationID)", "public async Task<int> CreateCalibrationRecordAsync(CalibrationRecord record)");
        }

        private static void AssertScheduledFilterNormalizesStatus(string source, string startMarker, string endMarker)
        {
            var method = ExtractMethod(source, startMarker, endMarker);
            Assert.Contains("TRIM(IFNULL(m.Status, '')) = 'Scheduled'", method, StringComparison.Ordinal);
            Assert.DoesNotContain("m.Status = 'Scheduled'", method, StringComparison.Ordinal);
        }

        private static void AssertReadMethodUsesMapper(string source, string startMarker, string endMarker)
        {
            var method = ExtractMethod(source, startMarker, endMarker);
            Assert.Contains("Map", method, StringComparison.Ordinal);
            Assert.Matches(@"Map(?:Maintenance|Calibration)Record\(reader\)", method);
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
