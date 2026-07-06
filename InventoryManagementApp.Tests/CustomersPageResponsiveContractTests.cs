using System;
using System.IO;
using Xunit;

namespace InventoryManagementApp.Tests
{
    public class CustomersPageResponsiveContractTests
    {
        [Fact]
        public void CustomersPage_KeepsCustomerSummaryCardsWrappedAndBounded()
        {
            var xaml = ReadRepoFile("InventoryManagementApp", "Views", "Pages", "CustomersPage.xaml");

            Assert.Contains("<WrapPanel Grid.Row=\"1\" Margin=\"0,0,0,6\">", xaml, StringComparison.Ordinal);
            Assert.Contains("<Setter Property=\"MinWidth\" Value=\"160\"/>", xaml, StringComparison.Ordinal);
            Assert.Contains("<Setter Property=\"MaxWidth\" Value=\"250\"/>", xaml, StringComparison.Ordinal);
            Assert.Contains("CustomerStatValueText", xaml, StringComparison.Ordinal);
            Assert.Contains("CustomerDirectoryVisibleCount", xaml, StringComparison.Ordinal);
            Assert.Contains("CustomerDirectoryMatchCount", xaml, StringComparison.Ordinal);
            Assert.Contains("CustomerVisibleWindowSummary", xaml, StringComparison.Ordinal);
            Assert.Contains("CustomerPrintSummary", xaml, StringComparison.Ordinal);
            Assert.DoesNotContain("<Grid Grid.Row=\"1\" Margin=\"0,0,0,6\">", xaml, StringComparison.Ordinal);
            Assert.DoesNotContain("<ColumnDefinition Width=\"1.35*\"/>", xaml, StringComparison.Ordinal);
        }

        [Fact]
        public void CustomersPage_AvoidsLargeFixedMinimumsInMainCustomerSplit()
        {
            var xaml = ReadRepoFile("InventoryManagementApp", "Views", "Pages", "CustomersPage.xaml");

            Assert.Contains("<ColumnDefinition Width=\"1.65*\" MinWidth=\"0\"/>", xaml, StringComparison.Ordinal);
            Assert.Contains("<ColumnDefinition Width=\"0.95*\" MinWidth=\"300\"/>", xaml, StringComparison.Ordinal);
            Assert.Contains("<GridSplitter Grid.Column=\"1\" Width=\"6\"", xaml, StringComparison.Ordinal);
            Assert.Contains("<Border Grid.Column=\"0\" Style=\"{StaticResource Card}\" Padding=\"0\" MinWidth=\"0\">", xaml, StringComparison.Ordinal);
            Assert.Contains("<Border Grid.Column=\"2\" Style=\"{StaticResource Card}\" Padding=\"0\" MinWidth=\"0\">", xaml, StringComparison.Ordinal);
            Assert.DoesNotContain("<ColumnDefinition Width=\"2.1*\" MinWidth=\"560\"/>", xaml, StringComparison.Ordinal);
            Assert.DoesNotContain("<ColumnDefinition Width=\"1.05*\" MinWidth=\"360\"/>", xaml, StringComparison.Ordinal);
        }

        [Fact]
        public void CustomersPage_EnablesDirectoryGridVirtualizationScrollingAndFullRowSelection()
        {
            var xaml = ReadRepoFile("InventoryManagementApp", "Views", "Pages", "CustomersPage.xaml");

            Assert.Contains("x:Name=\"CustomerDataGrid\"", xaml, StringComparison.Ordinal);
            Assert.Contains("EnableRowVirtualization=\"True\"", xaml, StringComparison.Ordinal);
            Assert.Contains("EnableColumnVirtualization=\"True\"", xaml, StringComparison.Ordinal);
            Assert.Contains("SelectionMode=\"Single\"", xaml, StringComparison.Ordinal);
            Assert.Contains("SelectionUnit=\"FullRow\"", xaml, StringComparison.Ordinal);
            Assert.Contains("ScrollViewer.CanContentScroll=\"True\"", xaml, StringComparison.Ordinal);
            Assert.Contains("ScrollViewer.HorizontalScrollBarVisibility=\"Auto\"", xaml, StringComparison.Ordinal);
            Assert.Contains("ScrollViewer.VerticalScrollBarVisibility=\"Auto\"", xaml, StringComparison.Ordinal);
        }

