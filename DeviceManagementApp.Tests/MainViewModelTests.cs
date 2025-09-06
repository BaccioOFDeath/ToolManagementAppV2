using System;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using DeviceManagementApp.ViewModels;
using Xunit;

namespace DeviceManagementApp.Tests
{
    public class MainViewModelTests
    {
        [Fact]
        public void Constructor_SetsInitialDevicesPage()
        {
            Exception? threadEx = null;
            Page? expectedPage = null;
            Page? currentPage = null;
            string? currentTitle = null;

            var thread = new Thread(() =>
            {
                try
                {
                    var app = new Application();
                    expectedPage = new Page();
                    var vm = new MainViewModel(expectedPage);
                    currentPage = vm.CurrentPage;
                    currentTitle = vm.CurrentPageTitle;
                    Application.Current?.Shutdown();
                }
                catch (Exception ex)
                {
                    threadEx = ex;
                }
            });
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            thread.Join();

            if (threadEx != null) throw threadEx;
            Assert.Same(expectedPage, currentPage);
            Assert.Equal("Devices", currentTitle);
        }
    }
}
