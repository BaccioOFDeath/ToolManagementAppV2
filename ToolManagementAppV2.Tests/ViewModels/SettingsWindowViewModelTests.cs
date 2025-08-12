using System;
using ToolManagementAppV2.ViewModels;
using Xunit;

namespace ToolManagementAppV2.Tests.ViewModels
{
    public class SettingsWindowViewModelTests
    {
        [Fact]
        public void Constructor_CreatesSettingsPageAndUsesProvidedViewModel()
        {
            var settingsVm = new SettingsViewModel();
            var vm = new SettingsWindowViewModel(settingsVm, () => { });
            Assert.NotNull(vm.SettingsPageContent);
            Assert.Same(settingsVm, vm.SettingsViewModel);
        }

        [Fact]
        public void Commands_InvokeProvidedActions()
        {
            bool saved = false, closed = false;
            var vm = new SettingsWindowViewModel(new SettingsViewModel(), () => closed = true, () => saved = true);
            vm.SaveSettingsCommand.Execute(null);
            vm.CloseCommand.Execute(null);
            Assert.True(saved);
            Assert.True(closed);
        }
    }
}

