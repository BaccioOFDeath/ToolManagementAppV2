using System;
using System.IO;
using Xunit;

namespace InventoryManagementApp.Tests
{
    public class CalibrationPageResponsiveContractTests
    {
        [Fact]
        public void CalibrationPage_KeepsCalibrationSummaryCardsWrappedAndBounded()
        {
            var xaml = ReadRepoFile("InventoryManagementApp", "Views", "Pages", "CalibrationPage.xaml");

            Assert.Contains("<WrapPanel Grid.Column=\"2\" HorizontalAlignment=\"Right\">", xaml, StringComparison.Ordinal);
            Assert.Contains("<Style x:Key=\"CalibrationMetricCard\"", xaml, StringComparison.Ordinal);
            Assert.Contains("<Setter Property=\"MinWidth\" Value=\"150\"/>", xaml, StringComparison.Ordinal);
            Assert.Contains("<Setter Property=\"MaxWidth\" Value=\"235\"/>", xaml, StringComparison.Ordinal);
            Assert.Contains("<ColumnDefinition Width=\"1.15*\" MinWidth=\"0\"/>", xaml, StringComparison.Ordinal);
            Assert.Contains("Text=\"{Binding CalibrationPrintStatus}\"", xaml, StringComparison.Ordinal);
            Assert.DoesNotContain("<UniformGrid Grid.Column=\"2\" Columns=\"4\">", xaml, StringComparison.Ordinal);
            Assert.DoesNotContain("<ColumnDefinition Width=\"2*\" MinWidth=\"380\"/>", xaml, StringComparison.Ordinal);
            Assert.DoesNotContain("<ColumnDefinition Width=\"3*\" MinWidth=\"520\"/>", xaml, StringComparison.Ordinal);
        }

        [Fact]
        public void CalibrationPage_AvoidsLargeFixedMinimumsInMainCalibrationSplit()
        {
            var xaml = ReadRepoFile("InventoryManagementApp", "Views", "Pages", "CalibrationPage.xaml");

            Assert.Contains("<ColumnDefinition Width=\"1.55*\" MinWidth=\"0\"/>", xaml, StringComparison.Ordinal);
            Assert.Contains("<ColumnDefinition Width=\"0.95*\" MinWidth=\"300\"/>", xaml, StringComparison.Ordinal);
            Assert.Contains("<GridSplitter Grid.Column=\"1\"", xaml, StringComparison.Ordinal);
            Assert.Contains("Width=\"6\"", xaml, StringComparison.Ordinal);
            Assert.Contains("<Border Grid.Column=\"0\" Style=\"{StaticResource Card}\" Padding=\"0\" MinWidth=\"0\">", xaml, StringComparison.Ordinal);
            Assert.Contains("<Border Grid.Column=\"2\" Style=\"{StaticResource Card}\" Padding=\"0\" MinWidth=\"0\">", xaml, StringComparison.Ordinal);
            Assert.DoesNotContain("<ColumnDefinition Width=\"2*\" MinWidth=\"630\"/>", xaml, StringComparison.Ordinal);
            Assert.DoesNotContain("<ColumnDefinition Width=\"440\" MinWidth=\"390\"/>", xaml, StringComparison.Ordinal);
        }

        [Fact]
        public void CalibrationPage_EnablesRegisterGridVirtualizationScrollingAndFullRowSelection()
        {
            var xaml = ReadRepoFile("InventoryManagementApp", "Views", "Pages", "CalibrationPage.xaml");

            Assert.Contains("x:Name=\"CalibrationGrid\"", xaml, StringComparison.Ordinal);
            Assert.Contains("EnableRowVirtualization=\"True\"", xaml, StringComparison.Ordinal);
            Assert.Contains("EnableColumnVirtualization=\"True\"", xaml, StringComparison.Ordinal);
            Assert.Contains("SelectionMode=\"Single\"", xaml, StringComparison.Ordinal);
            Assert.Contains("SelectionUnit=\"FullRow\"", xaml, StringComparison.Ordinal);
            Assert.Contains("ScrollViewer.CanContentScroll=\"True\"", xaml, StringComparison.Ordinal);
            Assert.Contains("ScrollViewer.HorizontalScrollBarVisibility=\"Auto\"", xaml, StringComparison.Ordinal);
            Assert.Contains("ScrollViewer.VerticalScrollBarVisibility=\"Auto\"", xaml, StringComparison.Ordinal);
        }

