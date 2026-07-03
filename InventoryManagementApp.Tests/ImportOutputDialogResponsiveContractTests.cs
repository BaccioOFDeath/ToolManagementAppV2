using System;
using System.IO;
using Xunit;

namespace InventoryManagementApp.Tests
{
    public class ImportOutputDialogResponsiveContractTests
    {
        [Fact]
        public void PrintLabelWindow_UsesResponsiveQueueReviewLayout()
        {
            var xaml = ReadRepositoryFile("InventoryManagementApp", "Views", "Windows", "PrintLabelWindow.xaml");
            var codeBehind = ReadRepositoryFile("InventoryManagementApp", "Views", "Windows", "PrintLabelWindow.xaml.cs");

            Assert.Contains("Width=\"820\" Height=\"540\"", xaml, StringComparison.Ordinal);
            Assert.Contains("MinWidth=\"700\" MinHeight=\"480\"", xaml, StringComparison.Ordinal);
            Assert.Contains("this.UseResponsiveDefaultSize(820, 540);", codeBehind, StringComparison.Ordinal);
            Assert.Contains("<WrapPanel>", xaml, StringComparison.Ordinal);
            Assert.Contains("EnableRowVirtualization=\"True\"", xaml, StringComparison.Ordinal);
            Assert.Contains("EnableColumnVirtualization=\"True\"", xaml, StringComparison.Ordinal);
            Assert.Contains("SelectionUnit=\"FullRow\"", xaml, StringComparison.Ordinal);
            Assert.Contains("ScrollViewer.CanContentScroll=\"True\"", xaml, StringComparison.Ordinal);
            Assert.Contains("ScrollViewer.HorizontalScrollBarVisibility=\"Auto\"", xaml, StringComparison.Ordinal);
            Assert.Contains("Width=\"1.2*\" MinWidth=\"140\"", xaml, StringComparison.Ordinal);
            Assert.Contains("PreviewCommand", xaml, StringComparison.Ordinal);
            Assert.Contains("PrintCommand", xaml, StringComparison.Ordinal);
            Assert.Contains("CloseCommand", xaml, StringComparison.Ordinal);
            Assert.DoesNotContain("<StackPanel Orientation=\"Horizontal\" HorizontalAlignment=\"Right\">", xaml, StringComparison.Ordinal);
        }

        [Fact]
        public void ImportMappingWindow_UsesResponsiveVirtualizedMappingTable()
        {
            var xaml = ReadRepositoryFile("InventoryManagementApp", "Views", "Windows", "ImportMappingWindow.xaml");
            var codeBehind = ReadRepositoryFile("InventoryManagementApp", "Views", "Windows", "ImportMappingWindow.xaml.cs");

            Assert.Contains("Width=\"980\" Height=\"700\"", xaml, StringComparison.Ordinal);
            Assert.Contains("MinWidth=\"760\" MinHeight=\"560\"", xaml, StringComparison.Ordinal);
            Assert.Contains("this.UseResponsiveDefaultSize(980, 700);", codeBehind, StringComparison.Ordinal);
            Assert.Contains("<WrapPanel Grid.Row=\"1\"", xaml, StringComparison.Ordinal);
            Assert.Contains("MinWidth=\"180\" MaxWidth=\"280\"", xaml, StringComparison.Ordinal);
            Assert.Contains("EnableRowVirtualization=\"True\"", xaml, StringComparison.Ordinal);
            Assert.Contains("EnableColumnVirtualization=\"True\"", xaml, StringComparison.Ordinal);
            Assert.Contains("SelectionUnit=\"FullRow\"", xaml, StringComparison.Ordinal);
            Assert.Contains("ScrollViewer.VerticalScrollBarVisibility=\"Auto\"", xaml, StringComparison.Ordinal);
            Assert.Contains("MaxDropDownHeight=\"320\"", xaml, StringComparison.Ordinal);
            Assert.Contains("ScrollViewer.HorizontalScrollBarVisibility=\"Disabled\"", xaml, StringComparison.Ordinal);
            Assert.Contains("DataContext.ColumnHeaders", xaml, StringComparison.Ordinal);
            Assert.Contains("SelectedColumn, Mode=TwoWay, UpdateSourceTrigger=PropertyChanged", xaml, StringComparison.Ordinal);
            Assert.Contains("CancelCommand", xaml, StringComparison.Ordinal);
            Assert.Contains("OkCommand", xaml, StringComparison.Ordinal);
            Assert.DoesNotContain("<UniformGrid Grid.Row=\"1\" Columns=\"3\"", xaml, StringComparison.Ordinal);
        }

        [Fact]
        public void ImageImportMappingWindow_UsesResponsiveIdentifierSetupLayout()
        {
            var xaml = ReadRepositoryFile("InventoryManagementApp", "Views", "Windows", "ImageImportMappingWindow.xaml");
            var codeBehind = ReadRepositoryFile("InventoryManagementApp", "Views", "Windows", "ImageImportMappingWindow.xaml.cs");

            Assert.Contains("Width=\"720\" Height=\"620\"", xaml, StringComparison.Ordinal);
            Assert.Contains("MinWidth=\"600\" MinHeight=\"500\"", xaml, StringComparison.Ordinal);
            Assert.Contains("this.UseResponsiveDefaultSize(720, 620);", codeBehind, StringComparison.Ordinal);
            Assert.Contains("<WrapPanel Grid.Row=\"1\"", xaml, StringComparison.Ordinal);
            Assert.Contains("MinWidth=\"170\" MaxWidth=\"250\"", xaml, StringComparison.Ordinal);
            Assert.Contains("<ColumnDefinition Width=\"1.25*\" MinWidth=\"0\"/>", xaml, StringComparison.Ordinal);
            Assert.Contains("<ColumnDefinition Width=\"8\"/>", xaml, StringComparison.Ordinal);
            Assert.Contains("HorizontalScrollBarVisibility=\"Disabled\"", xaml, StringComparison.Ordinal);
            Assert.Contains("UseItemNumber", xaml, StringComparison.Ordinal);
            Assert.Contains("UsePartNumber", xaml, StringComparison.Ordinal);
            Assert.Contains("UseName", xaml, StringComparison.Ordinal);
            Assert.Contains("CancelCommand", xaml, StringComparison.Ordinal);
            Assert.Contains("OkCommand", xaml, StringComparison.Ordinal);
            Assert.DoesNotContain("<UniformGrid Grid.Row=\"1\" Columns=\"3\"", xaml, StringComparison.Ordinal);
        }

        private static string ReadRepositoryFile(params string[] relativePathParts)
        {
            var directory = new DirectoryInfo(AppContext.BaseDirectory);
            while (directory != null && !File.Exists(Path.Combine(directory.FullName, "InventoryManagementApp.sln")))
                directory = directory.Parent;

            Assert.NotNull(directory);
            var path = Path.Combine(directory!.FullName, Path.Combine(relativePathParts));
            Assert.True(File.Exists(path), $"Expected repository file at {path}");
            return File.ReadAllText(path);
        }
    }
}
