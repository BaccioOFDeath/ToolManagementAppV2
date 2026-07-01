using System;
using System.IO;
using Xunit;

namespace InventoryManagementApp.Tests
{
    public class OperationalRecordNormalizationContractTests
    {
        [Fact]
        public void MaintenanceCreateNormalizesTextBeforeReferenceChecksAndInsert()
        {
            var source = ReadRepoFile("InventoryManagementApp", "Services", "Maintenance", "MaintenanceService.cs");
            var method = ExtractMethod(
                source,
                "public async Task<int> CreateMaintenanceRecordAsync",
                "public async Task<bool> UpdateMaintenanceRecordAsync");

            Assert.Contains("NormalizeMaintenanceRecordForSave(record);", method, StringComparison.Ordinal);
            Assert.True(
                method.IndexOf("NormalizeMaintenanceRecordForSave(record);", StringComparison.Ordinal) < method.IndexOf("EnsureItemExists(conn, record.ItemID);", StringComparison.Ordinal),
                "Maintenance creation should normalize user-entered text before reference checks and insert work.");
            Assert.True(
                method.IndexOf("NormalizeMaintenanceRecordForSave(record);", StringComparison.Ordinal) < method.IndexOf("cmd.Parameters.AddWithValue(\"@MaintenanceType\", record.MaintenanceType);", StringComparison.Ordinal),
                "Maintenance creation should only persist normalized maintenance type text.");
            Assert.True(
                method.IndexOf("NormalizeMaintenanceRecordForSave(record);", StringComparison.Ordinal) < method.IndexOf("cmd.Parameters.AddWithValue(\"@Status\", record.Status);", StringComparison.Ordinal),
                "Maintenance creation should only persist normalized status text.");
        }

        [Fact]
        public void MaintenanceUpdateNormalizesTextBeforeReferenceChecksAndUpdate()
        {
            var source = ReadRepoFile("InventoryManagementApp", "Services", "Maintenance", "MaintenanceService.cs");
            var method = ExtractMethod(
                source,
                "public async Task<bool> UpdateMaintenanceRecordAsync",
                "public async Task<bool> CompleteMaintenanceAsync");

            Assert.Contains("NormalizeMaintenanceRecordForSave(record);", method, StringComparison.Ordinal);
            Assert.True(
                method.IndexOf("NormalizeMaintenanceRecordForSave(record);", StringComparison.Ordinal) < method.IndexOf("EnsureMaintenanceRecordExists(conn, record.MaintenanceID);", StringComparison.Ordinal),
                "Maintenance updates should normalize user-entered text before existing-row checks.");
            Assert.True(
                method.IndexOf("NormalizeMaintenanceRecordForSave(record);", StringComparison.Ordinal) < method.IndexOf("cmd.Parameters.AddWithValue(\"@Description\", record.Description);", StringComparison.Ordinal),
                "Maintenance updates should only persist normalized description text.");
            Assert.True(
                method.IndexOf("NormalizeMaintenanceRecordForSave(record);", StringComparison.Ordinal) < method.IndexOf("cmd.Parameters.AddWithValue(\"@Notes\", record.Notes);", StringComparison.Ordinal),
                "Maintenance updates should only persist normalized notes text.");
        }

        [Fact]
        public void MaintenanceCompletionNormalizesPerformerAndNotesBeforeUpdate()
        {
            var source = ReadRepoFile("InventoryManagementApp", "Services", "Maintenance", "MaintenanceService.cs");
            var method = ExtractMethod(
                source,
                "public async Task<bool> CompleteMaintenanceAsync",
                "public async Task<bool> DeleteMaintenanceRecordAsync");

            Assert.Contains("var normalizedPerformedBy = NormalizeOptionalText(performedBy);", method, StringComparison.Ordinal);
            Assert.Contains("var normalizedNotes = NormalizeOptionalText(notes);", method, StringComparison.Ordinal);
            Assert.True(
                method.IndexOf("var normalizedPerformedBy = NormalizeOptionalText(performedBy);", StringComparison.Ordinal) < method.IndexOf("EnsureMaintenanceRecordExists(conn, maintenanceID);", StringComparison.Ordinal),
                "Maintenance completion should trim performer text before persistence work starts.");
            Assert.Contains("cmd.Parameters.AddWithValue(\"@PerformedBy\", normalizedPerformedBy);", method, StringComparison.Ordinal);
            Assert.Contains("cmd.Parameters.AddWithValue(\"@Notes\", normalizedNotes);", method, StringComparison.Ordinal);
        }

