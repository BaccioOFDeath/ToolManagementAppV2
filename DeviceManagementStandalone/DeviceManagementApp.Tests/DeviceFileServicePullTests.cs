using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using DeviceManagementApp.Models;
using DeviceManagementApp.Services;
using Xunit;

public class DeviceFileServicePullTests
{
    private sealed class TestDeviceFileService : DeviceFileService
    {
        private readonly string _remoteDir;
        public TestDeviceFileService(DatabaseService db, string remoteDir) : base(db) => _remoteDir = remoteDir;
        public override Task<System.Collections.Generic.IEnumerable<string>> ListFilesAsync(Device device, string? extensionFilter = null, System.Threading.CancellationToken cancellationToken = default)
        {
            var files = Directory.GetFiles(_remoteDir).Select(Path.GetFileName)!;
            return Task.FromResult<System.Collections.Generic.IEnumerable<string>>(files);
        }
        protected override Task<byte[]?> DownloadFileAsync(Device device, string file, System.Threading.CancellationToken cancellationToken)
        {
            var path = Path.Combine(_remoteDir, file);
            byte[] data = File.ReadAllBytes(path);
            return Task.FromResult<byte[]?>(data);
        }
    }

    [Fact]
    public async Task DownloadUnseenFiles_AvoidsDuplicates()
    {
        var remote = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(remote);
        await File.WriteAllTextAsync(Path.Combine(remote, "a.txt"), "hello");
        var dbPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".db");
        using var db = new DatabaseService(dbPath);
        var service = new TestDeviceFileService(db, remote);
        var device = new Device { Ip = "dev1" };
        var local = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        var first = await service.DownloadUnseenFilesAsync(device, local);
        var second = await service.DownloadUnseenFilesAsync(device, local);
        Assert.Equal(1, first);
        Assert.Equal(0, second);
        var storedDir = Path.Combine(local, "Devices", device.Ip);
        Assert.True(File.Exists(Path.Combine(storedDir, "a.txt")));
    }

    [Fact]
    public async Task DownloadUnseenFiles_IncludesPortInFolderName()
    {
        var remote = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(remote);
        await File.WriteAllTextAsync(Path.Combine(remote, "a.txt"), "hello");
        var dbPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".db");
        using var db = new DatabaseService(dbPath);
        var service = new TestDeviceFileService(db, remote);
        var device = new Device { Ip = "dev1", Port = 1234 };
        var local = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        await service.DownloadUnseenFilesAsync(device, local);
        var storedDir = Path.Combine(local, "Devices", "dev1_1234");
        Assert.True(Directory.Exists(storedDir));
    }
}
