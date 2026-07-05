using System;
using System.IO;
using Xunit;

namespace InventoryManagementApp.Tests
{
    public class ManageRentalsPageResponsiveContractTests
    {
        [Fact]
        public void ManageRentalsPage_KeepsRentalSummaryCardsWrappedAndBounded()
        {
            var xaml = NormalizeNewlines(ReadRepoFile("InventoryManagementApp", "Views", "Pages", "ManageRentalsPage.xaml"));

            Assert.Contains("<WrapPanel x:Name=\"RentalStatsStrip\" Grid.Row=\"1\" Margin=\"0,0,0,6\">", xaml, StringComparison.Ordinal);
            Assert.Contains("<Setter Property=\"MinWidth\" Value=\"150\"/>", xaml, StringComparison.Ordinal);
            Assert.Contains("<Setter Property=\"MaxWidth\" Value=\"235\"/>", xaml, StringComparison.Ordinal);
            Assert.Contains("RentalStatValueText", xaml, StringComparison.Ordinal);
            Assert.DoesNotContain("<Grid x:Name=\"RentalStatsStrip\" Grid.Row=\"1\"", xaml, StringComparison.Ordinal);
            Assert.DoesNotContain("<ColumnDefinition Width=\"1.25*\"/>", xaml, StringComparison.Ordinal);
        }

        [Fact]
        public void ManageRentalsPage_AvoidsLargeFixedMinimumsInRentalDeskSplit()
        {
            var xaml = NormalizeNewlines(ReadRepoFile("InventoryManagementApp", "Views", "Pages", "ManageRentalsPage.xaml"));
            var codeBehind = ReadRepoFile("InventoryManagementApp", "Views", "Pages", "ManageRentalsPage.xaml.cs");

            Assert.Contains("<ColumnDefinition Width=\"1.65*\" MinWidth=\"0\"/>", xaml, StringComparison.Ordinal);
            Assert.Contains("<ColumnDefinition Width=\"0.95*\" MinWidth=\"300\"/>", xaml, StringComparison.Ordinal);
            Assert.Contains("<GridSplitter Grid.Column=\"1\" Width=\"6\"", xaml, StringComparison.Ordinal);
            Assert.Contains("<Border Grid.Column=\"0\" Style=\"{StaticResource Card}\" Padding=\"0\" MinWidth=\"0\">", xaml, StringComparison.Ordinal);
            Assert.Contains("<Border Grid.Column=\"2\" Style=\"{StaticResource Card}\" Padding=\"0\" MinWidth=\"0\">", xaml, StringComparison.Ordinal);
            Assert.Contains("RequestDetailColumn.MinWidth = compactHeight ? 0 : 300;", codeBehind, StringComparison.Ordinal);
            Assert.DoesNotContain("<ColumnDefinition Width=\"1.7*\" MinWidth=\"460\"/>", xaml, StringComparison.Ordinal);
            Assert.DoesNotContain("<ColumnDefinition Width=\"1.05*\" MinWidth=\"280\"/>", xaml, StringComparison.Ordinal);
            Assert.DoesNotContain("RequestDetailColumn.MinWidth = compactHeight ? 0 : 260;", codeBehind, StringComparison.Ordinal);
        }

        [Fact]
        public void ManageRentalsPage_EnablesRentalGridVirtualizationScrollingAndFullRowSelection()
        {
            var xaml = ReadRepoFile("InventoryManagementApp", "Views", "Pages", "ManageRentalsPage.xaml");

            Assert.Contains("x:Name=\"RentalDeskGrid\"", xaml, StringComparison.Ordinal);
            Assert.Contains("EnableRowVirtualization=\"True\"", xaml, StringComparison.Ordinal);
            Assert.Contains("EnableColumnVirtualization=\"True\"", xaml, StringComparison.Ordinal);
            Assert.Contains("SelectionMode=\"Single\"", xaml, StringComparison.Ordinal);
            Assert.Contains("SelectionUnit=\"FullRow\"", xaml, StringComparison.Ordinal);
            Assert.Contains("ScrollViewer.CanContentScroll=\"True\"", xaml, StringComparison.Ordinal);
            Assert.Contains("ScrollViewer.HorizontalScrollBarVisibility=\"Auto\"", xaml, StringComparison.Ordinal);
            Assert.Contains("ScrollViewer.VerticalScrollBarVisibility=\"Auto\"", xaml, StringComparison.Ordinal);
        }

