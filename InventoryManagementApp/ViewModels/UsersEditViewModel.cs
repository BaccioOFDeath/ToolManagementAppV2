// ViewModels/UsersEditViewModel.cs
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using InventoryManagementApp.Interfaces;
using InventoryManagementApp.Models.Domain;
using System;
using System.IO;
using System.Threading.Tasks;

namespace InventoryManagementApp.ViewModels
{
    public class UsersEditViewModel : ObservableObject
    {
        public string Title { get; }
        public User EditingUser { get; }

        private readonly IFileDialogService _fileDialog;

        public IRelayCommand BrowseImageCommand { get; }
        public IRelayCommand RemoveImageCommand { get; }
        public IAsyncRelayCommand SaveCommand { get; }
        public IRelayCommand CancelCommand { get; }

        public UsersEditViewModel(User user, IFileDialogService fileDialog, Func<Task> onSave, Action onCancel)
        {
            EditingUser = user ?? new User();
            _fileDialog = fileDialog;
            Title = (EditingUser.UserID == 0) ? "Add User" : "Edit User";
            BrowseImageCommand = new RelayCommand(BrowseImage);
            RemoveImageCommand = new RelayCommand(RemoveImage);
            SaveCommand = new AsyncRelayCommand(onSave);
            CancelCommand = new RelayCommand(onCancel);
        }

        void BrowseImage()
        {
            var path = _fileDialog.OpenFile("Image Files|*.png;*.jpg;*.jpeg;*.bmp|All Files|*.*");
            if (string.IsNullOrEmpty(path))
            {
                return;
            }

            var fullPath = Path.GetFullPath(path);
            var baseDir = AppDomain.CurrentDomain.BaseDirectory;
            var targetPath = fullPath;

            if (!fullPath.StartsWith(baseDir, StringComparison.OrdinalIgnoreCase))
            {
                var assetsDir = Path.Combine(baseDir, "Assets", "UserPhotos");
                Directory.CreateDirectory(assetsDir);
                var fileName = Path.GetFileName(fullPath);
                targetPath = Path.Combine(assetsDir, fileName);
                File.Copy(fullPath, targetPath, true);
            }

            var relativePath = Path.GetRelativePath(baseDir, targetPath);
            EditingUser.UserPhotoPath = relativePath;
        }

        void RemoveImage()
        {
            EditingUser.UserPhotoPath = string.Empty;
        }
    }
}
