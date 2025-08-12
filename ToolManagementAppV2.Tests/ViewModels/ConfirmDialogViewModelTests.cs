using ToolManagementAppV2.ViewModels;
using Xunit;

namespace ToolManagementAppV2.Tests.ViewModels
{
    public class ConfirmDialogViewModelTests
    {
        [Fact]
        public void OkCommand_SetsDialogResultTrue()
        {
            bool? result = null;
            var vm = new ConfirmDialogViewModel("message", r => result = r);

            vm.OkCommand.Execute(null);

            Assert.True(result);
        }

        [Fact]
        public void CancelCommand_SetsDialogResultFalse()
        {
            bool? result = true;
            var vm = new ConfirmDialogViewModel("message", r => result = r);

            vm.CancelCommand.Execute(null);

            Assert.False(result);
        }
    }
}
