using System;
using System.IO;
using Xunit;

namespace InventoryManagementApp.Tests.ViewModels
{
    public class PrintPreviewWindowXamlTests
    {
        [Fact]
        public void PrintPreviewWindow_UsesACleanDocumentFirstReviewShell()
        {
            var xaml = ReadRepositoryFile("InventoryManagementApp", "Views", "Windows", "PrintPreviewWindow.xaml");

            Assert.DoesNotContain("Preview Workstation", xaml, StringComparison.Ordinal);
            Assert.DoesNotContain("Document Canvas", xaml, StringComparison.Ordinal);
            Assert.DoesNotContain("Print checklist", xaml, StringComparison.Ordinal);
            Assert.DoesNotContain("Ready for final print review", xaml, StringComparison.Ordinal);
        }

        [Fact]
        public void PrintPreviewWindow_PreservesDocumentViewerAndPrintCommands()
        {
            var xaml = ReadRepositoryFile("InventoryManagementApp", "Views", "Windows", "PrintPreviewWindow.xaml");

            Assert.Contains("x:Name=\"PreviewLogo\"", xaml, StringComparison.Ordinal);
            Assert.Contains("x:Name=\"PreviewTitle\"", xaml, StringComparison.Ordinal);
            Assert.Contains("x:Name=\"DocViewer\"", xaml, StringComparison.Ordinal);
            Assert.Contains("PageSetupCommand", xaml, StringComparison.Ordinal);
            Assert.Contains("PrintCommand", xaml, StringComparison.Ordinal);
            Assert.Contains("CloseCommand", xaml, StringComparison.Ordinal);
        }

        [Fact]
        public void PrintPreviewWindow_UsesResponsiveMinimumsAndWrappingHeaderActions()
        {
            var xaml = ReadRepositoryFile("InventoryManagementApp", "Views", "Windows", "PrintPreviewWindow.xaml");

            Assert.Contains("Width=\"1040\" Height=\"720\"", xaml, StringComparison.Ordinal);
            Assert.Contains("MinWidth=\"720\" MinHeight=\"520\"", xaml, StringComparison.Ordinal);
            Assert.Contains("<ColumnDefinition Width=\"*\" MinWidth=\"0\"/>", xaml, StringComparison.Ordinal);
            Assert.Contains("<DockPanel Grid.Column=\"0\" LastChildFill=\"True\" MinWidth=\"0\"", xaml, StringComparison.Ordinal);
            Assert.Contains("<WrapPanel Grid.Column=\"1\" HorizontalAlignment=\"Right\" VerticalAlignment=\"Center\" MaxWidth=\"300\">", xaml, StringComparison.Ordinal);
            Assert.Contains("TextTrimming=\"CharacterEllipsis\"", xaml, StringComparison.Ordinal);
            Assert.DoesNotContain("Width=\"1220\" Height=\"820\"", xaml, StringComparison.Ordinal);
            Assert.DoesNotContain("MinWidth=\"980\" MinHeight=\"680\"", xaml, StringComparison.Ordinal);
            Assert.DoesNotContain("<StackPanel Orientation=\"Horizontal\" DockPanel.Dock=\"Right\"", xaml, StringComparison.Ordinal);
        }

        [Fact]
        public void PrintPreviewWindow_KeepsPreviewCanvasShrinkableAndScrollable()
        {
            var xaml = ReadRepositoryFile("InventoryManagementApp", "Views", "Windows", "PrintPreviewWindow.xaml");

            Assert.Contains("<ColumnDefinition Width=\"*\" MinWidth=\"0\"/>", xaml, StringComparison.Ordinal);
            Assert.Contains("Style=\"{StaticResource ThemedWindowPane}\"", xaml, StringComparison.Ordinal);
            Assert.DoesNotContain("<GridSplitter", xaml, StringComparison.Ordinal);
            Assert.Contains("HorizontalScrollBarVisibility=\"Auto\"", xaml, StringComparison.Ordinal);
            Assert.DoesNotContain("<ColumnDefinition Width=\"280\"/>", xaml, StringComparison.Ordinal);
        }

