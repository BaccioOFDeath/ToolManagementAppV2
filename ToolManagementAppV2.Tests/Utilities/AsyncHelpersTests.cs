using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using ToolManagementAppV2.Interfaces;
using ToolManagementAppV2.Utilities;
using Xunit;

namespace ToolManagementAppV2.Tests.Utilities
{
    public class AsyncHelpersTests
    {
        class CapturingDialogService : IDialogService
        {
            public string? Message { get; private set; }
            public string? Title { get; private set; }
            public void ShowInfo(string message, string title)
            {
                Message = message;
                Title = title;
            }
            public Task ShowInfoAsync(string message, string title)
            {
                ShowInfo(message, title);
                return Task.CompletedTask;
            }
            public bool ShowConfirmation(string message, string title) => true;
            public Task<bool> ShowConfirmationAsync(string message, string title) => Task.FromResult(true);
            public ToolModel? ShowEditToolDialog(ToolModel tool) => null;
            public Task<ToolModel?> ShowEditToolDialogAsync(ToolModel tool) => Task.FromResult<ToolModel?>(null);
            public void ShowToolDetails(ToolModel tool) { }
            public (CustomerModel customer, DateTime dueDate)? ShowRentToolDialog(ToolModel tool, System.Collections.Generic.IEnumerable<CustomerModel> customers) => null;
            public CustomerModel? ShowAddCustomerDialog() => null;
            public void ShowRentalsFilter(ManageRentalsViewModel viewModel) { }
            public void ShowRentalHistory(ToolModel tool, System.Collections.Generic.IEnumerable<RentalModel> history) { }
            public System.Collections.Generic.Dictionary<string, string>? ShowImportMapping(System.Collections.Generic.IEnumerable<string> headers, System.Collections.Generic.IEnumerable<string> properties) => null;
            public Func<ToolModel, System.Collections.Generic.IEnumerable<string>>? ShowImageImportMapping() => null;
            public void ShowPrintPreview(System.Windows.Documents.FlowDocument document, string title, string description) { }
            public void ShowPrintLabelDialog() { }
        }

        [Fact]
        public async Task ExecuteSafelyAsync_LogsAndShowsMessage()
        {
            var logger = new TestLogger<AsyncHelpersTests>();
            var dialog = new CapturingDialogService();
            await AsyncHelpers.ExecuteSafelyAsync(_ => throw new InvalidOperationException("boom"), logger, dialog, "fail");
            Assert.Single(logger.Entries);
            Assert.Equal(LogLevel.Error, logger.Entries[0].Level);
            Assert.Equal("fail", dialog.Message);
            Assert.Equal("Error", dialog.Title);
        }

        [Fact]
        public async Task ExecuteSafelyAsync_PropagatesCancellation()
        {
            var logger = new TestLogger<AsyncHelpersTests>();
            var dialog = new CapturingDialogService();
            using var cts = new CancellationTokenSource();
            cts.Cancel();
            await Assert.ThrowsAsync<TaskCanceledException>(() =>
                AsyncHelpers.ExecuteSafelyAsync(async ct =>
                {
                    await Task.Delay(100, ct);
                }, logger, dialog, null, cts.Token));
            Assert.Empty(logger.Entries);
            Assert.Null(dialog.Message);
        }
    }
}