        [Fact]
        public void ManageRentalsPage_BoundsRentalFiltersEmptyStateAndHandoffScrolling()
        {
            var xaml = ReadRepoFile("InventoryManagementApp", "Views", "Pages", "ManageRentalsPage.xaml");

            Assert.Contains("<Setter Property=\"MinWidth\" Value=\"170\"/>", xaml, StringComparison.Ordinal);
            Assert.Contains("<Setter Property=\"MinWidth\" Value=\"142\"/>", xaml, StringComparison.Ordinal);
            Assert.Contains("<Border Grid.Row=\"2\" MaxWidth=\"320\" MinHeight=\"120\" Margin=\"12\"", xaml, StringComparison.Ordinal);
            Assert.Contains("<ScrollViewer Grid.Row=\"1\" VerticalScrollBarVisibility=\"Auto\" HorizontalScrollBarVisibility=\"Disabled\">", xaml, StringComparison.Ordinal);
            Assert.Contains("<StackPanel Margin=\"12\" MinWidth=\"0\">", xaml, StringComparison.Ordinal);
            Assert.DoesNotContain("VerticalScrollBarVisibility=\"Hidden\"", xaml, StringComparison.Ordinal);
            Assert.DoesNotContain("<Border Grid.Row=\"2\" Width=\"300\"", xaml, StringComparison.Ordinal);
        }

        [Fact]
        public void ManageRentalsPage_EnablesRequestQueueGridVirtualizationScrollingAndResponsiveDetailPane()
        {
            var xaml = ReadRepoFile("InventoryManagementApp", "Views", "Pages", "ManageRentalsPage.xaml");
            var codeBehind = ReadRepoFile("InventoryManagementApp", "Views", "Pages", "ManageRentalsPage.xaml.cs");

            Assert.Contains("x:Name=\"RequestQueueGrid\"", xaml, StringComparison.Ordinal);
            Assert.Contains("<ColumnDefinition x:Name=\"RequestListColumn\" Width=\"1.55*\" MinWidth=\"0\"/>", xaml, StringComparison.Ordinal);
            Assert.Contains("<ColumnDefinition x:Name=\"RequestDetailSplitterColumn\" Width=\"6\"/>", xaml, StringComparison.Ordinal);
            Assert.Contains("<ColumnDefinition x:Name=\"RequestDetailColumn\" Width=\"0.95*\" MinWidth=\"300\"/>", xaml, StringComparison.Ordinal);
            Assert.Contains("<GridSplitter x:Name=\"RequestDetailSplitter\" Grid.Column=\"1\" Width=\"6\"", xaml, StringComparison.Ordinal);
            Assert.Contains("<Border x:Name=\"RequestDetailPanel\" Grid.Column=\"2\"", xaml, StringComparison.Ordinal);
            Assert.Contains("Padding=\"8\" MinWidth=\"0\">", xaml, StringComparison.Ordinal);
            Assert.Contains("<ScrollViewer VerticalScrollBarVisibility=\"Auto\" HorizontalScrollBarVisibility=\"Disabled\">", xaml, StringComparison.Ordinal);
            Assert.Contains("RequestListColumn.Width = compactHeight ? new GridLength(1, GridUnitType.Star) : new GridLength(1.55, GridUnitType.Star);", codeBehind, StringComparison.Ordinal);
            Assert.Contains("RequestDetailSplitterColumn.Width = compactHeight ? new GridLength(0) : new GridLength(6);", codeBehind, StringComparison.Ordinal);
            Assert.Contains("RequestDetailColumn.Width = compactHeight ? new GridLength(0) : new GridLength(0.95, GridUnitType.Star);", codeBehind, StringComparison.Ordinal);
            Assert.DoesNotContain("<ColumnDefinition x:Name=\"RequestListColumn\" Width=\"1.65*\" MinWidth=\"430\"/>", xaml, StringComparison.Ordinal);
            Assert.DoesNotContain("<ColumnDefinition x:Name=\"RequestDetailColumn\" Width=\"1.05*\" MinWidth=\"260\"/>", xaml, StringComparison.Ordinal);
        }

