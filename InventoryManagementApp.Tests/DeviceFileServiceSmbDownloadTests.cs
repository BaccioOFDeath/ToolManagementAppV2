using System;
using System.Threading.Tasks;
using InventoryManagementApp.Models;
using InventoryManagementApp.Services.Core;
using InventoryManagementApp.Services.Devices;
using SMBLibrary;
using Xunit;

public class DeviceFileServiceSmbDownloadTests
{
    private sealed class RecordingFileStore
    {
        private readonly byte[] _data;
        private readonly bool _openSucceeds;
        public bool CloseFileCalled { get; private set; }
        public bool DisconnectCalled { get; private set; }
        public bool DisposeCalled { get; private set; }

        public RecordingFileStore(byte[] data, bool openSucceeds = true)
        {
            _data = data;
            _openSucceeds = openSucceeds;
        }

        public NTStatus CreateFile(out object handle, string path, AccessMask accessMask, System.IO.FileAttributes fileAttributes,
            ShareAccess shareAccess, CreateDisposition disposition, CreateOptions options, out object fileInfo)
        {
            fileInfo = new object();
            handle = new object();
            return _openSucceeds ? NTStatus.STATUS_SUCCESS : NTStatus.STATUS_UNSUCCESSFUL;
        }

        public NTStatus ReadFile(out byte[] data, object handle, long offset, int length)
        {
            data = _data;
            return NTStatus.STATUS_SUCCESS;
        }

        public void CloseFile(object handle) => CloseFileCalled = true;
        public void Disconnect() => DisconnectCalled = true;
        public void Dispose() => DisposeCalled = true;
    }

    private sealed class RecordingSmbClient
    {
        private readonly RecordingFileStore _store;
        private readonly bool _treeConnectSucceeds;

        public bool LogoffCalled { get; private set; }
        public bool DisconnectCalled { get; private set; }

        public RecordingSmbClient(RecordingFileStore store, bool treeConnectSucceeds = true)
        {
            _store = store;
            _treeConnectSucceeds = treeConnectSucceeds;
        }

        public bool Connect(string ip, SMBTransportType transport) => true;
        public NTStatus Login(string domain, string user, string password) => NTStatus.STATUS_SUCCESS;
        public NTStatus ListShares(out object[] shares)
        {
            shares = new object[] { "DATA" };
            return NTStatus.STATUS_SUCCESS;
        }
        public NTStatus TreeConnect(string shareName, out RecordingFileStore? fileStore)
        {
            if (_treeConnectSucceeds)
            {
                fileStore = _store;
                return NTStatus.STATUS_SUCCESS;
            }
            fileStore = null;
            return NTStatus.STATUS_UNSUCCESSFUL;
        }
        public void Logoff() => LogoffCalled = true;
        public void Disconnect() => DisconnectCalled = true;
    }

    private sealed class TestDeviceFileService : DeviceFileService
    {
        public TestDeviceFileService(DatabaseService db, Func<object> smbFactory)
            : base(db, null, smbFactory) { }

        public Task<byte[]?> InvokeDownloadAsync(Device device, string file)
            => base.DownloadFileAsync(device, file, default);
    }

    [Fact]
    public async Task SmbDownload_Succeeds()
    {
        var db = new DatabaseService(":memory:");
        var data = new byte[] { 1, 2, 3 };
        var store = new RecordingFileStore(data);
        var client = new RecordingSmbClient(store);
        var service = new TestDeviceFileService(db, () => client);
        var device = new Device { Ip = "host", Protocol = DeviceProtocol.Smb };

        var result = await service.InvokeDownloadAsync(device, "file.txt");

        Assert.NotNull(result);
        Assert.Equal(data, result);
        Assert.True(store.CloseFileCalled);
        Assert.True(store.DisconnectCalled);
        Assert.True(store.DisposeCalled);
        Assert.True(client.LogoffCalled);
        Assert.True(client.DisconnectCalled);
    }

    [Fact]
    public async Task SmbDownload_FailureHandled()
    {
        var db = new DatabaseService(":memory:");
        var store = new RecordingFileStore(Array.Empty<byte>(), openSucceeds: false);
        var client = new RecordingSmbClient(store);
        var service = new TestDeviceFileService(db, () => client);
        var device = new Device { Ip = "host", Protocol = DeviceProtocol.Smb };

        var result = await service.InvokeDownloadAsync(device, "missing.txt");

        Assert.Null(result);
        Assert.True(store.DisconnectCalled);
        Assert.True(store.DisposeCalled);
        Assert.True(client.LogoffCalled);
        Assert.True(client.DisconnectCalled);
    }
}
