using System;
using System.IO;
using System.Threading.Tasks;
using ToolManagementAppV2.Models;
using ToolManagementAppV2.Models.Domain;
using ToolManagementAppV2.Services.Core;
using ToolManagementAppV2.Services.Users;
using Xunit;

public class UserLoggingTests
{
    [Fact]
    public async Task AuthenticateUserAsync_LogsActivity()
    {
        var dbPath = Path.GetTempFileName();
        try
        {
            var dbService = new DatabaseService(dbPath);
            var ctx = new ApplicationUserContext();
            var logService = new ActivityLogService(dbService);
            var userService = new UserService(dbService, ctx, null, null, logService);
            await userService.AddUserAsync(new User { UserName = "log", PasswordHash = "Strong1!" });
            var auth = await userService.AuthenticateUserAsync("log", "Strong1!");
            Assert.Equal(AuthenticationResult.Success, auth.Result);
            var logs = await logService.GetRecentLogsAsync();
            Assert.Contains(logs.Value, l => l.Action.Contains("User login"));
        }
        finally
        {
            if (File.Exists(dbPath)) File.Delete(dbPath);
        }
    }
}