        [Fact]
        public void ManageRentalsPage_BoundsRequestEmptyStateAndWrapsRequestActions()
        {
            var xaml = NormalizeNewlines(ReadRepoFile("InventoryManagementApp", "Views", "Pages", "ManageRentalsPage.xaml"));

            Assert.Contains("<Border Grid.Column=\"0\" MaxWidth=\"330\" MinHeight=\"120\" Margin=\"12\"", xaml, StringComparison.Ordinal);
            Assert.Contains("<Border MaxHeight=\"156\" Padding=\"0\" Margin=\"0,0,0,8\" ClipToBounds=\"True\">", xaml, StringComparison.Ordinal);
            Assert.Contains("<WrapPanel Margin=\"0,4,0,0\">", xaml, StringComparison.Ordinal);
            Assert.Contains("<WrapPanel>\n                            <Button Style=\"{StaticResource RentalBusyGhostButton}\" Content=\"History\"", xaml, StringComparison.Ordinal);
            Assert.DoesNotContain("<Border Grid.Column=\"0\" Width=\"320\"", xaml, StringComparison.Ordinal);
            Assert.DoesNotContain("<UniformGrid Columns=\"2\" Margin=\"0,4,0,0\">", xaml, StringComparison.Ordinal);
            Assert.DoesNotContain("<UniformGrid Columns=\"2\">", xaml, StringComparison.Ordinal);
        }

        [Fact]
        public void ManageRentalsPage_LoadsOncePerViewModelAfterFirstPaintAndResetsOnDataContextChange()
        {
            var codeBehind = ReadRepoFile("InventoryManagementApp", "Views", "Pages", "ManageRentalsPage.xaml.cs");

            Assert.Contains("Task? _loadRentalsTask;", codeBehind, StringComparison.Ordinal);
            Assert.Contains("ManageRentalsViewModel? _loadedViewModel;", codeBehind, StringComparison.Ordinal);
            Assert.Contains("int _loadVersion;", codeBehind, StringComparison.Ordinal);
            Assert.Contains("DataContextChanged += ManageRentalsPage_DataContextChanged;", codeBehind, StringComparison.Ordinal);
            Assert.Contains("Unloaded += ManageRentalsPage_Unloaded;", codeBehind, StringComparison.Ordinal);
            Assert.Contains("SearchTextBox.Focus();", codeBehind, StringComparison.Ordinal);
            Assert.Contains("UpdateCompactHeightMode();", codeBehind, StringComparison.Ordinal);
            Assert.Contains("await LoadRentalsOnceAsync(vm);", codeBehind, StringComparison.Ordinal);
            Assert.Contains("if (ReferenceEquals(_loadedViewModel, vm) && _loadRentalsTask is { IsCompleted: false })", codeBehind, StringComparison.Ordinal);
            Assert.Contains("if (ReferenceEquals(_loadedViewModel, vm) && _loadRentalsTask is { IsCompletedSuccessfully: true })", codeBehind, StringComparison.Ordinal);
            Assert.Contains("await Dispatcher.Yield(DispatcherPriority.Background);", codeBehind, StringComparison.Ordinal);
            Assert.Contains("if (!IsCurrentLoad(vm, loadVersion) || vm.IsLoading)", codeBehind, StringComparison.Ordinal);
            Assert.Contains("_loadRentalsTask = vm.LoadRentalsAsync();", codeBehind, StringComparison.Ordinal);
            Assert.Contains("if (!IsCurrentLoad(vm, loadVersion))\n                    _loadRentalsTask = null;", NormalizeNewlines(codeBehind), StringComparison.Ordinal);
            Assert.Contains("_loadRentalsTask = null;", codeBehind, StringComparison.Ordinal);
            Assert.DoesNotContain("if (!ReferenceEquals(DataContext, vm) || vm.IsLoading)", codeBehind, StringComparison.Ordinal);
            Assert.DoesNotContain("if (DataContext is ManageRentalsViewModel vm && !ReferenceEquals(_loadedViewModel, vm))", codeBehind, StringComparison.Ordinal);
        }

