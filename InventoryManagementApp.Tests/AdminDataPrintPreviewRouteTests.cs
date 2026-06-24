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
        public void PrintPreviewDescriptionIsNotTreatedAsLogoPath()
        {
            var dialogService = ReadRepoFile("InventoryManagementApp", "Services", "DialogService.cs");
            var previewSource = ReadRepoFile("InventoryManagementApp", "Views", "Windows", "PrintPreviewWindow.xaml.cs");
            var previewXaml = ReadRepoFile("InventoryManagementApp", "Views", "Windows", "PrintPreviewWindow.xaml");

            Assert.Contains("ShowPreview(document, title, description)", dialogService, StringComparison.Ordinal);
            Assert.Contains("string? description = null, string? logoPath = null", previewSource, StringComparison.Ordinal);
            Assert.Contains("PreviewDescription.Text = _description", previewSource, StringComparison.Ordinal);
            Assert.Contains("_logoPath = logoPath ?? string.Empty", previewSource, StringComparison.Ordinal);
            Assert.Contains("ResolveLogoUri(_logoPath)", previewSource, StringComparison.Ordinal);
            Assert.Contains("x:Name=\"PreviewDescription\"", previewXaml, StringComparison.Ordinal);
        }

        [Fact]
        public void RentalPrintHandoffUsesUiThreadPreviewPath()
        {
            var dialogService = ReadRepoFile("InventoryManagementApp", "Services", "DialogService.cs");
            var itemDetails = ReadRepoFile("InventoryManagementApp", "ViewModels", "ItemDetailsViewModel.cs");
            var itemManagement = ReadRepoFile("InventoryManagementApp", "ViewModels", "ItemManagementViewModel.cs");

            Assert.Contains("return InvokeOnDispatcher(() => ShowRentItemDialogCore(item, customers)", dialogService, StringComparison.Ordinal);
            Assert.Contains("InvokeOnDispatcher(() => ShowPrintPreviewCore(document, title, description))", dialogService, StringComparison.Ordinal);
            Assert.Contains("dispatcher.Invoke(factory)", dialogService, StringComparison.Ordinal);

            Assert.Contains("await PromptToPrintRentalHandoffAsync(customer, dueDate);", itemDetails, StringComparison.Ordinal);
            Assert.DoesNotContain("await PromptToPrintRentalHandoffAsync(customer, dueDate).ConfigureAwait(false)", itemDetails, StringComparison.Ordinal);
            Assert.DoesNotContain("await FindNewActiveRentalAsync(customer, dueDate).ConfigureAwait(false)", itemDetails, StringComparison.Ordinal);

            Assert.Contains("await PromptToPrintRentalHandoffAsync(item, customer, dueDate);", itemManagement, StringComparison.Ordinal);
            Assert.DoesNotContain("await PromptToPrintRentalHandoffAsync(item, customer, dueDate).ConfigureAwait(false)", itemManagement, StringComparison.Ordinal);
            Assert.DoesNotContain("await FindNewActiveRentalAsync(item, customer, dueDate).ConfigureAwait(false)", itemManagement, StringComparison.Ordinal);
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
