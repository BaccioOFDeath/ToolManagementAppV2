using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Documents;
using InventoryManagementApp.Interfaces;
using InventoryManagementApp.Models;
using InventoryManagementApp.Models.Domain;
using InventoryManagementApp.ViewModels;
using Xunit;

namespace InventoryManagementApp.Tests
{
    public class UserPhotoTests
    {
        [Fact]
        public async Task UploadUserPhotoAsync_CopiesOutsideFileAndStoresRelativePath()
        {
            var tempFile = Path.GetTempFileName();
            File.WriteAllText(tempFile, "test");

            var user = new User { UserID = 1 };
            var fileDialog = new TestFileDialogService(tempFile);
            var userService = new TestUserService();
            var dialogService = new TestDialogService();

            var vm = new UserManagementViewModel(userService, fileDialog, dialogService);
            vm.SelectedUser = user;

            await vm.UploadUserPhotoAsync();

            var baseDir = AppDomain.CurrentDomain.BaseDirectory;
            var destDir = Path.Combine(baseDir, "Assets", "UserPhotos");
            var expected = Path.Combine(destDir, Path.GetFileName(tempFile));
            Assert.True(File.Exists(expected));
            Assert.Equal(Path.GetRelativePath(baseDir, expected), user.UserPhotoPath);

            File.Delete(tempFile);
            if (Directory.Exists(destDir)) Directory.Delete(destDir, true);
        }

        [Fact]
        public void AvatarSelectionViewModel_StoresRelativePath()
        {
            var baseDir = AppDomain.CurrentDomain.BaseDirectory;
            var avatarPath = Path.Combine(baseDir, "Resources", "Avatars", "1.png");
            var avatarUri = new Uri(avatarPath, UriKind.Absolute);
            var vm = new AvatarSelectionViewModel(new[] { avatarUri }, () => { });
            vm.SelectAvatarCommand.Execute(avatarUri);
            Assert.Equal(Path.GetRelativePath(baseDir, avatarPath), vm.SelectedAvatarPath);
        }
    }

    internal class TestFileDialogService : IFileDialogService
    {
        private readonly string _path;
        public TestFileDialogService(string path) => _path = path;
        public string? OpenFile(string filter, string? initialDirectory = null) => _path;
        public string? SaveFile(string filter) => null;
    }

    internal class TestUserService : IUserService
    {
        public List<User> UpdatedUsers { get; } = new();
        public Task AddUserAsync(User user) => Task.CompletedTask;
        public Task<(AuthenticationResult Result, User? User)> AuthenticateUserAsync(string userName, string password)
            => Task.FromResult((AuthenticationResult.IncorrectPassword, (User?)null));
        public Task<bool> ChangeUserPasswordAsync(int userID, string newPassword) => Task.FromResult(false);
        public Task<List<User>> GetAllUsersAsync() => Task.FromResult(new List<User>());
        public Task<User?> GetCurrentUserAsync() => Task.FromResult<User?>(null);
        public Task<User?> GetUserByIDAsync(int userID) => Task.FromResult<User?>(null);
        public Task<bool> TryDeleteUserAsync(int userID) => Task.FromResult(false);
        public Task UpdateUserAsync(User user)
        {
            UpdatedUsers.Add(user);
            return Task.CompletedTask;
        }
    }

    internal class TestDialogService : IDialogService
    {
        public void ShowInfo(string message, string title) { }
        public bool ShowConfirmation(string message, string title) => false;
        public ItemModel? ShowEditItemDialog(ItemModel item) => null;
        public void ShowItemDetails(ItemModel item) { }
        public (CustomerModel customer, DateTime dueDate)? ShowRentItemDialog(ItemModel item, IEnumerable<CustomerModel> customers) => null;
        public CustomerModel? ShowAddCustomerDialog() => null;
        public void ShowRentalsFilter(ManageRentalsViewModel viewModel) { }
        public void ShowRentalHistory(ItemModel item, IEnumerable<RentalModel> history) { }
        public Dictionary<string, string>? ShowImportMapping(IEnumerable<string> headers, IEnumerable<string> properties, IEnumerable<string>? requiredPropertyNames = null) => null;
        public Func<ItemModel, IEnumerable<string>>? ShowImageImportMapping() => null;
        public void ShowPrintPreview(FlowDocument document, string title, string description) { }
        public void ShowPrintLabelDialog() { }
    }
}
