using System;
using System.IO;
using System.Text.RegularExpressions;
using Xunit;

namespace InventoryManagementApp.Tests
{
    public class ReservationPageResponsiveContractTests
    {
        [Fact]
        public void ReservationPage_KeepsReservationSummaryCardsWrappedAndBounded()
        {
            var xaml = ReadRepoFile("InventoryManagementApp", "Views", "Pages", "ReservationPage.xaml");

            Assert.Contains("<WrapPanel Grid.Column=\"2\" HorizontalAlignment=\"Right\">", xaml, StringComparison.Ordinal);
            Assert.Contains("<Setter Property=\"MinWidth\" Value=\"150\"/>", xaml, StringComparison.Ordinal);
            Assert.Contains("<Setter Property=\"MaxWidth\" Value=\"235\"/>", xaml, StringComparison.Ordinal);
            Assert.Contains("ReservationStatValueText", xaml, StringComparison.Ordinal);
            Assert.Contains("<ColumnDefinition Width=\"1.15*\" MinWidth=\"0\"/>", xaml, StringComparison.Ordinal);
            Assert.Contains("<ColumnDefinition Width=\"1.85*\" MinWidth=\"0\"/>", xaml, StringComparison.Ordinal);
            Assert.Contains("Text=\"{Binding ReservationPrintStatus}\"", xaml, StringComparison.Ordinal);
            Assert.DoesNotContain("<UniformGrid Grid.Column=\"2\" Columns=\"4\">", xaml, StringComparison.Ordinal);
            Assert.DoesNotContain("<ColumnDefinition Width=\"2*\" MinWidth=\"380\"/>", xaml, StringComparison.Ordinal);
            Assert.DoesNotContain("<ColumnDefinition Width=\"3*\" MinWidth=\"540\"/>", xaml, StringComparison.Ordinal);
        }

        [Fact]
        public void ReservationPage_AvoidsLargeFixedMinimumsInMainHoldSplit()
        {
            var xaml = ReadRepoFile("InventoryManagementApp", "Views", "Pages", "ReservationPage.xaml");

            Assert.Contains("<ColumnDefinition Width=\"1.55*\" MinWidth=\"0\"/>", xaml, StringComparison.Ordinal);
            Assert.Contains("<ColumnDefinition Width=\"0.95*\" MinWidth=\"300\"/>", xaml, StringComparison.Ordinal);
            Assert.Matches(new Regex("<GridSplitter[^>]*Grid\\.Column=\"1\"[^>]*Width=\"6\"", RegexOptions.Singleline), xaml);
            Assert.Contains("<Border Grid.Column=\"0\" Style=\"{StaticResource Card}\" Padding=\"0\" MinWidth=\"0\">", xaml, StringComparison.Ordinal);
            Assert.Contains("<Border Grid.Column=\"2\" Style=\"{StaticResource Card}\" Padding=\"0\" MinWidth=\"0\">", xaml, StringComparison.Ordinal);
            Assert.DoesNotContain("<ColumnDefinition Width=\"2.45*\" MinWidth=\"620\"/>", xaml, StringComparison.Ordinal);
            Assert.DoesNotContain("<ColumnDefinition Width=\"430\" MinWidth=\"390\"/>", xaml, StringComparison.Ordinal);
        }

        [Fact]
        public void ReservationPage_EnablesHoldGridVirtualizationScrollingAndFullRowSelection()
        {
            var xaml = ReadRepoFile("InventoryManagementApp", "Views", "Pages", "ReservationPage.xaml");

            Assert.Contains("x:Name=\"ReservationGrid\"", xaml, StringComparison.Ordinal);
            Assert.Contains("EnableRowVirtualization=\"True\"", xaml, StringComparison.Ordinal);
            Assert.Contains("EnableColumnVirtualization=\"True\"", xaml, StringComparison.Ordinal);
            Assert.Contains("SelectionMode=\"Single\"", xaml, StringComparison.Ordinal);
            Assert.Contains("SelectionUnit=\"FullRow\"", xaml, StringComparison.Ordinal);
            Assert.Contains("ScrollViewer.CanContentScroll=\"True\"", xaml, StringComparison.Ordinal);
            Assert.Contains("ScrollViewer.HorizontalScrollBarVisibility=\"Auto\"", xaml, StringComparison.Ordinal);
            Assert.Contains("ScrollViewer.VerticalScrollBarVisibility=\"Auto\"", xaml, StringComparison.Ordinal);
        }

