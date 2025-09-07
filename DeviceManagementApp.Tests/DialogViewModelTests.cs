using DeviceManagementApp.ViewModels;
using Xunit;

namespace DeviceManagementApp.Tests
{
    public class DialogViewModelTests
    {
        [Fact]
        public void InfoDialog_OkCommand_InvokesClose()
        {
            bool closed = false;
            var vm = new InfoDialogViewModel("Test", () => closed = true);
            vm.OkCommand.Execute(null);
            Assert.True(closed);
        }

        [Fact]
        public void ConfirmDialog_OkCommand_SendsTrue()
        {
            bool? result = null;
            var vm = new ConfirmDialogViewModel("Test", r => result = r);
            vm.OkCommand.Execute(null);
            Assert.True(result);
        }

        [Fact]
        public void ConfirmDialog_CancelCommand_SendsFalse()
        {
            bool? result = null;
            var vm = new ConfirmDialogViewModel("Test", r => result = r);
            vm.CancelCommand.Execute(null);
            Assert.False(result);
        }
    }
}
