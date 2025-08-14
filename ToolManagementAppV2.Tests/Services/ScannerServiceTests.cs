using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ToolManagementAppV2.Services.Core;
using ToolManagementAppV2.Services.Devices;
using ToolManagementAppV2.Services.Settings;
using Xunit;

namespace ToolManagementAppV2.Tests.Services
{
    public class ScannerServiceTests
    {
        [Fact]
        public async Task GetScannerDevicesAsync_ReturnsConfiguredDevices()
        {
            var dbPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".db");
            try
            {
                var db = new DatabaseService(dbPath);
                var settings = new SettingsService(db);
                settings.SaveScannerIpAddresses(new[] { "127.0.0.1" });

                var service = new ScannerService(settings);

                var before = DateTime.UtcNow;
                var devices = await service.GetScannerDevicesAsync();
                var after = DateTime.UtcNow;

                var list = devices.ToList();
                Assert.Single(list);
                Assert.Equal("127.0.0.1", list[0].Ip);
                Assert.InRange(list[0].LastSeen, before, after);
            }
            finally
            {
                if (File.Exists(dbPath)) File.Delete(dbPath);
            }
        }
    }
}