        [Fact]
        public void MaintenanceNormalizerCoversPersistedWorkflowTextFields()
        {
            var source = ReadRepoFile("InventoryManagementApp", "Services", "Maintenance", "MaintenanceService.cs");
            var normalizer = ExtractMethod(
                source,
                "private static void NormalizeMaintenanceRecordForSave",
                "private static void EnsureMaintenanceCreateSucceeded");

            Assert.Contains("record.MaintenanceType = NormalizeOptionalText(record.MaintenanceType);", normalizer, StringComparison.Ordinal);
            Assert.Contains("record.Description = NormalizeOptionalText(record.Description);", normalizer, StringComparison.Ordinal);
            Assert.Contains("record.PerformedBy = NormalizeOptionalText(record.PerformedBy);", normalizer, StringComparison.Ordinal);
            Assert.Contains("record.Status = NormalizeMaintenanceStatus(record.Status);", normalizer, StringComparison.Ordinal);
            Assert.Contains("record.Notes = NormalizeOptionalText(record.Notes);", normalizer, StringComparison.Ordinal);
            Assert.Contains("return string.IsNullOrEmpty(normalizedStatus) ? \"Scheduled\" : normalizedStatus;", normalizer, StringComparison.Ordinal);
            Assert.Contains("private static string NormalizeOptionalText(string? value) => value?.Trim() ?? string.Empty;", normalizer, StringComparison.Ordinal);
        }

        [Fact]
        public void CalibrationCreateNormalizesTextBeforeReferenceChecksAndInsert()
        {
            var source = ReadRepoFile("InventoryManagementApp", "Services", "Calibration", "CalibrationService.cs");
            var method = ExtractMethod(
                source,
                "public async Task<int> CreateCalibrationRecordAsync",
                "public async Task<bool> UpdateCalibrationRecordAsync");

            Assert.Contains("NormalizeCalibrationRecordForSave(record);", method, StringComparison.Ordinal);
            Assert.True(
                method.IndexOf("NormalizeCalibrationRecordForSave(record);", StringComparison.Ordinal) < method.IndexOf("EnsureItemExists(conn, record.ItemID);", StringComparison.Ordinal),
                "Calibration creation should normalize user-entered text before reference checks and insert work.");
            Assert.True(
                method.IndexOf("NormalizeCalibrationRecordForSave(record);", StringComparison.Ordinal) < method.IndexOf("cmd.Parameters.AddWithValue(\"@CalibratedBy\", record.CalibratedBy);", StringComparison.Ordinal),
                "Calibration creation should only persist normalized technician text.");
            Assert.True(
                method.IndexOf("NormalizeCalibrationRecordForSave(record);", StringComparison.Ordinal) < method.IndexOf("cmd.Parameters.AddWithValue(\"@CertificateNumber\", record.CertificateNumber);", StringComparison.Ordinal),
                "Calibration creation should only persist normalized certificate text.");
        }

        [Fact]
        public void CalibrationUpdateNormalizesTextBeforeReferenceChecksAndUpdate()
        {
            var source = ReadRepoFile("InventoryManagementApp", "Services", "Calibration", "CalibrationService.cs");
            var method = ExtractMethod(
                source,
                "public async Task<bool> UpdateCalibrationRecordAsync",
                "public async Task<bool> DeleteCalibrationRecordAsync");

            Assert.Contains("NormalizeCalibrationRecordForSave(record);", method, StringComparison.Ordinal);
            Assert.True(
                method.IndexOf("NormalizeCalibrationRecordForSave(record);", StringComparison.Ordinal) < method.IndexOf("EnsureCalibrationRecordExists(conn, record.CalibrationID);", StringComparison.Ordinal),
                "Calibration updates should normalize user-entered text before existing-row checks.");
            Assert.True(
                method.IndexOf("NormalizeCalibrationRecordForSave(record);", StringComparison.Ordinal) < method.IndexOf("cmd.Parameters.AddWithValue(\"@Standard\", record.Standard);", StringComparison.Ordinal),
                "Calibration updates should only persist normalized standard text.");
            Assert.True(
                method.IndexOf("NormalizeCalibrationRecordForSave(record);", StringComparison.Ordinal) < method.IndexOf("cmd.Parameters.AddWithValue(\"@Notes\", record.Notes);", StringComparison.Ordinal),
                "Calibration updates should only persist normalized notes text.");
        }

        [Fact]
        public void CalibrationNormalizerCoversPersistedWorkflowTextFields()
        {
            var source = ReadRepoFile("InventoryManagementApp", "Services", "Calibration", "CalibrationService.cs");
            var normalizer = ExtractMethod(
                source,
                "private static void NormalizeCalibrationRecordForSave",
                "private static void EnsureCalibrationCreateSucceeded");

            Assert.Contains("record.CalibratedBy = NormalizeOptionalText(record.CalibratedBy);", normalizer, StringComparison.Ordinal);
            Assert.Contains("record.CertificateNumber = NormalizeOptionalText(record.CertificateNumber);", normalizer, StringComparison.Ordinal);
            Assert.Contains("record.Standard = NormalizeOptionalText(record.Standard);", normalizer, StringComparison.Ordinal);
            Assert.Contains("record.Result = NormalizeOptionalText(record.Result);", normalizer, StringComparison.Ordinal);
            Assert.Contains("record.Notes = NormalizeOptionalText(record.Notes);", normalizer, StringComparison.Ordinal);
            Assert.Contains("private static string NormalizeOptionalText(string? value) => value?.Trim() ?? string.Empty;", normalizer, StringComparison.Ordinal);
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
