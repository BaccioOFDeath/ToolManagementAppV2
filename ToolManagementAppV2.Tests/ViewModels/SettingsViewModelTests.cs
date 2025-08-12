using System;
using System.IO;
using ToolManagementAppV2.ViewModels;
using Xunit;

namespace ToolManagementAppV2.Tests.ViewModels
{
    public class SettingsViewModelTests
    {
        [Fact]
        public void Constructor_InitializesThemeDefaults()
        {
            var vm = new SettingsViewModel();
            Assert.Contains("Light", vm.ThemeOptions);
            Assert.Equal("Light", vm.Theme);
        }

        [Fact]
        public void TestDbCommand_CreatesDatabaseFile()
        {
            var path = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + ".db");
            try
            {
                var vm = new SettingsViewModel { ConnectionString = path };
                vm.TestDbCommand.Execute(null);
                Assert.True(File.Exists(path));
            }
            finally
            {
                if (File.Exists(path)) File.Delete(path);
            }
        }
    }
}

