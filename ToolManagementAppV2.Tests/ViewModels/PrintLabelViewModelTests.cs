using System;
using System.Collections.Generic;
using ToolManagementAppV2.ViewModels;
using ToolManagementAppV2.Interfaces;
using ToolManagementAppV2.Models;
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

        [Fact]
        public void PrintCommand_WhenPrintFails_LogsError()
        {
            var ds = new StubDialogService();
            var vm = new PrintLabelViewModel(ds, () => { }, _ => throw new Exception("print failed"));
            vm.PrintCommand.Execute(null);
            Assert.True(ds.InfoShown);
        }

        [Fact]
        public void Items_Accepts_ToolModel()
        {
            var vm = new PrintLabelViewModel(new StubDialogService(), () => { });
            vm.Items.Add(new ToolModel { ToolNumber = "T1" });
            Assert.Single(vm.Items);
        }
    }

    class StubDialogService : IDialogService
    {
        public bool InfoShown { get; private set; }
        public void ShowInfo(string message, string title) => InfoShown = true;
        public bool ShowConfirmation(string message, string title) => false;
        public ToolModel? ShowEditToolDialog(ToolModel tool) => null;
        public void ShowToolDetails(ToolModel tool) { }
        public (CustomerModel customer, DateTime dueDate)? ShowRentToolDialog(ToolModel tool, IEnumerable<CustomerModel> customers) => null;
        public CustomerModel? ShowAddCustomerDialog() => null;
    }
}
