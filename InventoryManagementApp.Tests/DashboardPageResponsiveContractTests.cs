using System;
using System.IO;
using Xunit;

namespace InventoryManagementApp.Tests
{
    public class DashboardPageResponsiveContractTests
    {
        [Fact]
        public void DashboardPage_KeepsOperationalMetricsWrappedAndBounded()
        {
            var xaml = ReadRepoFile("InventoryManagementApp", "Views", "Pages", "DashboardPage.xaml");

            Assert.Contains("<WrapPanel Grid.Row=\"2\" Margin=\"0,0,0,6\">", xaml, StringComparison.Ordinal);
            Assert.Contains("DashboardMetricCard", xaml, StringComparison.Ordinal);
            Assert.Contains("<Setter Property=\"MinWidth\" Value=\"150\"/>", xaml, StringComparison.Ordinal);
            Assert.Contains("<Setter Property=\"MaxWidth\" Value=\"230\"/>", xaml, StringComparison.Ordinal);
            Assert.Contains("DashboardMetricValueText", xaml, StringComparison.Ordinal);
            Assert.DoesNotContain("<UniformGrid Columns=\"4\">", xaml, StringComparison.Ordinal);
        }

        [Fact]
        public void DashboardPage_AvoidsLargeFixedMinimumsInMainWorkloadSplit()
        {
            var xaml = ReadRepoFile("InventoryManagementApp", "Views", "Pages", "DashboardPage.xaml");

            Assert.Contains("<ColumnDefinition Width=\"1.65*\" MinWidth=\"0\"/>", xaml, StringComparison.Ordinal);
            Assert.Contains("<ColumnDefinition Width=\"6\"/>", xaml, StringComparison.Ordinal);
            Assert.Contains("<ColumnDefinition Width=\"0.95*\" MinWidth=\"300\"/>", xaml, StringComparison.Ordinal);
            Assert.Contains("<GridSplitter Grid.Row=\"0\" Grid.RowSpan=\"2\" Grid.Column=\"1\" Width=\"6\"", xaml, StringComparison.Ordinal);
            Assert.Contains("<Border Grid.Row=\"0\" Grid.RowSpan=\"2\" Grid.Column=\"0\" Style=\"{StaticResource Card}\" Padding=\"0\" MinWidth=\"0\"", xaml, StringComparison.Ordinal);
            Assert.Contains("<Border Grid.Row=\"1\" Grid.Column=\"2\" Style=\"{StaticResource Card}\" Padding=\"0\" MinWidth=\"0\">", xaml, StringComparison.Ordinal);
            Assert.DoesNotContain("<ColumnDefinition Width=\"2*\" MinWidth=\"520\"/>", xaml, StringComparison.Ordinal);
            Assert.DoesNotContain("<ColumnDefinition Width=\"*\" MinWidth=\"360\"/>", xaml, StringComparison.Ordinal);
        }

        [Fact]
        public void DashboardPage_EnablesEveryDashboardGridVirtualizationScrollingAndFullRowSelection()
        {
            var xaml = ReadRepoFile("InventoryManagementApp", "Views", "Pages", "DashboardPage.xaml");
            var gridNames = new[]
            {
                "RentedItemsGrid",
                "CheckedOutItemsGrid",
                "RecentActivityGrid",
                "IncompleteItemsGrid",
                "CommonItemsGrid"
            };

            foreach (var gridName in gridNames)
                Assert.Contains($"x:Name=\"{gridName}\"", xaml, StringComparison.Ordinal);

            Assert.Equal(gridNames.Length, CountOccurrences(xaml, "EnableRowVirtualization=\"True\""));
            Assert.Equal(gridNames.Length, CountOccurrences(xaml, "EnableColumnVirtualization=\"True\""));
            Assert.Equal(gridNames.Length, CountOccurrences(xaml, "SelectionUnit=\"FullRow\""));
            Assert.Equal(gridNames.Length, CountOccurrences(xaml, "ScrollViewer.CanContentScroll=\"True\""));
            Assert.Equal(gridNames.Length, CountOccurrences(xaml, "ScrollViewer.HorizontalScrollBarVisibility=\"Auto\""));
            Assert.Equal(gridNames.Length, CountOccurrences(xaml, "ScrollViewer.VerticalScrollBarVisibility=\"Auto\""));
        }

