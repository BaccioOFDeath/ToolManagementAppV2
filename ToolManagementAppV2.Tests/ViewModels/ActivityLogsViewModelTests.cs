using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using ToolManagementAppV2.Models.Domain;
using ToolManagementAppV2.Services.Core;
using ToolManagementAppV2.Services.Users;
using ToolManagementAppV2.ViewModels;
using Xunit;

namespace ToolManagementAppV2.Tests.ViewModels
{
    public class ActivityLogsViewModelTests
    {
        [Fact]
        public async Task Constructor_LoadsLogs()
        {
            var dbPath = Path.GetTempFileName();
            try
            {
                var db = new DatabaseService(dbPath);
                var service = new ActivityLogService(db);
                service.LogAction(1, "user", "action");
                var vm = new ActivityLogsViewModel(service);
                await vm.RefreshCommand.ExecutionTask;
                Assert.Single(vm.Logs);
            }
            finally
            {
                if (File.Exists(dbPath))
                    File.Delete(dbPath);
            }
        }

        [Fact]
        public async Task RefreshCommand_ReloadsLogs()
        {
            var dbPath = Path.GetTempFileName();
            try
            {
                var db = new DatabaseService(dbPath);
                var service = new ActivityLogService(db);
                service.LogAction(1, "user", "first");
                var vm = new ActivityLogsViewModel(service);
                await vm.RefreshCommand.ExecutionTask;
                Assert.Single(vm.Logs);
                service.LogAction(1, "user", "second");
                Assert.Single(vm.Logs);
                await vm.RefreshCommand.ExecuteAsync(null);
                Assert.Equal(2, vm.Logs.Count);
            }
            finally
            {
                if (File.Exists(dbPath))
                    File.Delete(dbPath);
            }
        }

        [Fact]
        public async Task LoadLogs_ReturnsFalse_OnFailure()
        {
            var dbPath = Path.GetTempFileName();
            try
            {
                var db = new DatabaseService(dbPath);
                var service = new FailingActivityLogService(db);
                var vm = new ActivityLogsViewModel(service);
                Assert.False(await vm.LoadLogsAsync());
                Assert.Empty(vm.Logs);
            }
            finally
            {
                if (File.Exists(dbPath))
                    File.Delete(dbPath);
            }
        }

        class FailingActivityLogService : ActivityLogService
        {
            public FailingActivityLogService(DatabaseService db) : base(db) { }
            public override Task<List<ActivityLog>?> GetRecentLogsAsync(int count = 50) => throw new Exception("fail");
        }
    }
}
