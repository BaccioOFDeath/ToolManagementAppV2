using System;
using System.Threading.Tasks;
using InventoryManagementApp.Interfaces;
using InventoryManagementApp.Models.Domain;
using InventoryManagementApp.ViewModels;
using Xunit;

namespace InventoryManagementApp.Tests
{
    public class UsersEditViewModelTests
    {
        private sealed class DummyFileDialogService : IFileDialogService
        {
            public string? Result { get; set; }
            public string? OpenFile(string filter, string? initialDirectory = null) => Result;
            public string? SaveFile(string filter) => null;
        }

        [Fact]
        public void BrowseImageCommand_SetsPhotoPath()
        {
            var user = new User();
            var dialog = new DummyFileDialogService { Result = "test.png" };
            var vm = new UsersEditViewModel(user, dialog, onSave: () => Task.CompletedTask, onCancel: () => { });

            vm.BrowseImageCommand.Execute(null);

            Assert.Equal("test.png", user.UserPhotoPath);
        }

        [Fact]
        public void RemoveImageCommand_ClearsPhotoPath()
        {
            var user = new User { UserPhotoPath = "test.png" };
            var dialog = new DummyFileDialogService();
            var vm = new UsersEditViewModel(user, dialog, onSave: () => Task.CompletedTask, onCancel: () => { });

            vm.RemoveImageCommand.Execute(null);

            Assert.Equal(string.Empty, user.UserPhotoPath);
        }
    }
}

