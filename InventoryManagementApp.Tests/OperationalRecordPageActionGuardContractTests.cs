using System;
using System.IO;
using Xunit;

namespace InventoryManagementApp.Tests
{
    public class OperationalRecordPageActionGuardContractTests
    {
        [Fact]
        public void MaintenancePage_GuardsStartupLoadingThroughActiveViewModelAndCommandAvailability()
        {
            var source = ReadRepoFile("InventoryManagementApp", "Views", "Pages", "MaintenancePage.xaml.cs");

            Assert.Contains("FocusFirstSearchBox();", source, StringComparison.Ordinal);
            Assert.Contains("await Dispatcher.Yield(DispatcherPriority.Background);", source, StringComparison.Ordinal);
            Assert.Contains("!ReferenceEquals(DataContext, vm) || !vm.LoadMaintenanceCommand.CanExecute(null)", source, StringComparison.Ordinal);
            Assert.Contains("_loadMaintenanceTask = vm.LoadMaintenanceCommand.ExecuteAsync(null);", source, StringComparison.Ordinal);
        }

        [Fact]
        public void MaintenancePage_GuardsRowGesturesWhileMaintenanceRowsLoad()
        {
            var source = ReadRepoFile("InventoryManagementApp", "Views", "Pages", "MaintenancePage.xaml.cs");
            var doubleClick = ExtractSourceBlock(source, "private void MaintenanceRow_MouseDoubleClick", "private void MaintenanceRow_PreviewMouseRightButtonDown");
            var rightClick = ExtractSourceBlock(source, "private void MaintenanceRow_PreviewMouseRightButtonDown", "private void MaintenancePage_PreviewKeyDown");

            Assert.Contains("MaintenanceManagementViewModel { IsLoading: true }", doubleClick, StringComparison.Ordinal);
            Assert.Contains("e.Handled = true;", doubleClick, StringComparison.Ordinal);
            Assert.Contains("GridContextMenuSelection.SelectRow(sender, e) == null", doubleClick, StringComparison.Ordinal);
            Assert.Contains("OpenMaintenanceDetailsCommand.CanExecute(null)", doubleClick, StringComparison.Ordinal);
            Assert.Contains("MaintenanceManagementViewModel { IsLoading: true }", rightClick, StringComparison.Ordinal);
            Assert.Contains("GridContextMenuSelection.SelectRow(sender, e);", rightClick, StringComparison.Ordinal);
        }

        [Fact]
        public void MaintenancePage_GuardsKeyboardWorkflowThroughBusyStateAndCanExecute()
        {
            var source = ReadRepoFile("InventoryManagementApp", "Views", "Pages", "MaintenancePage.xaml.cs");
            var keyHandler = ExtractSourceBlock(source, "private void MaintenancePage_PreviewKeyDown", "private static bool IsMaintenanceActionShortcut");
            var busyShortcut = ExtractSourceBlock(source, "private static bool IsMaintenanceActionShortcut", "private void FocusFirstSearchBox");

            Assert.Contains("PreviewKeyDown += MaintenancePage_PreviewKeyDown;", source, StringComparison.Ordinal);
            Assert.Contains("Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.F", keyHandler, StringComparison.Ordinal);
            Assert.Contains("vm.IsLoading && IsMaintenanceActionShortcut(e)", keyHandler, StringComparison.Ordinal);
            Assert.Contains("AddMaintenanceCommand.CanExecute(null)", keyHandler, StringComparison.Ordinal);
            Assert.Contains("RefreshCommand.CanExecute(null)", keyHandler, StringComparison.Ordinal);
            Assert.Contains("PrintMaintenanceListCommand.CanExecute(null)", keyHandler, StringComparison.Ordinal);
            Assert.Contains("PrintSelectedMaintenanceCommand.CanExecute(null)", keyHandler, StringComparison.Ordinal);
            Assert.Contains("CopySelectedMaintenanceCommand.CanExecute(null)", keyHandler, StringComparison.Ordinal);
            Assert.Contains("OpenMaintenanceDetailsCommand.CanExecute(null)", keyHandler, StringComparison.Ordinal);
            Assert.Contains("EditMaintenanceCommand.CanExecute(null)", keyHandler, StringComparison.Ordinal);
            Assert.Contains("CompleteMaintenanceCommand.CanExecute(null)", keyHandler, StringComparison.Ordinal);
            Assert.Contains("DeleteMaintenanceCommand.CanExecute(null)", keyHandler, StringComparison.Ordinal);
            Assert.Contains("e.Key is Key.N or Key.R or Key.P or Key.C or Key.D or Key.E or Key.Enter", busyShortcut, StringComparison.Ordinal);
            Assert.Contains("return Keyboard.Modifiers == ModifierKeys.None && e.Key is Key.Enter or Key.Delete;", busyShortcut, StringComparison.Ordinal);
        }

