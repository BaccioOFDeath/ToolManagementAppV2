using ToolManagementAppV2.ViewModels;
using ToolManagementAppV2.Interfaces;
using ToolManagementAppV2.Models.Domain;
using Xunit;

namespace ToolManagementAppV2.Tests.ViewModels
{
    public class ToolEditViewModelTests
    {
        [Fact]
        public void BrowseImageCommand_SetsImagePath()
        {
            var tool = new ItemModel();
            var fileDialog = new StubFileDialogService { OpenPath = "img.png" };
            var vm = new ToolEditViewModel(tool, () => { }, () => { }, fileDialog);
            vm.BrowseImageCommand.Execute(null);
            Assert.Equal("img.png", tool.ImagePath);
        }

        [Fact]
        public void RemoveImageCommand_ClearsImagePath()
        {
            var tool = new ItemModel { ImagePath = "img.png" };
            var vm = new ToolEditViewModel(tool, () => { }, () => { }, new StubFileDialogService());
            vm.RemoveImageCommand.Execute(null);
            Assert.Equal(string.Empty, tool.ImagePath);
        }

        private class StubFileDialogService : IFileDialogService
        {
            public string? OpenPath { get; set; }
            public string? OpenFile(string filter, string? initialDirectory = null) => OpenPath;
            public string? SaveFile(string filter) => null;
        }
    }
}
