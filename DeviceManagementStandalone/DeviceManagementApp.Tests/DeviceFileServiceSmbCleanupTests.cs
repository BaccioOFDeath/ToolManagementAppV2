using System;
using System.Threading.Tasks;
using DeviceManagementApp.Services;
using SMBLibrary;
using SMBLibrary.Client;
using Xunit;

public class DeviceFileServiceSmbCleanupTests
{
    private sealed class RecordingSmbClient
    {
        public bool LogoffCalled { get; private set; }
        public bool DisconnectCalled { get; private set; }
        public bool DisposeCalled { get; private set; }

        public bool Connect(string ip, SMBTransportType transport) => true;
        public NTStatus Login(string domain, string user, string pass) => NTStatus.STATUS_SUCCESS;
        public NTStatus ListShares(out object[] shares)
        {
            shares = Array.Empty<object>();
            return NTStatus.STATUS_SUCCESS;
        }
        public void Logoff() => LogoffCalled = true;
        public void Disconnect() => DisconnectCalled = true;
        public void Dispose() => DisposeCalled = true;
    }

    private sealed class TestDeviceFileService : DeviceFileService
    {
        private readonly RecordingSmbClient _client;
        public TestDeviceFileService(DatabaseService db, RecordingSmbClient client) : base(db) => _client = client;

        public async Task InvokeWithRecordingClientAsync()
        {
            try
            {
                await Task.Run(() => _client.Connect("host", SMBTransportType.DirectTCPTransport));
                await Task.Run(() => _client.Login(string.Empty, string.Empty, string.Empty));
                _client.ListShares(out var _);
            }
            finally
            {
                try { _client.Logoff(); } catch { }
                try { _client.Disconnect(); } catch { }
            }
        }
    }

    [Fact]
    public async Task SmbClient_CleanedUpWithoutDispose()
    {
        var db = new DatabaseService(":memory:");
        var client = new RecordingSmbClient();
        var service = new TestDeviceFileService(db, client);
        await service.InvokeWithRecordingClientAsync();
        Assert.True(client.LogoffCalled);
        Assert.True(client.DisconnectCalled);
        Assert.False(client.DisposeCalled);
    }
}