        [Fact]
        public void CalibrationPage_GuardsStartupLoadingThroughActiveViewModelAndCommandAvailability()
        {
            var source = ReadRepoFile("InventoryManagementApp", "Views", "Pages", "CalibrationPage.xaml.cs");

            Assert.Contains("FocusFirstSearchBox();", source, StringComparison.Ordinal);
            Assert.Contains("await Dispatcher.Yield(DispatcherPriority.Background);", source, StringComparison.Ordinal);
            Assert.Contains("!ReferenceEquals(DataContext, vm) || !vm.LoadCalibrationCommand.CanExecute(null)", source, StringComparison.Ordinal);
            Assert.Contains("_loadCalibrationTask = vm.LoadCalibrationCommand.ExecuteAsync(null);", source, StringComparison.Ordinal);
        }

        [Fact]
        public void CalibrationPage_GuardsRowGesturesWhileCalibrationRowsLoad()
        {
            var source = ReadRepoFile("InventoryManagementApp", "Views", "Pages", "CalibrationPage.xaml.cs");
            var doubleClick = ExtractSourceBlock(source, "private void CalibrationRow_MouseDoubleClick", "private void CalibrationRow_PreviewMouseRightButtonDown");
            var rightClick = ExtractSourceBlock(source, "private void CalibrationRow_PreviewMouseRightButtonDown", "private void CalibrationPage_PreviewKeyDown");

            Assert.Contains("CalibrationManagementViewModel { IsLoading: true }", doubleClick, StringComparison.Ordinal);
            Assert.Contains("e.Handled = true;", doubleClick, StringComparison.Ordinal);
            Assert.Contains("GridContextMenuSelection.SelectRow(sender, e) == null", doubleClick, StringComparison.Ordinal);
            Assert.Contains("OpenCalibrationDetailsCommand.CanExecute(null)", doubleClick, StringComparison.Ordinal);
            Assert.Contains("CalibrationManagementViewModel { IsLoading: true }", rightClick, StringComparison.Ordinal);
            Assert.Contains("GridContextMenuSelection.SelectRow(sender, e);", rightClick, StringComparison.Ordinal);
        }

        [Fact]
        public void CalibrationPage_GuardsKeyboardWorkflowThroughBusyStateAndCanExecute()
        {
            var source = ReadRepoFile("InventoryManagementApp", "Views", "Pages", "CalibrationPage.xaml.cs");
            var keyHandler = ExtractSourceBlock(source, "private void CalibrationPage_PreviewKeyDown", "private static bool IsCalibrationActionShortcut");
            var busyShortcut = ExtractSourceBlock(source, "private static bool IsCalibrationActionShortcut", "private void FocusFirstSearchBox");

            Assert.Contains("PreviewKeyDown += CalibrationPage_PreviewKeyDown;", source, StringComparison.Ordinal);
            Assert.Contains("Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.F", keyHandler, StringComparison.Ordinal);
            Assert.Contains("vm.IsLoading && IsCalibrationActionShortcut(e)", keyHandler, StringComparison.Ordinal);
            Assert.Contains("AddCalibrationCommand.CanExecute(null)", keyHandler, StringComparison.Ordinal);
            Assert.Contains("RefreshCommand.CanExecute(null)", keyHandler, StringComparison.Ordinal);
            Assert.Contains("PrintCalibrationListCommand.CanExecute(null)", keyHandler, StringComparison.Ordinal);
            Assert.Contains("PrintSelectedCalibrationCommand.CanExecute(null)", keyHandler, StringComparison.Ordinal);
            Assert.Contains("CopySelectedCalibrationCommand.CanExecute(null)", keyHandler, StringComparison.Ordinal);
            Assert.Contains("OpenCalibrationDetailsCommand.CanExecute(null)", keyHandler, StringComparison.Ordinal);
            Assert.Contains("EditCalibrationCommand.CanExecute(null)", keyHandler, StringComparison.Ordinal);
            Assert.Contains("DeleteCalibrationCommand.CanExecute(null)", keyHandler, StringComparison.Ordinal);
            Assert.Contains("e.Key is Key.N or Key.R or Key.P or Key.C or Key.D or Key.E", busyShortcut, StringComparison.Ordinal);
            Assert.Contains("return Keyboard.Modifiers == ModifierKeys.None && e.Key is Key.Enter or Key.Delete;", busyShortcut, StringComparison.Ordinal);
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

        private static string ExtractSourceBlock(string source, string startMarker, string endMarker)
        {
            var start = source.IndexOf(startMarker, StringComparison.Ordinal);
            Assert.True(start >= 0, $"Could not find source block start marker: {startMarker}");

            var end = source.IndexOf(endMarker, start, StringComparison.Ordinal);
            Assert.True(end > start, $"Could not find source block end marker: {endMarker}");

            return source[start..end];
        }
    }
}
