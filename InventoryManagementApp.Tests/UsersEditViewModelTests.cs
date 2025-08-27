using System;
using System.IO;
using System.Threading.Tasks;
using InventoryManagementApp.Interfaces;
using InventoryManagementApp.Models.Domain;
using InventoryManagementApp.ViewModels;
using System.Windows.Media;
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
        public void BrowseImageCommand_UsesRelativePathForBaseDirectoryFile()
        {
            var user = new User();
            var dialog = new DummyFileDialogService();
            var baseDir = AppDomain.CurrentDomain.BaseDirectory;
            var filePath = Path.Combine(baseDir, Guid.NewGuid() + ".png");
            File.WriteAllText(filePath, "test");
            dialog.Result = filePath;
            var vm = new UsersEditViewModel(user, dialog, onSave: () => Task.CompletedTask, onCancel: () => { });

            vm.BrowseImageCommand.Execute(null);

            Assert.Equal(Path.GetFileName(filePath), user.UserPhotoPath);

            File.Delete(filePath);
        }

        [Fact]
        public void BrowseImageCommand_CopiesExternalFileToAssets()
        {
            var user = new User();
            var dialog = new DummyFileDialogService();
            var tempFile = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".png");
            File.WriteAllText(tempFile, "test");
            dialog.Result = tempFile;
            var vm = new UsersEditViewModel(user, dialog, onSave: () => Task.CompletedTask, onCancel: () => { });

            vm.BrowseImageCommand.Execute(null);

            var baseDir = AppDomain.CurrentDomain.BaseDirectory;
            var expectedRelative = Path.Combine("Assets", "UserPhotos", Path.GetFileName(tempFile));
            var destPath = Path.Combine(baseDir, expectedRelative);

            Assert.Equal(expectedRelative, user.UserPhotoPath);
            Assert.True(File.Exists(destPath));

            File.Delete(tempFile);
            File.Delete(destPath);
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

        [Fact]
        public void RemoveImageCommand_SetsInitialsBrushToVisibleBrush()
        {
            var user = new User { UserPhotoPath = "test.png", InitialsBrush = Brushes.Transparent };
            var dialog = new DummyFileDialogService();
            var vm = new UsersEditViewModel(user, dialog, onSave: () => Task.CompletedTask, onCancel: () => { });

            vm.RemoveImageCommand.Execute(null);

            Assert.Equal(Brushes.Black, user.InitialsBrush);
        }
    }
}

