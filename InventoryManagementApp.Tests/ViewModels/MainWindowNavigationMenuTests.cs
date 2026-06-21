using System;
using System.IO;
using Xunit;

namespace InventoryManagementApp.Tests.ViewModels
{
    public class MainWindowNavigationMenuTests
    {
        [Fact]
        public void SectionMenuHeaderClick_OnlyOpensDropDown()
        {
            var code = ReadRepositoryFile("InventoryManagementApp", "MainWindow.xaml.cs");
            var handler = ExtractMethod(code, "void SectionMenuItem_PreviewMouseLeftButtonDown");

            Assert.Contains("menuItem.IsSubmenuOpen = true;", handler, StringComparison.Ordinal);
            Assert.DoesNotContain("SelectOverviewSectionCommand", handler, StringComparison.Ordinal);
            Assert.DoesNotContain("SelectOperationsSectionCommand", handler, StringComparison.Ordinal);
            Assert.DoesNotContain("SelectInsightsSectionCommand", handler, StringComparison.Ordinal);
            Assert.DoesNotContain("SelectDataSectionCommand", handler, StringComparison.Ordinal);
            Assert.DoesNotContain("SelectAdminSectionCommand", handler, StringComparison.Ordinal);
            Assert.DoesNotContain(".Execute(null)", handler, StringComparison.Ordinal);
        }

        static string ExtractMethod(string source, string signature)
        {
            var start = source.IndexOf(signature, StringComparison.Ordinal);
            Assert.True(start >= 0, $"Expected to find method signature '{signature}'.");

            var openBrace = source.IndexOf('{', start);
            Assert.True(openBrace >= 0, $"Expected method '{signature}' to have a body.");

            var depth = 0;
            for (var index = openBrace; index < source.Length; index++)
            {
                if (source[index] == '{')
                    depth++;
                else if (source[index] == '}')
                {
                    depth--;
                    if (depth == 0)
                        return source[start..(index + 1)];
                }
            }

            throw new InvalidOperationException($"Could not parse method body for '{signature}'.");
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