        [Fact]
        public void DashboardPage_WrapsHeaderAndPaneActionsForScaledDesktopWidths()
        {
            var xaml = ReadRepoFile("InventoryManagementApp", "Views", "Pages", "DashboardPage.xaml");

            Assert.Contains("<StackPanel DockPanel.Dock=\"Left\" MinWidth=\"210\" MaxWidth=\"320\"", xaml, StringComparison.Ordinal);
            Assert.Contains("<Border Style=\"{StaticResource DesktopSummaryCard}\" MinWidth=\"92\" MaxWidth=\"160\"", xaml, StringComparison.Ordinal);
            Assert.True(CountOccurrences(xaml, "<WrapPanel DockPanel.Dock=\"Right\" VerticalAlignment=\"Center\">") >= 3);
            Assert.DoesNotContain("<StackPanel Orientation=\"Horizontal\" DockPanel.Dock=\"Right\" VerticalAlignment=\"Center\">", xaml, StringComparison.Ordinal);
        }

        [Fact]
        public void DashboardPage_ExposesBoundedLoadingFeedbackAndRetrySurface()
        {
            var xaml = ReadRepoFile("InventoryManagementApp", "Views", "Pages", "DashboardPage.xaml");
            var codeBehind = ReadRepoFile("InventoryManagementApp", "Views", "Pages", "DashboardPage.xaml.cs");

            Assert.Contains("x:Name=\"DashboardRoot\"", xaml, StringComparison.Ordinal);
            Assert.Contains("x:Name=\"DashboardLoadStatusBanner\"", xaml, StringComparison.Ordinal);
            Assert.Contains("Grid.Row=\"1\"", xaml, StringComparison.Ordinal);
            Assert.Contains("Visibility=\"Collapsed\"", xaml, StringComparison.Ordinal);
            Assert.Contains("MinWidth=\"0\"", xaml, StringComparison.Ordinal);
            Assert.Contains("x:Name=\"DashboardLoadStatusText\"", xaml, StringComparison.Ordinal);
            Assert.Contains("TextWrapping=\"Wrap\"", xaml, StringComparison.Ordinal);
            Assert.Contains("x:Name=\"DashboardLoadRetryButton\"", xaml, StringComparison.Ordinal);
            Assert.Contains("Click=\"DashboardLoadRetryButton_Click\"", xaml, StringComparison.Ordinal);

            Assert.Contains("private bool _isLoadingDashboard;", codeBehind, StringComparison.Ordinal);
            Assert.Contains("LoadDashboardAsync(\"Loading dashboard data...\")", codeBehind, StringComparison.Ordinal);
            Assert.Contains("LoadDashboardAsync(\"Refreshing dashboard data...\")", codeBehind, StringComparison.Ordinal);
            Assert.Contains("await Dispatcher.Yield(DispatcherPriority.Background);", codeBehind, StringComparison.Ordinal);
            Assert.Contains("if (_isLoadingDashboard || DataContext is not DashboardViewModel vm)", codeBehind, StringComparison.Ordinal);
            Assert.Contains("_loadCts?.Cancel();", codeBehind, StringComparison.Ordinal);
            Assert.Contains("SetDashboardLoadStatus(null, showRetry: false);", codeBehind, StringComparison.Ordinal);
            Assert.Contains("DashboardLoadRetryButton.IsEnabled = DashboardLoadRetryButton.Visibility == Visibility.Visible;", codeBehind, StringComparison.Ordinal);
            Assert.Contains("Cursor = Cursors.Wait;", codeBehind, StringComparison.Ordinal);
            Assert.Contains("Cursor = previousCursor;", codeBehind, StringComparison.Ordinal);
        }

