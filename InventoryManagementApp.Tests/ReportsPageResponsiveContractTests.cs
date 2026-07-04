using System;
using System.IO;
using Xunit;

namespace InventoryManagementApp.Tests
{
    public class ReportsPageResponsiveContractTests
    {
        [Fact]
        public void ReportsPage_KeepsReportSummaryCardsWrappedAndBounded()
        {
            var xaml = ReadRepoFile("InventoryManagementApp", "Views", "Pages", "ReportsPage.xaml");

            Assert.Contains("<WrapPanel Grid.Column=\"2\" HorizontalAlignment=\"Right\">", xaml, StringComparison.Ordinal);
            Assert.Contains("<Setter Property=\"MinWidth\" Value=\"150\"/>", xaml, StringComparison.Ordinal);
            Assert.Contains("<Setter Property=\"MaxWidth\" Value=\"240\"/>", xaml, StringComparison.Ordinal);
            Assert.Contains("<ColumnDefinition Width=\"1.15*\" MinWidth=\"0\"/>", xaml, StringComparison.Ordinal);
            Assert.DoesNotContain("<UniformGrid Grid.Column=\"2\" Columns=\"4\">", xaml, StringComparison.Ordinal);
            Assert.DoesNotContain("<ColumnDefinition Width=\"2*\" MinWidth=\"390\"/>", xaml, StringComparison.Ordinal);
            Assert.DoesNotContain("<ColumnDefinition Width=\"3*\" MinWidth=\"540\"/>", xaml, StringComparison.Ordinal);
        }

        [Fact]
        public void ReportsPage_AvoidsLargeFixedMinimumsInMainReportSplit()
        {
            var xaml = ReadRepoFile("InventoryManagementApp", "Views", "Pages", "ReportsPage.xaml");

            Assert.Contains("<ColumnDefinition Width=\"1.55*\" MinWidth=\"0\"/>", xaml, StringComparison.Ordinal);
            Assert.Contains("<ColumnDefinition Width=\"0.95*\" MinWidth=\"300\"/>", xaml, StringComparison.Ordinal);
            Assert.Contains("<GridSplitter Grid.Column=\"1\" Width=\"6\"", xaml, StringComparison.Ordinal);
            Assert.Contains("<Border Grid.Column=\"0\" Style=\"{StaticResource Card}\" Padding=\"0\" MinWidth=\"0\">", xaml, StringComparison.Ordinal);
            Assert.Contains("<Border Grid.Column=\"2\" Style=\"{StaticResource Card}\" Padding=\"0\" MinWidth=\"0\">", xaml, StringComparison.Ordinal);
            Assert.DoesNotContain("<ColumnDefinition Width=\"2.55*\" MinWidth=\"620\"/>", xaml, StringComparison.Ordinal);
            Assert.DoesNotContain("<ColumnDefinition Width=\"430\" MinWidth=\"380\"/>", xaml, StringComparison.Ordinal);
        }

        [Fact]
        public void ReportsPage_EnablesResultsGridVirtualizationScrollingAndFullRowSelection()
        {
            var xaml = ReadRepoFile("InventoryManagementApp", "Views", "Pages", "ReportsPage.xaml");

            Assert.Contains("x:Name=\"ReportGrid\"", xaml, StringComparison.Ordinal);
            Assert.Contains("EnableRowVirtualization=\"True\"", xaml, StringComparison.Ordinal);
            Assert.Contains("EnableColumnVirtualization=\"True\"", xaml, StringComparison.Ordinal);
            Assert.Contains("SelectionMode=\"Single\"", xaml, StringComparison.Ordinal);
            Assert.Contains("SelectionUnit=\"FullRow\"", xaml, StringComparison.Ordinal);
            Assert.Contains("ScrollViewer.CanContentScroll=\"True\"", xaml, StringComparison.Ordinal);
            Assert.Contains("ScrollViewer.HorizontalScrollBarVisibility=\"Auto\"", xaml, StringComparison.Ordinal);
            Assert.Contains("ScrollViewer.VerticalScrollBarVisibility=\"Auto\"", xaml, StringComparison.Ordinal);
        }

