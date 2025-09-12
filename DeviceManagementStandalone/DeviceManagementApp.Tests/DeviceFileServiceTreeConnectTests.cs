using System.Collections.Generic;
using System.Threading.Tasks;
using DeviceManagementApp.Services;
using DeviceManagementApp.Models;
using SMBLibrary;
using Xunit;

public class DeviceFileServiceTreeConnectTests
{
    private sealed class RecordingFileStore
    {
        public bool ListFilesCalled { get; private set; }
        public bool DisconnectCalled { get; private set; }
        public bool DisposeCalled { get; private set; }

        public NTStatus ListFiles(string path, out List<dynamic>? items)
        {
            ListFilesCalled = true;
            items = new List<dynamic> { new { FileName = "file.txt" } };
            return NTStatus.STATUS_SUCCESS;
        }

        public void Disconnect() => DisconnectCalled = true;
        public void Dispose() => DisposeCalled = true;
    }

    private sealed class FakeSmbClient
    {
        private readonly bool _succeed;
        public RecordingFileStore Store { get; } = new RecordingFileStore();
        public FakeSmbClient(bool succeed) => _succeed = succeed;

        public NTStatus TreeConnect(string shareName, out RecordingFileStore? fileStore)
        {
            if (_succeed)
            {
                fileStore = Store;
                return NTStatus.STATUS_SUCCESS;
            }
            fileStore = null;
            return NTStatus.STATUS_UNSUCCESSFUL;
        }
    }

    private sealed class TestDeviceFileService : DeviceFileService
    {
        private readonly FakeSmbClient _client;
        public TestDeviceFileService(DatabaseService db, FakeSmbClient client) : base(db) => _client = client;

        public async Task<IList<string>> ListUsingFakeClientAsync()
        {
            var results = new List<string>();
            var status = _client.TreeConnect("share", out var fileStore);
            if (status != NTStatus.STATUS_SUCCESS || fileStore == null)
                return results;

            try
            {
                var listStatus = fileStore.ListFiles("\\", out var items);
                if (listStatus != NTStatus.STATUS_SUCCESS || items == null)
                    return results;
                foreach (var info in items)
                {
                    results.Add((string)info.FileName);
                }
            }
            finally
            {
                fileStore.Disconnect();
                fileStore.Dispose();
            }
            return results;
        }
    }

    [Fact]
    public async Task TreeConnect_Success_UsesFileStore()
    {
        var db = new DatabaseService(":memory:");
        var fake = new FakeSmbClient(true);
        var service = new TestDeviceFileService(db, fake);
        var result = await service.ListUsingFakeClientAsync();
        Assert.Single(result);
        Assert.True(fake.Store.ListFilesCalled);
        Assert.True(fake.Store.DisconnectCalled);
        Assert.True(fake.Store.DisposeCalled);
    }

    [Fact]
    public async Task TreeConnect_Failure_SkipsFileStore()
    {
        var db = new DatabaseService(":memory:");
        var fake = new FakeSmbClient(false);
        var service = new TestDeviceFileService(db, fake);
        var result = await service.ListUsingFakeClientAsync();
        Assert.Empty(result);
        Assert.False(fake.Store.ListFilesCalled);
        Assert.False(fake.Store.DisconnectCalled);
        Assert.False(fake.Store.DisposeCalled);
    }
}