        [Fact]
        public void CustomersPage_BoundsSearchEmptyStateAndHandoffScrolling()
        {
            var xaml = ReadRepoFile("InventoryManagementApp", "Views", "Pages", "CustomersPage.xaml");

            Assert.Contains("<pages:SearchBar Width=\"300\"", xaml, StringComparison.Ordinal);
            Assert.Contains("MinWidth=\"220\"", xaml, StringComparison.Ordinal);
            Assert.Contains("<Border Grid.Row=\"2\" MaxWidth=\"330\" MinHeight=\"120\" Margin=\"12\" HorizontalAlignment=\"Center\" VerticalAlignment=\"Center\" Visibility=\"{Binding IsCustomerEmptyStateVisible, Converter={StaticResource BoolToVis}}\"", xaml, StringComparison.Ordinal);
            Assert.Contains("CustomerEmptyStateMessage", xaml, StringComparison.Ordinal);
            Assert.Contains("<ScrollViewer Grid.Row=\"1\" VerticalScrollBarVisibility=\"Auto\" HorizontalScrollBarVisibility=\"Disabled\">", xaml, StringComparison.Ordinal);
            Assert.DoesNotContain("<Border Grid.Row=\"2\" Width=\"310\"", xaml, StringComparison.Ordinal);
            Assert.DoesNotContain("<DataTrigger Binding=\"{Binding Customers.Count}\" Value=\"0\">", xaml, StringComparison.Ordinal);
            Assert.DoesNotContain("VerticalScrollBarVisibility=\"Hidden\"", xaml, StringComparison.Ordinal);
        }

        [Fact]
        public void CustomersPage_ShowsBoundedDirectoryLoadingOverlay()
        {
            var xaml = ReadRepoFile("InventoryManagementApp", "Views", "Pages", "CustomersPage.xaml");

            Assert.Contains("<Border Grid.Row=\"2\" MaxWidth=\"360\" MinHeight=\"120\" Margin=\"12\"", xaml, StringComparison.Ordinal);
            Assert.Contains("<DataTrigger Binding=\"{Binding IsCustomerDirectoryBusy}\" Value=\"True\">", xaml, StringComparison.Ordinal);
            Assert.Contains("<ProgressBar IsIndeterminate=\"True\" Height=\"6\"", xaml, StringComparison.Ordinal);
            Assert.Contains("Updating customer directory", xaml, StringComparison.Ordinal);
            Assert.Contains("Text=\"{Binding CustomerFilterStatus}\"", xaml, StringComparison.Ordinal);
        }

        [Fact]
        public void CustomersPage_BindsVisibleActionsToReadyState()
        {
            var xaml = ReadRepoFile("InventoryManagementApp", "Views", "Pages", "CustomersPage.xaml");

            Assert.Contains("Content=\"Add\" Command=\"{Binding AddCustomerCommand}\" IsEnabled=\"{Binding IsCustomerDirectoryActionAvailable}\"", xaml, StringComparison.Ordinal);
            Assert.Contains("Content=\"Details\" Command=\"{Binding OpenCustomerDetailsCommand}\" IsEnabled=\"{Binding IsSelectedCustomerActionAvailable}\"", xaml, StringComparison.Ordinal);
            Assert.Contains("Content=\"Edit\" Command=\"{Binding EditCustomerCommand}\" IsEnabled=\"{Binding IsSelectedCustomerActionAvailable}\"", xaml, StringComparison.Ordinal);
            Assert.Contains("Content=\"Copy Contact\" Command=\"{Binding CopySelectedCustomerCommand}\" IsEnabled=\"{Binding IsSelectedCustomerActionAvailable}\"", xaml, StringComparison.Ordinal);
            Assert.Contains("Content=\"Print Sheet\" Command=\"{Binding PrintSelectedCustomerCommand}\" IsEnabled=\"{Binding IsSelectedCustomerActionAvailable}\"", xaml, StringComparison.Ordinal);
            Assert.Contains("Content=\"Directory\" Command=\"{Binding PrintCustomerDirectoryCommand}\" IsEnabled=\"{Binding IsCustomerDirectoryPrintAvailable}\"", xaml, StringComparison.Ordinal);
            Assert.Contains("Content=\"Delete\" Command=\"{Binding DeleteCustomerCommand}\" IsEnabled=\"{Binding IsSelectedCustomerActionAvailable}\"", xaml, StringComparison.Ordinal);
            Assert.Contains("IsEnabled=\"{Binding IsCustomerDirectoryActionAvailable}\"", xaml, StringComparison.Ordinal);
            Assert.Contains("IsEnabled=\"{Binding IsCustomerDirectoryPrintAvailable}\"", xaml, StringComparison.Ordinal);
        }

