using System;
using System.IO;
using System.Text.RegularExpressions;
using Xunit;

namespace InventoryManagementApp.Tests.Views
{
    public class ItemEditWindowLayoutContractTests
    {
        [Fact]
        public void ItemEditWindowFitsOldLaptopHeightAndKeepsFormBodyScrollable()
        {
            var xaml = ReadRepositoryFile("InventoryManagementApp", "Views", "Windows", "ItemEditWindow.xaml");
            var codeBehind = ReadRepositoryFile("InventoryManagementApp", "Views", "Windows", "ItemEditWindow.xaml.cs");

            Assert.Equal(720, ReadWindowDimension(xaml, "Height"));
            Assert.Equal(620, ReadWindowDimension(xaml, "MinHeight"));
            Assert.True(ReadWindowDimension(xaml, "MinHeight") <= 720, "Item edit minimum height should leave room for window chrome on a 1366x768 laptop baseline.");
            Assert.Contains("<ScrollViewer Grid.Row=\"1\" VerticalScrollBarVisibility=\"Auto\">", xaml, StringComparison.Ordinal);
            Assert.Contains("<RowDefinition Height=\"*\"/>", xaml, StringComparison.Ordinal);
            Assert.Contains("UseResponsiveDefaultSize(840, 720)", codeBehind, StringComparison.Ordinal);
            Assert.DoesNotContain("UseResponsiveDefaultSize(980, 880)", codeBehind, StringComparison.Ordinal);
            Assert.DoesNotContain("MinHeight=\"780\"", xaml, StringComparison.Ordinal);
            Assert.DoesNotContain("Height=\"840\"", xaml, StringComparison.Ordinal);
        }

        private static int ReadWindowDimension(string xaml, string attribute)
        {
            var match = Regex.Match(xaml, $"\\b{attribute}=\\\"(?<value>\\d+)\\\"");
            Assert.True(match.Success, $"Expected ItemEditWindow.xaml to declare {attribute}.");
            return int.Parse(match.Groups["value"].Value);
        }

        private static string ReadRepositoryFile(params string[] relativePath)
        {
            var directory = new DirectoryInfo(AppContext.BaseDirectory);
            while (directory != null)
            {
                var candidate = Path.Combine(directory.FullName, Path.Combine(relativePath));
                if (File.Exists(candidate))
                    return File.ReadAllText(candidate);

                directory = directory.Parent;
            }

            throw new FileNotFoundException("Could not locate repository file.", Path.Combine(relativePath));
        }
    }
}