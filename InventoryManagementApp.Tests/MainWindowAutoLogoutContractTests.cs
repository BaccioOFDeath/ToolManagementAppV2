using System;
using System.IO;
using Xunit;

namespace InventoryManagementApp.Tests
{
    public class MainWindowAutoLogoutContractTests
    {
        [Fact]
        public void MainWindowResetsAutoLogoutFromAppWideInput()
        {
            var source = ReadRepoFile("InventoryManagementApp", "MainWindow.xaml.cs");

            Assert.Contains("InputManager.Current.PreProcessInput += InputManager_PreProcessInput;", source, StringComparison.Ordinal);
            Assert.Contains("InputManager.Current.PreProcessInput -= InputManager_PreProcessInput;", source, StringComparison.Ordinal);
            Assert.Contains("void InputManager_PreProcessInput(object sender, PreProcessInputEventArgs e)", source, StringComparison.Ordinal);
            Assert.Contains("e.StagingItem.Input is MouseEventArgs or KeyboardEventArgs", source, StringComparison.Ordinal);
            Assert.Contains("_mainViewModel.ResetAutoLogoutTimer();", source, StringComparison.Ordinal);
        }

        private static string ReadRepoFile(params string[] parts)
        {
            var directory = AppContext.BaseDirectory;

            while (!string.IsNullOrEmpty(directory))
            {
                var candidate = Path.Combine(directory, Path.Combine(parts));
                if (File.Exists(candidate))
                    return File.ReadAllText(candidate);

                var parent = Directory.GetParent(directory);
                if (parent is null)
                    break;

                directory = parent.FullName;
            }

            throw new FileNotFoundException($"Could not find repository file: {Path.Combine(parts)}");
        }
    }
}