        [Fact]
        public void DashboardPage_GatesStartupLoadsForSameViewModelAndResetsOnContextChange()
        {
            var codeBehind = ReadRepoFile("InventoryManagementApp", "Views", "Pages", "DashboardPage.xaml.cs");

            Assert.Contains("private DashboardViewModel? _loadedDashboardViewModel;", codeBehind, StringComparison.Ordinal);
            Assert.Contains("private bool _hasLoadedDashboardForViewModel;", codeBehind, StringComparison.Ordinal);
            Assert.Contains("DataContextChanged += DashboardPage_DataContextChanged;", codeBehind, StringComparison.Ordinal);
            Assert.Contains("ReferenceEquals(_loadedDashboardViewModel, vm) && _hasLoadedDashboardForViewModel", codeBehind, StringComparison.Ordinal);
            Assert.Contains("_loadedDashboardViewModel = vm;", codeBehind, StringComparison.Ordinal);
            Assert.Contains("_loadedDashboardViewModel = e.NewValue as DashboardViewModel;", codeBehind, StringComparison.Ordinal);
            Assert.Contains("_hasLoadedDashboardForViewModel = false;", codeBehind, StringComparison.Ordinal);
            Assert.Contains("DashboardLoadRetryButton_Click", codeBehind, StringComparison.Ordinal);
            Assert.DoesNotContain("_hasLoadedDashboardForViewModel = true;\n            await LoadDashboardAsync(\"Loading dashboard data...\");", codeBehind, StringComparison.Ordinal);
        }

        [Fact]
        public void DashboardPage_MarksStartupLoadCompleteOnlyAfterActiveViewModelFinishes()
        {
            var codeBehind = ReadRepoFile("InventoryManagementApp", "Views", "Pages", "DashboardPage.xaml.cs");

            Assert.Contains("if (token.IsCancellationRequested || !ReferenceEquals(DataContext, vm))\n                    return;", codeBehind, StringComparison.Ordinal);
            Assert.True(CountOccurrences(codeBehind, "!ReferenceEquals(DataContext, vm)") >= 2);
            Assert.Contains("await vm.LoadAsync(token);", codeBehind, StringComparison.Ordinal);
            Assert.Contains("_loadedDashboardViewModel = vm;\n                _hasLoadedDashboardForViewModel = true;\n                SetDashboardLoadStatus(null, showRetry: false);", codeBehind, StringComparison.Ordinal);
            Assert.Contains("_hasLoadedDashboardForViewModel = false;\n                if (ReferenceEquals(_loadCts, loadCts))", codeBehind, StringComparison.Ordinal);
        }

        [Fact]
        public void DashboardPage_KeepsStaleCancelledLoadsFromReenablingActions()
        {
            var codeBehind = ReadRepoFile("InventoryManagementApp", "Views", "Pages", "DashboardPage.xaml.cs");

            Assert.Contains("var loadCts = new CancellationTokenSource();", codeBehind, StringComparison.Ordinal);
            Assert.Contains("_loadCts = loadCts;", codeBehind, StringComparison.Ordinal);
            Assert.Contains("if (IsLoaded && !_isUnloadingDashboard && ReferenceEquals(_loadCts, loadCts))", codeBehind, StringComparison.Ordinal);
            Assert.Contains("if (ReferenceEquals(_loadCts, loadCts))\n                {\n                    Cursor = previousCursor;", codeBehind, StringComparison.Ordinal);
            Assert.Contains("SetDashboardInteractiveActionsEnabled(true);", codeBehind, StringComparison.Ordinal);
            Assert.Contains("_loadCts?.Dispose();\n                    _loadCts = null;", codeBehind, StringComparison.Ordinal);
            Assert.Contains("loadCts.Dispose();", codeBehind, StringComparison.Ordinal);
        }

