using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using InventoryManagementApp.Interfaces;
using InventoryManagementApp.Models;
using InventoryManagementApp.Services.Core;
using InventoryManagementApp.Services.Devices;
using InventoryManagementApp.Services.Settings;
using InventoryManagementApp.ViewModels;
using Xunit;

namespace InventoryManagementApp.Tests.ViewModels
{
    class FakeScannerService : IScannerService
    {
        public int CallCount { get; private set; }

        public Task<IEnumerable<ScannerDevice>> GetScannerDevicesAsync(CancellationToken cancellationToken)
        {
            CallCount++;
            IEnumerable<ScannerDevice> result = new[]
            {
                new ScannerDevice { Name = "A", Ip = "1.1.1.1", Status = "Online", LastSeen = DateTime.UtcNow }
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
            var vm = new ScannerStatusViewModel(svc, new StubDialogService());
            await vm.RefreshCommand.ExecuteAsync(null);
            Assert.Single(vm.Devices);
            Assert.Equal(1, svc.CallCount);
            Assert.Equal(DateTimeKind.Local, vm.Devices[0].LastSeen.Kind);
        }

        [Fact]
        public void AutoRefresh_TogglesTimer()
        {
            var svc = new FakeScannerService();
            var vm = new ScannerStatusViewModel(svc, new StubDialogService());
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
                await settings.SaveScannerIpAddressesAsync(new[] { "127.0.0.1" });
                var svc = new ScannerService(settings);
                var vm = new ScannerStatusViewModel(svc, new StubDialogService());
                await vm.RefreshCommand.ExecuteAsync(null);
                Assert.Single(vm.Devices);
                Assert.Equal("127.0.0.1", vm.Devices[0].Ip);
            }
            finally
            {
                if (File.Exists(dbPath)) File.Delete(dbPath);
            }
        }

        [Fact]
        public async Task RefreshCommand_ShowsDialog_OnServiceFailure()
        {
            var svc = new ThrowingScannerService();
            var dialog = new StubDialogService();
            var vm = new ScannerStatusViewModel(svc, dialog);
            await vm.RefreshCommand.ExecuteAsync(null);
            Assert.True(dialog.InfoShown);
            Assert.Empty(vm.Devices);
        }

        [Fact]
        public void Dispose_StopsTimer()
        {
            var svc = new FakeScannerService();
            var vm = new ScannerStatusViewModel(svc, new StubDialogService());
            vm.AutoRefresh = true;
            Assert.True(vm.IsTimerRunning);
            vm.Dispose();
            Assert.False(vm.IsTimerRunning);
        }
    }

    class ThrowingScannerService : IScannerService
    {
        public Task<IEnumerable<ScannerDevice>> GetScannerDevicesAsync(CancellationToken cancellationToken) => throw new InvalidOperationException("fail");
    }

    class StubDialogService : IDialogService
    {
        public bool InfoShown;
        public void ShowInfo(string message, string title) => InfoShown = true;
        public bool ShowConfirmation(string message, string title) => false;
        public ItemModel? ShowEditItemDialog(ItemModel item) => null;
        public void ShowItemDetails(ItemModel item) { }
        public (CustomerModel customer, DateTime dueDate)? ShowRentItemDialog(ItemModel item, IEnumerable<CustomerModel> customers) => null;
        public CustomerModel? ShowAddCustomerDialog() => null;
        public void ShowRentalsFilter(ManageRentalsViewModel viewModel) { }
        public void ShowRentalHistory(ItemModel item, IEnumerable<RentalModel> history) { }
        public Dictionary<string, string>? ShowImportMapping(IEnumerable<string> headers, IEnumerable<string> properties, IEnumerable<string>? requiredPropertyNames = null) => null;
        public Func<ItemModel, IEnumerable<string>>? ShowImageImportMapping() => null;
        public void ShowPrintPreview(System.Windows.Documents.FlowDocument document, string title, string description) { }
        public void ShowPrintLabelDialog() { }
    }
}