        [Fact]
        public void ManageRentalsPage_InvalidatesStartupLoadsOnUnloadAndDataContextReplacement()
        {
            var codeBehind = NormalizeNewlines(ReadRepoFile("InventoryManagementApp", "Views", "Pages", "ManageRentalsPage.xaml.cs"));
            var unloadBlock = ExtractSourceBlock(codeBehind, "private void ManageRentalsPage_Unloaded", "private void ManageRentalsPage_DataContextChanged");
            var dataContextBlock = ExtractSourceBlock(codeBehind, "private void ManageRentalsPage_DataContextChanged", "private async Task LoadRentalsOnceAsync");
            var loadBlock = ExtractSourceBlock(codeBehind, "private async Task LoadRentalsOnceAsync", "private bool IsCurrentLoad");
            var currentLoadBlock = ExtractSourceBlock(codeBehind, "private bool IsCurrentLoad", "private void ManageRentalsPage_SizeChanged");

            Assert.Contains("_loadVersion++;", unloadBlock, StringComparison.Ordinal);
            Assert.Contains("_loadedViewModel = null;", unloadBlock, StringComparison.Ordinal);
            Assert.Contains("_loadRentalsTask = null;", unloadBlock, StringComparison.Ordinal);
            Assert.Contains("_loadVersion++;", dataContextBlock, StringComparison.Ordinal);
            Assert.Contains("var loadVersion = _loadVersion;", loadBlock, StringComparison.Ordinal);
            Assert.Contains("if (!IsCurrentLoad(vm, loadVersion) || vm.IsLoading)", loadBlock, StringComparison.Ordinal);
            Assert.Contains("finally", loadBlock, StringComparison.Ordinal);
            Assert.Contains("if (!IsCurrentLoad(vm, loadVersion))\n                    _loadRentalsTask = null;", loadBlock, StringComparison.Ordinal);
            Assert.Contains("return loadVersion == _loadVersion && ReferenceEquals(DataContext, vm);", currentLoadBlock, StringComparison.Ordinal);
        }

        [Fact]
        public void ManageRentalsPage_KeyboardShortcutsRespectCommandAvailabilityBeforePrinting()
        {
            var codeBehind = ReadRepoFile("InventoryManagementApp", "Views", "Pages", "ManageRentalsPage.xaml.cs");

            Assert.Contains("e.Key == Key.P && vm.PrintSearchResultsCommand.CanExecute(null)", codeBehind, StringComparison.Ordinal);
            Assert.Contains("e.Key == Key.P && vm.PrintCheckedOutCommand.CanExecute(null)", codeBehind, StringComparison.Ordinal);
            Assert.Contains("e.Key == Key.R && vm.PrintRequestsCommand.CanExecute(null)", codeBehind, StringComparison.Ordinal);
            Assert.DoesNotContain("if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.P)\n            {\n                UiActionGuard.Run(this, \"Rentals\", () => vm.PrintSearchResultsCommand.Execute(null));", NormalizeNewlines(codeBehind), StringComparison.Ordinal);
            Assert.DoesNotContain("if (Keyboard.Modifiers == (ModifierKeys.Control | ModifierKeys.Shift) && e.Key == Key.P)\n            {\n                UiActionGuard.Run(this, \"Rentals\", () => vm.PrintCheckedOutCommand.Execute(null));", NormalizeNewlines(codeBehind), StringComparison.Ordinal);
            Assert.DoesNotContain("if (Keyboard.Modifiers == (ModifierKeys.Control | ModifierKeys.Shift) && e.Key == Key.R)\n            {\n                UiActionGuard.Run(this, \"Rentals\", () => vm.PrintRequestsCommand.Execute(null));", NormalizeNewlines(codeBehind), StringComparison.Ordinal);
        }

        [Fact]
        public void ManageRentalsPage_LoadingStateBlocksCodeBehindActionBypasses()
        {
            var codeBehind = ReadRepoFile("InventoryManagementApp", "Views", "Pages", "ManageRentalsPage.xaml.cs");
            var normalized = NormalizeNewlines(codeBehind);

            Assert.Contains("if (DataContext is ManageRentalsViewModel { IsLoading: true })\n            {\n                e.Handled = true;\n                return;\n            }", normalized, StringComparison.Ordinal);
            Assert.Contains("if (vm.IsLoading && IsRentalActionShortcut(e))", codeBehind, StringComparison.Ordinal);
            Assert.Contains("private static bool IsRentalActionShortcut(KeyEventArgs e)", codeBehind, StringComparison.Ordinal);
            Assert.Contains("return e.Key is Key.P or Key.D or Key.H or Key.I or Key.E or Key.R;", codeBehind, StringComparison.Ordinal);
            Assert.Contains("return e.Key is Key.P or Key.R;", codeBehind, StringComparison.Ordinal);
            Assert.Contains("return Keyboard.Modifiers == ModifierKeys.None && e.Key is Key.Enter or Key.Delete;", codeBehind, StringComparison.Ordinal);
            Assert.Contains("if (vm.IsLoading)\n                return;", normalized, StringComparison.Ordinal);
            Assert.DoesNotContain("if (vm.IsLoading)\n                return;\n\n            if (Keyboard.Modifiers == ModifierKeys.Control", normalized, StringComparison.Ordinal);
        }

