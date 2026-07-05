using System;
using System.IO;
using Xunit;

namespace InventoryManagementApp.Tests
{
    public class MaintenancePageResponsiveContractTests
    {
        [Fact]
        public void MaintenancePage_KeepsMaintenanceSummaryCardsWrappedAndBounded()
        {
            var xaml = ReadRepoFile("InventoryManagementApp", "Views", "Pages", "MaintenancePage.xaml");

            Assert.Contains("<WrapPanel Grid.Column=\"2\" HorizontalAlignment=\"Right\">", xaml, StringComparison.Ordinal);
            Assert.Contains("<Setter Property=\"MinWidth\" Value=\"150\"/>", xaml, StringComparison.Ordinal);
            Assert.Contains("<Setter Property=\"MaxWidth\" Value=\"235\"/>", xaml, StringComparison.Ordinal);
            Assert.Contains("<ColumnDefinition Width=\"1.15*\" MinWidth=\"0\"/>", xaml, StringComparison.Ordinal);
            Assert.Contains("Text=\"{Binding MaintenancePrintStatus}\"", xaml, StringComparison.Ordinal);
            Assert.DoesNotContain("<UniformGrid Grid.Column=\"2\" Columns=\"4\">", xaml, StringComparison.Ordinal);
            Assert.DoesNotContain("<ColumnDefinition Width=\"2*\" MinWidth=\"380\"/>", xaml, StringComparison.Ordinal);
            Assert.DoesNotContain("<ColumnDefinition Width=\"3*\" MinWidth=\"520\"/>", xaml, StringComparison.Ordinal);
        }

        [Fact]
        public void MaintenancePage_AvoidsLargeFixedMinimumsInMainMaintenanceSplit()
        {
            var xaml = ReadRepoFile("InventoryManagementApp", "Views", "Pages", "MaintenancePage.xaml");

            Assert.Contains("<ColumnDefinition Width=\"1.55*\" MinWidth=\"0\"/>", xaml, StringComparison.Ordinal);
            Assert.Contains("<ColumnDefinition Width=\"0.95*\" MinWidth=\"300\"/>", xaml, StringComparison.Ordinal);
            Assert.Contains("<GridSplitter Grid.Column=\"1\" Width=\"6\"", xaml, StringComparison.Ordinal);
            Assert.Contains("<Border Grid.Column=\"0\" Style=\"{StaticResource Card}\" Padding=\"0\" MinWidth=\"0\">", xaml, StringComparison.Ordinal);
            Assert.Contains("<Border Grid.Column=\"2\" Style=\"{StaticResource Card}\" Padding=\"0\" MinWidth=\"0\">", xaml, StringComparison.Ordinal);
            Assert.DoesNotContain("<ColumnDefinition Width=\"2*\" MinWidth=\"620\"/>", xaml, StringComparison.Ordinal);
            Assert.DoesNotContain("<ColumnDefinition Width=\"440\" MinWidth=\"390\"/>", xaml, StringComparison.Ordinal);
        }

        [Fact]
        public void MaintenancePage_EnablesScheduleGridVirtualizationScrollingAndFullRowSelection()
        {
            var xaml = ReadRepoFile("InventoryManagementApp", "Views", "Pages", "MaintenancePage.xaml");

            Assert.Contains("x:Name=\"MaintenanceGrid\"", xaml, StringComparison.Ordinal);
            Assert.Contains("EnableRowVirtualization=\"True\"", xaml, StringComparison.Ordinal);
            Assert.Contains("EnableColumnVirtualization=\"True\"", xaml, StringComparison.Ordinal);
            Assert.Contains("SelectionMode=\"Single\"", xaml, StringComparison.Ordinal);
            Assert.Contains("SelectionUnit=\"FullRow\"", xaml, StringComparison.Ordinal);
            Assert.Contains("ScrollViewer.CanContentScroll=\"True\"", xaml, StringComparison.Ordinal);
            Assert.Contains("ScrollViewer.HorizontalScrollBarVisibility=\"Auto\"", xaml, StringComparison.Ordinal);
            Assert.Contains("ScrollViewer.VerticalScrollBarVisibility=\"Auto\"", xaml, StringComparison.Ordinal);
        }

