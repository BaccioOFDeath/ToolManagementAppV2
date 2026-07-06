using System;
using System.IO;
using Xunit;

namespace InventoryManagementApp.Tests
{
    public class CategoriesPageLifecycleInputContractTests
    {
        [Fact]
        public void CategoriesPage_InvalidatesPageOwnedStartupWorkOnUnloadAndDataContextChanges()
        {
            var source = ReadRepoFile("InventoryManagementApp", "Views", "Pages", "CategoriesPage.xaml.cs");

            Assert.Contains("using System.Threading;", source, StringComparison.Ordinal);
            Assert.Contains("private CancellationTokenSource? _initializeCategoriesCancellation;", source, StringComparison.Ordinal);
            Assert.Contains("private int _initializeCategoriesVersion;", source, StringComparison.Ordinal);
            Assert.Contains("Unloaded += CategoriesPage_Unloaded;", source, StringComparison.Ordinal);
            Assert.Contains("private void CategoriesPage_Unloaded", source, StringComparison.Ordinal);
            Assert.Contains("CancelPageOwnedInitialization();", source, StringComparison.Ordinal);
            Assert.Contains("private void CancelPageOwnedInitialization()", source, StringComparison.Ordinal);
            Assert.Contains("_initializeCategoriesVersion++;", source, StringComparison.Ordinal);
            Assert.Contains("_initializeCategoriesCancellation?.Cancel();", source, StringComparison.Ordinal);
            Assert.Contains("_initializeCategoriesCancellation?.Dispose();", source, StringComparison.Ordinal);
        }

        [Fact]
        public void CategoriesPage_GatesStartupRefreshThroughCurrentPageVersionAndCancellation()
        {
            var source = ReadRepoFile("InventoryManagementApp", "Views", "Pages", "CategoriesPage.xaml.cs");
            var initialization = ExtractSourceBlock(source, "private async Task InitializeCategoriesOnceAsync", "private bool IsCurrentCategoryInitialization");
            var currentCheck = ExtractSourceBlock(source, "private bool IsCurrentCategoryInitialization", "private void CancelPageOwnedInitialization");

            Assert.Contains("CancelPageOwnedInitialization();", initialization, StringComparison.Ordinal);
            Assert.Contains("var loadVersion = ++_initializeCategoriesVersion;", initialization, StringComparison.Ordinal);
            Assert.Contains("_initializeCategoriesCancellation = new CancellationTokenSource();", initialization, StringComparison.Ordinal);
            Assert.Contains("var cancellationToken = _initializeCategoriesCancellation.Token;", initialization, StringComparison.Ordinal);
            Assert.Contains("await Dispatcher.Yield(DispatcherPriority.Background);", initialization, StringComparison.Ordinal);
            Assert.Contains("!IsCurrentCategoryInitialization(vm, loadVersion, cancellationToken) || vm.IsCategoryInteractionBusy", initialization, StringComparison.Ordinal);
            Assert.Contains("_initializeCategoriesTask = vm.InitializeAsync();", initialization, StringComparison.Ordinal);
            Assert.Contains("await _initializeCategoriesTask;", initialization, StringComparison.Ordinal);
            Assert.Contains("!IsCurrentCategoryInitialization(vm, loadVersion, cancellationToken)", initialization, StringComparison.Ordinal);
            Assert.Contains("!cancellationToken.IsCancellationRequested", currentCheck, StringComparison.Ordinal);
            Assert.Contains("loadVersion == _initializeCategoriesVersion", currentCheck, StringComparison.Ordinal);
            Assert.Contains("ReferenceEquals(DataContext, vm)", currentCheck, StringComparison.Ordinal);
        }

