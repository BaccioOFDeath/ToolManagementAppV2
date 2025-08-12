using System.IO;
using ToolManagementAppV2.Services.Core;
using ToolManagementAppV2.Services.Users;
using ToolManagementAppV2.ViewModels;
using Xunit;

namespace ToolManagementAppV2.Tests.ViewModels
{
    public class ActivityLogsViewModelTests
    {
        [Fact]
        public void Constructor_LoadsLogs()
        {
            var dbPath = Path.GetTempFileName();
            try
            {
                var db = new DatabaseService(dbPath);
                var service = new ActivityLogService(db);
                service.LogAction(1, "user", "action");
                var vm = new ActivityLogsViewModel(service);
                Assert.Single(vm.Logs);
            }
            finally
            {
                if (File.Exists(dbPath))
                    File.Delete(dbPath);
            }
        }

        [Fact]
        public void RefreshCommand_ReloadsLogs()
        {
            var dbPath = Path.GetTempFileName();
            try
            {
                var db = new DatabaseService(dbPath);
                var service = new ActivityLogService(db);
                service.LogAction(1, "user", "first");
                var vm = new ActivityLogsViewModel(service);
                Assert.Single(vm.Logs);
                service.LogAction(1, "user", "second");
                Assert.Single(vm.Logs);
                vm.RefreshCommand.Execute(null);
                Assert.Equal(2, vm.Logs.Count);
            }
            finally
            {
                if (File.Exists(dbPath))
                    File.Delete(dbPath);
            }
        }
    }
}