        [Fact]
        public void MaintenancePage_BoundsFiltersEmptyStateAndHandoffScrolling()
        {
            var xaml = ReadRepoFile("InventoryManagementApp", "Views", "Pages", "MaintenancePage.xaml");

            Assert.Contains("<TextBox Width=\"250\" MinWidth=\"190\"", xaml, StringComparison.Ordinal);
            Assert.Contains("<ComboBox Width=\"175\" MinWidth=\"145\"", xaml, StringComparison.Ordinal);
            Assert.Contains("<Border Grid.Row=\"2\" MaxWidth=\"360\" MinHeight=\"130\" Margin=\"12\"", xaml, StringComparison.Ordinal);
            Assert.Contains("<ScrollViewer Grid.Row=\"1\" VerticalScrollBarVisibility=\"Auto\" HorizontalScrollBarVisibility=\"Disabled\">", xaml, StringComparison.Ordinal);
            Assert.Contains("Text=\"{Binding MaintenanceEmptyTitle}\"", xaml, StringComparison.Ordinal);
            Assert.Contains("Text=\"{Binding MaintenanceEmptyMessage}\"", xaml, StringComparison.Ordinal);
            Assert.DoesNotContain("<Border Grid.Row=\"2\" HorizontalAlignment=\"Center\" VerticalAlignment=\"Center\" MaxWidth=\"360\"", xaml, StringComparison.Ordinal);
            Assert.DoesNotContain("VerticalScrollBarVisibility=\"Hidden\"", xaml, StringComparison.Ordinal);
        }

        [Fact]
        public void MaintenancePage_ShowsBoundedLoadingOverlayWhileRowsLoad()
        {
            var xaml = ReadRepoFile("InventoryManagementApp", "Views", "Pages", "MaintenancePage.xaml");

            Assert.Contains("<Condition Binding=\"{Binding IsLoading}\" Value=\"False\"/>", xaml, StringComparison.Ordinal);
            Assert.Contains("<DataTrigger Binding=\"{Binding IsLoading}\" Value=\"True\">", xaml, StringComparison.Ordinal);
            Assert.Contains("Loading maintenance schedule", xaml, StringComparison.Ordinal);
            Assert.Contains("Work-order actions and schedule printing are paused", xaml, StringComparison.Ordinal);
            Assert.Contains("MaxWidth=\"380\" MinHeight=\"118\"", xaml, StringComparison.Ordinal);
        }

        [Fact]
        public void MaintenancePage_PreservesPrimaryMaintenanceActionsAndContextMenuHandoff()
        {
            var xaml = ReadRepoFile("InventoryManagementApp", "Views", "Pages", "MaintenancePage.xaml");

            Assert.Contains("AddMaintenanceCommand", xaml, StringComparison.Ordinal);
            Assert.Contains("OpenMaintenanceDetailsCommand", xaml, StringComparison.Ordinal);
            Assert.Contains("EditMaintenanceCommand", xaml, StringComparison.Ordinal);
            Assert.Contains("CompleteMaintenanceCommand", xaml, StringComparison.Ordinal);
            Assert.Contains("CopySelectedMaintenanceCommand", xaml, StringComparison.Ordinal);
            Assert.Contains("PrintSelectedMaintenanceCommand", xaml, StringComparison.Ordinal);
            Assert.Contains("PrintMaintenanceListCommand", xaml, StringComparison.Ordinal);
            Assert.Contains("MaintenanceRow_MouseDoubleClick", xaml, StringComparison.Ordinal);
            Assert.Contains("MaintenanceRow_PreviewMouseRightButtonDown", xaml, StringComparison.Ordinal);
        }

        [Fact]
        public void MaintenanceViewModel_GuardsLoadingStateAndCommandAvailability()
        {
            var source = ReadRepoFile("InventoryManagementApp", "ViewModels", "MaintenanceManagementViewModel.cs");

            Assert.Contains("private bool _isLoading;", source, StringComparison.Ordinal);
            Assert.Contains("public bool IsLoading", source, StringComparison.Ordinal);
            Assert.Contains("if (IsLoading)", source, StringComparison.Ordinal);
            Assert.Contains("CanRefreshMaintenance", source, StringComparison.Ordinal);
            Assert.Contains("CanInteractWithMaintenanceList", source, StringComparison.Ordinal);
            Assert.Contains("!IsLoading && SelectedRecord != null", source, StringComparison.Ordinal);
            Assert.Contains("PrintMaintenanceListCommand.NotifyCanExecuteChanged();", source, StringComparison.Ordinal);
        }

