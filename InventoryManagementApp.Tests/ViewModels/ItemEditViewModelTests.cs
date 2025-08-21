using InventoryManagementApp.ViewModels;
using InventoryManagementApp.Interfaces;
using InventoryManagementApp.Models.Domain;
using Xunit;

namespace InventoryManagementApp.Tests.ViewModels
{
    public class ItemEditViewModelTests
    {
        [Fact]
        public void BrowseImageCommand_SetsImagePath()
        {
            var item = new ItemModel();
            var fileDialog = new StubFileDialogService { OpenPath = "img.png" };
            var vm = new ItemEditViewModel(item, () => { }, () => { }, fileDialog);
            vm.BrowseImageCommand.Execute(null);
            Assert.Equal("img.png", item.ImagePath);
        }

        [Fact]
        public void RemoveImageCommand_ClearsImagePath()
        {
            var item = new ItemModel { ImagePath = "img.png" };
            var vm = new ItemEditViewModel(item, () => { }, () => { }, new StubFileDialogService());
            vm.RemoveImageCommand.Execute(null);
            Assert.Equal(string.Empty, item.ImagePath);
        }

        private class StubFileDialogService : IFileDialogService
        {
            public string? OpenPath { get; set; }
            public string? OpenFile(string filter, string? initialDirectory = null) => OpenPath;
            public string? SaveFile(string filter) => null;
        }
    }
}
