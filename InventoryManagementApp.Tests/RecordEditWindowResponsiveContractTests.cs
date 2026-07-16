using System;
using System.IO;
using Xunit;

namespace InventoryManagementApp.Tests
{
    public class RecordEditWindowResponsiveContractTests
    {
        [Fact]
        public void ItemEditWindow_UsesSaferScaledDialogBoundsAndWrappingSummaryCards()
        {
            var xaml = ReadRepoFile("InventoryManagementApp", "Views", "Windows", "ItemEditWindow.xaml");

            Assert.Contains("Width=\"800\"", xaml, StringComparison.Ordinal);
            Assert.Contains("Height=\"660\"", xaml, StringComparison.Ordinal);
            Assert.Contains("MinWidth=\"660\"", xaml, StringComparison.Ordinal);
            Assert.Contains("MinHeight=\"520\"", xaml, StringComparison.Ordinal);
            Assert.Contains("<WrapPanel Margin=\"0,0,0,8\">", xaml, StringComparison.Ordinal);
            Assert.True(CountOccurrences(xaml, "Style=\"{StaticResource DesktopSummaryCard}\" MinWidth=\"150\" MaxWidth=\"210\"") >= 4);
            Assert.DoesNotContain("<UniformGrid Columns=\"4\"", xaml, StringComparison.Ordinal);
            Assert.DoesNotContain("Width=\"840\"", xaml, StringComparison.Ordinal);
            Assert.DoesNotContain("MinWidth=\"760\"", xaml, StringComparison.Ordinal);
        }

        [Fact]
        public void ItemEditWindow_ReducesSplitPressureAndKeepsSaveActionsReachable()
        {
            var xaml = ReadRepoFile("InventoryManagementApp", "Views", "Windows", "ItemEditWindow.xaml");

            Assert.Contains("<ScrollViewer Grid.Row=\"1\" VerticalScrollBarVisibility=\"Auto\" HorizontalScrollBarVisibility=\"Disabled\">", xaml, StringComparison.Ordinal);
            Assert.Contains("<ColumnDefinition Width=\"2*\" MinWidth=\"0\"/>", xaml, StringComparison.Ordinal);
            Assert.Contains("<ColumnDefinition Width=\"8\"/>", xaml, StringComparison.Ordinal);
            Assert.Contains("<ColumnDefinition Width=\"1*\" MinWidth=\"0\"/>", xaml, StringComparison.Ordinal);
            Assert.Contains("<ColumnDefinition Width=\"130\"/>", xaml, StringComparison.Ordinal);
            Assert.Contains("Height=\"90\" AcceptsReturn=\"True\" TextWrapping=\"Wrap\" VerticalScrollBarVisibility=\"Auto\" HorizontalScrollBarVisibility=\"Disabled\"", xaml, StringComparison.Ordinal);
            Assert.Contains("Height=\"190\" AcceptsReturn=\"True\" TextWrapping=\"Wrap\" VerticalScrollBarVisibility=\"Auto\" HorizontalScrollBarVisibility=\"Disabled\"", xaml, StringComparison.Ordinal);
            Assert.Contains("<controls:SaveCancelBar Grid.Row=\"2\"/>", xaml, StringComparison.Ordinal);
            Assert.DoesNotContain("<ColumnDefinition Width=\"150\"/>", xaml, StringComparison.Ordinal);
            Assert.DoesNotContain("Height=\"260\" AcceptsReturn=\"True\"", xaml, StringComparison.Ordinal);
        }

        [Fact]
        public void CustomerEditWindow_UsesWrappingSummaryAndScrollableShrinkableForm()
        {
            var xaml = ReadRepoFile("InventoryManagementApp", "Views", "Windows", "CustomerEditWindow.xaml");

            Assert.Contains("Width=\"860\" Height=\"700\"", xaml, StringComparison.Ordinal);
            Assert.Contains("MinWidth=\"620\" MinHeight=\"500\"", xaml, StringComparison.Ordinal);
            Assert.Contains("<WrapPanel Grid.Row=\"1\" Margin=\"0,0,0,2\">", xaml, StringComparison.Ordinal);
            Assert.Contains("<Style x:Key=\"CustomerEditorStepCard\" TargetType=\"Border\" BasedOn=\"{StaticResource DesktopSummaryCard}\">", xaml, StringComparison.Ordinal);
            Assert.Contains("<Setter Property=\"MinWidth\" Value=\"150\"/>", xaml, StringComparison.Ordinal);
            Assert.Contains("<Setter Property=\"MaxWidth\" Value=\"215\"/>", xaml, StringComparison.Ordinal);
            Assert.Contains("<ScrollViewer Grid.Row=\"1\"", xaml, StringComparison.Ordinal);
            Assert.Contains("HorizontalScrollBarVisibility=\"Disabled\"", xaml, StringComparison.Ordinal);
            Assert.Contains("CanContentScroll=\"False\"", xaml, StringComparison.Ordinal);
            Assert.Contains("<ColumnDefinition Width=\"88\" MinWidth=\"74\"/>", xaml, StringComparison.Ordinal);
            Assert.Contains("<ColumnDefinition Width=\"78\" MinWidth=\"68\"/>", xaml, StringComparison.Ordinal);
            Assert.DoesNotContain("<UniformGrid Grid.Row=\"1\" Columns=\"3\"", xaml, StringComparison.Ordinal);
            Assert.DoesNotContain("MinWidth=\"280\"", xaml, StringComparison.Ordinal);
            Assert.DoesNotContain("MinWidth=\"260\"", xaml, StringComparison.Ordinal);
        }

