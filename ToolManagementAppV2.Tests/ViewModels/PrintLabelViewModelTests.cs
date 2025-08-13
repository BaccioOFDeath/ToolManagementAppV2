using System;
using System.Collections.Generic;
using ToolManagementAppV2.ViewModels;
using ToolManagementAppV2.Interfaces;
using Xunit;

namespace ToolManagementAppV2.Tests.ViewModels
{
    public class PrintLabelViewModelTests
    {
        [Fact]
        public void Constructor_InitializesDefaults()
        {
            var vm = new PrintLabelViewModel(new StubDialogService(), () => { });
            Assert.NotEmpty(vm.Templates);
            Assert.Equal(vm.Templates[0], vm.SelectedTemplate);
            Assert.False(vm.IncludeQr);
            Assert.NotNull(vm.Items);
        }

        [Fact]
        public void CloseCommand_InvokesAction()
        {
            bool closed = false;
            var vm = new PrintLabelViewModel(new StubDialogService(), () => closed = true);
            vm.CloseCommand.Execute(null);
            Assert.True(closed);
        }
    }

    class StubDialogService : IDialogService
    {
        public void ShowInfo(string message, string title) { }
        public bool ShowConfirmation(string message, string title) => false;
        public ToolModel? ShowEditToolDialog(ToolModel tool) => null;
        public void ShowToolDetails(ToolModel tool) { }
        public (CustomerModel customer, DateTime dueDate)? ShowRentToolDialog(ToolModel tool, IEnumerable<CustomerModel> customers) => null;
        public CustomerModel? ShowAddCustomerDialog() => null;
    }
}
