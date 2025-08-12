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
            var settingsVm = new SettingsViewModel(new StubFileDialogService(), new StubSettingsService());
            var vm = new SettingsWindowViewModel(settingsVm, () => { });
            Assert.NotNull(vm.SettingsPageContent);
            Assert.Same(settingsVm, vm.SettingsViewModel);
        }

        [Fact]
        public void Commands_InvokeProvidedActions()
        {
            bool saved = false, closed = false;
            var vm = new SettingsWindowViewModel(new SettingsViewModel(new StubFileDialogService(), new StubSettingsService()), () => closed = true, () => saved = true);
            vm.SaveSettingsCommand.Execute(null);
            vm.CloseCommand.Execute(null);
            Assert.True(saved);
            Assert.True(closed);
        }
    }

    class StubFileDialogService : ToolManagementAppV2.Interfaces.IFileDialogService
    {
        public string? OpenFile(string filter) => null;
        public string? SaveFile(string filter) => null;
    }

    class StubSettingsService : ToolManagementAppV2.Interfaces.ISettingsService
    {
        public void SaveSetting(string key, string value) { }
        public string GetSetting(string key) => string.Empty;
        public Dictionary<string, string> GetAllSettings() => new();
        public void UpdateSettings(Dictionary<string, string> settings) { }
        public void DeleteSetting(string key) { }
    }
}