        [Fact]
        public void CustomersPage_ContextMenuAndHandoffActionsRespectReadyState()
        {
            var xaml = ReadRepoFile("InventoryManagementApp", "Views", "Pages", "CustomersPage.xaml");

            Assert.Contains("Header=\"Open Customer Details\" Command=\"{Binding OpenCustomerDetailsCommand}\" IsEnabled=\"{Binding IsSelectedCustomerActionAvailable}\"", xaml, StringComparison.Ordinal);
            Assert.Contains("Header=\"Edit Customer\" Command=\"{Binding EditCustomerCommand}\" IsEnabled=\"{Binding IsSelectedCustomerActionAvailable}\"", xaml, StringComparison.Ordinal);
            Assert.Contains("Header=\"Copy Contact Handoff\" Command=\"{Binding CopySelectedCustomerCommand}\" IsEnabled=\"{Binding IsSelectedCustomerActionAvailable}\"", xaml, StringComparison.Ordinal);
            Assert.Contains("Header=\"Print Customer Sheet\" Command=\"{Binding PrintSelectedCustomerCommand}\" IsEnabled=\"{Binding IsSelectedCustomerActionAvailable}\"", xaml, StringComparison.Ordinal);
            Assert.Contains("Header=\"Print Customer Directory\" Command=\"{Binding PrintCustomerDirectoryCommand}\" IsEnabled=\"{Binding IsCustomerDirectoryPrintAvailable}\"", xaml, StringComparison.Ordinal);
            Assert.Contains("Header=\"Delete Customer\" Command=\"{Binding DeleteCustomerCommand}\" IsEnabled=\"{Binding IsSelectedCustomerActionAvailable}\"", xaml, StringComparison.Ordinal);
            Assert.Contains("Content=\"Copy Contact\" Command=\"{Binding CopySelectedCustomerCommand}\" IsEnabled=\"{Binding IsSelectedCustomerActionAvailable}\"", xaml, StringComparison.Ordinal);
            Assert.Contains("Content=\"Clear Search\" Command=\"{Binding ClearCustomerSearchCommand}\" IsEnabled=\"{Binding IsCustomerDirectoryActionAvailable}\"", xaml, StringComparison.Ordinal);
        }

        [Fact]
        public void CustomersPage_PreservesPrimaryCustomerActionsAndRowHandoff()
        {
            var xaml = ReadRepoFile("InventoryManagementApp", "Views", "Pages", "CustomersPage.xaml");

            Assert.Contains("AddCustomerCommand", xaml, StringComparison.Ordinal);
            Assert.Contains("OpenCustomerDetailsCommand", xaml, StringComparison.Ordinal);
            Assert.Contains("EditCustomerCommand", xaml, StringComparison.Ordinal);
            Assert.Contains("CopySelectedCustomerCommand", xaml, StringComparison.Ordinal);
            Assert.Contains("PrintSelectedCustomerCommand", xaml, StringComparison.Ordinal);
            Assert.Contains("PrintCustomerDirectoryCommand", xaml, StringComparison.Ordinal);
            Assert.Contains("DeleteCustomerCommand", xaml, StringComparison.Ordinal);
            Assert.Contains("CustomerRow_MouseDoubleClick", xaml, StringComparison.Ordinal);
            Assert.Contains("CustomerRow_PreviewMouseRightButtonDown", xaml, StringComparison.Ordinal);
        }

