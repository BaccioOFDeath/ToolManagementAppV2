using System;
using System.IO;
using System.Linq;
using InventoryManagementApp.Models.Domain;
using InventoryManagementApp.ViewModels;
using Xunit;

namespace InventoryManagementApp.Tests
{
    public class ImageImportMappingWindowResponsiveContractTests
    {
        [Fact]
        public void ImageImportMappingWindow_UsesCompactResponsiveSizingAndRootBounds()
        {
            var xaml = ReadRepoFile("InventoryManagementApp", "Views", "Windows", "ImageImportMappingWindow.xaml");
            var codeBehind = ReadRepoFile("InventoryManagementApp", "Views", "Windows", "ImageImportMappingWindow.xaml.cs");

            Assert.Contains("Width=\"680\" Height=\"560\"", xaml, StringComparison.Ordinal);
            Assert.Contains("MinWidth=\"520\" MinHeight=\"420\"", xaml, StringComparison.Ordinal);
            Assert.Contains("x:Name=\"ImageImportMappingRoot\" Margin=\"10\" MinWidth=\"0\" ClipToBounds=\"True\"", xaml, StringComparison.Ordinal);
            Assert.Contains("this.UseResponsiveDefaultSize(680, 560);", codeBehind, StringComparison.Ordinal);
            Assert.DoesNotContain("Width=\"720\" Height=\"620\"", xaml, StringComparison.Ordinal);
            Assert.DoesNotContain("MinWidth=\"600\" MinHeight=\"500\"", xaml, StringComparison.Ordinal);
            Assert.DoesNotContain("this.UseResponsiveDefaultSize(720, 620);", codeBehind, StringComparison.Ordinal);
        }

        [Fact]
        public void ImageImportMappingWindow_BoundsHeaderAndSummaryCardsForScaledDesktop()
        {
            var xaml = ReadRepoFile("InventoryManagementApp", "Views", "Windows", "ImageImportMappingWindow.xaml");

            Assert.Contains("<Border Grid.Row=\"0\" Style=\"{StaticResource DesktopPaneHeader}\" Margin=\"0,0,0,8\" MinWidth=\"0\">", xaml, StringComparison.Ordinal);
            Assert.Contains("<StackPanel MinWidth=\"0\" MaxWidth=\"480\">", xaml, StringComparison.Ordinal);
            Assert.Contains("MaxWidth=\"150\"", xaml, StringComparison.Ordinal);
            Assert.True(CountOccurrences(xaml, "MinWidth=\"145\" MaxWidth=\"220\"") >= 3);
            Assert.DoesNotContain("MaxWidth=\"170\"", xaml, StringComparison.Ordinal);
            Assert.DoesNotContain("MinWidth=\"170\" MaxWidth=\"250\"", xaml, StringComparison.Ordinal);
        }

        [Fact]
        public void ImageImportMappingWindow_UsesScrollableWrappingRuleCardsInsteadOfFixedColumns()
        {
            var xaml = NormalizeNewlines(ReadRepoFile("InventoryManagementApp", "Views", "Windows", "ImageImportMappingWindow.xaml"));

            Assert.Contains("<ScrollViewer Grid.Row=\"1\" VerticalScrollBarVisibility=\"Auto\" HorizontalScrollBarVisibility=\"Disabled\">", xaml, StringComparison.Ordinal);
            Assert.Contains("<WrapPanel Margin=\"10\">", xaml, StringComparison.Ordinal);
            Assert.True(CountOccurrences(xaml, "MinWidth=\"220\" MaxWidth=\"320\"") >= 3);
            Assert.Contains("Mapping Readiness", xaml, StringComparison.Ordinal);
            Assert.DoesNotContain("<ColumnDefinition Width=\"1.25*\" MinWidth=\"0\"/>", xaml, StringComparison.Ordinal);
            Assert.DoesNotContain("<ColumnDefinition Width=\"8\"/>", xaml, StringComparison.Ordinal);
            Assert.DoesNotContain("<Border Grid.Column=\"2\" Style=\"{StaticResource AdminHandoffCard}\"", xaml, StringComparison.Ordinal);
        }

