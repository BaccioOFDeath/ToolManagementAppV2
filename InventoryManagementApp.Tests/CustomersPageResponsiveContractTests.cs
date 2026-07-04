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
            Assert.Contains("CustomerFilterStatus", xaml, StringComparison.Ordinal);
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
            Assert.Contains("<Border Grid.Row=\"2\" MaxWidth=\"330\" MinHeight=\"120\" Margin=\"12\"", xaml, StringComparison.Ordinal);
            Assert.Contains("CustomerEmptyStateMessage", xaml, StringComparison.Ordinal);
            Assert.Contains("<ScrollViewer Grid.Row=\"1\" VerticalScrollBarVisibility=\"Auto\" HorizontalScrollBarVisibility=\"Disabled\">", xaml, StringComparison.Ordinal);
            Assert.DoesNotContain("<Border Grid.Row=\"2\" Width=\"310\"", xaml, StringComparison.Ordinal);
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
            Assert.Contains("DataContextChanged += CustomersPage_DataContextChanged;", source, StringComparison.Ordinal);
            Assert.Contains("FocusFirstSearchBox();\n\n            if (DataContext is CustomerManagementViewModel vm)", source, StringComparison.Ordinal);
            Assert.Contains("await Dispatcher.Yield(DispatcherPriority.Background);", source, StringComparison.Ordinal);
            Assert.Contains("if (!ReferenceEquals(DataContext, vm) || vm.IsCustomerDirectoryBusy)", source, StringComparison.Ordinal);
            Assert.Contains("_loadCustomersTask = vm.LoadCustomersAsync();", source, StringComparison.Ordinal);
            Assert.Contains("IsCompletedSuccessfully", source, StringComparison.Ordinal);
            Assert.Contains("_loadCustomersTask = null;", source, StringComparison.Ordinal);
        }

        [Fact]
        public void CustomersPage_BlocksStaleRowAndShortcutActionsWhileRowsLoad()
        {
            var source = ReadRepoFile("InventoryManagementApp", "Views", "Pages", "CustomersPage.xaml.cs");

            Assert.Contains("CustomerManagementViewModel { IsCustomerDirectoryBusy: true }", source, StringComparison.Ordinal);
            Assert.Contains("GridContextMenuSelection.SelectRow(sender, e);", source, StringComparison.Ordinal);
            Assert.Contains("if (vm.IsCustomerDirectoryBusy && IsCustomerActionShortcut(e))", source, StringComparison.Ordinal);
            Assert.Contains("private static bool IsCustomerActionShortcut(KeyEventArgs e)", source, StringComparison.Ordinal);
            Assert.Contains("e.Key is Key.N or Key.P or Key.C or Key.D", source, StringComparison.Ordinal);
            Assert.Contains("Keyboard.Modifiers == ModifierKeys.None && (e.Key is Key.Enter or Key.Delete)", source, StringComparison.Ordinal);
            Assert.Contains("Key == Key.N && vm.AddCustomerCommand.CanExecute(null)", source, StringComparison.Ordinal);
            Assert.Contains("Key == Key.P && vm.PrintCustomerDirectoryCommand.CanExecute(null)", source, StringComparison.Ordinal);
            Assert.Contains("Key == Key.Delete && vm.DeleteCustomerCommand.CanExecute(null)", source, StringComparison.Ordinal);
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
            Assert.Contains("private bool CanInteractWithSelectedCustomer() => !IsCustomerDirectoryBusy && SelectedCustomer != null;", source, StringComparison.Ordinal);
            Assert.Contains("private bool CanInteractWithCustomer(CustomerModel? customer) => !IsCustomerDirectoryBusy && customer != null;", source, StringComparison.Ordinal);
            Assert.Contains("NotifySelectedCustomerActionStateChanged();", source, StringComparison.Ordinal);
        }

        [Fact]
        public void CustomerViewModel_ExposesBusyPrintAndActionStatus()
        {
            var source = ReadRepoFile("InventoryManagementApp", "ViewModels", "CustomerManagementViewModel.cs");

            Assert.Contains("Print paused while customer rows load", source, StringComparison.Ordinal);
            Assert.Contains("Customer actions are paused while the directory refreshes", source, StringComparison.Ordinal);
            Assert.Contains("ShowCustomerDirectoryBusyMessage", source, StringComparison.Ordinal);
            Assert.Contains("Customer rows are still updating. Try again after the directory finishes loading.", source, StringComparison.Ordinal);
            Assert.Contains("if (IsCustomerDirectoryBusy || SelectedCustomer == null", source, StringComparison.Ordinal);
            Assert.Contains("if (IsCustomerDirectoryBusy || customer == null", source, StringComparison.Ordinal);
            Assert.Contains("OnPropertyChanged(nameof(CustomerPrintSummary));", source, StringComparison.Ordinal);
            Assert.Contains("OnPropertyChanged(nameof(CustomerOperationsSummary));", source, StringComparison.Ordinal);
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
