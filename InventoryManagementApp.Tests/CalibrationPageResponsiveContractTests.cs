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
            var printListMethod = ExtractSourceBlock(source, "private void PrintCalibrationList()", "private void PrintSelectedCalibration()");

            Assert.Contains("private const int MaxCalibrationPrintRows = 250;", source, StringComparison.Ordinal);
            Assert.Contains("FilteredCalibrationRecords.Take(MaxCalibrationPrintRows).ToList();", printListMethod, StringComparison.Ordinal);
            Assert.Contains("Visible: {visibleRows} | Printed: {printRows.Count} | Omitted: {omittedRows}", printListMethod, StringComparison.Ordinal);
            Assert.Contains("Large calibration preview limited to the first", printListMethod, StringComparison.Ordinal);
            Assert.Contains("new GridLength(1.05, GridUnitType.Star)", printListMethod, StringComparison.Ordinal);
            Assert.Contains("new GridLength(1.65, GridUnitType.Star)", printListMethod, StringComparison.Ordinal);
            Assert.Contains("Review overdue rows, due-soon certificates", printListMethod, StringComparison.Ordinal);
            Assert.DoesNotContain("table.Columns.Add(new TableColumn { Width = new GridLength(90) });", printListMethod, StringComparison.Ordinal);
            Assert.DoesNotContain("table.Columns.Add(new TableColumn { Width = new GridLength(150) });", printListMethod, StringComparison.Ordinal);
            Assert.DoesNotContain("foreach (var record in FilteredCalibrationRecords)", printListMethod, StringComparison.Ordinal);
        }

        [Fact]
        public void CalibrationPage_LoadsOnceAfterFirstPaintAndResetsForNewViewModels()
        {
            var source = ReadRepoFile("InventoryManagementApp", "Views", "Pages", "CalibrationPage.xaml.cs");

            Assert.Contains("private Task? _loadCalibrationTask;", source, StringComparison.Ordinal);
            Assert.Contains("private CalibrationManagementViewModel? _loadedViewModel;", source, StringComparison.Ordinal);
            Assert.Contains("private CancellationTokenSource? _startupLoadCancellation;", source, StringComparison.Ordinal);
            Assert.Contains("private int _startupLoadVersion;", source, StringComparison.Ordinal);
            Assert.Contains("Unloaded += CalibrationPage_Unloaded;", source, StringComparison.Ordinal);
            Assert.Contains("DataContextChanged += CalibrationPage_DataContextChanged;", source, StringComparison.Ordinal);
            Assert.Contains("await Dispatcher.Yield(DispatcherPriority.Background);", source, StringComparison.Ordinal);
            Assert.Contains("LoadCalibrationOnceAsync", source, StringComparison.Ordinal);
            Assert.Contains("IsCompletedSuccessfully", source, StringComparison.Ordinal);
            Assert.Contains("CancelStartupLoad();", source, StringComparison.Ordinal);
            Assert.Contains("token.ThrowIfCancellationRequested();", source, StringComparison.Ordinal);
            Assert.Contains("loadVersion != _startupLoadVersion", source, StringComparison.Ordinal);
            Assert.Contains("!ReferenceEquals(DataContext, vm)", source, StringComparison.Ordinal);
            Assert.Contains("catch (OperationCanceledException) when (token.IsCancellationRequested || !IsLoaded || !ReferenceEquals(DataContext, vm))", source, StringComparison.Ordinal);
            Assert.Contains("_startupLoadCancellation?.Cancel();", source, StringComparison.Ordinal);
            Assert.Contains("_startupLoadCancellation?.Dispose();", source, StringComparison.Ordinal);
            Assert.Contains("_loadCalibrationTask = null;", source, StringComparison.Ordinal);
        }

        [Fact]
        public void CalibrationPage_PreservesTextEditingSuppressesBusyMenusAndUsesIterativeLookup()
        {
            var source = ReadRepoFile("InventoryManagementApp", "Views", "Pages", "CalibrationPage.xaml.cs");
            var keyHandler = ExtractSourceBlock(source, "private void CalibrationPage_PreviewKeyDown", "private static bool IsCalibrationActionShortcut");
            var doubleClick = ExtractSourceBlock(source, "private void CalibrationRow_MouseDoubleClick", "private void CalibrationRow_PreviewMouseRightButtonDown");
            var findDescendant = ExtractSourceBlock(source, "private static T? FindDescendant", "private static int GetVisualChildCount");

            Assert.Contains("CalibrationGrid.ContextMenuOpening += CalibrationGrid_ContextMenuOpening;", source, StringComparison.Ordinal);
            Assert.Contains("private void CalibrationGrid_ContextMenuOpening(object sender, ContextMenuEventArgs e)", source, StringComparison.Ordinal);
            Assert.Contains("CalibrationManagementViewModel { IsLoading: true }", source, StringComparison.Ordinal);
            Assert.Contains("if (IsTextInputFocused() && IsCalibrationActionShortcut(e))", keyHandler, StringComparison.Ordinal);
            Assert.Contains("Keyboard.FocusedElement is TextBoxBase or PasswordBox or ComboBox", source, StringComparison.Ordinal);
            Assert.Contains("Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.F", keyHandler, StringComparison.Ordinal);
            Assert.True(
                keyHandler.IndexOf("Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.F", StringComparison.Ordinal) <
                keyHandler.IndexOf("if (IsTextInputFocused() && IsCalibrationActionShortcut(e))", StringComparison.Ordinal),
                "Ctrl+F should keep focusing search before text-edit shortcuts are preserved.");
            Assert.True(
                keyHandler.IndexOf("if (IsTextInputFocused() && IsCalibrationActionShortcut(e))", StringComparison.Ordinal) <
                keyHandler.IndexOf("Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.N", StringComparison.Ordinal),
                "Text-edit guard should run before calibration action shortcuts dispatch.");
            Assert.Contains("e.Handled = true;\n        }", doubleClick, StringComparison.Ordinal);
            Assert.Contains("using System.Collections.Generic;", source, StringComparison.Ordinal);
            Assert.Contains("var pending = new Stack<DependencyObject>();", findDescendant, StringComparison.Ordinal);
            Assert.Contains("while (pending.Count > 0)", findDescendant, StringComparison.Ordinal);
            Assert.Contains("pending.Push(child);", findDescendant, StringComparison.Ordinal);
            Assert.DoesNotContain("var nested = FindDescendant<T>(child);", findDescendant, StringComparison.Ordinal);
            Assert.Contains("private static int GetVisualChildCount(DependencyObject current)", source, StringComparison.Ordinal);
            Assert.Contains("catch (InvalidOperationException)", source, StringComparison.Ordinal);
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