        [Fact]
        public void DashboardPage_UnloadSuppressesCancelledRetryNoise()
        {
            var codeBehind = ReadRepoFile("InventoryManagementApp", "Views", "Pages", "DashboardPage.xaml.cs");

            Assert.Contains("private bool _isUnloadingDashboard;", codeBehind, StringComparison.Ordinal);
            Assert.Contains("_isUnloadingDashboard = false;\n            Focus();", codeBehind, StringComparison.Ordinal);
            Assert.Contains("_isUnloadingDashboard = true;", codeBehind, StringComparison.Ordinal);
            Assert.Contains("!_isUnloadingDashboard", codeBehind, StringComparison.Ordinal);
            Assert.Contains("_loadCts?.Cancel();\n            _loadCts?.Dispose();\n            _loadCts = null;", codeBehind, StringComparison.Ordinal);
        }

        [Fact]
        public void DashboardPage_DisablesVisibleCommandButtonsWhileRowsRefresh()
        {
            var codeBehind = ReadRepoFile("InventoryManagementApp", "Views", "Pages", "DashboardPage.xaml.cs");

            Assert.Contains("using System.Collections.Generic;", codeBehind, StringComparison.Ordinal);
            Assert.Contains("using System.Windows.Media;", codeBehind, StringComparison.Ordinal);
            Assert.Contains("SetDashboardInteractiveActionsEnabled(false);", codeBehind, StringComparison.Ordinal);
            Assert.Contains("SetDashboardInteractiveActionsEnabled(true);", codeBehind, StringComparison.Ordinal);
            Assert.Contains("private void SetDashboardInteractiveActionsEnabled(bool isEnabled)", codeBehind, StringComparison.Ordinal);
            Assert.Contains("EnumerateVisualDescendants(DashboardRoot)", codeBehind, StringComparison.Ordinal);
            Assert.Contains("ReferenceEquals(element, DashboardLoadRetryButton)", codeBehind, StringComparison.Ordinal);
            Assert.Contains("case Button button:", codeBehind, StringComparison.Ordinal);
            Assert.Contains("button.IsEnabled = isEnabled;", codeBehind, StringComparison.Ordinal);
            Assert.Contains("case MenuItem menuItem:", codeBehind, StringComparison.Ordinal);
            Assert.Contains("menuItem.IsEnabled = isEnabled;", codeBehind, StringComparison.Ordinal);
            Assert.Contains("VisualTreeHelper.GetChildrenCount(current)", codeBehind, StringComparison.Ordinal);
            Assert.Contains("pending.Push(child);", codeBehind, StringComparison.Ordinal);
            Assert.DoesNotContain("foreach (var descendant in EnumerateVisualDescendants(child))", codeBehind, StringComparison.Ordinal);
        }

        [Fact]
        public void DashboardPage_BlocksKeyboardPrintAndNavigationActionsWhileLoading()
        {
            var codeBehind = ReadRepoFile("InventoryManagementApp", "Views", "Pages", "DashboardPage.xaml.cs");

            Assert.Contains("if (!_isLoadingDashboard && vm.PrintDashboardSnapshotCommand.CanExecute(null))", codeBehind, StringComparison.Ordinal);
            Assert.Contains("if (!_isLoadingDashboard && vm.PrintCheckedOutItemsCommand.CanExecute(null))", codeBehind, StringComparison.Ordinal);
            Assert.Contains("if (_isLoadingDashboard && IsDashboardActionShortcut(e))", codeBehind, StringComparison.Ordinal);
            Assert.Contains("private static bool IsDashboardActionShortcut(KeyEventArgs e)", codeBehind, StringComparison.Ordinal);
            Assert.Contains("return e.Key is Key.I or Key.R or Key.P;", codeBehind, StringComparison.Ordinal);
            Assert.Contains("return e.Key == Key.P;", codeBehind, StringComparison.Ordinal);
            Assert.Contains("Keyboard.Modifiers == ModifierKeys.None && e.Key == Key.Enter", codeBehind, StringComparison.Ordinal);
            Assert.Contains("e.Handled = true;", codeBehind, StringComparison.Ordinal);
            Assert.Contains("if (vm.OpenItemsCommand.CanExecute(null))", codeBehind, StringComparison.Ordinal);
            Assert.Contains("if (vm.OpenRentalsCommand.CanExecute(null))", codeBehind, StringComparison.Ordinal);
            Assert.Contains("UiActionGuard.Run(this, \"Dashboard\", () => OpenFocusedRow(vm));", codeBehind, StringComparison.Ordinal);
        }

