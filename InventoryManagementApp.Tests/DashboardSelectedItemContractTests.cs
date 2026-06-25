using System;
using System.IO;
using System.Text.RegularExpressions;
using Xunit;

namespace InventoryManagementApp.Tests
{
    public class DashboardSelectedItemContractTests
    {
        [Fact]
        public void SelectedDashboardItemCommandsOpenItemDetailsInsteadOfManageItems()
        {
            var source = ReadRepoFile("InventoryManagementApp", "ViewModels", "DashboardViewModel.cs");

            Assert.Matches(BuildCommandPattern("OpenSelectedCommonItemCommand", "OpenItemDetails", "SelectedCommonlyUsedItem", "HasSelectedCommonItem"), source);
            Assert.Matches(BuildCommandPattern("OpenSelectedCheckedOutItemCommand", "OpenItemDetails", "SelectedCheckedOutItem", "HasSelectedCheckedOutItem"), source);
            Assert.Matches(BuildCommandPattern("OpenSelectedIncompleteItemCommand", "OpenItemDetails", "SelectedIncompleteItem", "HasSelectedIncompleteItem"), source);
            Assert.Contains("OpenItemsCommand = new RelayCommand(OpenItemsWorkflow);", source);

            Assert.DoesNotMatch(BuildCommandPattern("OpenSelectedCommonItemCommand", "OpenItemsWorkflow", "", "HasSelectedCommonItem"), source);
            Assert.DoesNotMatch(BuildCommandPattern("OpenSelectedCheckedOutItemCommand", "OpenItemsWorkflow", "", "HasSelectedCheckedOutItem"), source);
            Assert.DoesNotMatch(BuildCommandPattern("OpenSelectedIncompleteItemCommand", "OpenItemsWorkflow", "", "HasSelectedIncompleteItem"), source);
        }

        [Fact]
        public void DashboardItemDetailsUsesDialogServiceWithAppFallback()
        {
            var source = ReadRepoFile("InventoryManagementApp", "ViewModels", "DashboardViewModel.cs");
            var method = ExtractMethod(source, "private void OpenItemDetails(ItemModel? item)");

            Assert.Contains("if (item == null)", method);
            Assert.Contains("_dialogService", method);
            Assert.Contains("Host.Services.GetService<IDialogService>()", method);
            Assert.Contains("dialogService.ShowItemDetails(item)", method);
            Assert.Contains("Item details service is not available", method);
        }

        private static Regex BuildCommandPattern(string commandName, string actionName, string selectedProperty, string canExecuteProperty)
        {
            var actionPattern = string.IsNullOrEmpty(selectedProperty)
                ? Regex.Escape(actionName)
                : $"\\(\\)\\s*=>\\s*{Regex.Escape(actionName)}\\({Regex.Escape(selectedProperty)}\\)";

            return new Regex(
                $"{Regex.Escape(commandName)}\\s*=\\s*new\\s+RelayCommand\\(\\s*{actionPattern}\\s*,\\s*\\(\\)\\s*=>\\s*{Regex.Escape(canExecuteProperty)}\\s*\\)\\s*;",
                RegexOptions.Singleline);
        }

        private static string ExtractMethod(string source, string signature)
        {
            var start = source.IndexOf(signature, StringComparison.Ordinal);
            Assert.True(start >= 0, $"Expected to find method signature '{signature}'.");

            var braceStart = source.IndexOf('{', start);
            Assert.True(braceStart >= 0, $"Expected to find method body for '{signature}'.");

            var depth = 0;
            for (var index = braceStart; index < source.Length; index++)
            {
                if (source[index] == '{')
                    depth++;
                else if (source[index] == '}')
                    depth--;

                if (depth == 0)
                    return source.Substring(start, index - start + 1);
            }

            throw new InvalidOperationException($"Could not extract method body for '{signature}'.");
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