        [Fact]
        public void ImageImportMappingWindow_ShowsReadinessStatusInBodyAndFooter()
        {
            var xaml = ReadRepoFile("InventoryManagementApp", "Views", "Windows", "ImageImportMappingWindow.xaml");

            Assert.Contains("Text=\"{Binding SelectedRuleCount, StringFormat='{}{0} rules selected'}\"", xaml, StringComparison.Ordinal);
            Assert.True(CountOccurrences(xaml, "Text=\"{Binding MappingReadinessText}\"") >= 2);
            Assert.Contains("RightCommand=\"{Binding OkCommand}\"", xaml, StringComparison.Ordinal);
            Assert.Contains("MaxWidth=\"500\"", xaml, StringComparison.Ordinal);
            Assert.DoesNotContain("Image matching setup ready", xaml, StringComparison.Ordinal);
            Assert.DoesNotContain("MaxWidth=\"560\"", xaml, StringComparison.Ordinal);
        }

        [Fact]
        public void ImageImportMappingViewModel_GatesOkCommandUntilRuleIsSelected()
        {
            var vm = new ImageImportMappingViewModel(() => { }, () => { });

            Assert.True(vm.CanConfirmMapping);
            Assert.Equal(1, vm.SelectedRuleCount);
            Assert.True(vm.OkCommand.CanExecute(null));

            vm.UseItemNumber = false;

            Assert.False(vm.CanConfirmMapping);
            Assert.Equal(0, vm.SelectedRuleCount);
            Assert.False(vm.OkCommand.CanExecute(null));
            Assert.Contains("Choose at least one", vm.MappingReadinessText, StringComparison.Ordinal);

            vm.UseName = true;

            Assert.True(vm.CanConfirmMapping);
            Assert.Equal(1, vm.SelectedRuleCount);
            Assert.True(vm.OkCommand.CanExecute(null));
            Assert.Contains("Ready with 1", vm.MappingReadinessText, StringComparison.Ordinal);
        }

        [Fact]
        public void ImageImportMappingViewModel_ReportsPluralRuleReadinessAndPreservesSelectorNormalization()
        {
            var vm = new ImageImportMappingViewModel(() => { }, () => { })
            {
                UsePartNumber = true,
                UseName = true
            };

            Assert.Equal(3, vm.SelectedRuleCount);
            Assert.Contains("3 filename matching rules", vm.MappingReadinessText, StringComparison.Ordinal);

            var keys = vm.BuildSelector()(new ItemModel
            {
                ItemNumber = " item-7 ",
                PartNumber = " pn-44 ",
                Name = " pump kit "
            }).ToArray();

            Assert.Equal(new[] { "ITEM-7", "PN-44", "PUMP KIT" }, keys);
        }

        [Fact]
        public void ImageImportMappingViewModel_RaisesReadinessNotificationsWhenRulesChange()
        {
            var vm = new ImageImportMappingViewModel(() => { }, () => { });
            var notifications = string.Empty;
            vm.PropertyChanged += (_, e) => notifications += $"|{e.PropertyName}";

            vm.UseItemNumber = false;

            Assert.Contains("|SelectedRuleCount", notifications, StringComparison.Ordinal);
            Assert.Contains("|CanConfirmMapping", notifications, StringComparison.Ordinal);
            Assert.Contains("|MappingReadinessText", notifications, StringComparison.Ordinal);
        }

        private static string NormalizeNewlines(string value) => value.Replace("\r\n", "\n", StringComparison.Ordinal);

        private static int CountOccurrences(string source, string value)
        {
            var count = 0;
            var index = 0;
            while ((index = source.IndexOf(value, index, StringComparison.Ordinal)) >= 0)
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