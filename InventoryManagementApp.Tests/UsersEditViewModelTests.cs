using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using InventoryManagementApp.Interfaces;
using InventoryManagementApp.Models.Domain;
using InventoryManagementApp.Utilities.Helpers;
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
            public string? SaveFile(string filter, string? initialDirectory = null) => null;
            public string? BrowseFolder(string? initialDirectory = null) => null;
        }

        [Fact]
        public void SaveCommand_RequiresUsernameAndRefreshesReadiness()
        {
            var user = new User { UserName = "   " };
            var dialog = new DummyFileDialogService();
            var vm = new UsersEditViewModel(user, dialog, onSave: () => Task.CompletedTask, onCancel: () => { });

            Assert.False(vm.CanSaveUserProfile);
            Assert.False(vm.SaveCommand.CanExecute(null));
            Assert.Contains("Enter a username", vm.SaveStatusText);

            user.UserName = "workshop-manager";

            Assert.True(vm.CanSaveUserProfile);
            Assert.True(vm.SaveCommand.CanExecute(null));
            Assert.Contains("User profile ready", vm.SaveStatusText);
        }

        [Fact]
        public async Task SaveCommand_ExposesBusyStateAndDisablesEditorActionsWhileRunning()
        {
            var user = new User { UserName = "workshop-manager" };
            var dialog = new DummyFileDialogService();
            var saveStarted = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            var releaseSave = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            var saveCalls = 0;
            var vm = new UsersEditViewModel(user, dialog, onSave: async () =>
            {
                saveCalls++;
                saveStarted.SetResult();
                await releaseSave.Task;
            }, onCancel: () => { });

            var saveTask = vm.SaveCommand.ExecuteAsync(null);
            await saveStarted.Task;

            Assert.True(vm.IsSaving);
            Assert.False(vm.CanEditProfile);
            Assert.False(vm.CanSaveUserProfile);
            Assert.False(vm.SaveCommand.CanExecute(null));
            Assert.False(vm.CancelCommand.CanExecute(null));
            Assert.False(vm.BrowseImageCommand.CanExecute(null));
            Assert.False(vm.RemoveImageCommand.CanExecute(null));
            Assert.False(vm.SelectAdvisorPresetCommand.CanExecute(null));
            Assert.False(vm.SelectTechnicianPresetCommand.CanExecute(null));
            Assert.False(vm.SelectAdminPresetCommand.CanExecute(null));
            Assert.False(vm.ClearPermissionsCommand.CanExecute(null));
            Assert.Contains("Saving user profile", vm.SaveStatusText);
            Assert.Equal(1, saveCalls);

            releaseSave.SetResult();
            await saveTask;

            Assert.False(vm.IsSaving);
            Assert.True(vm.CanEditProfile);
            Assert.True(vm.SaveCommand.CanExecute(null));
            Assert.True(vm.CancelCommand.CanExecute(null));
            Assert.True(vm.BrowseImageCommand.CanExecute(null));
            Assert.Contains("User profile ready", vm.SaveStatusText);
            Assert.Equal(1, saveCalls);
        }

        [Fact]
        public void BrowseImageCommand_UsesRelativePathForBaseDirectoryFile()
        {
            var user = new User { UserName = "photo-user" };
            var dialog = new DummyFileDialogService();
            var baseDir = AppDomain.CurrentDomain.BaseDirectory;
            var filePath = Path.Combine(baseDir, Guid.NewGuid() + ".png");
            File.WriteAllText(filePath, "test");
            dialog.Result = filePath;
            var vm = new UsersEditViewModel(user, dialog, onSave: () => Task.CompletedTask, onCancel: () => { });

            vm.BrowseImageCommand.Execute(null);

            Assert.StartsWith(Path.Combine("Assets", "UserPhotos"), user.UserPhotoPath);
            Assert.True(File.Exists(Path.Combine(AppAssetHelper.ResolveAssetPath(user.UserPhotoPath)!)));

            File.Delete(filePath);
            File.Delete(AppAssetHelper.ResolveAssetPath(user.UserPhotoPath)!);
        }

        [Fact]
        public void BrowseImageCommand_CopiesExternalFileToAssets()
        {
            var user = new User { UserName = "photo-user" };
            var dialog = new DummyFileDialogService();
            var tempFile = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".png");
            File.WriteAllText(tempFile, "test");
            dialog.Result = tempFile;
            var vm = new UsersEditViewModel(user, dialog, onSave: () => Task.CompletedTask, onCancel: () => { });

            vm.BrowseImageCommand.Execute(null);

            var destPath = AppAssetHelper.ResolveAssetPath(user.UserPhotoPath);

            Assert.StartsWith(Path.Combine("Assets", "UserPhotos"), user.UserPhotoPath);
            Assert.NotNull(destPath);
            Assert.True(File.Exists(destPath));

            File.Delete(tempFile);
            File.Delete(destPath);
        }

        [Fact]
        public void RemoveImageCommand_ClearsPhotoPath()
        {
            var user = new User { UserName = "photo-user", UserPhotoPath = "test.png" };
            var dialog = new DummyFileDialogService();
            var vm = new UsersEditViewModel(user, dialog, onSave: () => Task.CompletedTask, onCancel: () => { });

            vm.RemoveImageCommand.Execute(null);

            Assert.Equal(string.Empty, user.UserPhotoPath);
        }

        [Fact]
        public void RemoveImageCommand_SetsInitialsBrushToVisibleBrush()
        {
            var user = new User { UserName = "photo-user", UserPhotoPath = "test.png", InitialsBrush = Brushes.Transparent };
            var dialog = new DummyFileDialogService();
            var vm = new UsersEditViewModel(user, dialog, onSave: () => Task.CompletedTask, onCancel: () => { });

            vm.RemoveImageCommand.Execute(null);

            var expected = Application.Current?.TryFindResource("ForegroundBrush") as Brush ?? Brushes.Black;
            Assert.Equal(expected, user.InitialsBrush);
        }
    }
}