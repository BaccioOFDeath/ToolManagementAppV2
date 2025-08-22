using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using InventoryManagementApp.Services.Core;
using InventoryManagementApp.Services.Devices;
using InventoryManagementApp.Services.Settings;
using Xunit;

namespace InventoryManagementApp.Tests.Services
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
                await settings.SaveScannerIpAddressesAsync(new[] { "127.0.0.1" });

                var service = new ScannerService(settings);

                var before = DateTime.UtcNow.ToLocalTime();
                var devices = await service.GetScannerDevicesAsync(CancellationToken.None);
                var after = DateTime.UtcNow.ToLocalTime();

                var list = devices.ToList();
                Assert.Single(list);
                Assert.Equal("127.0.0.1", list[0].Ip);
                Assert.NotNull(list[0].LastSeen);
                Assert.InRange(list[0].LastSeen!.Value, before, after);
                Assert.Equal(DateTimeKind.Local, list[0].LastSeen!.Value.Kind);
            }
            finally
            {
                if (File.Exists(dbPath)) File.Delete(dbPath);
            }
        }

        [Fact]
        public async Task GetScannerDevicesAsync_ReturnsAllConfiguredDevices()
        {
            var dbPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".db");
            try
            {
                var db = new DatabaseService(dbPath);
                var settings = new SettingsService(db);
                await settings.SaveScannerIpAddressesAsync(new[] { "127.0.0.1", "127.0.0.2" });

                var service = new ScannerService(settings);

                var devices = await service.GetScannerDevicesAsync(CancellationToken.None);
                var list = devices.ToList();
                Assert.Equal(2, list.Count);
                Assert.Contains(list, d => d.Ip == "127.0.0.1");
                Assert.Contains(list, d => d.Ip == "127.0.0.2");
            }
            finally
            {
                if (File.Exists(dbPath)) File.Delete(dbPath);
            }
        }

        [Fact]
        public async Task GetScannerDevicesAsync_DeduplicatesIps()
        {
            var dbPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".db");
            try
            {
                var db = new DatabaseService(dbPath);
                var settings = new SettingsService(db);
                await settings.SaveScannerIpAddressesAsync(new[] { "127.0.0.1", "127.0.0.1" });

                var service = new ScannerService(settings);

                var devices = await service.GetScannerDevicesAsync(CancellationToken.None);
                var list = devices.ToList();
                Assert.Single(list);
                Assert.Equal("127.0.0.1", list[0].Ip);
            }
            finally
            {
                if (File.Exists(dbPath)) File.Delete(dbPath);
            }
        }
        [Fact]
        public async Task GetScannerDevicesAsync_HonorsCancellation()
        {
            var dbPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".db");
            try
            {
                var db = new DatabaseService(dbPath);
                var settings = new SettingsService(db);
                await settings.SaveScannerIpAddressesAsync(new[] { "127.0.0.1" });

                var service = new ScannerService(settings);

                using var cts = new CancellationTokenSource();
                cts.Cancel();

                await Assert.ThrowsAsync<OperationCanceledException>(
                    () => service.GetScannerDevicesAsync(cts.Token));
            }
            finally
            {
                if (File.Exists(dbPath)) File.Delete(dbPath);
            }
        }
    }
}
