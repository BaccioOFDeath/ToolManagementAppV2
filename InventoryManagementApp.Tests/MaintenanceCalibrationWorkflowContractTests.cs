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
