using System;
using System.IO;
using Xunit;

namespace InventoryManagementApp.Tests
{
    public class OperationalServiceRegisterVisibleWindowContractTests
    {
        [Fact]
        public void MaintenanceViewModel_BoundsLiveRowsAndKeepsFullMatchState()
        {
            var source = ReadRepoFile("InventoryManagementApp", "ViewModels", "MaintenanceManagementViewModel.cs");

            Assert.Contains("private const int MaxMaintenanceVisibleRows = 500;", source, StringComparison.Ordinal);
            Assert.Contains("private int _maintenanceMatchCount;", source, StringComparison.Ordinal);
            Assert.Contains("public int MaintenanceMatchCount => _maintenanceMatchCount;", source, StringComparison.Ordinal);
            Assert.Contains("public int MaintenanceVisibleCount => FilteredMaintenanceRecords.Count;", source, StringComparison.Ordinal);
            Assert.Contains("public int MaintenanceOmittedCount => Math.Max(0, MaintenanceMatchCount - MaintenanceVisibleCount);", source, StringComparison.Ordinal);
            Assert.Contains("public bool IsMaintenanceWindowCapped => MaintenanceOmittedCount > 0;", source, StringComparison.Ordinal);
            Assert.Contains("public string MaintenanceVisibleWindowSummary", source, StringComparison.Ordinal);
            Assert.Contains("Showing first {MaintenanceVisibleCount} of {MaintenanceMatchCount} matching maintenance rows", source, StringComparison.Ordinal);
            Assert.Contains("? $\"{MaintenanceVisibleCount} of {MaintenanceMatchCount} maintenance records shown\"", source, StringComparison.Ordinal);
        }

        [Fact]
        public void MaintenanceViewModel_AppliesCappedWindowWithoutUnchangedGridChurn()
        {
            var source = ReadRepoFile("InventoryManagementApp", "ViewModels", "MaintenanceManagementViewModel.cs");
            var applyFilter = ExtractSourceBlock(source, "private void ApplyFilter", "private void OpenMaintenanceDetails");
            var sameWindow = ExtractSourceBlock(source, "private static bool IsSameVisibleWindow", "private static FlowDocument CreateMaintenanceDocument");

            Assert.Contains("var visibleList = filteredList.Take(MaxMaintenanceVisibleRows).ToList();", applyFilter, StringComparison.Ordinal);
            Assert.Contains("_maintenanceMatchCount = filteredList.Count;", applyFilter, StringComparison.Ordinal);
            Assert.Contains("if (!IsSameVisibleWindow(FilteredMaintenanceRecords, visibleList))", applyFilter, StringComparison.Ordinal);
            Assert.Contains("FilteredMaintenanceRecords.Clear();", applyFilter, StringComparison.Ordinal);
            Assert.Contains("foreach (var record in visibleList)", applyFilter, StringComparison.Ordinal);
            Assert.Contains("ReferenceEquals(currentRows[i], nextRows[i])", sameWindow, StringComparison.Ordinal);
            Assert.Contains("System.Collections.Generic.IReadOnlyList<MaintenanceRecord>", sameWindow, StringComparison.Ordinal);
            Assert.DoesNotContain("foreach (var record in filteredList)\n            {\n                FilteredMaintenanceRecords.Add(record);", applyFilter, StringComparison.Ordinal);
        }

        [Fact]
        public void MaintenancePrintPreview_ReportsMatchedVisiblePrintedAndHiddenRows()
        {
            var source = ReadRepoFile("InventoryManagementApp", "ViewModels", "MaintenanceManagementViewModel.cs");
            var printList = ExtractSourceBlock(source, "private void PrintMaintenanceList()", "private void PrintSelectedMaintenance()");

            Assert.Contains("var matchedRows = MaintenanceMatchCount;", printList, StringComparison.Ordinal);
            Assert.Contains("var visibleRows = MaintenanceVisibleCount;", printList, StringComparison.Ordinal);
            Assert.Contains("var omittedRows = Math.Max(0, matchedRows - printRows.Count);", printList, StringComparison.Ordinal);
            Assert.Contains("var hiddenRows = Math.Max(0, matchedRows - visibleRows);", printList, StringComparison.Ordinal);
            Assert.Contains("Matched: {matchedRows} | Visible: {visibleRows} | Printed: {printRows.Count} | Omitted: {omittedRows}", printList, StringComparison.Ordinal);
            Assert.Contains("additional matching maintenance rows are outside the live grid window", printList, StringComparison.Ordinal);
            Assert.Contains("live-grid limits", printList, StringComparison.Ordinal);
        }