        [Fact]
        public void CustomersPage_LoadsOnceAfterFirstPaintAndResetsForNewViewModels()
        {
            var source = ReadRepoFile("InventoryManagementApp", "Views", "Pages", "CustomersPage.xaml.cs");

            Assert.Contains("private Task? _loadCustomersTask;", source, StringComparison.Ordinal);
            Assert.Contains("private CustomerManagementViewModel? _loadedViewModel;", source, StringComparison.Ordinal);
            Assert.Contains("private CancellationTokenSource? _loadCustomersCancellation;", source, StringComparison.Ordinal);
            Assert.Contains("private int _loadCustomersVersion;", source, StringComparison.Ordinal);
            Assert.Contains("Unloaded += CustomersPage_Unloaded;", source, StringComparison.Ordinal);
            Assert.Contains("DataContextChanged += CustomersPage_DataContextChanged;", source, StringComparison.Ordinal);
            Assert.Contains("FocusFirstSearchBox();\n\n            if (DataContext is CustomerManagementViewModel vm)", source, StringComparison.Ordinal);
            Assert.Contains("private void CustomersPage_Unloaded(object sender, RoutedEventArgs e)", source, StringComparison.Ordinal);
            Assert.Contains("CancelPageOwnedLoad();", source, StringComparison.Ordinal);
            Assert.Contains("await Dispatcher.Yield(DispatcherPriority.Background);", source, StringComparison.Ordinal);
            Assert.Contains("cancellationToken.IsCancellationRequested || loadVersion != _loadCustomersVersion || !ReferenceEquals(DataContext, vm) || vm.IsCustomerDirectoryBusy", source, StringComparison.Ordinal);
            Assert.Contains("_loadCustomersTask = vm.LoadCustomersAsync();", source, StringComparison.Ordinal);
            Assert.Contains("IsCompletedSuccessfully", source, StringComparison.Ordinal);
            Assert.Contains("_loadCustomersTask = null;", source, StringComparison.Ordinal);
        }

        [Fact]
        public void CustomersPage_BlocksStaleRowAndShortcutActionsWhileRowsLoad()
        {
            var source = ReadRepoFile("InventoryManagementApp", "Views", "Pages", "CustomersPage.xaml.cs");
            var doubleClick = ExtractSourceBlock(source, "private void CustomerRow_MouseDoubleClick", "private void CustomerRow_PreviewMouseRightButtonDown");

            Assert.Contains("if (vm.IsCustomerDirectoryBusy)", doubleClick, StringComparison.Ordinal);
            Assert.Contains("GridContextMenuSelection.SelectRow(sender, e) == null", doubleClick, StringComparison.Ordinal);
            Assert.Contains("CustomerManagementViewModel { IsCustomerDirectoryBusy: true }", source, StringComparison.Ordinal);
            Assert.Contains("GridContextMenuSelection.SelectRow(sender, e);", source, StringComparison.Ordinal);
            Assert.Contains("if (vm.IsCustomerDirectoryBusy && IsCustomerActionShortcut(e))", source, StringComparison.Ordinal);
            Assert.Contains("private static bool IsCustomerActionShortcut(KeyEventArgs e)", source, StringComparison.Ordinal);
            Assert.Contains("e.Key is Key.N or Key.R or Key.E or Key.P or Key.C or Key.D", source, StringComparison.Ordinal);
            Assert.Contains("Keyboard.Modifiers == ModifierKeys.None && (e.Key is Key.Enter or Key.Delete)", source, StringComparison.Ordinal);
            Assert.Contains("Key == Key.N && vm.AddCustomerCommand.CanExecute(null)", source, StringComparison.Ordinal);
            Assert.Contains("Key == Key.R && vm.SearchCustomersCommand.CanExecute(null)", source, StringComparison.Ordinal);
            Assert.Contains("Key == Key.E && vm.EditCustomerCommand.CanExecute(null)", source, StringComparison.Ordinal);
            Assert.Contains("Key == Key.P && vm.PrintCustomerDirectoryCommand.CanExecute(null)", source, StringComparison.Ordinal);
            Assert.Contains("Key == Key.Delete && vm.DeleteCustomerCommand.CanExecute(null)", source, StringComparison.Ordinal);
        }

