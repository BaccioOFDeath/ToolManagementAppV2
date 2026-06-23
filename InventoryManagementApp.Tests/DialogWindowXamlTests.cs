using System;
using System.IO;
using Xunit;

namespace InventoryManagementApp.Tests
{
    public class DialogWindowXamlTests
    {
        [Theory]
        [InlineData("InfoDialogWindow.xaml")]
        [InlineData("ConfirmDialogWindow.xaml")]
        [InlineData("InputDialogWindow.xaml")]
        public void DialogMessages_AreScrollable(string fileName)
        {
            var xaml = ReadWindowXaml(fileName);

            Assert.Contains("<ScrollViewer", xaml, StringComparison.Ordinal);
            Assert.Contains("VerticalScrollBarVisibility=\"Auto\"", xaml, StringComparison.Ordinal);
            Assert.Contains("Text=\"{Binding Message}\"", xaml, StringComparison.Ordinal);
        }

        private static string ReadWindowXaml(string fileName)
            => File.ReadAllText(Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "InventoryManagementApp", "Views", "Windows", fileName)));
    }
}
