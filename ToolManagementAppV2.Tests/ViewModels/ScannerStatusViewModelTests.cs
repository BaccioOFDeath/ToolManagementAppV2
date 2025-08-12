using System;
using System.Collections.Generic;
using ToolManagementAppV2.Interfaces;
using ToolManagementAppV2.Models;
using ToolManagementAppV2.ViewModels;
using Xunit;

namespace ToolManagementAppV2.Tests.ViewModels
{
    class FakeScannerService : IScannerService
    {
        public int CallCount { get; private set; }

        public IEnumerable<ScannerDevice> GetScannerDevices()
        {
            CallCount++;
            return new[]
            {
                new ScannerDevice { Name = "A", Ip = "1.1.1.1", Status = "Online", LastSeen = DateTime.Now }
            };
        }
    }

    public class ScannerStatusViewModelTests
    {
        [Fact]
        public void RefreshCommand_PopulatesDevices()
        {
            var svc = new FakeScannerService();
            var vm = new ScannerStatusViewModel(svc);
            vm.RefreshCommand.Execute(null);
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
    }
}