        [Fact]
        public void CustomersPage_PreservesTextEditingAndSuppressesBusyContextMenus()
        {
            var xaml = ReadRepoFile("InventoryManagementApp", "Views", "Pages", "CustomersPage.xaml");
            var source = ReadRepoFile("InventoryManagementApp", "Views", "Pages", "CustomersPage.xaml.cs");
            var keyDown = ExtractSourceBlock(source, "private void CustomersPage_PreviewKeyDown", "private static bool IsCustomerActionShortcut");

            Assert.Contains("ContextMenuOpening=\"CustomerDataGrid_ContextMenuOpening\"", xaml, StringComparison.Ordinal);
            Assert.Contains("private void CustomerDataGrid_ContextMenuOpening(object sender, ContextMenuEventArgs e)", source, StringComparison.Ordinal);
            Assert.Contains("if (DataContext is CustomerManagementViewModel { IsCustomerDirectoryBusy: true })", source, StringComparison.Ordinal);
            Assert.Contains("if (IsTextInputFocused() && IsCustomerActionShortcut(e))", keyDown, StringComparison.Ordinal);
            Assert.Contains("return;", keyDown, StringComparison.Ordinal);
            Assert.Contains("Keyboard.FocusedElement is TextBoxBase or PasswordBox or ComboBox", source, StringComparison.Ordinal);
            Assert.Contains("Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.F", keyDown, StringComparison.Ordinal);
            Assert.True(
                keyDown.IndexOf("Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.F", StringComparison.Ordinal) <
                keyDown.IndexOf("if (IsTextInputFocused() && IsCustomerActionShortcut(e))", StringComparison.Ordinal),
                "Ctrl+F should keep focusing search before text-edit shortcuts are preserved.");
            Assert.True(
                keyDown.IndexOf("if (IsTextInputFocused() && IsCustomerActionShortcut(e))", StringComparison.Ordinal) <
                keyDown.IndexOf("Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.N", StringComparison.Ordinal),
                "Text-edit guard should run before customer action shortcuts dispatch.");
        }

        [Fact]
        public void CustomersPage_HandlesUnavailableDoubleClicksAndUsesIterativeSearchBoxLookup()
        {
            var source = ReadRepoFile("InventoryManagementApp", "Views", "Pages", "CustomersPage.xaml.cs");
            var doubleClick = ExtractSourceBlock(source, "private void CustomerRow_MouseDoubleClick", "private void CustomerRow_PreviewMouseRightButtonDown");
            var findDescendant = ExtractSourceBlock(source, "private static T? FindDescendant", "private static int GetVisualChildCount");

            Assert.Contains("e.Handled = true;\n                return;", doubleClick, StringComparison.Ordinal);
            Assert.Contains("e.Handled = true;\n        }", doubleClick, StringComparison.Ordinal);
            Assert.Contains("using System.Collections.Generic;", source, StringComparison.Ordinal);
            Assert.Contains("var pending = new Stack<DependencyObject>();", findDescendant, StringComparison.Ordinal);
            Assert.Contains("while (pending.Count > 0)", findDescendant, StringComparison.Ordinal);
            Assert.Contains("pending.Push(child);", findDescendant, StringComparison.Ordinal);
            Assert.DoesNotContain("var nested = FindDescendant<T>(child);", findDescendant, StringComparison.Ordinal);
            Assert.Contains("private static int GetVisualChildCount(DependencyObject current)", source, StringComparison.Ordinal);
            Assert.Contains("catch (System.InvalidOperationException)", source, StringComparison.Ordinal);
        }

