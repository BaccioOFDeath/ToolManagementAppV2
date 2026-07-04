using System;
using System.IO;
using Xunit;

namespace InventoryManagementApp.Tests
{
    public class CategoriesPageResponsiveContractTests
    {
        [Fact]
        public void CategoriesPage_KeepsCategorySummaryCardsWrappedAndBounded()
        {
            var xaml = ReadRepoFile("InventoryManagementApp", "Views", "Pages", "CategoriesPage.xaml");

            Assert.Contains("<WrapPanel Grid.Column=\"2\" HorizontalAlignment=\"Right\">", xaml, StringComparison.Ordinal);
            Assert.Contains("<Setter Property=\"MinWidth\" Value=\"150\"/>", xaml, StringComparison.Ordinal);
            Assert.Contains("<Setter Property=\"MaxWidth\" Value=\"235\"/>", xaml, StringComparison.Ordinal);
            Assert.Contains("CategoryStatValueText", xaml, StringComparison.Ordinal);
            Assert.Contains("<ColumnDefinition Width=\"1.15*\" MinWidth=\"0\"/>", xaml, StringComparison.Ordinal);
            Assert.Contains("PRINT", xaml, StringComparison.Ordinal);
            Assert.Contains("CategoryPrintSummary", xaml, StringComparison.Ordinal);
            Assert.DoesNotContain("<UniformGrid Grid.Column=\"2\" Columns=\"4\">", xaml, StringComparison.Ordinal);
            Assert.DoesNotContain("<ColumnDefinition Width=\"2*\" MinWidth=\"380\"/>", xaml, StringComparison.Ordinal);
            Assert.DoesNotContain("<ColumnDefinition Width=\"3*\" MinWidth=\"520\"/>", xaml, StringComparison.Ordinal);
        }

        [Fact]
        public void CategoriesPage_AvoidsLargeFixedMinimumsInMainCategorySplit()
        {
            var xaml = ReadRepoFile("InventoryManagementApp", "Views", "Pages", "CategoriesPage.xaml");

            Assert.Contains("<ColumnDefinition Width=\"1.55*\" MinWidth=\"0\"/>", xaml, StringComparison.Ordinal);
            Assert.Contains("<ColumnDefinition Width=\"0.95*\" MinWidth=\"300\"/>", xaml, StringComparison.Ordinal);
            Assert.Contains("<GridSplitter Grid.Column=\"1\" Width=\"6\"", xaml, StringComparison.Ordinal);
            Assert.Contains("<Border Grid.Column=\"0\" Style=\"{StaticResource Card}\" Padding=\"0\" MinWidth=\"0\">", xaml, StringComparison.Ordinal);
            Assert.Contains("<Border Grid.Column=\"2\" Style=\"{StaticResource Card}\" Padding=\"0\" MinWidth=\"0\">", xaml, StringComparison.Ordinal);
            Assert.DoesNotContain("<ColumnDefinition Width=\"2.35*\" MinWidth=\"620\"/>", xaml, StringComparison.Ordinal);
            Assert.DoesNotContain("<ColumnDefinition Width=\"430\" MinWidth=\"380\"/>", xaml, StringComparison.Ordinal);
        }

        [Fact]
        public void CategoriesPage_EnablesDirectoryGridVirtualizationScrollingAndFullRowSelection()
        {
            var xaml = ReadRepoFile("InventoryManagementApp", "Views", "Pages", "CategoriesPage.xaml");

            Assert.Contains("x:Name=\"CategoryGrid\"", xaml, StringComparison.Ordinal);
            Assert.Contains("EnableRowVirtualization=\"True\"", xaml, StringComparison.Ordinal);
            Assert.Contains("EnableColumnVirtualization=\"True\"", xaml, StringComparison.Ordinal);
            Assert.Contains("SelectionMode=\"Single\"", xaml, StringComparison.Ordinal);
            Assert.Contains("SelectionUnit=\"FullRow\"", xaml, StringComparison.Ordinal);
            Assert.Contains("ScrollViewer.CanContentScroll=\"True\"", xaml, StringComparison.Ordinal);
            Assert.Contains("ScrollViewer.HorizontalScrollBarVisibility=\"Auto\"", xaml, StringComparison.Ordinal);
            Assert.Contains("ScrollViewer.VerticalScrollBarVisibility=\"Auto\"", xaml, StringComparison.Ordinal);
        }

