using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using ToolManagementAppV2.Interfaces;
using ToolManagementAppV2.Models;
using ToolManagementAppV2.Services.Core;
using ToolManagementAppV2.Services.Devices;
using ToolManagementAppV2.Services.Settings;
using ToolManagementAppV2.ViewModels;
using Xunit;

namespace ToolManagementAppV2.Tests.ViewModels
{
    class FakeScannerService : IScannerService
    {
        public int CallCount { get; private set; }

        public Task<IEnumerable<ScannerDevice>> GetScannerDevicesAsync()
        {
            CallCount++;
            IEnumerable<ScannerDevice> result = new[]
            {
                new ScannerDevice { Name = "A", Ip = "1.1.1.1", Status = "Online", LastSeen = DateTime.Now }
            };
            return Task.FromResult(result);
        }
    }

    public class ScannerStatusViewModelTests
    {
        [Fact]
        public async Task RefreshCommand_PopulatesDevices()
        {
            var svc = new FakeScannerService();
            var vm = new ScannerStatusViewModel(svc);
            await vm.RefreshCommand.ExecuteAsync(null);
            Assert.Single(vm.Devices);
            Assert.Equal(1, svc.CallCount);
        }

        [Fact]
        public void AutoRefresh_TogglesTimer()
        {
            var svc = new FakeScannerService();
            var vm = new ScannerStatusViewModel(svc);
            vm.AutoRefresh = true;
            Assert.True(vm.IsTimerRunning);
            vm.AutoRefresh = false;
            Assert.False(vm.IsTimerRunning);
        }

        [Fact]
        public async Task RefreshCommand_UsesIpsFromSettings()
        {
            var dbPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + ".db");
            try
            {
                var db = new DatabaseService(dbPath);
                var settings = new SettingsService(db);
                settings.SaveScannerIpAddresses(new[] { "127.0.0.1" });
                var svc = new ScannerService(settings);
                var vm = new ScannerStatusViewModel(svc);
                await vm.RefreshCommand.ExecuteAsync(null);
                Assert.Single(vm.Devices);
                Assert.Equal("127.0.0.1", vm.Devices[0].Ip);
            }
            finally
            {
                if (File.Exists(dbPath)) File.Delete(dbPath);
            }
        }
    }
}