        [Fact]
        public void MaintenanceViewModel_ExposesProfessionalEmptyAndPrintState()
        {
            var source = ReadRepoFile("InventoryManagementApp", "ViewModels", "MaintenanceManagementViewModel.cs");

            Assert.Contains("public bool IsFilterActive", source, StringComparison.Ordinal);
            Assert.Contains("public string MaintenanceEmptyTitle", source, StringComparison.Ordinal);
            Assert.Contains("public string MaintenanceEmptyMessage", source, StringComparison.Ordinal);
            Assert.Contains("public bool CanPrintMaintenanceList", source, StringComparison.Ordinal);
            Assert.Contains("public string MaintenancePrintStatus", source, StringComparison.Ordinal);
            Assert.Contains("Print paused while maintenance rows load", source, StringComparison.Ordinal);
            Assert.Contains("No filtered rows ready to print", source, StringComparison.Ordinal);
            Assert.Contains("Ready to print first", source, StringComparison.Ordinal);
        }

        [Fact]
        public void MaintenancePrintPreview_IsBoundedAndUsesProportionalColumns()
        {
            var source = ReadRepoFile("InventoryManagementApp", "ViewModels", "MaintenanceManagementViewModel.cs");

            Assert.Contains("private const int MaxMaintenancePrintRows = 250;", source, StringComparison.Ordinal);
            Assert.Contains("FilteredMaintenanceRecords.Take(MaxMaintenancePrintRows).ToList();", source, StringComparison.Ordinal);
            Assert.Contains("Visible: {visibleRows} | Printed: {printRows.Count} | Omitted: {omittedRows}", source, StringComparison.Ordinal);
            Assert.Contains("Large schedule preview limited to the first", source, StringComparison.Ordinal);
            Assert.Contains("new GridLength(1.05, GridUnitType.Star)", source, StringComparison.Ordinal);
            Assert.Contains("new GridLength(1.65, GridUnitType.Star)", source, StringComparison.Ordinal);
            Assert.Contains("Review overdue rows, technician assignment", source, StringComparison.Ordinal);
            Assert.DoesNotContain("table.Columns.Add(new TableColumn { Width = new GridLength(90) });", source, StringComparison.Ordinal);
            Assert.DoesNotContain("table.Columns.Add(new TableColumn { Width = new GridLength(110) });", source, StringComparison.Ordinal);
            Assert.DoesNotContain("foreach (var record in FilteredMaintenanceRecords)", source, StringComparison.Ordinal);
        }

        [Fact]
        public void MaintenancePage_LoadsOnceAfterFirstPaintAndResetsForNewViewModels()
        {
            var source = ReadRepoFile("InventoryManagementApp", "Views", "Pages", "MaintenancePage.xaml.cs");

            Assert.Contains("private Task? _loadMaintenanceTask;", source, StringComparison.Ordinal);
            Assert.Contains("private MaintenanceManagementViewModel? _loadedViewModel;", source, StringComparison.Ordinal);
            Assert.Contains("private CancellationTokenSource? _startupLoadCancellation;", source, StringComparison.Ordinal);
            Assert.Contains("private int _startupLoadVersion;", source, StringComparison.Ordinal);
            Assert.Contains("Unloaded += MaintenancePage_Unloaded;", source, StringComparison.Ordinal);
            Assert.Contains("DataContextChanged += MaintenancePage_DataContextChanged;", source, StringComparison.Ordinal);
            Assert.Contains("await Dispatcher.Yield(DispatcherPriority.Background);", source, StringComparison.Ordinal);
            Assert.Contains("LoadMaintenanceOnceAsync", source, StringComparison.Ordinal);
            Assert.Contains("IsCompletedSuccessfully", source, StringComparison.Ordinal);
            Assert.Contains("CancelStartupLoad();", source, StringComparison.Ordinal);
            Assert.Contains("token.ThrowIfCancellationRequested();", source, StringComparison.Ordinal);
            Assert.Contains("loadVersion != _startupLoadVersion", source, StringComparison.Ordinal);
            Assert.Contains("!ReferenceEquals(DataContext, vm)", source, StringComparison.Ordinal);
            Assert.Contains("catch (OperationCanceledException) when (token.IsCancellationRequested || !IsLoaded || !ReferenceEquals(DataContext, vm))", source, StringComparison.Ordinal);
            Assert.Contains("_startupLoadCancellation?.Cancel();", source, StringComparison.Ordinal);
            Assert.Contains("_startupLoadCancellation?.Dispose();", source, StringComparison.Ordinal);
            Assert.Contains("_loadMaintenanceTask = null;", source, StringComparison.Ordinal);
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