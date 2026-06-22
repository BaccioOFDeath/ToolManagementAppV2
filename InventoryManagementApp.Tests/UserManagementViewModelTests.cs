using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Documents;
using InventoryManagementApp.Interfaces;
using InventoryManagementApp.Models;
using InventoryManagementApp.Models.Domain;
using InventoryManagementApp.ViewModels;
using Xunit;
using UserModel = InventoryManagementApp.Models.Domain.User;

namespace InventoryManagementApp.Tests
{
    public class UserManagementViewModelTests
    {
        [Fact]
        public async Task LoadUsersAsync_WhenReloadFails_ClearsStaleRowsAndSelection()
        {
            var user = new UserModel { UserID = 4, UserName = "workshop4", Role = "Advisor" };
            var service = new StubUserService();
            service.Users.Add(user);
            var dialog = new StubDialogService();
            var vm = new UserManagementViewModel(service, new StubFileDialogService(), dialog);

            await vm.LoadUsersAsync();
            vm.SelectedUser = user;
            Assert.Single(vm.Users);
            Assert.True(vm.UpdateUserCommand.CanExecute(null));
            Assert.True(vm.EditUserCommand.CanExecute(null));

            service.ThrowOnGetAllUsers = true;
            await vm.LoadUsersAsync();

            Assert.Empty(vm.Users);
            Assert.Null(vm.SelectedUser);
            Assert.False(vm.UpdateUserCommand.CanExecute(null));
            Assert.False(vm.EditUserCommand.CanExecute(null));
            Assert.Equal("Error", dialog.LastTitle);
            Assert.Contains("User rows were cleared until refresh succeeds", dialog.LastMessage);

            vm.ClearUserSearchCommand.Execute(null);
            Assert.Empty(vm.Users);
        }

        [Fact]
        public async Task ResetPasswordFromRowCommand_WhenPasswordChangeFails_ShowsErrorAndDoesNotMarkExpired()
        {
            var user = new UserModel { UserID = 7, UserName = "workshop7", PasswordExpired = false };
            var service = new StubUserService { ChangePasswordResult = false };
            service.Users.Add(user);
            var dialog = new StubDialogService();
            var vm = new UserManagementViewModel(service, new StubFileDialogService(), dialog);

            await vm.ResetPasswordFromRowCommand.ExecuteAsync(user);

            Assert.False(user.PasswordExpired);
            Assert.Equal(0, service.UpdateCallCount);
            Assert.Equal("Error", dialog.LastTitle);
            Assert.Equal("Failed to reset password.", dialog.LastMessage);
        }

        [Fact]
        public async Task ResetPasswordFromRowCommand_WhenPasswordChangeSucceeds_MarksPasswordExpiredAndShowsSuccess()
        {
            var visibleUser = new UserModel { UserID = 9, UserName = "workshop9", PasswordExpired = false };
            var storedUser = new UserModel { UserID = 9, UserName = "workshop9", PasswordExpired = false };
            var service = new StubUserService();
            service.Users.Add(storedUser);
            var dialog = new StubDialogService();
            var vm = new UserManagementViewModel(service, new StubFileDialogService(), dialog);

            await vm.ResetPasswordFromRowCommand.ExecuteAsync(visibleUser);

            Assert.True(visibleUser.PasswordExpired);
            Assert.True(storedUser.PasswordExpired);
            Assert.Equal(1, service.UpdateCallCount);
            Assert.Equal("Password Reset", dialog.LastTitle);
            Assert.Contains("must change it at next login", dialog.LastMessage);
        }

        private sealed class StubUserService : IUserService
        {
            public List<UserModel> Users { get; } = new();
            public bool ChangePasswordResult { get; set; } = true;
            public bool ThrowOnGetAllUsers { get; set; }
            public int UpdateCallCount { get; private set; }

            public Task<List<UserModel>> GetAllUsersAsync(CancellationToken cancellationToken = default)
            {
                if (ThrowOnGetAllUsers)
                    throw new InvalidOperationException("Users are offline");

                return Task.FromResult(Users.ToList());
            }

            public Task<int> CountUsersAsync(CancellationToken cancellationToken = default)
                => Task.FromResult(Users.Count);

            public Task<UserModel?> GetUserByIDAsync(int userID, CancellationToken cancellationToken = default)
                => Task.FromResult(Users.FirstOrDefault(user => user.UserID == userID));

            public Task<(AuthenticationResult Result, UserModel? User)> AuthenticateUserAsync(string userName, string password)
                => Task.FromResult((default(AuthenticationResult), (UserModel?)null));

            public Task<UserModel?> GetCurrentUserAsync()
                => Task.FromResult<UserModel?>(null);

            public Task AddUserAsync(UserModel user)
            {
                Users.Add(user);
                return Task.CompletedTask;
            }

            public Task UpdateUserAsync(UserModel user)
            {
                UpdateCallCount++;
                var idx = Users.FindIndex(existing => existing.UserID == user.UserID);
                if (idx >= 0) Users[idx] = user;
                return Task.CompletedTask;
            }

            public Task<bool> TryDeleteUserAsync(int userID)
            {
                Users.RemoveAll(user => user.UserID == userID);
                return Task.FromResult(true);
            }

            public Task<bool> ChangeUserPasswordAsync(int userID, string newPassword)
            {
                if (!ChangePasswordResult)
                    return Task.FromResult(false);

                var user = Users.FirstOrDefault(existing => existing.UserID == userID);
                if (user != null)
                {
                    user.PasswordHash = $"hash:{newPassword}";
                    user.PasswordSalt = "salt";
                }

                return Task.FromResult(true);
            }
        }

        private sealed class StubFileDialogService : IFileDialogService
        {
            public string? OpenFile(string filter, string? initialDirectory = null) => null;
            public string? SaveFile(string filter, string? initialDirectory = null) => null;
            public string? BrowseFolder(string? initialDirectory = null) => null;
        }

        private sealed class StubDialogService : IDialogService
        {
            public string LastMessage { get; private set; } = string.Empty;
            public string LastTitle { get; private set; } = string.Empty;

            public void ShowInfo(string message, string title)
            {
                LastMessage = message;
                LastTitle = title;
            }

            public Task ShowInfoAsync(string message, string title)
            {
                ShowInfo(message, title);
                return Task.CompletedTask;
            }

            public bool ShowConfirmation(string message, string title) => true;
            public Task<bool> ShowConfirmationAsync(string message, string title) => Task.FromResult(true);
            public ItemModel? ShowEditItemDialog(ItemModel item) => null;
            public Task<ItemModel?> ShowEditItemDialogAsync(ItemModel item) => Task.FromResult<ItemModel?>(null);
            public void ShowItemDetails(ItemModel item) { }
            public (CustomerModel customer, DateTime dueDate)? ShowRentItemDialog(ItemModel item, IEnumerable<CustomerModel> customers) => null;
            public CustomerModel? ShowAddCustomerDialog() => null;
            public CustomerModel? ShowEditCustomerDialog(CustomerModel customer) => null;
            public void ShowRentalsFilter(ManageRentalsViewModel viewModel) { }
            public void ShowRentalHistory(ItemModel item, IEnumerable<RentalModel> history) { }
            public Dictionary<string, string>? ShowImportMapping(IEnumerable<string> headers, IEnumerable<string> properties, IEnumerable<string>? requiredPropertyNames = null) => null;
            public Func<ItemModel, IEnumerable<string>>? ShowImageImportMapping() => null;
            public void ShowPrintPreview(FlowDocument document, string title, string description) { }
            public void ShowPrintLabelDialog() { }
        }
    }
}
