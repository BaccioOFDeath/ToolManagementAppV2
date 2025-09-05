using System;
using System.IO;
using Xunit;

namespace DeviceManagementApp.Tests
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
        public void SideMenu_DoesNotContainDeviceSettingsButton()
        {
            var path = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "InventoryManagementApp", "MainWindow.xaml"));
            var xaml = File.ReadAllText(path);
            Assert.DoesNotContain("Content=\"Device Settings\"", xaml);
            Assert.DoesNotContain("OpenDeviceSettingsCommand", xaml);
        }

        [Fact]
        public void SideMenu_DoesNotContainDeviceStatusButton()
        {
            var path = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "InventoryManagementApp", "MainWindow.xaml"));
            var xaml = File.ReadAllText(path);
            Assert.DoesNotContain("Content=\"Device Status\"", xaml);
            Assert.DoesNotContain("OpenDeviceStatusCommand", xaml);
        }
    }
}
