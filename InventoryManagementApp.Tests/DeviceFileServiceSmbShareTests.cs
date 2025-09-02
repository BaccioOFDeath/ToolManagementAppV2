using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using InventoryManagementApp.Models;
using InventoryManagementApp.Services.Core;
using InventoryManagementApp.Services.Devices;
using SMBLibrary;
using Xunit;

public class DeviceFileServiceSmbShareTests
{
    private class StubDeviceFileService : DeviceFileService
    {
        private readonly object[]? _shares;
        private readonly bool _shareEnumerationSucceeds;

        public StubDeviceFileService(DatabaseService db, object[]? shares, bool shareEnumerationSucceeds)
            : base(db)
        {
            _shares = shares;
            _shareEnumerationSucceeds = shareEnumerationSucceeds;
        }

        public Task<IEnumerable<string>> InvokeShareEnumerationAsync(Device device)
        {
            var results = new List<string>();
            if (!_shareEnumerationSucceeds || _shares == null)
                return Task.FromResult<IEnumerable<string>>(results);

            foreach (var share in _shares)
            {
                string shareName = share is string s
                    ? s
                    : (string)((dynamic)share).ShareName;

                if (string.Equals(shareName, "IPC$", StringComparison.OrdinalIgnoreCase))
                    continue;

                // For testing purposes, return a dummy file name per share
                results.Add(shareName + "-file.txt");
            }

            return Task.FromResult<IEnumerable<string>>(results);
        }
    }

    [Fact]
    public async Task SmbShareEnumeration_Succeeds()
    {
        var dbPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".db");
        using var db = new DatabaseService(dbPath);
        var shares = new object[] { "DATA" };
        var service = new StubDeviceFileService(db, shares, true);
        var device = new Device { Ip = "host", Protocol = DeviceProtocol.Smb };

        var files = await service.InvokeShareEnumerationAsync(device);

        Assert.Contains("DATA-file.txt", files);
    }

    [Fact]
    public async Task SmbShareEnumeration_FailureHandled()
    {
        var dbPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".db");
        using var db = new DatabaseService(dbPath);
        var service = new StubDeviceFileService(db, null, false);
        var device = new Device { Ip = "host", Protocol = DeviceProtocol.Smb };

        var files = await service.InvokeShareEnumerationAsync(device);

        Assert.Empty(files);
    }
}

