using System;
using System.Collections.Generic;
using ToolManagementAppV2.ViewModels;
using ToolManagementAppV2.Interfaces;
using ToolManagementAppV2.Models.Domain;
using Xunit;

namespace ToolManagementAppV2.Tests.ViewModels
{
    public class PasswordPromptViewModelTests
    {
        [Fact]
        public void OkCommand_Succeeds_WhenPasswordValid()
        {
            bool success = false;
            string? error = null;
            var vm = new PasswordPromptViewModel(new StubDialogService(), () => success = true, () => { }, m => error = m)
            {
                ValidatePassword = p => p == "secret"
            };
            vm.EnteredPassword = "secret";

            vm.OkCommand.Execute(null);

            Assert.True(success);
            Assert.Null(error);
        }

        [Fact]
        public void OkCommand_ShowsError_WhenPasswordInvalid()
        {
            bool success = false;
            string? error = null;
            var vm = new PasswordPromptViewModel(new StubDialogService(), () => success = true, () => { }, m => error = m)
            {
                ValidatePassword = p => p == "secret"
            };
            vm.EnteredPassword = "wrong";

            vm.OkCommand.Execute(null);

            Assert.False(success);
            Assert.Equal("Incorrect password. Please try again.", error);
        }

        [Fact]
        public void ResetPasswordCommand_ShowsInfo_ForNonAdmin()
        {
            var dialog = new StubDialogService();
            bool success = false;
            var vm = new PasswordPromptViewModel(dialog, () => success = true, () => { }, _ => { })
            {
                SelectedUser = new User { IsAdmin = false }
            };

            vm.ResetPasswordCommand.Execute(null);

            Assert.True(dialog.InfoShown);
            Assert.False(success);
            Assert.False(vm.IsPasswordResetRequested);
        }

        [Fact]
        public void ResetPasswordCommand_SetsFlag_WhenConfirmed()
        {
            var dialog = new StubDialogService { ConfirmationResult = true };
            bool success = false;
            var vm = new PasswordPromptViewModel(dialog, () => success = true, () => { }, _ => { })
            {
                SelectedUser = new User { IsAdmin = true }
            };

            vm.ResetPasswordCommand.Execute(null);

            Assert.True(success);
            Assert.True(vm.IsPasswordResetRequested);
        }
    }

    class StubDialogService : IDialogService
    {
        public bool InfoShown { get; private set; }
        public bool ConfirmationResult { get; set; }

        public void ShowInfo(string message, string title) => InfoShown = true;
        public bool ShowConfirmation(string message, string title) => ConfirmationResult;
        public ToolModel? ShowEditToolDialog(ToolModel tool) => null;
        public void ShowToolDetails(ToolModel tool) { }
        public (CustomerModel customer, DateTime dueDate)? ShowRentToolDialog(ToolModel tool, IEnumerable<CustomerModel> customers) => null;
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