        [Fact]
        public void ReportsPage_BoundsEmptyStateHandoffPaneAndHandoffText()
        {
            var xaml = ReadRepoFile("InventoryManagementApp", "Views", "Pages", "ReportsPage.xaml");

            Assert.Contains("<Border Grid.Row=\"2\" MaxWidth=\"360\" Margin=\"12\"", xaml, StringComparison.Ordinal);
            Assert.Contains("<ScrollViewer Grid.Row=\"1\" VerticalScrollBarVisibility=\"Auto\" HorizontalScrollBarVisibility=\"Disabled\">", xaml, StringComparison.Ordinal);
            Assert.Contains("MinHeight=\"130\"", xaml, StringComparison.Ordinal);
            Assert.Contains("MaxHeight=\"260\"", xaml, StringComparison.Ordinal);
            Assert.Contains("HorizontalScrollBarVisibility=\"Disabled\"", xaml, StringComparison.Ordinal);
            Assert.Contains("<MultiDataTrigger>", xaml, StringComparison.Ordinal);
            Assert.Contains("<Condition Binding=\"{Binding IsBusy}\" Value=\"False\"/>", xaml, StringComparison.Ordinal);
            Assert.Contains("Text=\"Generating report\"", xaml, StringComparison.Ordinal);
            Assert.DoesNotContain("<Border Grid.Row=\"2\" Width=\"360\"", xaml, StringComparison.Ordinal);
            Assert.DoesNotContain("MinHeight=\"150\"", xaml, StringComparison.Ordinal);
        }

        [Fact]
        public void ReportsPage_PreservesReportActionsAndContextMenuHandoff()
        {
            var xaml = ReadRepoFile("InventoryManagementApp", "Views", "Pages", "ReportsPage.xaml");

            Assert.Contains("RunReportCommand", xaml, StringComparison.Ordinal);
            Assert.Contains("OpenSourcePage_Click", xaml, StringComparison.Ordinal);
            Assert.Contains("PrintReport_Click", xaml, StringComparison.Ordinal);
            Assert.Contains("CopySelectedRow_Click", xaml, StringComparison.Ordinal);
            Assert.Contains("ClearReportCommand", xaml, StringComparison.Ordinal);
            Assert.Contains("ReportGrid_MouseDoubleClick", xaml, StringComparison.Ordinal);
            Assert.Contains("ReportGrid_PreviewMouseRightButtonDown", xaml, StringComparison.Ordinal);
            Assert.Contains("SelectedLineHandoff", xaml, StringComparison.Ordinal);
        }

        [Fact]
        public void ReportsPage_DisablesRowAndPrintActionsWhileReportsGenerate()
        {
            var xaml = ReadRepoFile("InventoryManagementApp", "Views", "Pages", "ReportsPage.xaml");
            var codeBehind = ReadRepoFile("InventoryManagementApp", "Views", "Pages", "ReportsPage.xaml.cs");
            var viewModel = ReadRepoFile("InventoryManagementApp", "ViewModels", "ReportsViewModel.cs");

            Assert.Contains("public bool CanUseReportRows => !IsBusy && ReportLines.Count > 0;", viewModel, StringComparison.Ordinal);
            Assert.Contains("public bool CanPrintCurrentReport => !IsBusy && LastRunAt.HasValue", viewModel, StringComparison.Ordinal);
            Assert.Contains("OnPropertyChanged(nameof(CanPrintCurrentReport));", viewModel, StringComparison.Ordinal);
            Assert.Contains("OnPropertyChanged(nameof(CanUseReportRows));", viewModel, StringComparison.Ordinal);
            Assert.Contains("IsEnabled=\"{Binding CanUseReportRows}\"", xaml, StringComparison.Ordinal);
            Assert.Contains("IsEnabled=\"{Binding CanPrintCurrentReport}\"", xaml, StringComparison.Ordinal);
            Assert.Contains("DataContext=\"{Binding PlacementTarget.DataContext, RelativeSource={RelativeSource Self}}\"", xaml, StringComparison.Ordinal);
            Assert.Contains("ReportsViewModel { CanUseReportRows: true }", codeBehind, StringComparison.Ordinal);
            Assert.Contains("ReportsViewModel { IsBusy: true }", codeBehind, StringComparison.Ordinal);
            Assert.Contains("e.Handled = true;", codeBehind, StringComparison.Ordinal);
            Assert.Contains("Wait for the report to finish generating before opening a source page.", codeBehind, StringComparison.Ordinal);
            Assert.Contains("Wait for the report to finish generating before copying a handoff.", codeBehind, StringComparison.Ordinal);
            Assert.Contains("Wait for the report to finish generating before opening print preview.", codeBehind, StringComparison.Ordinal);
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