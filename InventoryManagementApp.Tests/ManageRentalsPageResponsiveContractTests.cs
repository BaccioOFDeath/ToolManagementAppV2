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
            var xaml = ReadRepoFile("InventoryManagementApp", "Views", "Pages", "ManageRentalsPage.xaml");

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
            var xaml = ReadRepoFile("InventoryManagementApp", "Views", "Pages", "ManageRentalsPage.xaml");
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
            var xaml = ReadRepoFile("InventoryManagementApp", "Views", "Pages", "ManageRentalsPage.xaml");

            Assert.Contains("<Border Grid.Column=\"0\" MaxWidth=\"330\" MinHeight=\"120\" Margin=\"12\"", xaml, StringComparison.Ordinal);
            Assert.Contains("<Border MaxHeight=\"156\" Padding=\"0\" Margin=\"0,0,0,8\" ClipToBounds=\"True\">", xaml, StringComparison.Ordinal);
            Assert.Contains("<WrapPanel Margin=\"0,4,0,0\">", xaml, StringComparison.Ordinal);
            Assert.Contains("<WrapPanel>\n                            <Button Style=\"{StaticResource GhostButton}\" Content=\"History\"", xaml, StringComparison.Ordinal);
            Assert.DoesNotContain("<Border Grid.Column=\"0\" Width=\"320\"", xaml, StringComparison.Ordinal);
            Assert.DoesNotContain("<UniformGrid Columns=\"2\" Margin=\"0,4,0,0\">", xaml, StringComparison.Ordinal);
            Assert.DoesNotContain("<UniformGrid Columns=\"2\">", xaml, StringComparison.Ordinal);
        }

        [Fact]
        public void ManageRentalsPage_LoadsOncePerViewModelAfterFirstPaintAndResetsOnDataContextChange()
        {
            var codeBehind = ReadRepoFile("InventoryManagementApp", "Views", "Pages", "ManageRentalsPage.xaml.cs");

            Assert.Contains("ManageRentalsViewModel? _loadedViewModel;", codeBehind, StringComparison.Ordinal);
            Assert.Contains("DataContextChanged += ManageRentalsPage_DataContextChanged;", codeBehind, StringComparison.Ordinal);
            Assert.Contains("SearchTextBox.Focus();", codeBehind, StringComparison.Ordinal);
            Assert.Contains("UpdateCompactHeightMode();", codeBehind, StringComparison.Ordinal);
            Assert.Contains("if (DataContext is ManageRentalsViewModel vm && !ReferenceEquals(_loadedViewModel, vm))", codeBehind, StringComparison.Ordinal);
            Assert.Contains("_loadedViewModel = vm;", codeBehind, StringComparison.Ordinal);
            Assert.Contains("await System.Windows.Threading.Dispatcher.Yield(System.Windows.Threading.DispatcherPriority.Background);", codeBehind, StringComparison.Ordinal);
            Assert.Contains("await vm.LoadRentalsAsync();", codeBehind, StringComparison.Ordinal);
            Assert.Contains("if (!ReferenceEquals(e.NewValue, _loadedViewModel))", codeBehind, StringComparison.Ordinal);
            Assert.DoesNotContain("if (DataContext is ManageRentalsViewModel vm)\n            {\n                await vm.LoadRentalsAsync();\n            }", NormalizeNewlines(codeBehind), StringComparison.Ordinal);
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

            Assert.Contains("!vm.IsLoading && vm.OpenRentalDetailsCommand.CanExecute(null)", codeBehind, StringComparison.Ordinal);
            Assert.Contains("!vm.IsLoading && vm.OpenRequestDetailsCommand.CanExecute(null)", codeBehind, StringComparison.Ordinal);
            Assert.Contains("if (vm.IsLoading)\n                return;", NormalizeNewlines(codeBehind), StringComparison.Ordinal);
            Assert.Contains("if (DataContext is ManageRentalsViewModel { IsLoading: true })", codeBehind, StringComparison.Ordinal);
            Assert.Contains("e.Handled = true;\n                return;", NormalizeNewlines(codeBehind), StringComparison.Ordinal);
            Assert.DoesNotContain("DataContext is ManageRentalsViewModel vm && vm.OpenRentalDetailsCommand.CanExecute(null)", codeBehind, StringComparison.Ordinal);
            Assert.DoesNotContain("DataContext is ManageRentalsViewModel vm && vm.OpenRequestDetailsCommand.CanExecute(null)", codeBehind, StringComparison.Ordinal);
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