        [Fact]
        public void CustomerViewModel_GuardsBusyStateAndSelectedCommandAvailability()
        {
            var source = ReadRepoFile("InventoryManagementApp", "ViewModels", "CustomerManagementViewModel.cs");

            Assert.Contains("AddCustomerCommand = new AsyncRelayCommand(AddCustomerAsync, CanRefreshCustomerDirectory);", source, StringComparison.Ordinal);
            Assert.Contains("UpdateCustomerCommand = new AsyncRelayCommand(UpdateCustomerAsync, CanInteractWithSelectedCustomer);", source, StringComparison.Ordinal);
            Assert.Contains("EditCustomerFromRowCommand = new AsyncRelayCommand<CustomerModel>(EditCustomerAsync, CanInteractWithCustomer);", source, StringComparison.Ordinal);
            Assert.Contains("DeleteCustomerFromRowCommand = new AsyncRelayCommand<CustomerModel>(c => DeleteCustomerAsync(c), CanInteractWithCustomer);", source, StringComparison.Ordinal);
            Assert.Contains("OpenCustomerDetailsCommand = new RelayCommand(OpenCustomerDetails, CanInteractWithSelectedCustomer);", source, StringComparison.Ordinal);
            Assert.Contains("PrintSelectedCustomerCommand = new RelayCommand(PrintSelectedCustomer, CanInteractWithSelectedCustomer);", source, StringComparison.Ordinal);
            Assert.Contains("CopySelectedCustomerCommand = new RelayCommand(CopySelectedCustomer, CanInteractWithSelectedCustomer);", source, StringComparison.Ordinal);
            Assert.Contains("private bool CanInteractWithSelectedCustomer() => IsSelectedCustomerActionAvailable;", source, StringComparison.Ordinal);
            Assert.Contains("private bool CanInteractWithCustomer(CustomerModel? customer) => !IsCustomerDirectoryBusy && customer != null;", source, StringComparison.Ordinal);
            Assert.Contains("NotifySelectedCustomerActionStateChanged();", source, StringComparison.Ordinal);
        }

        [Fact]
        public void CustomerViewModel_ExposesBusyPrintAndActionStatus()
        {
            var source = ReadRepoFile("InventoryManagementApp", "ViewModels", "CustomerManagementViewModel.cs");

            Assert.Contains("public bool IsCustomerDirectoryActionAvailable => !IsCustomerDirectoryBusy;", source, StringComparison.Ordinal);
            Assert.Contains("public bool IsSelectedCustomerActionAvailable => !IsCustomerDirectoryBusy && SelectedCustomer != null;", source, StringComparison.Ordinal);
            Assert.Contains("public bool IsCustomerDirectoryPrintAvailable => !IsCustomerDirectoryBusy && Customers.Count > 0;", source, StringComparison.Ordinal);
            Assert.Contains("public bool IsCustomerEmptyStateVisible => !IsCustomerDirectoryBusy && Customers.Count == 0;", source, StringComparison.Ordinal);
            Assert.Contains("Print paused while customer rows load", source, StringComparison.Ordinal);
            Assert.Contains("Customer actions are paused while the directory refreshes", source, StringComparison.Ordinal);
            Assert.Contains("ShowCustomerDirectoryBusyMessage", source, StringComparison.Ordinal);
            Assert.Contains("Customer rows are still updating. Try again after the directory finishes loading.", source, StringComparison.Ordinal);
            Assert.Contains("if (IsCustomerDirectoryBusy || SelectedCustomer == null", source, StringComparison.Ordinal);
            Assert.Contains("if (IsCustomerDirectoryBusy || customer == null", source, StringComparison.Ordinal);
            Assert.Contains("OnPropertyChanged(nameof(CustomerPrintSummary));", source, StringComparison.Ordinal);
            Assert.Contains("OnPropertyChanged(nameof(CustomerOperationsSummary));", source, StringComparison.Ordinal);
            Assert.Contains("NotifyCustomerAvailabilityStateChanged();", source, StringComparison.Ordinal);
        }