        [Fact]
        public void ReservationPage_BoundsFiltersEmptyStateAndHandoffScrolling()
        {
            var xaml = ReadRepoFile("InventoryManagementApp", "Views", "Pages", "ReservationPage.xaml");

            Assert.Contains("<TextBox Width=\"230\"", xaml, StringComparison.Ordinal);
            Assert.Contains("<ComboBox Width=\"160\"", xaml, StringComparison.Ordinal);
            Assert.Contains("<Border Grid.Row=\"2\" HorizontalAlignment=\"Center\" VerticalAlignment=\"Center\" MaxWidth=\"330\" MinHeight=\"120\" Margin=\"12\" Padding=\"16\">", xaml, StringComparison.Ordinal);
            Assert.Contains("<ScrollViewer Grid.Row=\"1\" VerticalScrollBarVisibility=\"Auto\" HorizontalScrollBarVisibility=\"Disabled\">", xaml, StringComparison.Ordinal);
            Assert.Contains("<StackPanel Margin=\"12,12,12,4\" MinWidth=\"0\">", xaml, StringComparison.Ordinal);
            Assert.Contains("Text=\"{Binding ReservationEmptyTitle}\"", xaml, StringComparison.Ordinal);
            Assert.Contains("Text=\"{Binding ReservationEmptyMessage}\"", xaml, StringComparison.Ordinal);
            Assert.DoesNotContain("VerticalScrollBarVisibility=\"Hidden\"", xaml, StringComparison.Ordinal);
            Assert.DoesNotContain("MaxWidth=\"370\"", xaml, StringComparison.Ordinal);
        }

        [Fact]
        public void ReservationPage_ShowsBoundedLoadingOverlayWhileRowsLoad()
        {
            var xaml = ReadRepoFile("InventoryManagementApp", "Views", "Pages", "ReservationPage.xaml");

            Assert.Contains("<Condition Binding=\"{Binding IsLoading}\" Value=\"False\"/>", xaml, StringComparison.Ordinal);
            Assert.Contains("<DataTrigger Binding=\"{Binding IsLoading}\" Value=\"True\">", xaml, StringComparison.Ordinal);
            Assert.Contains("Loading reservation directory", xaml, StringComparison.Ordinal);
            Assert.Contains("Hold actions and directory printing are paused", xaml, StringComparison.Ordinal);
            Assert.Contains("MaxWidth=\"380\" MinHeight=\"118\"", xaml, StringComparison.Ordinal);
        }

        [Fact]
        public void ReservationPage_PreservesPrimaryReservationActionsAndRowHandoff()
        {
            var xaml = ReadRepoFile("InventoryManagementApp", "Views", "Pages", "ReservationPage.xaml");
            var requiredContracts = new[]
            {
                "AddReservationCommand",
                "ConfirmReservationCommand",
                "FulfillReservationCommand",
                "OpenReservationDetailsCommand",
                "EditReservationCommand",
                "CancelReservationCommand",
                "CopyReservationHandoffCommand",
                "PrintReservationHandoffCommand",
                "DeleteReservationCommand",
                "ShowActiveReservationsCommand",
                "ShowPendingReservationsCommand",
                "ShowConfirmedReservationsCommand",
                "ShowUpcomingReservationsCommand",
                "ClearReservationSearchCommand",
                "RefreshCommand",
                "PrintReservationDirectoryCommand",
                "ReservationRow_MouseDoubleClick",
                "ReservationRow_PreviewMouseRightButtonDown"
            };

            foreach (var contract in requiredContracts)
                Assert.Contains(contract, xaml, StringComparison.Ordinal);
        }

        [Fact]
        public void ReservationViewModel_GuardsLoadingStateAndCommandAvailability()
        {
            var source = ReadRepoFile("InventoryManagementApp", "ViewModels", "ReservationManagementViewModel.cs");

            Assert.Contains("private bool _isLoading;", source, StringComparison.Ordinal);
            Assert.Contains("public bool IsLoading", source, StringComparison.Ordinal);
            Assert.Contains("if (IsLoading)", source, StringComparison.Ordinal);
            Assert.Contains("CanRefreshReservations", source, StringComparison.Ordinal);
            Assert.Contains("CanInteractWithReservations", source, StringComparison.Ordinal);
            Assert.Contains("!IsLoading && SelectedReservation != null", source, StringComparison.Ordinal);
            Assert.Contains("PrintReservationDirectoryCommand.NotifyCanExecuteChanged();", source, StringComparison.Ordinal);
        }

