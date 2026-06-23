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

            Assert.Contains("ClearMaintenanceStateAfterLoadFailure();\n                await _dialogService.ShowErrorAsync(\"Error loading maintenance records\", $\"{ex.Message} Maintenance rows were cleared until reload succeeds.\");", source, StringComparison.Ordinal);
            Assert.Contains("private void ClearMaintenanceStateAfterLoadFailure()", source, StringComparison.Ordinal);
            Assert.Contains("MaintenanceRecords.Clear();\n            FilteredMaintenanceRecords.Clear();\n            SelectedRecord = null;\n            NotifyCommandStatesAndSummaries();", source, StringComparison.Ordinal);
            Assert.Contains("CompleteMaintenanceCommand.NotifyCanExecuteChanged();", source, StringComparison.Ordinal);
            Assert.Contains("OnPropertyChanged(nameof(MaintenanceBacklogSummary));\n            OnPropertyChanged(nameof(MaintenanceResultsSummary));", source, StringComparison.Ordinal);
            Assert.DoesNotContain("await _dialogService.ShowErrorAsync(\"Error loading maintenance records\", ex.Message);", source, StringComparison.Ordinal);
        }

        [Fact]
        public void MaintenanceMutationFailuresRefreshRowsOrClearAfterRecoveryFailure()
        {
            var source = ReadRepoFile("InventoryManagementApp", "ViewModels", "MaintenanceManagementViewModel.cs");

            Assert.Contains("await RefreshMaintenanceAfterMutationFailureAsync(\n                        newRecord.MaintenanceID > 0 ? newRecord.MaintenanceID : null,\n                        \"Error creating maintenance record\",\n                        $\"{ex.Message} Maintenance rows were refreshed from saved data.\");", source, StringComparison.Ordinal);
            Assert.Contains("await RefreshMaintenanceAfterMutationFailureAsync(\n                        clone.MaintenanceID,\n                        \"Error updating maintenance record\",\n                        $\"{ex.Message} Maintenance rows were refreshed from saved data.\");", source, StringComparison.Ordinal);
            Assert.Contains("var deletedRecord = SelectedRecord;\n                try", source, StringComparison.Ordinal);
            Assert.Contains("await RefreshMaintenanceAfterMutationFailureAsync(\n                        deletedRecord.MaintenanceID,\n                        \"Error deleting maintenance record\",\n                        $\"{ex.Message} Maintenance rows were refreshed from saved data.\",\n                        clearSelectionWhenAffectedRecordIsGone: true);", source, StringComparison.Ordinal);
            Assert.Contains("var completedId = SelectedRecord.MaintenanceID;\n                try", source, StringComparison.Ordinal);
            Assert.Contains("await RefreshMaintenanceAfterMutationFailureAsync(\n                        completedId,\n                        \"Error completing maintenance\",\n                        $\"{ex.Message} Maintenance rows were refreshed from saved data.\");", source, StringComparison.Ordinal);
            Assert.Contains("private async Task RefreshMaintenanceAfterMutationFailureAsync(", source, StringComparison.Ordinal);
            Assert.Contains("var records = await _maintenanceService.GetAllMaintenanceRecordsAsync();\n                MaintenanceRecords.Clear();", source, StringComparison.Ordinal);
            Assert.Contains("clearSelectionWhenAffectedRecordIsGone\n                    && preferredMaintenanceId.HasValue\n                    && MaintenanceRecords.All(r => r.MaintenanceID != preferredMaintenanceId.Value)", source, StringComparison.Ordinal);
            Assert.Contains("SelectedRecord = null;", source, StringComparison.Ordinal);
            Assert.Contains("Recovery refresh also failed: {refreshEx.Message} Maintenance rows were cleared until reload succeeds.", source, StringComparison.Ordinal);
            Assert.DoesNotContain("await _dialogService.ShowErrorAsync(\"Error creating maintenance record\", ex.Message);", source, StringComparison.Ordinal);
            Assert.DoesNotContain("await _dialogService.ShowErrorAsync(\"Error updating maintenance record\", ex.Message);", source, StringComparison.Ordinal);
            Assert.DoesNotContain("await _dialogService.ShowErrorAsync(\"Error deleting maintenance record\", ex.Message);", source, StringComparison.Ordinal);
            Assert.DoesNotContain("await _dialogService.ShowErrorAsync(\"Error completing maintenance\", ex.Message);", source, StringComparison.Ordinal);
        }

        [Fact]
        public void CalibrationLoadFailuresClearStaleRowsAndSelection()
        {
            var source = ReadRepoFile("InventoryManagementApp", "ViewModels", "CalibrationManagementViewModel.cs");

            Assert.Contains("ClearCalibrationStateAfterLoadFailure();\n                await _dialogService.ShowErrorAsync(\"Error loading calibration records\", $\"{ex.Message} Calibration rows were cleared until reload succeeds.\");", source, StringComparison.Ordinal);
            Assert.Contains("private void ClearCalibrationStateAfterLoadFailure()", source, StringComparison.Ordinal);
            Assert.Contains("CalibrationRecords.Clear();\n            FilteredCalibrationRecords.Clear();\n            SelectedRecord = null;\n            NotifyCommandStatesAndSummaries();", source, StringComparison.Ordinal);
            Assert.Contains("EditCalibrationCommand.NotifyCanExecuteChanged();\n            DeleteCalibrationCommand.NotifyCanExecuteChanged();\n            OpenCalibrationDetailsCommand.NotifyCanExecuteChanged();", source, StringComparison.Ordinal);
            Assert.Contains("OnPropertyChanged(nameof(CalibrationBacklogSummary));\n            OnPropertyChanged(nameof(CalibrationResultsSummary));", source, StringComparison.Ordinal);
            Assert.DoesNotContain("await _dialogService.ShowErrorAsync(\"Error loading calibration records\", ex.Message);", source, StringComparison.Ordinal);
        }

        [Fact]
        public void CalibrationMutationFailuresRefreshRowsOrClearAfterRecoveryFailure()
        {
            var source = ReadRepoFile("InventoryManagementApp", "ViewModels", "CalibrationManagementViewModel.cs");

            Assert.Contains("await RefreshCalibrationAfterMutationFailureAsync(\n                        newRecord.CalibrationID > 0 ? newRecord.CalibrationID : null,\n                        \"Error creating calibration record\",\n                        $\"{ex.Message} Calibration rows were refreshed from saved data.\");", source, StringComparison.Ordinal);
            Assert.Contains("await RefreshCalibrationAfterMutationFailureAsync(\n                        clone.CalibrationID,\n                        \"Error updating calibration record\",\n                        $\"{ex.Message} Calibration rows were refreshed from saved data.\");", source, StringComparison.Ordinal);
            Assert.Contains("var deletedRecord = SelectedRecord;\n                try", source, StringComparison.Ordinal);
            Assert.Contains("await RefreshCalibrationAfterMutationFailureAsync(\n                        deletedRecord.CalibrationID,\n                        \"Error deleting calibration record\",\n                        $\"{ex.Message} Calibration rows were refreshed from saved data.\",\n                        clearSelectionWhenAffectedRecordIsGone: true);", source, StringComparison.Ordinal);
            Assert.Contains("private async Task RefreshCalibrationAfterMutationFailureAsync(", source, StringComparison.Ordinal);
            Assert.Contains("var records = await _calibrationService.GetAllCalibrationRecordsAsync();\n                CalibrationRecords.Clear();", source, StringComparison.Ordinal);
            Assert.Contains("clearSelectionWhenAffectedRecordIsGone\n                    && preferredCalibrationId.HasValue\n                    && CalibrationRecords.All(r => r.CalibrationID != preferredCalibrationId.Value)", source, StringComparison.Ordinal);
            Assert.Contains("SelectedRecord = null;", source, StringComparison.Ordinal);
            Assert.Contains("Recovery refresh also failed: {refreshEx.Message} Calibration rows were cleared until reload succeeds.", source, StringComparison.Ordinal);
            Assert.DoesNotContain("await _dialogService.ShowErrorAsync(\"Error creating calibration record\", ex.Message);", source, StringComparison.Ordinal);
            Assert.DoesNotContain("await _dialogService.ShowErrorAsync(\"Error updating calibration record\", ex.Message);", source, StringComparison.Ordinal);
            Assert.DoesNotContain("await _dialogService.ShowErrorAsync(\"Error deleting calibration record\", ex.Message);", source, StringComparison.Ordinal);
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