        [Fact]
        public void CategoriesPage_BoundsInputsEmptyStateLoadingStateAndHandoffScrolling()
        {
            var xaml = ReadRepoFile("InventoryManagementApp", "Views", "Pages", "CategoriesPage.xaml");

            Assert.Contains("<TextBox x:Name=\"CategoryNameBox\"", xaml, StringComparison.Ordinal);
            Assert.Contains("Width=\"250\"", xaml, StringComparison.Ordinal);
            Assert.Contains("MinWidth=\"190\"", xaml, StringComparison.Ordinal);
            Assert.Contains("Visibility=\"{Binding IsCategoryEmptyStateVisible, Converter={StaticResource BoolToVis}}\"", xaml, StringComparison.Ordinal);
            Assert.Contains("CategoryEmptyStateTitle", xaml, StringComparison.Ordinal);
            Assert.Contains("CategoryEmptyStateMessage", xaml, StringComparison.Ordinal);
            Assert.Contains("Visibility=\"{Binding IsCategoryInteractionBusy, Converter={StaticResource BoolToVis}}\"", xaml, StringComparison.Ordinal);
            Assert.Contains("Loading category rows", xaml, StringComparison.Ordinal);
            Assert.Contains("<ScrollViewer Grid.Row=\"1\" VerticalScrollBarVisibility=\"Auto\" HorizontalScrollBarVisibility=\"Disabled\">", xaml, StringComparison.Ordinal);
            Assert.DoesNotContain("<Border Grid.Row=\"2\" Width=\"340\"", xaml, StringComparison.Ordinal);
            Assert.DoesNotContain("VerticalScrollBarVisibility=\"Hidden\"", xaml, StringComparison.Ordinal);
        }

        [Fact]
        public void CategoriesPage_DisablesDirectoryPrintWhileRowsAreNotReady()
        {
            var xaml = ReadRepoFile("InventoryManagementApp", "Views", "Pages", "CategoriesPage.xaml");

            Assert.Contains("Content=\"Print Directory\" Click=\"PrintCategories_Click\" IsEnabled=\"{Binding IsDirectoryPrintAvailable}\"", xaml, StringComparison.Ordinal);
            Assert.Contains("Header=\"Print Current Directory\" Click=\"PrintCategories_Click\" IsEnabled=\"{Binding IsDirectoryPrintAvailable}\"", xaml, StringComparison.Ordinal);
            Assert.Contains("CategoryPrintSummary", xaml, StringComparison.Ordinal);
        }

        [Fact]
        public void CategoryViewModel_GuardsLoadingCommandsAndProfessionalDirectoryState()
        {
            var source = ReadRepoFile("InventoryManagementApp", "ViewModels", "CategoryManagementViewModel.cs");

            Assert.Contains("public bool IsCategoryInteractionBusy => IsBusy;", source, StringComparison.Ordinal);
            Assert.Contains("public bool IsDirectoryPrintAvailable => !IsCategoryInteractionBusy && FilteredCategories.Count > 0;", source, StringComparison.Ordinal);
            Assert.Contains("public bool IsCategoryEmptyStateVisible => !IsCategoryInteractionBusy && FilteredCategories.Count == 0;", source, StringComparison.Ordinal);
            Assert.Contains("CategoryPrintSummary", source, StringComparison.Ordinal);
            Assert.Contains("CategoryEmptyStateTitle", source, StringComparison.Ordinal);
            Assert.Contains("CategoryEmptyStateMessage", source, StringComparison.Ordinal);
            Assert.Contains("if (IsBusy)", source, StringComparison.Ordinal);
            Assert.Contains("Category refresh is already running.", source, StringComparison.Ordinal);
            Assert.Contains("_refreshCommand = new AsyncCommand(LoadAsync, () => !IsCategoryInteractionBusy && SelectedInventoryId > 0);", source, StringComparison.Ordinal);
            Assert.Contains("_clearSearchCommand = new AsyncCommand(ClearSearchAsync, () => !IsCategoryInteractionBusy && !string.IsNullOrWhiteSpace(SearchText));", source, StringComparison.Ordinal);
            Assert.Contains("RaiseCommandStates();", source, StringComparison.Ordinal);
        }

