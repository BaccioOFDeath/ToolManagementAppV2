using System;
using System.IO;
using Xunit;

namespace InventoryManagementApp.Tests
{
    public class AdminDataPrintPreviewRouteTests
    {
        [Theory]
        [InlineData("InventoryManagementApp", "Views", "Pages", "UsersPage.xaml.cs")]
        [InlineData("InventoryManagementApp", "Views", "Pages", "CategoriesPage.xaml.cs")]
        [InlineData("InventoryManagementApp", "Views", "Pages", "ImportExportPage.xaml.cs")]
        public void AdminDataPagePrintActionsUseSharedPreviewWindow(params string[] path)
        {
            var source = ReadRepoFile(path);

            Assert.Contains("PrintPreviewWindow", source, StringComparison.Ordinal);
            Assert.Contains("ShowPreview(document", source, StringComparison.Ordinal);
            Assert.DoesNotContain("WpfPrintDialog", source, StringComparison.Ordinal);
            Assert.DoesNotContain("new PrintDialog", source, StringComparison.Ordinal);
            Assert.DoesNotContain("PrintDocument(((IDocumentPaginatorSource)document).DocumentPaginator", source, StringComparison.Ordinal);
        }

        [Theory]
        [InlineData("InventoryManagementApp", "ViewModels", "CustomerManagementViewModel.cs")]
        [InlineData("InventoryManagementApp", "ViewModels", "KitManagementViewModel.cs")]
        public void AdminDataViewModelPrintActionsUseDialogPreviewService(params string[] path)
        {
            var source = ReadRepoFile(path);

            Assert.Contains("_dialogService.ShowPrintPreview(doc", source, StringComparison.Ordinal);
            Assert.DoesNotContain("WpfPrintDialog", source, StringComparison.Ordinal);
            Assert.DoesNotContain("new PrintDialog", source, StringComparison.Ordinal);
            Assert.DoesNotContain("PrintDocument(((IDocumentPaginatorSource)document).DocumentPaginator", source, StringComparison.Ordinal);
        }

        [Fact]
        public void DashboardPrintActionsUseDialogPreviewService()
        {
            var source = ReadRepoFile("InventoryManagementApp", "ViewModels", "DashboardViewModel.cs");

            Assert.Contains("IDialogService? dialogService", source, StringComparison.Ordinal);
            Assert.Contains("var dialogService = _dialogService", source, StringComparison.Ordinal);
            Assert.Contains("Host.Services.GetService<IDialogService>()", source, StringComparison.Ordinal);
            Assert.Contains("dialogService.ShowPrintPreview(document, title, description)", source, StringComparison.Ordinal);
            Assert.Contains("Dashboard checked-out item handoff", source, StringComparison.Ordinal);
            Assert.Contains("Dashboard operations snapshot", source, StringComparison.Ordinal);
            Assert.DoesNotContain("new System.Windows.Controls.PrintDialog", source, StringComparison.Ordinal);
            Assert.DoesNotContain("new PrintDialog", source, StringComparison.Ordinal);
            Assert.DoesNotContain("PrintDocument(((System.Windows.Documents.IDocumentPaginatorSource)doc).DocumentPaginator", source, StringComparison.Ordinal);
        }

        [Fact]
        public void CategoriesKeepBothDirectoryAndSelectedSheetPreviewRoutes()
        {
            var source = ReadRepoFile("InventoryManagementApp", "Views", "Pages", "CategoriesPage.xaml.cs");

            Assert.Contains("ShowPrintPreview(document, \"Category Directory\")", source, StringComparison.Ordinal);
            Assert.Contains("ShowPrintPreview(document, $\"Category Sheet - {category.Name}\")", source, StringComparison.Ordinal);
        }

        [Fact]
        public void CustomerAndKitOutputsKeepDirectoryAndSelectedSheetPreviewRoutes()
        {
            var customerSource = ReadRepoFile("InventoryManagementApp", "ViewModels", "CustomerManagementViewModel.cs");
            var kitSource = ReadRepoFile("InventoryManagementApp", "ViewModels", "KitManagementViewModel.cs");

            Assert.Contains("_dialogService.ShowPrintPreview(doc, \"Customer Directory\"", customerSource, StringComparison.Ordinal);
            Assert.Contains("CreateCustomerDocument($\"Customer Sheet - {ValueOrNotRecorded(customer.Company)}\")", customerSource, StringComparison.Ordinal);
            Assert.Contains("_dialogService.ShowPrintPreview(doc, \"Kit Directory\"", kitSource, StringComparison.Ordinal);
            Assert.Contains("CreateKitDocument($\"Kit Pick Sheet - {ValueOrNotRecorded(kit.Name)}\")", kitSource, StringComparison.Ordinal);
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