        [Fact]
        public void CustomerEditWindow_PreservesCustomerBindingsAndFooterStatus()
        {
            var xaml = ReadRepoFile("InventoryManagementApp", "Views", "Windows", "CustomerEditWindow.xaml");
            var requiredContracts = new[]
            {
                "Customer.Company",
                "Customer.Contact",
                "Customer.Email",
                "Customer.Phone",
                "Customer.Mobile",
                "Customer.Address",
                "StatusMessage",
                "<controls:SaveCancelBar Grid.Row=\"3\" Margin=\"0,8,0,0\"/>"
            };

            foreach (var contract in requiredContracts)
                Assert.Contains(contract, xaml, StringComparison.Ordinal);

            Assert.Contains("HorizontalScrollBarVisibility=\"Disabled\"", xaml, StringComparison.Ordinal);
            Assert.Contains("MinHeight=\"72\"", xaml, StringComparison.Ordinal);
            Assert.Contains("MaxHeight=\"128\"", xaml, StringComparison.Ordinal);
        }

        [Fact]
        public void KitEditWindow_UsesWrappingSummaryAndLowerSplitPressure()
        {
            var xaml = ReadRepoFile("InventoryManagementApp", "Views", "Windows", "KitEditWindow.xaml");

            Assert.Contains("Width=\"720\"", xaml, StringComparison.Ordinal);
            Assert.Contains("Height=\"580\"", xaml, StringComparison.Ordinal);
            Assert.Contains("MinWidth=\"600\"", xaml, StringComparison.Ordinal);
            Assert.Contains("MinHeight=\"500\"", xaml, StringComparison.Ordinal);
            Assert.Contains("<WrapPanel Grid.Row=\"1\" Margin=\"0,0,0,8\">", xaml, StringComparison.Ordinal);
            Assert.True(CountOccurrences(xaml, "Style=\"{StaticResource DesktopSummaryCard}\" MinWidth=\"155\" MaxWidth=\"225\"") >= 3);
            Assert.Contains("<ColumnDefinition Width=\"1.28*\" MinWidth=\"0\"/>", xaml, StringComparison.Ordinal);
            Assert.Contains("<ColumnDefinition Width=\"8\"/>", xaml, StringComparison.Ordinal);
            Assert.Contains("<ColumnDefinition Width=\"*\" MinWidth=\"0\"/>", xaml, StringComparison.Ordinal);
            Assert.DoesNotContain("<UniformGrid Grid.Row=\"1\" Columns=\"3\"", xaml, StringComparison.Ordinal);
            Assert.DoesNotContain("MinWidth=\"680\"", xaml, StringComparison.Ordinal);
        }

        [Fact]
        public void KitEditWindow_PreservesKitBindingsWithReachableScrollingAndSaveCancel()
        {
            var xaml = ReadRepoFile("InventoryManagementApp", "Views", "Windows", "KitEditWindow.xaml");
            var requiredContracts = new[]
            {
                "Kit.KitNumber",
                "Kit.Name",
                "Kit.Category",
                "Kit.IsActive",
                "Kit.Description",
                "<controls:SaveCancelBar Grid.Row=\"3\" Margin=\"0,8,0,0\"/>"
            };

            foreach (var contract in requiredContracts)
                Assert.Contains(contract, xaml, StringComparison.Ordinal);

            Assert.Contains("<ScrollViewer Grid.Row=\"1\" VerticalScrollBarVisibility=\"Auto\" HorizontalScrollBarVisibility=\"Disabled\">", xaml, StringComparison.Ordinal);
            Assert.Contains("<ColumnDefinition Width=\"110\"/>", xaml, StringComparison.Ordinal);
            Assert.Contains("MinHeight=\"150\"", xaml, StringComparison.Ordinal);
            Assert.Contains("HorizontalScrollBarVisibility=\"Disabled\"", xaml, StringComparison.Ordinal);
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
