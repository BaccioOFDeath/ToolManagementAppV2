using System;
using ToolManagementAppV2.ViewModels;
using Xunit;

namespace ToolManagementAppV2.Tests.ViewModels
{
    public class SettingsWindowViewModelTests
    {
        [Fact]
        public void Constructor_CreatesSettingsPage()
        {
            var vm = new SettingsWindowViewModel(() => { });
            Assert.NotNull(vm.SettingsPageContent);
        }

        [Fact]
        public void Commands_InvokeProvidedActions()
        {
            bool saved = false, closed = false;
            var vm = new SettingsWindowViewModel(() => closed = true, () => saved = true);
            vm.SaveSettingsCommand.Execute(null);
            vm.CloseCommand.Execute(null);
            Assert.True(saved);
            Assert.True(closed);
        }
    }
}