        [Fact]
        public void MaintenanceViewModel_NotifiesVisibleWindowPropertiesAndResetsAfterFailure()
        {
            var source = ReadRepoFile("InventoryManagementApp", "ViewModels", "MaintenanceManagementViewModel.cs");
            var clearState = ExtractSourceBlock(source, "private void ClearMaintenanceStateAfterLoadFailure", "private async Task RefreshMaintenanceAfterMutationFailureAsync");
            var notifications = ExtractSourceBlock(source, "private void NotifyMaintenanceListStateChanged", "private void OnSelectedRecordSummariesChanged");

            Assert.Contains("_maintenanceMatchCount = 0;", clearState, StringComparison.Ordinal);
            Assert.Contains("OnPropertyChanged(nameof(MaintenanceMatchCount));", notifications, StringComparison.Ordinal);
            Assert.Contains("OnPropertyChanged(nameof(MaintenanceVisibleCount));", notifications, StringComparison.Ordinal);
            Assert.Contains("OnPropertyChanged(nameof(MaintenanceOmittedCount));", notifications, StringComparison.Ordinal);
            Assert.Contains("OnPropertyChanged(nameof(IsMaintenanceWindowCapped));", notifications, StringComparison.Ordinal);
            Assert.Contains("OnPropertyChanged(nameof(MaintenanceVisibleWindowSummary));", notifications, StringComparison.Ordinal);
        }

        [Fact]
        public void CalibrationViewModel_BoundsLiveRowsAndKeepsFullMatchState()
        {
            var source = ReadRepoFile("InventoryManagementApp", "ViewModels", "CalibrationManagementViewModel.cs");

            Assert.Contains("private const int MaxCalibrationVisibleRows = 500;", source, StringComparison.Ordinal);
            Assert.Contains("private int _calibrationMatchCount;", source, StringComparison.Ordinal);
            Assert.Contains("public int CalibrationMatchCount => _calibrationMatchCount;", source, StringComparison.Ordinal);
            Assert.Contains("public int CalibrationVisibleCount => FilteredCalibrationRecords.Count;", source, StringComparison.Ordinal);
            Assert.Contains("public int CalibrationOmittedCount => Math.Max(0, CalibrationMatchCount - CalibrationVisibleCount);", source, StringComparison.Ordinal);
            Assert.Contains("public bool IsCalibrationWindowCapped => CalibrationOmittedCount > 0;", source, StringComparison.Ordinal);
            Assert.Contains("public string CalibrationVisibleWindowSummary", source, StringComparison.Ordinal);
            Assert.Contains("Showing first {CalibrationVisibleCount} of {CalibrationMatchCount} matching calibration rows", source, StringComparison.Ordinal);
            Assert.Contains("? $\"{CalibrationVisibleCount} of {CalibrationMatchCount} calibration records shown\"", source, StringComparison.Ordinal);
        }

