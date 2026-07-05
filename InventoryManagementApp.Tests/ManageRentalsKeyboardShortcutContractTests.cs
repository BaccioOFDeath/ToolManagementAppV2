using System;
using System.IO;
using Xunit;

namespace InventoryManagementApp.Tests;

public class ManageRentalsKeyboardShortcutContractTests
{
    private static string ReadRepoFile(string relativePath)
    {
        var baseDir = AppContext.BaseDirectory;
        var root = Path.GetFullPath(Path.Combine(baseDir, "..", "..", "..", ".."));
        var path = Path.Combine(root, relativePath);
        return File.ReadAllText(path);
    }

    [Fact]
    public void ManageRentalsShortcuts_DoNotHijackTextEditingControls()
    {
        var source = ReadRepoFile("InventoryManagementApp/Views/Pages/ManageRentalsPage.xaml.cs");
        var handler = ExtractBlock(source, "private void ManageRentalsPage_PreviewKeyDown");

        Assert.Contains("if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.F)", handler);
        Assert.Contains("SearchTextBox.Focus();", handler);
        Assert.Contains("SearchTextBox.SelectAll();", handler);
        Assert.Contains("if (IsTextEditingElement(e.OriginalSource) && IsRentalActionShortcut(e))", handler);
        Assert.Contains("if (vm.IsLoading && IsRentalActionShortcut(e))", handler);

        Assert.True(
            handler.IndexOf("if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.F)", StringComparison.Ordinal)
                < handler.IndexOf("if (IsTextEditingElement(e.OriginalSource) && IsRentalActionShortcut(e))", StringComparison.Ordinal));
        Assert.True(
            handler.IndexOf("if (IsTextEditingElement(e.OriginalSource) && IsRentalActionShortcut(e))", StringComparison.Ordinal)
                < handler.IndexOf("if (vm.IsLoading && IsRentalActionShortcut(e))", StringComparison.Ordinal));
    }

    [Fact]
    public void ManageRentalsShortcuts_TreatsFilterInputsAsTextEditingSurfaces()
    {
        var source = ReadRepoFile("InventoryManagementApp/Views/Pages/ManageRentalsPage.xaml.cs");
        var helper = ExtractBlock(source, "private static bool IsTextEditingElement");

        Assert.Contains("object? source", helper);
        Assert.Contains("source is TextBox or ComboBox or DatePicker", helper);
    }

    [Fact]
    public void ManageRentalsRowDoubleClick_StopsAfterSelectionEvenWhenCommandsAreUnavailable()
    {
        var source = ReadRepoFile("InventoryManagementApp/Views/Pages/ManageRentalsPage.xaml.cs");
        var rentalHandler = ExtractBlock(source, "private void RentalRow_MouseDoubleClick");
        var requestHandler = ExtractBlock(source, "private void RequestRow_MouseDoubleClick");

        AssertHandledBeforeCommandAvailability(rentalHandler, "vm.OpenRentalDetailsCommand.CanExecute(null)");
        AssertHandledBeforeCommandAvailability(requestHandler, "vm.OpenRequestDetailsCommand.CanExecute(null)");
    }

    private static void AssertHandledBeforeCommandAvailability(string handler, string commandAvailabilityCheck)
    {
        var selectIndex = handler.IndexOf("if (SelectRowForContextMenu(sender, e) == null)", StringComparison.Ordinal);
        var handledIndex = handler.IndexOf("e.Handled = true;", selectIndex, StringComparison.Ordinal);
        var canExecuteIndex = handler.IndexOf(commandAvailabilityCheck, StringComparison.Ordinal);

        Assert.True(selectIndex >= 0);
        Assert.True(handledIndex > selectIndex);
        Assert.True(canExecuteIndex > handledIndex);
    }

    private static string ExtractBlock(string source, string signature)
    {
        var start = source.IndexOf(signature, StringComparison.Ordinal);
        Assert.True(start >= 0, $"Could not find '{signature}'.");

        var braceStart = source.IndexOf('{', start);
        Assert.True(braceStart >= 0, $"Could not find body for '{signature}'.");

        var depth = 0;
        for (var i = braceStart; i < source.Length; i++)
        {
            if (source[i] == '{')
                depth++;
            else if (source[i] == '}')
                depth--;

            if (depth == 0)
                return source.Substring(start, i - start + 1);
        }

        throw new InvalidOperationException($"Could not parse body for '{signature}'.");
    }
}
