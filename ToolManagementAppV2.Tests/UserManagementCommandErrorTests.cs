using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using ToolManagementAppV2.Interfaces;
using ToolManagementAppV2.Models.Domain;
using ToolManagementAppV2.ViewModels;
using Xunit;

namespace ToolManagementAppV2.Tests;

public class UserManagementCommandErrorTests
{
    class ThrowingUserService : IUserService
    {
        public List<User> GetAllUsers() => new();
        public Task<List<User>> GetAllUsersAsync() => Task.FromResult(new List<User>());
        public User? GetUserByID(int userID) => null;
        public Task<User?> GetUserByIDAsync(int userID) => Task.FromResult<User?>(null);
        public User? AuthenticateUser(string userName, string password) => null;
        public Task<User?> AuthenticateUserAsync(string userName, string password) => Task.FromResult<User?>(null);
        public User? GetCurrentUser() => null;
        public Task<User?> GetCurrentUserAsync() => Task.FromResult<User?>(null);
        public void AddUser(User user) { }
        public Task AddUserAsync(User user) => Task.CompletedTask;
        public void UpdateUser(User user) { }
        public Task UpdateUserAsync(User user) => Task.CompletedTask;
        public Task<bool> TryDeleteUserAsync(int userID) => throw new InvalidOperationException("fail");
        public bool ChangeUserPassword(int userID, string newPassword) => false;
        public Task<bool> ChangeUserPasswordAsync(int userID, string newPassword) => Task.FromResult(false);
    }

    class StubFileDialogService : IFileDialogService
    {
        public string OpenFile(string filter) => string.Empty;
        public string SaveFile(string filter) => string.Empty;
    }

    [Fact]
    public async Task DeleteUserCommand_LogsAndShowsDialogOnError()
    {
        var logger = new TestLogger<UserManagementViewModel>();
        var dialog = new RecordingDialogService();
        var vm = new UserManagementViewModel(new ThrowingUserService(), new StubFileDialogService(), dialog, logger);
        var user = new UserModel { UserID = 1 };
        await vm.DeleteUserFromRowCommand.ExecuteAsync(user);
        Assert.Contains(logger.Entries, e => e.Level == LogLevel.Error && e.Exception is InvalidOperationException);
        Assert.Contains(dialog.Messages, m => m.Contains("Failed to delete user"));
    }
}
