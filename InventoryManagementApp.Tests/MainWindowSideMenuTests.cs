using System;
using System.IO;
using Xunit;

namespace InventoryManagementApp.Tests
{
    public class MainWindowSideMenuTests
    {
        [Fact]
        public void SideMenu_DoesNotContainPrintPreviewButton()
        {
            var path = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "InventoryManagementApp", "MainWindow.xaml"));
            var xaml = File.ReadAllText(path);
            Assert.DoesNotContain("Content=\"Print Preview\"", xaml);
            Assert.DoesNotContain("OpenPrintPreviewWindowCommand", xaml);
        }

        [Fact]
        public void SideMenu_ContainsDeviceSettingsButton()
        {
            var path = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "InventoryManagementApp", "MainWindow.xaml"));
            var xaml = File.ReadAllText(path);
            Assert.Contains("Content=\"Device Settings\"", xaml);
            Assert.Contains("OpenDeviceSettingsCommand", xaml);
        }
    }
}