        [Fact]
        public void DashboardPage_BlocksRowActionsAndSelectionRetargetingWhileLoading()
        {
            var codeBehind = ReadRepoFile("InventoryManagementApp", "Views", "Pages", "DashboardPage.xaml.cs");

            Assert.True(CountOccurrences(codeBehind, "if (_isLoadingDashboard)") >= 7);
            Assert.True(CountOccurrences(codeBehind, "e.Handled = true;\n                return;") >= 7);
            Assert.Contains("if (DataContext is not DashboardViewModel vm)\n                return;", codeBehind, StringComparison.Ordinal);
            Assert.Contains("private static T? SelectInvokedDashboardRow<T>(object sender, MouseButtonEventArgs e) where T : class", codeBehind, StringComparison.Ordinal);
            Assert.Contains("GridContextMenuSelection.FindAncestor<DataGridRow>(e.OriginalSource as DependencyObject)", codeBehind, StringComparison.Ordinal);
            Assert.Contains("grid.SelectedItem = item;", codeBehind, StringComparison.Ordinal);
            Assert.Contains("private void DashboardGrid_PreviewMouseRightButtonDown", codeBehind, StringComparison.Ordinal);
            Assert.Contains("GridContextMenuSelection.SelectRow(sender, e)", codeBehind, StringComparison.Ordinal);
        }

        [Fact]
        public void DashboardPage_RetargetsInvokedDoubleClickRowsBeforeOpeningWorkflows()
        {
            var codeBehind = ReadRepoFile("InventoryManagementApp", "Views", "Pages", "DashboardPage.xaml.cs");

            Assert.Contains("var item = SelectInvokedDashboardRow<ItemModel>(sender, e);\n            if (item != null)\n                vm.SelectedCommonlyUsedItem = item;", codeBehind, StringComparison.Ordinal);
            Assert.Contains("var item = SelectInvokedDashboardRow<ItemModel>(sender, e);\n            if (item != null)\n                vm.SelectedCheckedOutItem = item;", codeBehind, StringComparison.Ordinal);
            Assert.Contains("var rental = SelectInvokedDashboardRow<RentalModel>(sender, e);\n            if (rental != null)\n                vm.SelectedRental = rental;", codeBehind, StringComparison.Ordinal);
            Assert.Contains("var activity = SelectInvokedDashboardRow<ActivityLog>(sender, e);\n            if (activity != null)\n                vm.SelectedActivity = activity;", codeBehind, StringComparison.Ordinal);
            Assert.Contains("var item = SelectInvokedDashboardRow<ItemModel>(sender, e);\n            if (item != null)\n                vm.SelectedIncompleteItem = item;", codeBehind, StringComparison.Ordinal);
            Assert.True(CountOccurrences(codeBehind, "e.Handled = true;") >= 13);
            Assert.True(CountOccurrences(codeBehind, "e.Handled = item != null;") >= 3);
            Assert.Contains("e.Handled = rental != null;", codeBehind, StringComparison.Ordinal);
            Assert.Contains("e.Handled = activity != null;", codeBehind, StringComparison.Ordinal);
        }