        [Fact]
        public void ReservationViewModel_ExposesProfessionalEmptyAndPrintState()
        {
            var source = ReadRepoFile("InventoryManagementApp", "ViewModels", "ReservationManagementViewModel.cs");

            Assert.Contains("public bool IsFilterActive", source, StringComparison.Ordinal);
            Assert.Contains("public string ReservationEmptyTitle", source, StringComparison.Ordinal);
            Assert.Contains("public string ReservationEmptyMessage", source, StringComparison.Ordinal);
            Assert.Contains("public bool CanPrintReservationDirectory", source, StringComparison.Ordinal);
            Assert.Contains("public string ReservationPrintStatus", source, StringComparison.Ordinal);
            Assert.Contains("Print paused while reservation rows load", source, StringComparison.Ordinal);
            Assert.Contains("No filtered hold rows ready to print", source, StringComparison.Ordinal);
            Assert.Contains("Ready to print first", source, StringComparison.Ordinal);
        }

        [Fact]
        public void ReservationPrintPreview_IsBoundedAndUsesProportionalColumns()
        {
            var source = ReadRepoFile("InventoryManagementApp", "ViewModels", "ReservationManagementViewModel.cs");

            Assert.Contains("private const int MaxReservationPrintRows = 250;", source, StringComparison.Ordinal);
            Assert.Contains("FilteredReservations.Take(MaxReservationPrintRows).ToList();", source, StringComparison.Ordinal);
            Assert.Contains("Visible: {visibleRows} | Printed: {printRows.Count} | Omitted: {omittedRows}", source, StringComparison.Ordinal);
            Assert.Contains("Large reservation preview limited to the first", source, StringComparison.Ordinal);
            Assert.Contains("new GridLength(0.85, GridUnitType.Star)", source, StringComparison.Ordinal);
            Assert.Contains("new GridLength(1.65, GridUnitType.Star)", source, StringComparison.Ordinal);
            Assert.Contains("Review pending, confirmed, upcoming", source, StringComparison.Ordinal);
            Assert.DoesNotContain("table.Columns.Add(new TableColumn { Width = new GridLength(80) });", source, StringComparison.Ordinal);
            Assert.DoesNotContain("table.Columns.Add(new TableColumn { Width = new GridLength(165) });", source, StringComparison.Ordinal);
            Assert.DoesNotContain("foreach (var reservation in FilteredReservations)", source, StringComparison.Ordinal);
        }

        [Fact]
        public void ReservationPage_LoadsOnceAfterFirstPaintAndResetsForNewViewModels()
        {
            var source = ReadRepoFile("InventoryManagementApp", "Views", "Pages", "ReservationPage.xaml.cs");

            Assert.Contains("private Task? _loadReservationsTask;", source, StringComparison.Ordinal);
            Assert.Contains("private ReservationManagementViewModel? _loadedViewModel;", source, StringComparison.Ordinal);
            Assert.Contains("DataContextChanged += ReservationPage_DataContextChanged;", source, StringComparison.Ordinal);
            Assert.Contains("FocusFirstSearchBox();\n\n            if (DataContext is ReservationManagementViewModel vm)", source, StringComparison.Ordinal);
            Assert.Contains("await Dispatcher.Yield(DispatcherPriority.Background);", source, StringComparison.Ordinal);
            Assert.Contains("if (!ReferenceEquals(DataContext, vm) || !vm.LoadReservationsCommand.CanExecute(null))", source, StringComparison.Ordinal);
            Assert.Contains("LoadReservationsOnceAsync", source, StringComparison.Ordinal);
            Assert.Contains("IsCompletedSuccessfully", source, StringComparison.Ordinal);
            Assert.Contains("_loadReservationsTask = null;", source, StringComparison.Ordinal);
            Assert.Contains("Key == Key.N && vm.AddReservationCommand.CanExecute(null)", source, StringComparison.Ordinal);
        }

        [Fact]
        public void ReservationPage_BlocksStaleRowAndShortcutActionsWhileRowsLoad()
        {
            var source = ReadRepoFile("InventoryManagementApp", "Views", "Pages", "ReservationPage.xaml.cs");

            Assert.Contains("ReservationManagementViewModel { IsLoading: true }", source, StringComparison.Ordinal);
            Assert.Contains("GridContextMenuSelection.SelectRow(sender, e);", source, StringComparison.Ordinal);
            Assert.Contains("if (vm.IsLoading && IsReservationActionShortcut(e))", source, StringComparison.Ordinal);
            Assert.Contains("private static bool IsReservationActionShortcut(KeyEventArgs e)", source, StringComparison.Ordinal);
            Assert.Contains("e.Key is Key.N or Key.P or Key.C or Key.D or Key.Enter", source, StringComparison.Ordinal);
            Assert.Contains("e.Key is Key.P or Key.Enter", source, StringComparison.Ordinal);
            Assert.Contains("Keyboard.Modifiers == ModifierKeys.None && e.Key is Key.Enter or Key.Delete", source, StringComparison.Ordinal);
            Assert.Contains("e.Handled = true;", source, StringComparison.Ordinal);
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
