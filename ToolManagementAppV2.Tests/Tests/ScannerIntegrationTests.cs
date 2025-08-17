using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using ToolManagementAppV2.Services.Core;
using ToolManagementAppV2.Services.Devices;
using ToolManagementAppV2.Services.Settings;
using Xunit;

namespace ToolManagementAppV2.Tests.Tests
{
    public class ScannerIntegrationTests
    {
        [Fact]
        public async Task GetScannerDevicesAsync_ScansLocalhost()
        {
            var dbPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".db");
            try
            {
                var db = new DatabaseService(dbPath);
                var settings = new SettingsService(db);
                await settings.SaveScannerIpAddressesAsync(new[] { "127.0.0.1" });

                var service = new ScannerService(settings);
                var devices = await service.GetScannerDevicesAsync(CancellationToken.None);
                var device = Assert.Single(devices);
                Assert.Equal("127.0.0.1", device.Ip);
                Assert.Equal("Online", device.Status);
            }
            finally
            {
                if (File.Exists(dbPath)) File.Delete(dbPath);
            }
        }
    }
}