        [Fact]
        public void CategoriesPage_BoundsDirectoryPrintPreviewAndUsesFlexibleColumns()
        {
            var source = ReadRepoFile("InventoryManagementApp", "Views", "Pages", "CategoriesPage.xaml.cs");

            Assert.Contains("private const int MaxDirectoryPrintRows = 250;", source, StringComparison.Ordinal);
            Assert.Contains("!vm.IsDirectoryPrintAvailable", source, StringComparison.Ordinal);
            Assert.Contains("vm.FilteredCategories.Take(MaxDirectoryPrintRows).ToList()", source, StringComparison.Ordinal);
            Assert.Contains("Rows visible: {visibleRowCount}. Rows printed: {printedRowCount}. Rows omitted: {omittedRowCount}.", source, StringComparison.Ordinal);
            Assert.Contains("Review note: verify category names, item assignments, search/filter coverage, and any omitted rows", source, StringComparison.Ordinal);
            Assert.Contains("new GridLength(0.7, GridUnitType.Star)", source, StringComparison.Ordinal);
            Assert.Contains("new GridLength(1.6, GridUnitType.Star)", source, StringComparison.Ordinal);
            Assert.Contains("new GridLength(2.4, GridUnitType.Star)", source, StringComparison.Ordinal);
            Assert.Contains("ValueOrNotRecorded", source, StringComparison.Ordinal);
            Assert.DoesNotContain("vm.FilteredCategories.ToList()", source, StringComparison.Ordinal);
            Assert.DoesNotContain("new GridLength(90)", source, StringComparison.Ordinal);
            Assert.DoesNotContain("new GridLength(220)", source, StringComparison.Ordinal);
            Assert.DoesNotContain("new GridLength(280)", source, StringComparison.Ordinal);
        }

        [Fact]
        public void CategoriesPage_PreservesPrimaryCategoryActionsAndRowHandoff()
        {
            var xaml = ReadRepoFile("InventoryManagementApp", "Views", "Pages", "CategoriesPage.xaml");

            Assert.Contains("AddCommand", xaml, StringComparison.Ordinal);
            Assert.Contains("SaveCommand", xaml, StringComparison.Ordinal);
            Assert.Contains("DeleteCommand", xaml, StringComparison.Ordinal);
            Assert.Contains("ClearSearchCommand", xaml, StringComparison.Ordinal);
            Assert.Contains("RefreshCommand", xaml, StringComparison.Ordinal);
            Assert.Contains("OpenCategoryDetail_Click", xaml, StringComparison.Ordinal);
            Assert.Contains("CopyCategory_Click", xaml, StringComparison.Ordinal);
            Assert.Contains("PrintSelectedCategory_Click", xaml, StringComparison.Ordinal);
            Assert.Contains("PrintCategories_Click", xaml, StringComparison.Ordinal);
            Assert.Contains("CategoryRow_MouseDoubleClick", xaml, StringComparison.Ordinal);
            Assert.Contains("CategoryRow_PreviewMouseRightButtonDown", xaml, StringComparison.Ordinal);
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
