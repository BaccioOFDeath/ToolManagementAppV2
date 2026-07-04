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
        public void CategoriesPage_DisablesSelectedCategoryActionsWhileRowsAreBusyOrUnselected()
        {
            var xaml = ReadRepoFile("InventoryManagementApp", "Views", "Pages", "CategoriesPage.xaml");

            Assert.Contains("Content=\"Open\" Click=\"OpenCategoryDetail_Click\" IsEnabled=\"{Binding IsSelectedCategoryActionAvailable}\"", xaml, StringComparison.Ordinal);
            Assert.Contains("Content=\"Copy Handoff\" Click=\"CopyCategory_Click\" IsEnabled=\"{Binding IsSelectedCategoryActionAvailable}\"", xaml, StringComparison.Ordinal);
            Assert.Contains("Content=\"Print Sheet\" Click=\"PrintSelectedCategory_Click\" IsEnabled=\"{Binding IsSelectedCategoryActionAvailable}\"", xaml, StringComparison.Ordinal);
            Assert.Contains("Header=\"Open Category Detail\" Click=\"OpenCategoryDetail_Click\" IsEnabled=\"{Binding IsSelectedCategoryActionAvailable}\"", xaml, StringComparison.Ordinal);
            Assert.Contains("Header=\"Copy Setup Handoff\" Click=\"CopyCategory_Click\" IsEnabled=\"{Binding IsSelectedCategoryActionAvailable}\"", xaml, StringComparison.Ordinal);
            Assert.Contains("Header=\"Print Selected Sheet\" Click=\"PrintSelectedCategory_Click\" IsEnabled=\"{Binding IsSelectedCategoryActionAvailable}\"", xaml, StringComparison.Ordinal);
        }

        [Fact]
        public void CategoryViewModel_GuardsLoadingCommandsAndProfessionalDirectoryState()
        {
            var source = ReadRepoFile("InventoryManagementApp", "ViewModels", "CategoryManagementViewModel.cs");

            Assert.Contains("public bool IsCategoryInteractionBusy => IsBusy;", source, StringComparison.Ordinal);
            Assert.Contains("public bool IsCategoryActionAvailable => !IsCategoryInteractionBusy;", source, StringComparison.Ordinal);
            Assert.Contains("public bool IsSelectedCategoryActionAvailable => !IsCategoryInteractionBusy && SelectedCategory != null;", source, StringComparison.Ordinal);
            Assert.Contains("public bool IsDirectoryPrintAvailable => !IsCategoryInteractionBusy && FilteredCategories.Count > 0;", source, StringComparison.Ordinal);
            Assert.Contains("public bool IsCategoryEmptyStateVisible => !IsCategoryInteractionBusy && FilteredCategories.Count == 0;", source, StringComparison.Ordinal);
            Assert.Contains("CategoryPrintSummary", source, StringComparison.Ordinal);
            Assert.Contains("CategoryEmptyStateTitle", source, StringComparison.Ordinal);
            Assert.Contains("CategoryEmptyStateMessage", source, StringComparison.Ordinal);
            Assert.Contains("if (IsBusy)", source, StringComparison.Ordinal);
            Assert.Contains("Category refresh is already running.", source, StringComparison.Ordinal);
            Assert.Contains("_refreshCommand = new AsyncCommand(LoadAsync, () => !IsCategoryInteractionBusy && SelectedInventoryId > 0);", source, StringComparison.Ordinal);
            Assert.Contains("_clearSearchCommand = new AsyncCommand(ClearSearchAsync, () => !IsCategoryInteractionBusy && !string.IsNullOrWhiteSpace(SearchText));", source, StringComparison.Ordinal);
            Assert.Contains("OnPropertyChanged(nameof(IsCategoryActionAvailable));", source, StringComparison.Ordinal);
            Assert.Contains("OnPropertyChanged(nameof(IsSelectedCategoryActionAvailable));", source, StringComparison.Ordinal);
            Assert.Contains("RaiseCommandStates();", source, StringComparison.Ordinal);
        }

        [Fact]
        public void CategoryViewModel_PreservesVisibleRowsWhenDirectoryRefreshFails()
        {
            var source = ReadRepoFile("InventoryManagementApp", "ViewModels", "CategoryManagementViewModel.cs");
            var loadBlock = ExtractSourceBlock(source, "private async Task LoadCategoryDirectoryAsync", "private void ShowCategoryLoadFailureDialogOnce");

            Assert.Contains("Category refresh failed. Existing category rows were kept so current work can continue.", loadBlock, StringComparison.Ordinal);
            Assert.Contains("Existing category rows were kept when available", source, StringComparison.Ordinal);
            Assert.Contains("RaiseDirectoryProperties();", loadBlock, StringComparison.Ordinal);
            Assert.DoesNotContain("ClearCategoryStateAfterLoadFailure();", loadBlock, StringComparison.Ordinal);
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
        public void CategoriesPage_GuardsStartupLoadingThroughActiveViewModelAndFirstPaintYield()
        {
            var source = ReadRepoFile("InventoryManagementApp", "Views", "Pages", "CategoriesPage.xaml.cs");

            Assert.Contains("private Task? _initializeCategoriesTask;", source, StringComparison.Ordinal);
            Assert.Contains("private CategoryManagementViewModel? _initializedViewModel;", source, StringComparison.Ordinal);
            Assert.Contains("Loaded += CategoriesPage_Loaded;", source, StringComparison.Ordinal);
            Assert.Contains("DataContextChanged += CategoriesPage_DataContextChanged;", source, StringComparison.Ordinal);
            Assert.Contains("FindBox.Focus();", source, StringComparison.Ordinal);
            Assert.Contains("await Dispatcher.Yield(DispatcherPriority.Background);", source, StringComparison.Ordinal);
            Assert.Contains("!ReferenceEquals(DataContext, vm) || vm.IsCategoryInteractionBusy", source, StringComparison.Ordinal);
            Assert.Contains("_initializeCategoriesTask = vm.InitializeAsync();", source, StringComparison.Ordinal);
            Assert.DoesNotContain("private bool _hasInitialized;", source, StringComparison.Ordinal);
            Assert.DoesNotContain("Loaded += async (_, __) =>", source, StringComparison.Ordinal);
        }

        [Fact]
        public void CategoriesPage_GuardsRowGesturesAndKeyboardActionsWhileRowsLoad()
        {
            var source = ReadRepoFile("InventoryManagementApp", "Views", "Pages", "CategoriesPage.xaml.cs");
            var doubleClick = ExtractSourceBlock(source, "private void CategoryRow_MouseDoubleClick", "private void CategoryRow_PreviewMouseRightButtonDown");
            var rightClick = ExtractSourceBlock(source, "private void CategoryRow_PreviewMouseRightButtonDown", "private void OpenCategoryDetail_Click");
            var keyHandler = ExtractSourceBlock(source, "private void Page_PreviewKeyDown", "private static bool IsCategoryActionShortcut");
            var busyShortcut = ExtractSourceBlock(source, "private static bool IsCategoryActionShortcut", "private static bool IsTextInputFocused");

            Assert.Contains("ViewModel is { IsCategoryInteractionBusy: true }", doubleClick, StringComparison.Ordinal);
            Assert.Contains("GridContextMenuSelection.SelectRow(sender, e) == null", doubleClick, StringComparison.Ordinal);
            Assert.Contains("e.Handled = true;", doubleClick, StringComparison.Ordinal);
            Assert.Contains("ViewModel is { IsCategoryInteractionBusy: true }", rightClick, StringComparison.Ordinal);
            Assert.Contains("GridContextMenuSelection.SelectRow(sender, e);", rightClick, StringComparison.Ordinal);
            Assert.Contains("ViewModel.IsCategoryInteractionBusy && IsCategoryActionShortcut(e)", keyHandler, StringComparison.Ordinal);
            Assert.Contains("Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.R", keyHandler, StringComparison.Ordinal);
            Assert.Contains("!IsTextInputFocused() && Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.C", keyHandler, StringComparison.Ordinal);
            Assert.Contains("e.Key is Key.R or Key.S or Key.P or Key.C", busyShortcut, StringComparison.Ordinal);
            Assert.Contains("return Keyboard.Modifiers == ModifierKeys.None && e.Key is Key.Enter or Key.Delete;", busyShortcut, StringComparison.Ordinal);
        }

        [Fact]
        public void CategoriesPage_CodeBehindActionsCheckBusyStateBeforeOpeningSelectionWorkflows()
        {
            var source = ReadRepoFile("InventoryManagementApp", "Views", "Pages", "CategoriesPage.xaml.cs");

            Assert.Contains("private bool AreCategoryRowsReady(string title)", source, StringComparison.Ordinal);
            Assert.Contains("Category rows are still loading. Wait for the refresh to finish before using category actions.", source, StringComparison.Ordinal);
            Assert.Contains("!AreCategoryRowsReady(\"Category Detail\") || !TryGetSelectedCategory(out var category)", source, StringComparison.Ordinal);
            Assert.Contains("!AreCategoryRowsReady(\"Category Sheet\") || !TryGetSelectedCategory(out var category)", source, StringComparison.Ordinal);
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

        private static string ExtractSourceBlock(string source, string startMarker, string endMarker)
        {
            var start = source.IndexOf(startMarker, StringComparison.Ordinal);
            Assert.True(start >= 0, $"Could not find source block start marker: {startMarker}");

            var end = source.IndexOf(endMarker, start, StringComparison.Ordinal);
            Assert.True(end > start, $"Could not find source block end marker: {endMarker}");

            return source[start..end];
        }
    }
}