        [Fact]
        public void ManageRentalsPage_RowGesturesSelectInvokedRowsAndStopDuringLoading()
        {
            var codeBehind = ReadRepoFile("InventoryManagementApp", "Views", "Pages", "ManageRentalsPage.xaml.cs");
            var rentalDoubleClick = ExtractSourceBlock(codeBehind, "private void RentalRow_MouseDoubleClick", "private void RentalRow_PreviewMouseRightButtonDown");
            var requestDoubleClick = ExtractSourceBlock(codeBehind, "private void RequestRow_MouseDoubleClick", "private void RequestRow_PreviewMouseRightButtonDown");
            var selectionBlock = ExtractSourceBlock(codeBehind, "private DataGridRow? SelectRowForContextMenu", "    }\n}");

            Assert.Contains("if (SelectRowForContextMenu(sender, e) == null)", rentalDoubleClick, StringComparison.Ordinal);
            Assert.Contains("if (SelectRowForContextMenu(sender, e) == null)", requestDoubleClick, StringComparison.Ordinal);
            Assert.Contains("e.Handled = true;", rentalDoubleClick, StringComparison.Ordinal);
            Assert.Contains("e.Handled = true;", requestDoubleClick, StringComparison.Ordinal);
            Assert.Contains("private DataGridRow? SelectRowForContextMenu", codeBehind, StringComparison.Ordinal);
            Assert.Contains("if (DataContext is ManageRentalsViewModel { IsLoading: true })", selectionBlock, StringComparison.Ordinal);
            Assert.Contains("e.Handled = true;", selectionBlock, StringComparison.Ordinal);
            Assert.Contains("return row;", selectionBlock, StringComparison.Ordinal);
            Assert.DoesNotContain("private void SelectRowForContextMenu", codeBehind, StringComparison.Ordinal);
        }

        [Fact]
        public void ManageRentalsPage_DisablesRentalActionsAndGridsDuringLoading()
        {
            var xaml = NormalizeNewlines(ReadRepoFile("InventoryManagementApp", "Views", "Pages", "ManageRentalsPage.xaml"));

            Assert.Contains("<Style x:Key=\"RentalBusyPrimaryButton\" TargetType=\"Button\" BasedOn=\"{StaticResource PrimaryButton}\">", xaml, StringComparison.Ordinal);
            Assert.Contains("<Style x:Key=\"RentalBusyGhostButton\" TargetType=\"Button\" BasedOn=\"{StaticResource GhostButton}\">", xaml, StringComparison.Ordinal);
            Assert.Contains("<Style x:Key=\"RentalBusyDataGridStyle\" TargetType=\"DataGrid\" BasedOn=\"{StaticResource VirtualizedDataGridStyle}\">", xaml, StringComparison.Ordinal);
            Assert.Contains("<DataTrigger Binding=\"{Binding IsLoading}\" Value=\"True\">\n                    <Setter Property=\"IsEnabled\" Value=\"False\"/>", xaml, StringComparison.Ordinal);
            Assert.Contains("<Setter Property=\"Opacity\" Value=\"0.72\"/>", xaml, StringComparison.Ordinal);
            Assert.Contains("Style=\"{StaticResource RentalBusyDataGridStyle}\">", xaml, StringComparison.Ordinal);
            Assert.Contains("Style=\"{StaticResource RentalBusyPrimaryButton}\" Content=\"Check In\"", xaml, StringComparison.Ordinal);
            Assert.Contains("Style=\"{StaticResource RentalBusyGhostButton}\" Content=\"Print Queue\"", xaml, StringComparison.Ordinal);
            Assert.DoesNotContain("Style=\"{StaticResource PrimaryButton}\" Content=\"Check In\"", xaml, StringComparison.Ordinal);
            Assert.DoesNotContain("Style=\"{StaticResource VirtualizedDataGridStyle}\">", xaml, StringComparison.Ordinal);
        }

