using System;
using ToolManagementAppV2.ViewModels;
using Xunit;

namespace ToolManagementAppV2.Tests.ViewModels
{
    public class PrintLabelViewModelTests
    {
        [Fact]
        public void Constructor_InitializesDefaults()
        {
            var vm = new PrintLabelViewModel(() => { });
            Assert.NotEmpty(vm.Templates);
            Assert.Equal(vm.Templates[0], vm.SelectedTemplate);
            Assert.False(vm.IncludeQr);
            Assert.NotNull(vm.Items);
        }

        [Fact]
        public void CloseCommand_InvokesAction()
        {
            bool closed = false;
            var vm = new PrintLabelViewModel(() => closed = true);
            vm.CloseCommand.Execute(null);
            Assert.True(closed);
        }
    }
}