        [Fact]
        public void PrintPreviewWindow_DoesNotDuplicateActionsInAnInstructionPane()
        {
            var xaml = ReadRepositoryFile("InventoryManagementApp", "Views", "Windows", "PrintPreviewWindow.xaml");

            Assert.DoesNotContain("Text=\"Available actions\"", xaml, StringComparison.Ordinal);
            Assert.DoesNotContain("Text=\"Fast review\"", xaml, StringComparison.Ordinal);
            Assert.DoesNotContain("Text=\"Output Review\"", xaml, StringComparison.Ordinal);
            Assert.Contains("TextWrapping=\"Wrap\"", xaml, StringComparison.Ordinal);
            Assert.Contains("x:Name=\"PreviewFooterStatus\"", xaml, StringComparison.Ordinal);
        }

        [Fact]
        public void PrintPreviewWindow_AppliesSharedDocumentPolishToEveryPreview()
        {
            var codeBehind = ReadRepositoryFile("InventoryManagementApp", "Views", "Windows", "PrintPreviewWindow.xaml.cs");

            Assert.Contains("ApplyDocumentPolish(_document, _title)", codeBehind, StringComparison.Ordinal);
            Assert.Contains("PrintPolishHeader", codeBehind, StringComparison.Ordinal);
            Assert.Contains("Inventory Print Package", codeBehind, StringComparison.Ordinal);
            Assert.Contains("Prepared {DateTime.Now:g}", codeBehind, StringComparison.Ordinal);
            Assert.Contains("PrintPolishFooter", codeBehind, StringComparison.Ordinal);
            Assert.Contains("ApplyTablePolish", codeBehind, StringComparison.Ordinal);
            Assert.Contains("ApplyTablePolish(_document)", codeBehind, StringComparison.Ordinal);
            Assert.Contains("isKeyValueTable", codeBehind, StringComparison.Ordinal);
            Assert.Contains("table.Tag as string, \"KeyValue\"", codeBehind, StringComparison.Ordinal);
            Assert.Contains("else if (!isKeyValueTable && rowIndex % 2 == 0)", codeBehind, StringComparison.Ordinal);
            Assert.Contains("foreach (var rowGroup in table.RowGroups)", codeBehind, StringComparison.Ordinal);
            Assert.Contains("Generated from InventoryManagementApp print preview", codeBehind, StringComparison.Ordinal);
        }

        [Fact]
        public void PrintPreviewWindow_FitsPrintedDocumentToPrinterPrintableArea()
        {
            var codeBehind = ReadRepositoryFile("InventoryManagementApp", "Views", "Windows", "PrintPreviewWindow.xaml.cs");

            Assert.Contains("ConfigureDocumentForPage(_document, dlg.PrintableAreaWidth, dlg.PrintableAreaHeight)", codeBehind, StringComparison.Ordinal);
            Assert.Contains("paginator.PageSize = new Size(dlg.PrintableAreaWidth, dlg.PrintableAreaHeight)", codeBehind, StringComparison.Ordinal);
            Assert.Contains("document.ColumnWidth = contentWidth", codeBehind, StringComparison.Ordinal);
            Assert.Contains("RebalanceTableColumns(table, contentWidth)", codeBehind, StringComparison.Ordinal);
            Assert.Contains("safeContentWidth", codeBehind, StringComparison.Ordinal);
            Assert.Contains("table.Columns.Count == 2", codeBehind, StringComparison.Ordinal);
            Assert.Contains("safeContentWidth * 0.68", codeBehind, StringComparison.Ordinal);
            Assert.Contains(": 80", codeBehind, StringComparison.Ordinal);
            Assert.DoesNotContain("GridUnitType.Star", codeBehind, StringComparison.Ordinal);
        }

        static string ReadRepositoryFile(params string[] relativePathParts)
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
