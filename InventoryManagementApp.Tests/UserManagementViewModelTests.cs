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
        public async Task AddUserAsync_WhenAddFailsAfterPersistence_RefreshesRowsAndSelectsSavedUser()
        {
            var service = new StubUserService { ThrowAfterAdd = true };
            var dialog = new StubDialogService();
            var vm = new UserManagementViewModel(service, new StubFileDialogService(), dialog);

            await vm.AddUserAsync();

            var user = Assert.Single(vm.Users);
            Assert.Single(service.Users);
            Assert.Same(user, vm.SelectedUser);
            Assert.Equal("workshop1", user.UserName);
            Assert.Equal("Error", dialog.LastTitle);
            Assert.Contains("Failed to add user", dialog.LastMessage);
            Assert.Contains("User rows were refreshed from saved data", dialog.LastMessage);
        }

        [Fact]
        public async Task DeleteUserFromRowCommand_WhenDeleteFailsAfterPersistence_RefreshesRowsAndClearsDeletedSelection()
        {
            var deleteTarget = new UserModel { UserID = 11, UserName = "remove-me", Role = "Workshop Staff" };
            var keepTarget = new UserModel { UserID = 12, UserName = "keep-me", Role = "Advisor" };
            var service = new StubUserService { ThrowAfterDelete = true };
            service.Users.Add(deleteTarget);
            service.Users.Add(keepTarget);
            var dialog = new StubDialogService();
            var vm = new UserManagementViewModel(service, new StubFileDialogService(), dialog);

            await vm.LoadUsersAsync();
            vm.SelectedUser = deleteTarget;
            vm.UserSearchText = "keep";
            vm.SearchUsersCommand.Execute(null);

            await vm.DeleteUserFromRowCommand.ExecuteAsync(deleteTarget);

            var user = Assert.Single(vm.Users);
            Assert.Same(keepTarget, user);
            Assert.DoesNotContain(service.Users, existing => existing.UserID == deleteTarget.UserID);
            Assert.Null(vm.SelectedUser);
            Assert.Equal("keep", vm.UserSearchText);
            Assert.Equal("Error", dialog.LastTitle);
            Assert.Contains("Failed to delete user", dialog.LastMessage);
            Assert.Contains("User rows were refreshed from saved data", dialog.LastMessage);
        }

        [Fact]
        public async Task DeleteUserFromRowCommand_WhenDeleteAndRecoveryRefreshFail_ClearsRowsAndSelection()
        {
            var deleteTarget = new UserModel { UserID = 21, UserName = "remove-me", Role = "Workshop Staff" };
            var service = new StubUserService { ThrowAfterDelete = true };
            service.Users.Add(deleteTarget);
            var dialog = new StubDialogService();
            var vm = new UserManagementViewModel(service, new StubFileDialogService(), dialog);

            await vm.LoadUsersAsync();
            vm.SelectedUser = deleteTarget;
            service.ThrowOnGetAllUsersAfterMutation = true;

            await vm.DeleteUserFromRowCommand.ExecuteAsync(deleteTarget);

            Assert.Empty(vm.Users);
            Assert.Null(vm.SelectedUser);
            Assert.False(vm.UpdateUserCommand.CanExecute(null));
            Assert.False(vm.EditUserCommand.CanExecute(null));
            Assert.Equal("Error", dialog.LastTitle);
            Assert.Contains("User rows were cleared because the recovery refresh failed", dialog.LastMessage);
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
            public bool ThrowOnGetAllUsersAfterMutation { get; set; }
            public bool ThrowAfterAdd { get; set; }
            public bool ThrowAfterDelete { get; set; }
            public int UpdateCallCount { get; private set; }

            public Task<List<UserModel>> GetAllUsersAsync(CancellationToken cancellationToken = default)
            {
                if (ThrowOnGetAllUsers || ThrowOnGetAllUsersAfterMutation)
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
                if (user.UserID == 0)
                    user.UserID = Users.Count == 0 ? 1 : Users.Max(existing => existing.UserID) + 1;

                Users.Add(user);

                if (ThrowAfterAdd)
                    throw new InvalidOperationException("Add failed after save");

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

                if (ThrowAfterDelete)
                    throw new InvalidOperationException("Delete failed after save");

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
