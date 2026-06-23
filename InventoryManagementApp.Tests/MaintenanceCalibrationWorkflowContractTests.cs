using System;
using System.IO;
using Xunit;

namespace InventoryManagementApp.Tests
{
    public class MaintenanceCalibrationWorkflowContractTests
    {
        [Fact]
        public void MaintenanceLoadFailuresClearStaleRowsAndSelection()
        {
            var source = ReadRepoFile("InventoryManagementApp", "ViewModels", "MaintenanceManagementViewModel.cs");

            AssertContainsAll(
                source,
                "ClearMaintenanceStateAfterLoadFailure();",
                "await _dialogService.ShowErrorAsync(\"Error loading maintenance records\", $\"{ex.Message} Maintenance rows were cleared until reload succeeds.\");",
                "private void ClearMaintenanceStateAfterLoadFailure()",
                "MaintenanceRecords.Clear();",
                "FilteredMaintenanceRecords.Clear();",
                "SelectedRecord = null;",
                "NotifyCommandStatesAndSummaries();",
                "CompleteMaintenanceCommand.NotifyCanExecuteChanged();",
                "OnPropertyChanged(nameof(MaintenanceBacklogSummary));",
                "OnPropertyChanged(nameof(MaintenanceResultsSummary));");
            Assert.DoesNotContain("await _dialogService.ShowErrorAsync(\"Error loading maintenance records\", ex.Message);", source, StringComparison.Ordinal);
        }

        [Fact]
        public void MaintenanceMutationFailuresRefreshRowsOrClearAfterRecoveryFailure()
        {
            var source = ReadRepoFile("InventoryManagementApp", "ViewModels", "MaintenanceManagementViewModel.cs");

            AssertContainsAll(
                source,
                "newRecord.MaintenanceID > 0 ? newRecord.MaintenanceID : null",
                "\"Error creating maintenance record\"",
                "clone.MaintenanceID",
                "\"Error updating maintenance record\"",
                "var deletedRecord = SelectedRecord;",
                "deletedRecord.MaintenanceID",
                "\"Error deleting maintenance record\"",
                "var completedId = SelectedRecord.MaintenanceID;",
                "\"Error completing maintenance\"",
                "private async Task RefreshMaintenanceAfterMutationFailureAsync(",
                "var records = await _maintenanceService.GetAllMaintenanceRecordsAsync();",
                "MaintenanceRecords.Clear();",
                "clearSelectionWhenAffectedRecordIsGone",
                "MaintenanceRecords.All(r => r.MaintenanceID != preferredMaintenanceId.Value)",
                "SelectedRecord = null;",
                "Recovery refresh also failed: {refreshEx.Message} Maintenance rows were cleared until reload succeeds.");
            Assert.True(
                CountOccurrences(source, "await RefreshMaintenanceAfterMutationFailureAsync(") >= 4,
                "Expected create, update, delete, and complete maintenance failure paths to refresh or clear rows.");
            Assert.DoesNotContain("await _dialogService.ShowErrorAsync(\"Error creating maintenance record\", ex.Message);", source, StringComparison.Ordinal);
            Assert.DoesNotContain("await _dialogService.ShowErrorAsync(\"Error updating maintenance record\", ex.Message);", source, StringComparison.Ordinal);
            Assert.DoesNotContain("await _dialogService.ShowErrorAsync(\"Error deleting maintenance record\", ex.Message);", source, StringComparison.Ordinal);
            Assert.DoesNotContain("await _dialogService.ShowErrorAsync(\"Error completing maintenance\", ex.Message);", source, StringComparison.Ordinal);
        }

        [Fact]
        public void CalibrationLoadFailuresClearStaleRowsAndSelection()
        {
            var source = ReadRepoFile("InventoryManagementApp", "ViewModels", "CalibrationManagementViewModel.cs");

            AssertContainsAll(
                source,
                "ClearCalibrationStateAfterLoadFailure();",
                "await _dialogService.ShowErrorAsync(\"Error loading calibration records\", $\"{ex.Message} Calibration rows were cleared until reload succeeds.\");",
                "private void ClearCalibrationStateAfterLoadFailure()",
                "CalibrationRecords.Clear();",
                "FilteredCalibrationRecords.Clear();",
                "SelectedRecord = null;",
                "NotifyCommandStatesAndSummaries();",
                "EditCalibrationCommand.NotifyCanExecuteChanged();",
                "DeleteCalibrationCommand.NotifyCanExecuteChanged();",
                "OpenCalibrationDetailsCommand.NotifyCanExecuteChanged();",
                "OnPropertyChanged(nameof(CalibrationBacklogSummary));",
                "OnPropertyChanged(nameof(CalibrationResultsSummary));");
            Assert.DoesNotContain("await _dialogService.ShowErrorAsync(\"Error loading calibration records\", ex.Message);", source, StringComparison.Ordinal);
        }

        [Fact]
        public void CalibrationMutationFailuresRefreshRowsOrClearAfterRecoveryFailure()
        {
            var source = ReadRepoFile("InventoryManagementApp", "ViewModels", "CalibrationManagementViewModel.cs");

            AssertContainsAll(
                source,
                "newRecord.CalibrationID > 0 ? newRecord.CalibrationID : null",
                "\"Error creating calibration record\"",
                "clone.CalibrationID",
                "\"Error updating calibration record\"",
                "var deletedRecord = SelectedRecord;",
                "deletedRecord.CalibrationID",
                "\"Error deleting calibration record\"",
                "private async Task RefreshCalibrationAfterMutationFailureAsync(",
                "var records = await _calibrationService.GetAllCalibrationRecordsAsync();",
                "CalibrationRecords.Clear();",
                "clearSelectionWhenAffectedRecordIsGone",
                "CalibrationRecords.All(r => r.CalibrationID != preferredCalibrationId.Value)",
                "SelectedRecord = null;",
                "Recovery refresh also failed: {refreshEx.Message} Calibration rows were cleared until reload succeeds.");
            Assert.True(
                CountOccurrences(source, "await RefreshCalibrationAfterMutationFailureAsync(") >= 3,
                "Expected create, update, and delete calibration failure paths to refresh or clear rows.");
            Assert.DoesNotContain("await _dialogService.ShowErrorAsync(\"Error creating calibration record\", ex.Message);", source, StringComparison.Ordinal);
            Assert.DoesNotContain("await _dialogService.ShowErrorAsync(\"Error updating calibration record\", ex.Message);", source, StringComparison.Ordinal);
            Assert.DoesNotContain("await _dialogService.ShowErrorAsync(\"Error deleting calibration record\", ex.Message);", source, StringComparison.Ordinal);
        }

        private static void AssertContainsAll(string source, params string[] expectedSnippets)
        {
            foreach (var snippet in expectedSnippets)
            {
                Assert.Contains(snippet, source, StringComparison.Ordinal);
            }
        }

        private static int CountOccurrences(string source, string value)
        {
            var count = 0;
            var index = 0;

            while ((index = source.IndexOf(value, index, StringComparison.Ordinal)) >= 0)
            {
                count++;
                index += value.Length;
            }

            return count;
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