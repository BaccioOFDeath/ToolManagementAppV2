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

            Assert.Contains("Width=\"760\" Height=\"520\"", xaml, StringComparison.Ordinal);
            Assert.Contains("MinWidth=\"560\" MinHeight=\"420\"", xaml, StringComparison.Ordinal);
            Assert.Contains("this.UseResponsiveDefaultSize(760, 520);", codeBehind, StringComparison.Ordinal);
            Assert.Contains("<WrapPanel>", xaml, StringComparison.Ordinal);
            Assert.Contains("EnableRowVirtualization=\"True\"", xaml, StringComparison.Ordinal);
            Assert.Contains("EnableColumnVirtualization=\"True\"", xaml, StringComparison.Ordinal);
            Assert.Contains("SelectionUnit=\"FullRow\"", xaml, StringComparison.Ordinal);
            Assert.Contains("ScrollViewer.CanContentScroll=\"True\"", xaml, StringComparison.Ordinal);
            Assert.Contains("ScrollViewer.HorizontalScrollBarVisibility=\"Auto\"", xaml, StringComparison.Ordinal);
            Assert.Contains("Width=\"2*\" MinWidth=\"140\"", xaml, StringComparison.Ordinal);
            Assert.Contains("QueueStatusText", xaml, StringComparison.Ordinal);
            Assert.Contains("LabelActionStatusText", xaml, StringComparison.Ordinal);
            Assert.Contains("EmptyQueueVisibility", xaml, StringComparison.Ordinal);
            Assert.Contains("No labels queued", xaml, StringComparison.Ordinal);
            Assert.Contains("PreviewCommand", xaml, StringComparison.Ordinal);
            Assert.Contains("PrintCommand", xaml, StringComparison.Ordinal);
            Assert.Contains("CloseCommand", xaml, StringComparison.Ordinal);
            Assert.DoesNotContain("<StackPanel Orientation=\"Horizontal\" HorizontalAlignment=\"Right\">", xaml, StringComparison.Ordinal);
        }

        [Fact]
        public void PrintLabelViewModel_BoundsPreviewAndPrintsProfessionalLabelSheets()
        {
            var source = ReadRepositoryFile("InventoryManagementApp", "ViewModels", "PrintLabelViewModel.cs");

            Assert.Contains("private const int MaxPrintableLabels = 250;", source, StringComparison.Ordinal);
            Assert.Contains("public bool HasItems => Items.Count > 0;", source, StringComparison.Ordinal);
            Assert.Contains("public Visibility EmptyQueueVisibility => HasItems ? Visibility.Collapsed : Visibility.Visible;", source, StringComparison.Ordinal);
            Assert.Contains("public int VisibleLabelCount => Math.Min(Items.Count, MaxPrintableLabels);", source, StringComparison.Ordinal);
            Assert.Contains("public int OmittedLabelCount => Math.Max(0, Items.Count - MaxPrintableLabels);", source, StringComparison.Ordinal);
            Assert.Contains("PreviewCommand = new RelayCommand(Preview, () => CanGenerateLabels);", source, StringComparison.Ordinal);
            Assert.Contains("PrintCommand = new RelayCommand(Print, () => CanGenerateLabels);", source, StringComparison.Ordinal);
            Assert.Contains("Items.CollectionChanged += Items_CollectionChanged;", source, StringComparison.Ordinal);
            Assert.Contains("PreviewCommand.NotifyCanExecuteChanged();", source, StringComparison.Ordinal);
            Assert.Contains("PrintCommand.NotifyCanExecuteChanged();", source, StringComparison.Ordinal);
            Assert.Contains(".Take(MaxPrintableLabels)", source, StringComparison.Ordinal);
            Assert.Contains("SelectedTemplate, \"Compact\", StringComparison.OrdinalIgnoreCase) ? 3 : 2", source, StringComparison.Ordinal);
            Assert.Contains("new TableColumn { Width = new GridLength(1, GridUnitType.Star) }", source, StringComparison.Ordinal);
            Assert.Contains("PrintDocumentTheme.PageBackgroundBrush", source, StringComparison.Ordinal);
            Assert.Contains("PrintDocumentTheme.RuleBorderBrush", source, StringComparison.Ordinal);
            Assert.Contains("Prepared {DateTime.Now:g}", source, StringComparison.Ordinal);
            Assert.Contains("Large queue note", source, StringComparison.Ordinal);
            Assert.Contains("No label rows are queued", source, StringComparison.Ordinal);
            Assert.Contains("Normalize(item.ItemNumber, \"Unnumbered item\")", source, StringComparison.Ordinal);
            Assert.Contains("Normalize(item.Name, \"Unnamed item\")", source, StringComparison.Ordinal);
            Assert.Contains("Normalize(item.Location, \"No location\")", source, StringComparison.Ordinal);
            Assert.Contains("PrintDocumentTheme.ApplyLightTheme(doc);", source, StringComparison.Ordinal);
            Assert.DoesNotContain("new BlockUIContainer", source, StringComparison.Ordinal);
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
            Assert.Contains("PreviewMouseWheel=\"MappingComboBox_PreviewMouseWheel\"", xaml, StringComparison.Ordinal);
            Assert.Contains("private void MappingComboBox_PreviewMouseWheel(object sender, MouseWheelEventArgs e)", codeBehind, StringComparison.Ordinal);
            Assert.Contains("sender is not ComboBox { IsDropDownOpen: false }", codeBehind, StringComparison.Ordinal);
            Assert.Contains("e.Handled = true;", codeBehind, StringComparison.Ordinal);
            Assert.Contains("FindAncestor<DataGrid>(comboBox)", codeBehind, StringComparison.Ordinal);
            Assert.Contains("RoutedEvent = UIElement.MouseWheelEvent", codeBehind, StringComparison.Ordinal);
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
            return NormalizeLineEndings(File.ReadAllText(path));
        }
        static string NormalizeLineEndings(string text)
            => text.Replace("\r\n", "\n");

    }
}
