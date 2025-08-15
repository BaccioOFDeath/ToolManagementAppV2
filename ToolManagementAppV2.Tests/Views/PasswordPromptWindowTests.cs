using System;
using System.Threading;
using ToolManagementAppV2.Interfaces;
using ToolManagementAppV2.Models.Domain;
using ToolManagementAppV2.Views;
using Xunit;

namespace ToolManagementAppV2.Tests.Views
{
    public class PasswordPromptWindowTests
    {
        [Fact]
        public void ResetPasswordCommand_SetsFlag_WhenConfirmed()
        {
            Exception? threadException = null;

            var thread = new Thread(() =>
            {
                try
                {
                    var dialog = new StubDialogService { ConfirmationResult = true };
                    var window = new PasswordPromptWindow(dialog)
                    {
                        SelectedUser = new User { IsAdmin = true }
                    };

                    window.VM.ResetPasswordCommand.ExecuteAsync(null).GetAwaiter().GetResult();

                    Assert.True(window.IsPasswordResetRequested);
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
            {
                throw threadException;
            }
        }

        [Fact]
        public void ResetPasswordCommand_DoesNotSetFlag_WhenCancelled()
        {
            Exception? threadException = null;

            var thread = new Thread(() =>
            {
                try
                {
                    var dialog = new StubDialogService { ConfirmationResult = false };
                    var window = new PasswordPromptWindow(dialog)
                    {
                        SelectedUser = new User { IsAdmin = true }
                    };

                    window.VM.ResetPasswordCommand.ExecuteAsync(null).GetAwaiter().GetResult();

                    Assert.False(window.IsPasswordResetRequested);
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
            {
                throw threadException;
            }
        }

        private class StubDialogService : IDialogService
        {
            public bool ConfirmationResult { get; set; }

            public void ShowInfo(string message, string title) { }
            public bool ShowConfirmation(string message, string title) => ConfirmationResult;
            public ToolModel? ShowEditToolDialog(ToolModel tool) => null;
            public void ShowToolDetails(ToolModel tool) { }
            public (CustomerModel customer, DateTime dueDate)? ShowRentToolDialog(ToolModel tool, System.Collections.Generic.IEnumerable<CustomerModel> customers) => null;
            public CustomerModel? ShowAddCustomerDialog() => null;
            public void ShowRentalsFilter(ToolManagementAppV2.ViewModels.ManageRentalsViewModel viewModel) { }
            public void ShowRentalHistory(ToolModel tool, System.Collections.Generic.IEnumerable<RentalModel> history) { }
            public System.Collections.Generic.Dictionary<string, string>? ShowImportMapping(System.Collections.Generic.IEnumerable<string> headers, System.Collections.Generic.IEnumerable<string> properties) => null;
            public System.Func<ToolModel, System.Collections.Generic.IEnumerable<string>>? ShowImageImportMapping() => null;
            public void ShowPrintPreview(System.Windows.Documents.FlowDocument document, string title, string description) { }
            public void ShowPrintLabelDialog() { }
            public void ShowScannerStatus() { }
        }
    }
}

