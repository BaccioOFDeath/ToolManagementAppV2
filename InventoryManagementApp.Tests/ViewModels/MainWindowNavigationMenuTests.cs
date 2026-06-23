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

        [Fact]
        public void SectionDropDownItem_UsesExplicitHoverForegroundForDarkThemeReadability()
        {
            var styles = ReadRepositoryFile("InventoryManagementApp", "Resources", "Styles.xaml");
            var style = ExtractStyle(styles, "x:Key=\"SectionDropDownItem\"");

            Assert.Contains("<ControlTemplate TargetType=\"MenuItem\">", style, StringComparison.Ordinal);
            Assert.Contains("SystemColors.HighlightBrushKey", style, StringComparison.Ordinal);
            Assert.Contains("SystemColors.HighlightTextBrushKey", style, StringComparison.Ordinal);
            Assert.Contains("Property=\"Background\" Value=\"{DynamicResource ThemeMenuDropDownBackgroundBrush}\"", style, StringComparison.Ordinal);
            Assert.DoesNotContain("Property=\"Background\" Value=\"{DynamicResource SurfaceBrush}\"", style, StringComparison.Ordinal);
            Assert.Contains("TargetName=\"Root\" Property=\"Background\" Value=\"{DynamicResource AccentBrush}\"", style, StringComparison.Ordinal);
            Assert.Contains("Property=\"Foreground\" Value=\"{DynamicResource OnAccentForegroundBrush}\"", style, StringComparison.Ordinal);
            Assert.Contains("Property=\"Foreground\" Value=\"{DynamicResource DisabledForegroundBrush}\"", style, StringComparison.Ordinal);
            Assert.Contains("<MultiTrigger>", style, StringComparison.Ordinal);
        }

        static string ExtractStyle(string source, string marker)
        {
            var start = source.IndexOf(marker, StringComparison.Ordinal);
            Assert.True(start >= 0, $"Expected to find style marker '{marker}'.");

            var styleStart = source.LastIndexOf("<Style", start, StringComparison.Ordinal);
            Assert.True(styleStart >= 0, $"Expected marker '{marker}' to be inside a Style.");

            var end = source.IndexOf("</Style>", start, StringComparison.Ordinal);
            Assert.True(end >= 0, $"Expected style '{marker}' to have a closing tag.");

            return source[styleStart..(end + "</Style>".Length)];
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
