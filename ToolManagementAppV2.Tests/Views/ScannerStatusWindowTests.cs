using System;
using System.Threading;
using ToolManagementAppV2.Views;
using ToolManagementAppV2.Interfaces;
using Xunit;

namespace ToolManagementAppV2.Tests.Views
{
    public class ScannerStatusWindowTests
    {
        [Fact]
        public void DisposesDataContextWhenClosed()
        {
            Exception? threadException = null;
            var disposed = false;

            var thread = new Thread(() =>
            {
                try
                {
                    var window = new ScannerStatusWindow(new StubScannerService(), new StubDialogService());
                    window.DataContext = new DisposableVm(() => disposed = true);
                    window.Close();
                }
                catch (Exception ex)
                {
                    threadException = ex;
                }
            });

            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            thread.Join();

            if (threadException != null)
                throw threadException!;

            Assert.True(disposed);
        }

        class DisposableVm : IDisposable
        {
            readonly Action _onDispose;
            public DisposableVm(Action onDispose) => _onDispose = onDispose;
            public void Dispose() => _onDispose();
        }

        class StubScannerService : IScannerService
        {
            public System.Threading.Tasks.Task<System.Collections.Generic.IEnumerable<ToolManagementAppV2.Models.ScannerDevice>> GetScannerDevicesAsync(System.Threading.CancellationToken cancellationToken)
                => System.Threading.Tasks.Task.FromResult<System.Collections.Generic.IEnumerable<ToolManagementAppV2.Models.ScannerDevice>>(Array.Empty<ToolManagementAppV2.Models.ScannerDevice>());
        }

        class StubDialogService : IDialogService
        {
            public void ShowInfo(string message, string title) { }
            public bool ShowConfirmation(string message, string title) => false;
            public ToolManagementAppV2.Models.Domain.ToolModel? ShowEditToolDialog(ToolManagementAppV2.Models.Domain.ToolModel tool) => null;
            public void ShowToolDetails(ToolManagementAppV2.Models.Domain.ToolModel tool) { }
            public (ToolManagementAppV2.Models.Domain.CustomerModel customer, DateTime dueDate)? ShowRentToolDialog(ToolManagementAppV2.Models.Domain.ToolModel tool, System.Collections.Generic.IEnumerable<ToolManagementAppV2.Models.Domain.CustomerModel> customers) => null;
            public ToolManagementAppV2.Models.Domain.CustomerModel? ShowAddCustomerDialog() => null;
            public void ShowRentalsFilter(ToolManagementAppV2.ViewModels.ManageRentalsViewModel viewModel) { }
            public void ShowRentalHistory(ToolManagementAppV2.Models.Domain.ToolModel tool, System.Collections.Generic.IEnumerable<ToolManagementAppV2.Models.Domain.RentalModel> history) { }
            public System.Collections.Generic.Dictionary<string, string>? ShowImportMapping(System.Collections.Generic.IEnumerable<string> headers, System.Collections.Generic.IEnumerable<string> properties) => null;
            public System.Func<ToolManagementAppV2.Models.Domain.ToolModel, System.Collections.Generic.IEnumerable<string>>? ShowImageImportMapping() => null;
            public void ShowPrintPreview(System.Windows.Documents.FlowDocument document, string title, string description) { }
            public void ShowPrintLabelDialog() { }
            public void ShowScannerStatus() { }
        }
    }
}
