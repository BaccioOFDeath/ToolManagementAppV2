using System;
using System.IO;
using Xunit;

namespace InventoryManagementApp.Tests
{
    public class ManageRentalsPageLoadingInputContractTests
    {
        [Fact]
        public void ManageRentalsPage_FreezesFilterEditorsDuringRefreshButKeepsNavigationKeys()
        {
            var source = ReadRepoFile("InventoryManagementApp", "Views", "Pages", "ManageRentalsPage.xaml.cs");
            var keyDownBlock = ExtractSourceBlock(source, "private void ManageRentalsPage_PreviewKeyDown", "private static bool IsRentalActionShortcut");

            Assert.Contains("Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.F", keyDownBlock, StringComparison.Ordinal);
            Assert.Contains("if (vm.IsLoading && IsTextEditingElement(e.OriginalSource) && e.Key is not Key.Tab and not Key.Escape)", keyDownBlock, StringComparison.Ordinal);
            Assert.Contains("e.Handled = true;\n                return;", keyDownBlock, StringComparison.Ordinal);
            Assert.True(
                keyDownBlock.IndexOf("Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.F", StringComparison.Ordinal) <
                keyDownBlock.IndexOf("if (vm.IsLoading && IsTextEditingElement(e.OriginalSource)", StringComparison.Ordinal),
                "Ctrl+F should still move focus to search while the rental desk refreshes.");
            Assert.True(
                keyDownBlock.IndexOf("if (vm.IsLoading && IsTextEditingElement(e.OriginalSource)", StringComparison.Ordinal) <
                keyDownBlock.IndexOf("if (IsTextEditingElement(e.OriginalSource) && IsRentalActionShortcut(e))", StringComparison.Ordinal),
                "Busy editor suppression should run before normal text-edit shortcut preservation.");
            Assert.True(
                keyDownBlock.IndexOf("if (vm.IsLoading && IsTextEditingElement(e.OriginalSource)", StringComparison.Ordinal) <
                keyDownBlock.IndexOf("if (vm.IsLoading && IsRentalActionShortcut(e))", StringComparison.Ordinal),
                "Filter-editor input should be paused before command shortcut handling during refresh.");
        }

        [Fact]
        public void ManageRentalsPage_LoadingEditorGuardCoversSearchDatesStatusAndPasswordEditors()
        {
            var source = ReadRepoFile("InventoryManagementApp", "Views", "Pages", "ManageRentalsPage.xaml.cs");
            var editBlock = ExtractSourceBlock(source, "private static bool IsTextEditingElement", "private void OpenFocusedDetails");

            Assert.Contains("source is TextBox or ComboBox or DatePicker or PasswordBox", editBlock, StringComparison.Ordinal);
            Assert.Contains("GridContextMenuSelection.FindAncestor<TextBox>(element) != null", editBlock, StringComparison.Ordinal);
            Assert.Contains("GridContextMenuSelection.FindAncestor<ComboBox>(element) != null", editBlock, StringComparison.Ordinal);
            Assert.Contains("GridContextMenuSelection.FindAncestor<DatePicker>(element) != null", editBlock, StringComparison.Ordinal);
            Assert.Contains("GridContextMenuSelection.FindAncestor<PasswordBox>(element) != null", editBlock, StringComparison.Ordinal);
        }

        private static string ReadRepoFile(params string[] parts)
        {
            var directory = AppContext.BaseDirectory;

            while (!string.IsNullOrEmpty(directory))
            {
                var candidate = Path.Combine(directory, Path.Combine(parts));
                if (File.Exists(candidate))
                    return NormalizeLineEndings(File.ReadAllText(candidate));

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

        private static string NormalizeLineEndings(string text)
            => text.Replace("\r\n", "\n");
    }
}