        [Fact]
        public void CalibrationPage_BoundsFiltersEmptyStateAndHandoffScrolling()
        {
            var xaml = ReadRepoFile("InventoryManagementApp", "Views", "Pages", "CalibrationPage.xaml");

            Assert.Contains("<TextBox Width=\"250\" MinWidth=\"190\"", xaml, StringComparison.Ordinal);
            Assert.Contains("<ComboBox Width=\"175\" MinWidth=\"145\"", xaml, StringComparison.Ordinal);
            Assert.Contains("<Border Grid.Row=\"2\" MaxWidth=\"360\" MinHeight=\"130\" Margin=\"12\"", xaml, StringComparison.Ordinal);
            Assert.Contains("<ScrollViewer Grid.Row=\"1\" VerticalScrollBarVisibility=\"Auto\" HorizontalScrollBarVisibility=\"Disabled\">", xaml, StringComparison.Ordinal);
            Assert.Contains("Text=\"{Binding CalibrationEmptyTitle}\"", xaml, StringComparison.Ordinal);
            Assert.Contains("Text=\"{Binding CalibrationEmptyMessage}\"", xaml, StringComparison.Ordinal);
            Assert.DoesNotContain("<Border Grid.Row=\"2\" HorizontalAlignment=\"Center\" VerticalAlignment=\"Center\" MaxWidth=\"380\"", xaml, StringComparison.Ordinal);
            Assert.DoesNotContain("VerticalScrollBarVisibility=\"Hidden\"", xaml, StringComparison.Ordinal);
        }

        [Fact]
        public void CalibrationPage_ShowsBoundedLoadingOverlayWhileRowsLoad()
        {
            var xaml = ReadRepoFile("InventoryManagementApp", "Views", "Pages", "CalibrationPage.xaml");

            Assert.Contains("<Condition Binding=\"{Binding IsLoading}\" Value=\"False\"/>", xaml, StringComparison.Ordinal);
            Assert.Contains("<DataTrigger Binding=\"{Binding IsLoading}\" Value=\"True\">", xaml, StringComparison.Ordinal);
            Assert.Contains("Loading calibration register", xaml, StringComparison.Ordinal);
            Assert.Contains("Certificate actions and due-report printing are paused", xaml, StringComparison.Ordinal);
            Assert.Contains("MaxWidth=\"380\" MinHeight=\"118\"", xaml, StringComparison.Ordinal);
        }

        [Fact]
        public void CalibrationPage_PreservesPrimaryCalibrationActionsAndContextMenuHandoff()
        {
            var xaml = ReadRepoFile("InventoryManagementApp", "Views", "Pages", "CalibrationPage.xaml");

            Assert.Contains("AddCalibrationCommand", xaml, StringComparison.Ordinal);
            Assert.Contains("OpenCalibrationDetailsCommand", xaml, StringComparison.Ordinal);
            Assert.Contains("EditCalibrationCommand", xaml, StringComparison.Ordinal);
            Assert.Contains("CopySelectedCalibrationCommand", xaml, StringComparison.Ordinal);
            Assert.Contains("PrintSelectedCalibrationCommand", xaml, StringComparison.Ordinal);
            Assert.Contains("PrintCalibrationListCommand", xaml, StringComparison.Ordinal);
            Assert.Contains("ShowOverdueCommand", xaml, StringComparison.Ordinal);
            Assert.Contains("ShowDueSoonCommand", xaml, StringComparison.Ordinal);
            Assert.Contains("ShowCurrentCommand", xaml, StringComparison.Ordinal);
            Assert.Contains("CalibrationRow_MouseDoubleClick", xaml, StringComparison.Ordinal);
            Assert.Contains("CalibrationRow_PreviewMouseRightButtonDown", xaml, StringComparison.Ordinal);
        }

