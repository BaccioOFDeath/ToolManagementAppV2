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
