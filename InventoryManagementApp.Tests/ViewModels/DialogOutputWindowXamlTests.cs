using System;
using System.IO;
using Xunit;

namespace InventoryManagementApp.Tests.ViewModels
{
    public class DialogOutputWindowXamlTests
    {
        [Fact]
        public void MessageDialogs_UsePolishedHeadersFootersAndPreserveCommands()
        {
            var info = ReadRepositoryFile("InventoryManagementApp", "Views", "Windows", "InfoDialogWindow.xaml");
            var confirm = ReadRepositoryFile("InventoryManagementApp", "Views", "Windows", "ConfirmDialogWindow.xaml");
            var input = ReadRepositoryFile("InventoryManagementApp", "Views", "Windows", "InputDialogWindow.xaml");

            Assert.Contains("Information Notice", info, StringComparison.Ordinal);
            Assert.Contains("Message reviewed when OK is selected.", info, StringComparison.Ordinal);
            Assert.Contains("OkCommand", info, StringComparison.Ordinal);

            Assert.Contains("Confirm Action", confirm, StringComparison.Ordinal);
            Assert.Contains("Action Review", confirm, StringComparison.Ordinal);
            Assert.Contains("CancelCommand", confirm, StringComparison.Ordinal);
            Assert.Contains("OkCommand", confirm, StringComparison.Ordinal);

            Assert.Contains("Input Required", input, StringComparison.Ordinal);
            Assert.Contains("Input is applied only after OK is selected.", input, StringComparison.Ordinal);
            Assert.Contains("InputText, UpdateSourceTrigger=PropertyChanged", input, StringComparison.Ordinal);
            Assert.Contains("CancelCommand", input, StringComparison.Ordinal);
            Assert.Contains("OkCommand", input, StringComparison.Ordinal);
        }

        [Fact]
        public void OutputAndMappingDialogs_UseWorkbenchStructureAndPreserveCommands()
        {
            var labels = ReadRepositoryFile("InventoryManagementApp", "Views", "Windows", "PrintLabelWindow.xaml");
            var mapping = ReadRepositoryFile("InventoryManagementApp", "Views", "Windows", "ImportMappingWindow.xaml");
            var imageMapping = ReadRepositoryFile("InventoryManagementApp", "Views", "Windows", "ImageImportMappingWindow.xaml");

            Assert.Contains("Label Output Workbench", labels, StringComparison.Ordinal);
            Assert.Contains("Queued Label Items", labels, StringComparison.Ordinal);
            Assert.Contains("PreviewCommand", labels, StringComparison.Ordinal);
            Assert.Contains("PrintCommand", labels, StringComparison.Ordinal);
            Assert.Contains("CloseCommand", labels, StringComparison.Ordinal);

            Assert.Contains("Import Mapping Workbench", mapping, StringComparison.Ordinal);
            Assert.Contains("Field Mapping Table", mapping, StringComparison.Ordinal);
            Assert.Contains("DataContext.ColumnHeaders", mapping, StringComparison.Ordinal);
            Assert.Contains("CancelCommand", mapping, StringComparison.Ordinal);
            Assert.Contains("OkCommand", mapping, StringComparison.Ordinal);

            Assert.Contains("Picture Matching Setup", imageMapping, StringComparison.Ordinal);
            Assert.Contains("Import confidence", imageMapping, StringComparison.Ordinal);
            Assert.Contains("UseItemNumber", imageMapping, StringComparison.Ordinal);
            Assert.Contains("UsePartNumber", imageMapping, StringComparison.Ordinal);
            Assert.Contains("UseName", imageMapping, StringComparison.Ordinal);
            Assert.Contains("CancelCommand", imageMapping, StringComparison.Ordinal);
            Assert.Contains("OkCommand", imageMapping, StringComparison.Ordinal);
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
