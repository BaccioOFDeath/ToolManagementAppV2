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
        public void Items_Accepts_ItemModel()
        {
            var vm = new PrintLabelViewModel(new StubDialogService(), () => { });
            vm.Items.Add(new ItemModel { ItemNumber = "T1" });
            Assert.Single(vm.Items);
        }
    }

    class StubDialogService : IDialogService
    {
        public bool InfoShown { get; private set; }
        public void ShowInfo(string message, string title) => InfoShown = true;
        public bool ShowConfirmation(string message, string title) => false;
        public ItemModel? ShowEditItemDialog(ItemModel item) => null;
        public void ShowItemDetails(ItemModel item) { }
        public (CustomerModel customer, DateTime dueDate)? ShowRentItemDialog(ItemModel item, IEnumerable<CustomerModel> customers) => null;
        public CustomerModel? ShowAddCustomerDialog() => null;
        public void ShowRentalsFilter(ToolManagementAppV2.ViewModels.ManageRentalsViewModel viewModel) { }
        public void ShowRentalHistory(ItemModel item, System.Collections.Generic.IEnumerable<RentalModel> history) { }
        public System.Collections.Generic.Dictionary<string, string>? ShowImportMapping(System.Collections.Generic.IEnumerable<string> headers, System.Collections.Generic.IEnumerable<string> properties) => null;
        public System.Func<ItemModel, System.Collections.Generic.IEnumerable<string>>? ShowImageImportMapping() => null;
        public void ShowPrintPreview(System.Windows.Documents.FlowDocument document, string title, string description) { }
        public void ShowPrintLabelDialog() { }
    }
}
