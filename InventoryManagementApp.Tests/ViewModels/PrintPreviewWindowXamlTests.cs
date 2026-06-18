using System;
using System.IO;
using Xunit;

namespace InventoryManagementApp.Tests.ViewModels
{
    public class PrintPreviewWindowXamlTests
    {
        [Fact]
        public void PrintPreviewWindow_UsesPolishedReviewShellAndStatusFooter()
        {
            var xaml = ReadRepositoryFile("InventoryManagementApp", "Views", "Windows", "PrintPreviewWindow.xaml");

            Assert.Contains("Preview Workstation", xaml, StringComparison.Ordinal);
            Assert.Contains("Document Canvas", xaml, StringComparison.Ordinal);
            Assert.Contains("Print checklist", xaml, StringComparison.Ordinal);
            Assert.Contains("Branding confidence", xaml, StringComparison.Ordinal);
            Assert.Contains("Ready for final print review", xaml, StringComparison.Ordinal);
            Assert.Contains("Preview footer status", xaml, StringComparison.Ordinal);
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
        public void PrintPreviewWindow_AppliesSharedDocumentPolishToEveryPreview()
        {
            var codeBehind = ReadRepositoryFile("InventoryManagementApp", "Views", "Windows", "PrintPreviewWindow.xaml.cs");

            Assert.Contains("ApplyDocumentPolish(_document, _title)", codeBehind, StringComparison.Ordinal);
            Assert.Contains("PrintPolishHeader", codeBehind, StringComparison.Ordinal);
            Assert.Contains("Inventory Print Package", codeBehind, StringComparison.Ordinal);
            Assert.Contains("Prepared {DateTime.Now:g}", codeBehind, StringComparison.Ordinal);
            Assert.Contains("PrintPolishFooter", codeBehind, StringComparison.Ordinal);
            Assert.Contains("ApplyTablePolish", codeBehind, StringComparison.Ordinal);
            Assert.Contains("TableRowGroup", codeBehind, StringComparison.Ordinal);
            Assert.Contains("Generated from InventoryManagementApp print preview", codeBehind, StringComparison.Ordinal);
        }

        static string ReadRepositoryFile(params string[] relativePathParts)
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