        [Fact]
        public void ManageRentalsPage_SuppressesEmptyStatesAndShowsBoundedLoadingOverlaysDuringRefresh()
        {
            var xaml = NormalizeNewlines(ReadRepoFile("InventoryManagementApp", "Views", "Pages", "ManageRentalsPage.xaml"));

            Assert.Contains("<MultiDataTrigger>\n                                        <MultiDataTrigger.Conditions>\n                                            <Condition Binding=\"{Binding Rentals.Count}\" Value=\"0\"/>\n                                            <Condition Binding=\"{Binding IsLoading}\" Value=\"False\"/>", xaml, StringComparison.Ordinal);
            Assert.Contains("<MultiDataTrigger>\n                                        <MultiDataTrigger.Conditions>\n                                            <Condition Binding=\"{Binding PendingRequests.Count}\" Value=\"0\"/>\n                                            <Condition Binding=\"{Binding IsLoading}\" Value=\"False\"/>", xaml, StringComparison.Ordinal);
            Assert.Contains("<Border Grid.Row=\"2\" MaxWidth=\"360\" MinHeight=\"104\" Margin=\"12\" HorizontalAlignment=\"Center\" VerticalAlignment=\"Center\" Visibility=\"{Binding IsLoading, Converter={StaticResource BoolToVis}}\">", xaml, StringComparison.Ordinal);
            Assert.Contains("<Border Grid.Column=\"0\" MaxWidth=\"360\" MinHeight=\"104\" Margin=\"12\" HorizontalAlignment=\"Center\" VerticalAlignment=\"Center\" Visibility=\"{Binding IsLoading, Converter={StaticResource BoolToVis}}\">", xaml, StringComparison.Ordinal);
            Assert.Contains("Text=\"Refreshing rental desk\"", xaml, StringComparison.Ordinal);
            Assert.Contains("Text=\"Refreshing request queue\"", xaml, StringComparison.Ordinal);
            Assert.Contains("Keeping the current rows visible while rental, request, and print actions pause until the refresh completes.", xaml, StringComparison.Ordinal);
            Assert.Contains("Request rows remain visible while details, status changes, and print actions pause for the latest rental state.", xaml, StringComparison.Ordinal);
        }

        [Fact]
        public void ManageRentalsPage_PreservesRentalAndRequestCommandsAndRowHandlers()
        {
            var xaml = ReadRepoFile("InventoryManagementApp", "Views", "Pages", "ManageRentalsPage.xaml");
            var requiredContracts = new[]
            {
                "OpenRentalDetailsCommand",
                "CheckInCommand",
                "ExtendCommand",
                "PlaceRequestCommand",
                "PrintPickingSlipCommand",
                "PrintInvoiceCommand",
                "OpenHistoryCommand",
                "DeleteRentalCommand",
                "PrintRentalCommand",
                "PrintSearchResultsCommand",
                "PrintCheckedOutCommand",
                "OpenRequestDetailsCommand",
                "ConfirmRequestCommand",
                "CancelRequestCommand",
                "PrintRequestCommand",
                "PrintRequestsCommand",
                "RentalRow_MouseDoubleClick",
                "RentalRow_PreviewMouseRightButtonDown",
                "RequestRow_MouseDoubleClick",
                "RequestRow_PreviewMouseRightButtonDown"
            };

            foreach (var contract in requiredContracts)
                Assert.Contains(contract, xaml, StringComparison.Ordinal);
        }

        private static string NormalizeNewlines(string value) => value.Replace("\r\n", "\n", StringComparison.Ordinal);

        private static string ExtractSourceBlock(string source, string startMarker, string endMarker)
        {
            var start = source.IndexOf(startMarker, StringComparison.Ordinal);
            Assert.True(start >= 0, $"Could not find source block start marker: {startMarker}");

            var end = source.IndexOf(endMarker, start, StringComparison.Ordinal);
            Assert.True(end > start, $"Could not find source block end marker: {endMarker}");

            return source[start..end];
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