        [Fact]
        public void CategoriesPage_PreservesTextEditingBeforeGlobalCategoryShortcuts()
        {
            var source = ReadRepoFile("InventoryManagementApp", "Views", "Pages", "CategoriesPage.xaml.cs");
            var keyHandler = ExtractSourceBlock(source, "private void Page_PreviewKeyDown", "private static bool IsCategoryActionShortcut");
            var shortcut = ExtractSourceBlock(source, "private static bool IsCategoryActionShortcut", "private static bool IsTextInputFocused");

            Assert.Contains("Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.F", keyHandler, StringComparison.Ordinal);
            Assert.Contains("Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.N", keyHandler, StringComparison.Ordinal);
            Assert.Contains("ViewModel.IsCategoryInteractionBusy && IsCategoryActionShortcut(e)", keyHandler, StringComparison.Ordinal);
            Assert.Contains("if (IsTextInputFocused() && IsCategoryActionShortcut(e))", keyHandler, StringComparison.Ordinal);
            Assert.Contains("return;", keyHandler, StringComparison.Ordinal);
            Assert.Contains("Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.R", keyHandler, StringComparison.Ordinal);
            Assert.Contains("Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.S", keyHandler, StringComparison.Ordinal);
            Assert.Contains("Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.P", keyHandler, StringComparison.Ordinal);
            Assert.Contains("Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.C", keyHandler, StringComparison.Ordinal);
            Assert.Contains("e.Key == Key.Delete", keyHandler, StringComparison.Ordinal);
            Assert.Contains("e.Key == Key.Enter", keyHandler, StringComparison.Ordinal);
            Assert.Contains("e.Key is Key.R or Key.S or Key.P or Key.C", shortcut, StringComparison.Ordinal);
            Assert.Contains("e.Key is Key.Enter or Key.Delete", shortcut, StringComparison.Ordinal);
            Assert.DoesNotContain("!IsTextInputFocused() && Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.C", keyHandler, StringComparison.Ordinal);
            Assert.DoesNotContain("if (e.Key == Key.Enter && !IsTextInputFocused())", keyHandler, StringComparison.Ordinal);
        }

        [Fact]
        public void CategoriesPage_BlocksContextMenuInvocationWhileRowsAreBusy()
        {
            var source = ReadRepoFile("InventoryManagementApp", "Views", "Pages", "CategoriesPage.xaml.cs");
            var contextMenu = ExtractSourceBlock(source, "private void CategoryGrid_ContextMenuOpening", "private void OpenCategoryDetail_Click");

            Assert.Contains("CategoryGrid.ContextMenuOpening += CategoryGrid_ContextMenuOpening;", source, StringComparison.Ordinal);
            Assert.Contains("ViewModel is { IsCategoryInteractionBusy: true }", contextMenu, StringComparison.Ordinal);
            Assert.Contains("e.Handled = true;", contextMenu, StringComparison.Ordinal);
        }

        [Fact]
        public void CategoriesPage_KeepsExistingFirstPaintAndBusyGestureProtections()
        {
            var source = ReadRepoFile("InventoryManagementApp", "Views", "Pages", "CategoriesPage.xaml.cs");
            var loaded = ExtractSourceBlock(source, "private async void CategoriesPage_Loaded", "private void CategoriesPage_Unloaded");
            var doubleClick = ExtractSourceBlock(source, "private void CategoryRow_MouseDoubleClick", "private void CategoryRow_PreviewMouseRightButtonDown");
            var rightClick = ExtractSourceBlock(source, "private void CategoryRow_PreviewMouseRightButtonDown", "private void CategoryGrid_ContextMenuOpening");

            Assert.Contains("FindBox.Focus();", loaded, StringComparison.Ordinal);
            Assert.Contains("FindBox.SelectAll();", loaded, StringComparison.Ordinal);
            Assert.Contains("await InitializeCategoriesOnceAsync(vm);", loaded, StringComparison.Ordinal);
            Assert.Contains("ViewModel is { IsCategoryInteractionBusy: true }", doubleClick, StringComparison.Ordinal);
            Assert.Contains("GridContextMenuSelection.SelectRow(sender, e) == null", doubleClick, StringComparison.Ordinal);
            Assert.Contains("OpenCategoryDetail_Click(sender, e);", doubleClick, StringComparison.Ordinal);
            Assert.Contains("e.Handled = true;", doubleClick, StringComparison.Ordinal);
            Assert.Contains("ViewModel is { IsCategoryInteractionBusy: true }", rightClick, StringComparison.Ordinal);
            Assert.Contains("GridContextMenuSelection.SelectRow(sender, e);", rightClick, StringComparison.Ordinal);
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