        [Fact]
        public void CustomerViewModel_BoundsVisibleRowsAndTracksFullDirectoryCounts()
        {
            var source = ReadRepoFile("InventoryManagementApp", "ViewModels", "CustomerManagementViewModel.cs");

            Assert.Contains("private const int MaxCustomerDirectoryVisibleRows = 500;", source, StringComparison.Ordinal);
            Assert.Contains("public int CustomerDirectoryMatchCount => _customerDirectoryMatchCount;", source, StringComparison.Ordinal);
            Assert.Contains("public int CustomerDirectoryVisibleCount => Customers.Count;", source, StringComparison.Ordinal);
            Assert.Contains("public int CustomerDirectoryOmittedCount => Math.Max(0, CustomerDirectoryMatchCount - CustomerDirectoryVisibleCount);", source, StringComparison.Ordinal);
            Assert.Contains("public bool IsCustomerDirectoryWindowCapped => CustomerDirectoryOmittedCount > 0;", source, StringComparison.Ordinal);
            Assert.Contains("public string CustomerVisibleWindowSummary", source, StringComparison.Ordinal);
            Assert.Contains("ApplyCustomerDirectoryWindow(OrderCustomersForDirectory(all), preferredCustomerId);", source, StringComparison.Ordinal);
            Assert.Contains("ApplyCustomerDirectoryWindow(all, preferredCustomerId);", source, StringComparison.Ordinal);
            Assert.Contains("orderedCustomers.Take(MaxCustomerDirectoryVisibleRows).ToList();", source, StringComparison.Ordinal);
            Assert.Contains("IsSameVisibleCustomerWindow", source, StringComparison.Ordinal);
            Assert.Contains("Customers.ReplaceRange(visibleCustomers);", source, StringComparison.Ordinal);
            Assert.Contains("OnPropertyChanged(nameof(CustomerDirectoryMatchCount));", source, StringComparison.Ordinal);
            Assert.Contains("OnPropertyChanged(nameof(CustomerDirectoryVisibleCount));", source, StringComparison.Ordinal);
            Assert.Contains("OnPropertyChanged(nameof(CustomerDirectoryOmittedCount));", source, StringComparison.Ordinal);
            Assert.Contains("OnPropertyChanged(nameof(IsCustomerDirectoryWindowCapped));", source, StringComparison.Ordinal);
            Assert.Contains("OnPropertyChanged(nameof(CustomerVisibleWindowSummary));", source, StringComparison.Ordinal);
        }

        [Fact]
        public void CustomerViewModel_PrintDirectoryUsesFullMatchAndVisibleWindowAccounting()
        {
            var source = ReadRepoFile("InventoryManagementApp", "ViewModels", "CustomerManagementViewModel.cs");
            var printBlock = ExtractSourceBlock(source, "void PrintCustomerDirectory()", "void PrintSelectedCustomer()");

            Assert.Contains("var matchCount = Math.Max(CustomerDirectoryMatchCount, Customers.Count);", printBlock, StringComparison.Ordinal);
            Assert.Contains("var hiddenFromGridCount = Math.Max(0, matchCount - visibleCount);", printBlock, StringComparison.Ordinal);
            Assert.Contains("Matched: {matchCount} | Visible: {visibleCount} | Printed: {printableCustomers.Count} | Omitted: {omittedCount}", printBlock, StringComparison.Ordinal);
            Assert.Contains("additional matching customers are outside the live grid", printBlock, StringComparison.Ordinal);
            Assert.Contains("matched-row count, visible-row window, and omitted-row count", printBlock, StringComparison.Ordinal);
            Assert.Contains("matched-row counts, visible-row windows, and large-directory limits", printBlock, StringComparison.Ordinal);
        }

        [Fact]
        public void CustomerViewModel_PreservesVisibleRowsWhenLoadOrSearchFails()
        {
            var source = ReadRepoFile("InventoryManagementApp", "ViewModels", "CustomerManagementViewModel.cs");
            var loadBlock = ExtractSourceBlock(source, "public async Task LoadCustomersAsync", "async Task AddCustomerAsync");
            var searchBlock = ExtractSourceBlock(source, "async Task SearchCustomersAsync", "private async Task RefreshCustomerDirectoryAfterMutationFailureAsync");

            Assert.Contains("Existing customer rows were kept when available", loadBlock, StringComparison.Ordinal);
            Assert.Contains("Existing customer rows were kept when available", searchBlock, StringComparison.Ordinal);
            Assert.Contains("NotifyCustomerDirectoryStateChanged();", loadBlock, StringComparison.Ordinal);
            Assert.Contains("NotifyCustomerDirectoryStateChanged();", searchBlock, StringComparison.Ordinal);
            Assert.DoesNotContain("ClearCustomerDirectoryAfterLoadFailure();", loadBlock, StringComparison.Ordinal);
            Assert.DoesNotContain("ClearCustomerDirectoryAfterLoadFailure();", searchBlock, StringComparison.Ordinal);
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

        static string NormalizeLineEndings(string text)
            => text.Replace("\r\n", "\n");
    }
}