        [Fact]
        public void CalibrationViewModel_GuardsLoadingStateAndCommandAvailability()
        {
            var source = ReadRepoFile("InventoryManagementApp", "ViewModels", "CalibrationManagementViewModel.cs");

            Assert.Contains("private bool _isLoading;", source, StringComparison.Ordinal);
            Assert.Contains("public bool IsLoading", source, StringComparison.Ordinal);
            Assert.Contains("if (IsLoading)", source, StringComparison.Ordinal);
            Assert.Contains("CanRefreshCalibration", source, StringComparison.Ordinal);
            Assert.Contains("CanInteractWithCalibrationList", source, StringComparison.Ordinal);
            Assert.Contains("!IsLoading && SelectedRecord != null", source, StringComparison.Ordinal);
            Assert.Contains("PrintCalibrationListCommand.NotifyCanExecuteChanged();", source, StringComparison.Ordinal);
        }

        [Fact]
        public void CalibrationViewModel_ExposesProfessionalEmptyAndPrintState()
        {
            var source = ReadRepoFile("InventoryManagementApp", "ViewModels", "CalibrationManagementViewModel.cs");

            Assert.Contains("public bool IsFilterActive", source, StringComparison.Ordinal);
            Assert.Contains("public string CalibrationEmptyTitle", source, StringComparison.Ordinal);
            Assert.Contains("public string CalibrationEmptyMessage", source, StringComparison.Ordinal);
            Assert.Contains("public bool CanPrintCalibrationList", source, StringComparison.Ordinal);
            Assert.Contains("public string CalibrationPrintStatus", source, StringComparison.Ordinal);
            Assert.Contains("Print paused while calibration rows load", source, StringComparison.Ordinal);
            Assert.Contains("No filtered certificate rows ready to print", source, StringComparison.Ordinal);
            Assert.Contains("Ready to print first", source, StringComparison.Ordinal);
        }

        [Fact]
        public void CalibrationPrintPreview_IsBoundedAndUsesProportionalColumns()
        {
            var source = ReadRepoFile("InventoryManagementApp", "ViewModels", "CalibrationManagementViewModel.cs");

            Assert.Contains("private const int MaxCalibrationPrintRows = 250;", source, StringComparison.Ordinal);
            Assert.Contains("FilteredCalibrationRecords.Take(MaxCalibrationPrintRows).ToList();", source, StringComparison.Ordinal);
            Assert.Contains("Visible: {visibleRows} | Printed: {printRows.Count} | Omitted: {omittedRows}", source, StringComparison.Ordinal);
            Assert.Contains("Large calibration preview limited to the first", source, StringComparison.Ordinal);
            Assert.Contains("new GridLength(1.05, GridUnitType.Star)", source, StringComparison.Ordinal);
            Assert.Contains("new GridLength(1.65, GridUnitType.Star)", source, StringComparison.Ordinal);
            Assert.Contains("Review overdue rows, due-soon certificates", source, StringComparison.Ordinal);
            Assert.DoesNotContain("table.Columns.Add(new TableColumn { Width = new GridLength(90) });", source, StringComparison.Ordinal);
            Assert.DoesNotContain("table.Columns.Add(new TableColumn { Width = new GridLength(150) });", source, StringComparison.Ordinal);
            Assert.DoesNotContain("foreach (var record in FilteredCalibrationRecords)", source, StringComparison.Ordinal);
        }

        [Fact]
        public void CalibrationPage_LoadsOnceAfterFirstPaintAndResetsForNewViewModels()
        {
            var source = ReadRepoFile("InventoryManagementApp", "Views", "Pages", "CalibrationPage.xaml.cs");

            Assert.Contains("private Task? _loadCalibrationTask;", source, StringComparison.Ordinal);
            Assert.Contains("private CalibrationManagementViewModel? _loadedViewModel;", source, StringComparison.Ordinal);
            Assert.Contains("DataContextChanged += CalibrationPage_DataContextChanged;", source, StringComparison.Ordinal);
            Assert.Contains("await Dispatcher.Yield(DispatcherPriority.Background);", source, StringComparison.Ordinal);
            Assert.Contains("LoadCalibrationOnceAsync", source, StringComparison.Ordinal);
            Assert.Contains("IsCompletedSuccessfully", source, StringComparison.Ordinal);
            Assert.Contains("_loadCalibrationTask = null;", source, StringComparison.Ordinal);
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