        [Fact]
        public void CalibrationViewModel_AppliesCappedWindowWithoutUnchangedGridChurn()
        {
            var source = ReadRepoFile("InventoryManagementApp", "ViewModels", "CalibrationManagementViewModel.cs");
            var applyFilter = ExtractSourceBlock(source, "private void ApplyFilter", "private void OpenCalibrationDetails");
            var sameWindow = ExtractSourceBlock(source, "private static bool IsSameVisibleWindow", "private static FlowDocument CreateCalibrationDocument");

            Assert.Contains("var visibleList = filteredList.Take(MaxCalibrationVisibleRows).ToList();", applyFilter, StringComparison.Ordinal);
            Assert.Contains("_calibrationMatchCount = filteredList.Count;", applyFilter, StringComparison.Ordinal);
            Assert.Contains("if (!IsSameVisibleWindow(FilteredCalibrationRecords, visibleList))", applyFilter, StringComparison.Ordinal);
            Assert.Contains("FilteredCalibrationRecords.Clear();", applyFilter, StringComparison.Ordinal);
            Assert.Contains("foreach (var record in visibleList)", applyFilter, StringComparison.Ordinal);
            Assert.Contains("ReferenceEquals(currentRows[i], nextRows[i])", sameWindow, StringComparison.Ordinal);
            Assert.Contains("System.Collections.Generic.IReadOnlyList<CalibrationRecord>", sameWindow, StringComparison.Ordinal);
            Assert.DoesNotContain("foreach (var record in filteredList)\n            {\n                FilteredCalibrationRecords.Add(record);", applyFilter, StringComparison.Ordinal);
        }

        [Fact]
        public void CalibrationPrintPreview_ReportsMatchedVisiblePrintedAndHiddenRows()
        {
            var source = ReadRepoFile("InventoryManagementApp", "ViewModels", "CalibrationManagementViewModel.cs");
            var printList = ExtractSourceBlock(source, "private void PrintCalibrationList()", "private void PrintSelectedCalibration()");

            Assert.Contains("var matchedRows = CalibrationMatchCount;", printList, StringComparison.Ordinal);
            Assert.Contains("var visibleRows = CalibrationVisibleCount;", printList, StringComparison.Ordinal);
            Assert.Contains("var omittedRows = Math.Max(0, matchedRows - printRows.Count);", printList, StringComparison.Ordinal);
            Assert.Contains("var hiddenRows = Math.Max(0, matchedRows - visibleRows);", printList, StringComparison.Ordinal);
            Assert.Contains("Matched: {matchedRows} | Visible: {visibleRows} | Printed: {printRows.Count} | Omitted: {omittedRows}", printList, StringComparison.Ordinal);
            Assert.Contains("additional matching calibration rows are outside the live grid window", printList, StringComparison.Ordinal);
            Assert.Contains("live-grid limits", printList, StringComparison.Ordinal);
        }

        [Fact]
        public void CalibrationViewModel_NotifiesVisibleWindowPropertiesAndResetsAfterFailure()
        {
            var source = ReadRepoFile("InventoryManagementApp", "ViewModels", "CalibrationManagementViewModel.cs");
            var clearState = ExtractSourceBlock(source, "private void ClearCalibrationStateAfterLoadFailure", "private async Task RefreshCalibrationAfterMutationFailureAsync");
            var notifications = ExtractSourceBlock(source, "private void NotifyCalibrationListStateChanged", "private void OnSelectedRecordSummariesChanged");

            Assert.Contains("_calibrationMatchCount = 0;", clearState, StringComparison.Ordinal);
            Assert.Contains("OnPropertyChanged(nameof(CalibrationMatchCount));", notifications, StringComparison.Ordinal);
            Assert.Contains("OnPropertyChanged(nameof(CalibrationVisibleCount));", notifications, StringComparison.Ordinal);
            Assert.Contains("OnPropertyChanged(nameof(CalibrationOmittedCount));", notifications, StringComparison.Ordinal);
            Assert.Contains("OnPropertyChanged(nameof(IsCalibrationWindowCapped));", notifications, StringComparison.Ordinal);
            Assert.Contains("OnPropertyChanged(nameof(CalibrationVisibleWindowSummary));", notifications, StringComparison.Ordinal);
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

        private static string ExtractSourceBlock(string source, string startMarker, string endMarker)
        {
            var start = source.IndexOf(startMarker, StringComparison.Ordinal);
            Assert.True(start >= 0, $"Could not find source block start marker: {startMarker}");

            var end = source.IndexOf(endMarker, start, StringComparison.Ordinal);
            Assert.True(end > start, $"Could not find source block end marker: {endMarker}");

            return source[start..end];
        }

        private static string NormalizeLineEndings(string text)
            => text.Replace("\r\n", "\n");
    }
}