        [Fact]
        public void DashboardPage_BindsRowActionsToSelectionReadiness()
        {
            var xaml = ReadRepoFile("InventoryManagementApp", "Views", "Pages", "DashboardPage.xaml");

            Assert.Contains("Content=\"Open\" Command=\"{Binding OpenSelectedRentalCommand}\" IsEnabled=\"{Binding HasSelectedRental}\"", xaml, StringComparison.Ordinal);
            Assert.Contains("Content=\"Return\" Command=\"{Binding ReturnSelectedRentalCommand}\" IsEnabled=\"{Binding HasSelectedRental}\"", xaml, StringComparison.Ordinal);
            Assert.Contains("Content=\"Open\" Command=\"{Binding OpenSelectedCheckedOutItemCommand}\" IsEnabled=\"{Binding HasSelectedCheckedOutItem}\"", xaml, StringComparison.Ordinal);
            Assert.Contains("Content=\"Check In\" Command=\"{Binding CheckInSelectedItemCommand}\" IsEnabled=\"{Binding HasSelectedCheckedOutItem}\"", xaml, StringComparison.Ordinal);
            Assert.Contains("Content=\"Open Related\" Command=\"{Binding OpenActivityDestinationCommand}\" IsEnabled=\"{Binding HasSelectedActivity}\"", xaml, StringComparison.Ordinal);
            Assert.Contains("Content=\"Open\" Command=\"{Binding OpenSelectedIncompleteItemCommand}\" IsEnabled=\"{Binding HasSelectedIncompleteItem}\"", xaml, StringComparison.Ordinal);
            Assert.Contains("Content=\"Open\" Command=\"{Binding OpenSelectedCommonItemCommand}\" IsEnabled=\"{Binding HasSelectedCommonItem}\"", xaml, StringComparison.Ordinal);
            Assert.Contains("Content=\"Check Out / In\" Command=\"{Binding ToggleSelectedCommonItemCommand}\" IsEnabled=\"{Binding HasSelectedCommonItem}\"", xaml, StringComparison.Ordinal);
            Assert.True(CountOccurrences(xaml, "PlacementTarget.DataContext.HasSelectedRental") >= 2);
            Assert.True(CountOccurrences(xaml, "PlacementTarget.DataContext.HasSelectedCheckedOutItem") >= 2);
            Assert.True(CountOccurrences(xaml, "PlacementTarget.DataContext.HasSelectedActivity") >= 1);
            Assert.True(CountOccurrences(xaml, "PlacementTarget.DataContext.HasSelectedIncompleteItem") >= 1);
            Assert.True(CountOccurrences(xaml, "PlacementTarget.DataContext.HasSelectedCommonItem") >= 2);
        }

        [Fact]
        public void DashboardPage_PreservesPrimaryDashboardActionsAndRowHandoff()
        {
            var xaml = ReadRepoFile("InventoryManagementApp", "Views", "Pages", "DashboardPage.xaml");
            var codeBehind = ReadRepoFile("InventoryManagementApp", "Views", "Pages", "DashboardPage.xaml.cs");

            Assert.Contains("NewItemCommand", xaml, StringComparison.Ordinal);
            Assert.Contains("OpenItemsCommand", xaml, StringComparison.Ordinal);
            Assert.Contains("OpenRentalsCommand", xaml, StringComparison.Ordinal);
            Assert.Contains("PrintDashboardSnapshotCommand", xaml, StringComparison.Ordinal);
            Assert.Contains("PrintCheckedOutItemsCommand", xaml, StringComparison.Ordinal);
            Assert.Contains("OpenActivityDestinationCommand", xaml, StringComparison.Ordinal);
            Assert.Contains("OpenSelectedIncompleteItemCommand", xaml, StringComparison.Ordinal);
            Assert.Contains("OpenSelectedCommonItemCommand", xaml, StringComparison.Ordinal);
            Assert.Contains("DashboardGrid_PreviewMouseRightButtonDown", xaml, StringComparison.Ordinal);
            Assert.Contains("GridContextMenuSelection.SelectRow(sender, e)", codeBehind, StringComparison.Ordinal);
            Assert.Contains("OpenFocusedRow", codeBehind, StringComparison.Ordinal);
        }

        private static int CountOccurrences(string text, string value)
        {
            var count = 0;
            var index = 0;

            while ((index = text.IndexOf(value, index, StringComparison.Ordinal)) >= 0)
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
                    return NormalizeLineEndings(File.ReadAllText(candidate));

                var parent = Directory.GetParent(directory);
                if (parent is null)
                    break;

                directory = parent.FullName;
            }

            throw new FileNotFoundException($"Could not find repository file: {Path.Combine(parts)}");
        }
        static string NormalizeLineEndings(string text)
            => text.Replace("\r\n", "\n");

    }
}